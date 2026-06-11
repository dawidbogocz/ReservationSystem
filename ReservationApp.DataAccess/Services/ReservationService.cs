using ReservationApp.DataAccess.Repository.IRepository;
using ReservationApp.Models;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ReservationApp.DataAccess.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservationService> _logger;
        private readonly IConfiguration _config;

        public ReservationService(
            IServiceScopeFactory scopeFactory,
            IConfiguration config,
            ILogger<ReservationService> logger)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 15 * 60)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SendUpcomingReservationReminders()
        {
            using var scope = _scopeFactory.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            var now = DateTime.Now;
            var reminderWindowHours = _config.GetValue<int>("ReminderSettings:WindowHours", 24);

            var reservations = await unitOfWork.Reservation.GetAllAsync(
                r => r.Approval == Approval.Accepted
                     && !r.EmailReminderSent
                     && r.PickupDate > now
                     && r.PickupDate <= now.AddHours(reminderWindowHours),
                includeProperties: "Asset,User");

            foreach (var reservation in reservations)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(reservation.User?.Email))
                        continue;

                    var claimedRows = await unitOfWork.Context.Reservation
                        .Where(r => r.Id == reservation.Id && !r.EmailReminderSent)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(r => r.EmailReminderSent, true));

                    if (claimedRows == 0)
                        continue;

                    var vehicleName = GetAssetName(reservation.Asset.AssetType);

                    string subject = "Reservation Reminder";
                    string body = $"This is a reminder that you have a reservation for {vehicleName} {reservation.AssetTag} " +
                                  $"on {reservation.PickupDate:dd-MM-yyyy HH:mm}.";

                    await emailSender.SendEmailAsync(reservation.User.Email, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send reservation reminder for reservation {ReservationId}",
                        reservation.Id);
                }
            }
        }

        public async Task CheckAndSendFeedbackReminders()
        {
            using var scope = _scopeFactory.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var departmentNotifications = scope.ServiceProvider.GetRequiredService<IDepartmentNotificationService>();

            var expirationDays = _config.GetValue<int>("FeedbackSettings:ExpirationDays", 2);
            var now = DateTime.Now;
            var oneDayAgo = now.AddDays(-1);
            var oneDayFromNow = now.AddDays(1);
            var expirationDate = now.AddDays(-expirationDays);

            // Only fetch reservations within the relevant date window
            var relevantReservations = await unitOfWork.Reservation.GetAllAsync(
                r => r.Approval == Approval.Accepted
                     && r.PickupDate >= expirationDate
                     && (r.PickupDate <= oneDayFromNow || r.ReturnDate >= oneDayAgo || r.PickupFeedbackDate == null),
                includeProperties: "Asset,User");

            if (!relevantReservations.Any())
                return;

            var reservationIds = relevantReservations.Select(r => r.Id).ToList();

            // Only fetch feedback logs for these reservations
            var allFeedbackLogs = await unitOfWork.Context.Set<FeedbackLog>()
                .Where(f => reservationIds.Contains(f.ReservationId))
                .ToListAsync();

            foreach (var reservation in relevantReservations)
            {
                using var transaction = await unitOfWork.Context.Database.BeginTransactionAsync();

                try
                {
                    var pickupLog = allFeedbackLogs.FirstOrDefault(f =>
                        f.ReservationId == reservation.Id &&
                        f.Kind == FeedbackKind.Pickup);

                    var returnLog = allFeedbackLogs.FirstOrDefault(f =>
                        f.ReservationId == reservation.Id &&
                        f.Kind == FeedbackKind.Return);

                    bool newPickupStarted = relevantReservations.Any(r =>
                        r.AssetTag == reservation.AssetTag &&
                        r.Id != reservation.Id &&
                        r.PickupDate > reservation.ReturnDate &&
                        allFeedbackLogs.Any(f =>
                            f.ReservationId == r.Id &&
                            f.Kind == FeedbackKind.Pickup &&
                            f.Status == FeedbackStatus.Pending));

                    if (pickupLog == null &&
                        reservation.PickupDate >= oneDayAgo &&
                        reservation.PickupDate <= oneDayFromNow &&
                        reservation.PickupFeedbackDate == null)
                    {
                        pickupLog = new FeedbackLog
                        {
                            ReservationId = reservation.Id,
                            AssetTag = reservation.AssetTag,
                            UserId = reservation.UserId,
                            Kind = FeedbackKind.Pickup,
                            Status = FeedbackStatus.Pending,
                            CreatedAt = now
                        };

                        await unitOfWork.Context.Set<FeedbackLog>().AddAsync(pickupLog);
                    }

                    if (returnLog == null &&
                        reservation.ReturnDate >= oneDayAgo &&
                        reservation.ReturnDate <= oneDayFromNow &&
                        reservation.PickupFeedbackDate != null &&
                        reservation.ReturnFeedbackDate == null)
                    {
                        returnLog = new FeedbackLog
                        {
                            ReservationId = reservation.Id,
                            AssetTag = reservation.AssetTag,
                            UserId = reservation.UserId,
                            Kind = FeedbackKind.Return,
                            Status = FeedbackStatus.Pending,
                            CreatedAt = now
                        };

                        await unitOfWork.Context.Set<FeedbackLog>().AddAsync(returnLog);
                    }

                    bool pickupExpiredNow = false;
                    bool returnExpiredNow = false;

                    if ((now >= reservation.ReturnDate || reservation.PickupDate < expirationDate) &&
                        reservation.PickupFeedbackDate == null &&
                        pickupLog?.Status == FeedbackStatus.Pending)
                    {
                        pickupLog.Status = FeedbackStatus.Expired;
                        reservation.PickupFeedbackDate = now;
                        pickupExpiredNow = true;
                    }

                    if ((newPickupStarted || reservation.ReturnDate < expirationDate) &&
                        reservation.ReturnFeedbackDate == null &&
                        returnLog?.Status == FeedbackStatus.Pending)
                    {
                        returnLog.Status = FeedbackStatus.Expired;
                        reservation.ReturnFeedbackDate = now;
                        returnExpiredNow = true;
                    }

                    unitOfWork.Reservation.Update(reservation);
                    unitOfWork.Save();

                    await transaction.CommitAsync();

                    if (pickupExpiredNow)
                    {
                        await SendExpiredFeedbackAlertAsync(
                            departmentNotifications,
                            reservation,
                            "pickup");
                    }

                    if (returnExpiredNow)
                    {
                        await SendExpiredFeedbackAlertAsync(
                            departmentNotifications,
                            reservation,
                            "return");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error processing feedback reminders for reservation {ReservationId}",
                        reservation.Id);

                    await transaction.RollbackAsync();
                }
            }
        }

        private async Task SendExpiredFeedbackAlertAsync(
            IDepartmentNotificationService departmentNotifications,
            Reservation reservation,
            string feedbackType)
        {
            string subject = $"Missing feedback after {feedbackType}";

            string body =
                $"Hello,<br/><br/>" +
                $"User <strong>{reservation.User?.FirstName} {reservation.User?.LastName}</strong> " +
                $"did not complete the feedback after {feedbackType}.<br/><br/>" +
                "<ul>" +
                $"<li><strong>Reservation ID:</strong> {reservation.Id}</li>" +
                $"<li><strong>Asset:</strong> {reservation.AssetTag}</li>" +
                $"<li><strong>From:</strong> {reservation.PickupDate:dd-MM-yyyy HH:mm}</li>" +
                $"<li><strong>To:</strong> {reservation.ReturnDate:dd-MM-yyyy HH:mm}</li>" +
                "</ul>";

            await departmentNotifications.SendToGroupManagersAndAdminsAsync(
                reservation.UserId,
                subject,
                body);
        }

        private string GetAssetName(AssetType type)
        {
            return "asset";
        }
    }
}