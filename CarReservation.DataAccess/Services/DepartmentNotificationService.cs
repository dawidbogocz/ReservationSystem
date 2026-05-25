using CarReservation.DataAccess.Repository.IRepository;
using CarReservation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarReservation.DataAccess.Services
{
    public class DepartmentNotificationService : IDepartmentNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<DepartmentNotificationService> _logger;

        public DepartmentNotificationService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<DepartmentNotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<List<ApplicationUser>> GetAdminsAsync()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = admins.Select(x => x.Id).Distinct().ToList();

            if (!adminIds.Any())
                return new List<ApplicationUser>();

            return await _unitOfWork.Context.ApplicationUser
                .Where(u => adminIds.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetGroupManagersAsync(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<ApplicationUser>();

            var userGroupId = await _unitOfWork.Context.ApplicationUser
                .Where(u => u.Id == userId)
                .Select(u => u.UserGroupId)
                .FirstOrDefaultAsync();

            if (userGroupId == null)
                return new List<ApplicationUser>();

            return await _unitOfWork.Context.UserGroupManagers
                .Where(x => x.UserGroupId == userGroupId.Value)
                .Include(x => x.Manager)
                .Select(x => x.Manager)
                .ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetGroupManagersAndAdminsAsync(string? userId)
        {
            var result = new Dictionary<string, ApplicationUser>();

            var managers = await GetGroupManagersAsync(userId);
            foreach (var manager in managers)
            {
                if (!string.IsNullOrWhiteSpace(manager.Id))
                    result[manager.Id] = manager;
            }

            var admins = await GetAdminsAsync();
            foreach (var admin in admins)
            {
                if (!string.IsNullOrWhiteSpace(admin.Id))
                    result[admin.Id] = admin;
            }

            return result.Values
                .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                .ToList();
        }

        public async Task SendToAdminsAsync(string subject, string body)
        {
            var admins = await GetAdminsAsync();
            await SendAsync(admins, subject, body);
        }

        public async Task SendToGroupManagersAndAdminsAsync(string? userId, string subject, string body)
        {
            var recipients = await GetGroupManagersAndAdminsAsync(userId);
            await SendAsync(recipients, subject, body);
        }

        private async Task SendAsync(IEnumerable<ApplicationUser> recipients, string subject, string body)
        {
            foreach (var recipient in recipients
                         .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                         .GroupBy(x => x.Email!.Trim().ToLower())
                         .Select(g => g.First()))
            {
                try
                {
                    await _emailSender.SendEmailAsync(recipient.Email!, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send department notification to {Email}",
                        recipient.Email);
                }
            }
        }
    }
}