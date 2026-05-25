using ReservationApp.DataAccess.Repository.IRepository;
using ReservationApp.Models;
using ReservationApp.Models.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ReservationApp.Areas.Manager.Controllers
{
    /// <summary>
    /// Provides reservation management functionality for managers and administrators,
    /// including approval, deletion, and export to Excel.
    /// </summary>
    [Area("Manager")]
    [Authorize(Policy = "AdminManager")]
    public class ReservationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            ILogger<ReservationController> logger)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
            _config = config;
            _logger = logger;
        }

        #region VIEWS

        /// <summary>
        /// Returns the correct Polish noun forms for a given vehicle type.
        /// </summary>
        private (string mianownik, string dopełniacz, string narzędnik) GetVehicleWord(AssetType type)
        {
            return type == AssetType.Lift
                ? ("podnośnik", "podnośnika", "podnośnikiem")
                : ("samochód", "samochodu", "samochodem");
        }

        /// <summary>
        /// Displays a list of all reservations in the system.
        /// </summary>
        public async Task<IActionResult> Index(int? reservationId)
        {
            try
            {
                var reservations = await GetVisibleReservationsForCurrentUserAsync(reservationId);

                ViewBag.CarPlates = reservations
                    .Select(r => r.AssetTag)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToList();

                ViewBag.Users = reservations
                    .Select(r => $"{r.User.FirstName} {r.User.LastName}")
                    .Distinct()
                    .OrderBy(u => u)
                    .ToList();

                ViewBag.SelectedReservationId = reservationId;

                return View(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load reservation list.");
                TempData["error"] = "Failed to load reservations.";
                return View(new List<Reservation>());
            }
        }

        /// <summary>
        /// Displays the upsert view for creating or editing a reservation.
        /// </summary>
        public async Task<IActionResult> Upsert(int? id)
        {
            try
            {
                var usersForManager = await GetVisibleUsersForCurrentUserAsync();

                var reservationVM = new ReservationVM
                {
                    Reservation = new Reservation(),
                    AssetList = (await _unitOfWork.Asset.GetAllAsync(c => !c.IsDeleted))
                        .Select(i => new SelectListItem
                        {
                            Text = i.AssetTag,
                            Value = i.AssetTag
                        }),
                    UserList = usersForManager
                        .Select(i => new SelectListItem
                        {
                            Text = $"{i.FirstName} {i.LastName}",
                            Value = i.Id
                        })
                };

                if (id == null || id == 0)
                    return View(reservationVM);

                var reservation = await _unitOfWork.Context.Reservation
                    .Include(r => r.Asset)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == id.Value);

                if (reservation == null)
                    return NotFound();

                if (!await CanCurrentUserAccessReservationAsync(reservation))
                    return Forbid();

                reservationVM.Reservation = reservation;
                return View(reservationVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load reservation edit form for ID {Id}", id);
                TempData["error"] = "Failed to load reservation.";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region UPSERT

        /// <summary>
        /// Handles creating or updating a reservation from the manager panel.
        /// Sends appropriate notifications upon approval or rejection.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Upsert(ReservationVM reservationVM)
        {
            var usersForManager = await GetVisibleUsersForCurrentUserAsync();

            if (!ModelState.IsValid)
            {
                reservationVM.AssetList = (await _unitOfWork.Asset.GetAllAsync(c => !c.IsDeleted))
                    .Select(i => new SelectListItem { Text = i.AssetTag, Value = i.AssetTag });

                reservationVM.UserList = usersForManager
                    .Select(i => new SelectListItem { Text = $"{i.FirstName} {i.LastName}", Value = i.Id });

                TempData["error"] = "Coś poszło nie tak";
                return View(reservationVM);
            }

            if (!usersForManager.Any(u => u.Id == reservationVM.Reservation.UserId))
                return Forbid();

            try
            {
                bool isNewReservation = reservationVM.Reservation.Id == 0;
                string statusMessage = isNewReservation ? "Dodano nową rezerwację" : "Zaktualizowano rezerwację";

                var reservation = isNewReservation
                    ? new Reservation()
                    : await _unitOfWork.Context.Reservation
                        .Include(r => r.User)
                        .Include(r => r.Asset)
                        .FirstOrDefaultAsync(r => r.Id == reservationVM.Reservation.Id);

                if (reservation == null)
                    return NotFound();

                if (!isNewReservation && !await CanCurrentUserAccessReservationAsync(reservation))
                    return Forbid();

                reservation.AssetTag = reservationVM.Reservation.AssetTag;
                reservation.UserId = reservationVM.Reservation.UserId;
                reservation.PickupDate = reservationVM.Reservation.PickupDate;
                reservation.ReturnDate = reservationVM.Reservation.ReturnDate;
                reservation.Destination = reservationVM.Reservation.Destination;
                reservation.Approval = reservationVM.Reservation.Approval;
                reservation.ReturnMileage = reservationVM.Reservation.ReturnMileage;

                var currentUser = await _userManager.GetUserAsync(User);
                reservation.ApprovedBy = currentUser?.UserName;
                reservation.ApprovalDate = DateTime.Now;

                if (isNewReservation)
                    await _unitOfWork.Reservation.AddAsync(reservation);
                else
                    _unitOfWork.Reservation.Update(reservation);

                _unitOfWork.Save();

                TempData["success"] = statusMessage;

                if (reservation.Approval == Approval.Zaakceptowane || reservation.Approval == Approval.Odrzucone)
                {
                    await SendReservationEmailsAsync(reservation);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create/update reservation for ID {Id}", reservationVM.Reservation.Id);
                TempData["error"] = "An error occurred while saving the reservation.";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region EMAIL

        /// <summary>
        /// Sends notification emails for reservation approval or rejection.
        /// </summary>
        private async Task SendReservationEmailsAsync(Reservation reservation)
        {
            try
            {
                var user = await _unitOfWork.ApplicationUser.GetAsync(u => u.Id == reservation.UserId);

                if (user == null)
                    return;

                string approvalStatus = reservation.Approval.ToString();

                string userSubject = "Status Twojej rezerwacji";

                string userBody = $"Witaj {user.FirstName},<br/><br/>" +
                                  $"Twoja rezerwacja pojazdu o numerze rejestracyjnym " +
                                  $"<strong>{reservation.AssetTag}</strong> " +
                                  $"na okres od <strong>{reservation.PickupDate:yyyy-MM-dd HH:mm}</strong> " +
                                  $"do <strong>{reservation.ReturnDate:yyyy-MM-dd HH:mm}</strong> " +
                                  $"została oznaczona jako <strong>{approvalStatus}</strong>.";

                await _emailSender.SendEmailAsync(user.Email, userSubject, userBody);

                var managers = await GetGroupManagersForUserAsync(user.Id);

                foreach (var manager in managers.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
                {
                    string managerSubject = "Powiadomienie o rezerwacji użytkownika";

                    string managerBody = $"Dzień dobry,<br/><br/>" +
                                         $"Użytkownik <strong>{user.FirstName} {user.LastName}</strong> " +
                                         $"ma rezerwację pojazdu o numerze rejestracyjnym " +
                                         $"<strong>{reservation.AssetTag}</strong> " +
                                         $"na okres od <strong>{reservation.PickupDate:yyyy-MM-dd HH:mm}</strong> " +
                                         $"do <strong>{reservation.ReturnDate:yyyy-MM-dd HH:mm}</strong>.<br/><br/>" +
                                         $"Status rezerwacji: <strong>{approvalStatus}</strong>.";

                    await _emailSender.SendEmailAsync(manager.Email, managerSubject, managerBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reservation emails for reservation ID {Id}", reservation.Id);
            }
        }

        private async Task SendReservationDeletionEmailsAsync(Reservation reservation)
        {
            try
            {
                var user = await _unitOfWork.ApplicationUser.GetAsync(u => u.Id == reservation.UserId);

                if (user == null)
                    return;

                string userSubject = "Twoja rezerwacja została usunięta";

                string userBody = $"Witaj {user.FirstName},<br/><br/>" +
                                  $"Twoja rezerwacja pojazdu o numerze rejestracyjnym " +
                                  $"<strong>{reservation.AssetTag}</strong> " +
                                  $"na okres od <strong>{reservation.PickupDate:yyyy-MM-dd HH:mm}</strong> " +
                                  $"do <strong>{reservation.ReturnDate:yyyy-MM-dd HH:mm}</strong> " +
                                  $"została usunięta.";

                await _emailSender.SendEmailAsync(user.Email, userSubject, userBody);

                var managers = await GetGroupManagersForUserAsync(user.Id);

                foreach (var manager in managers.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
                {
                    string managerSubject = "Powiadomienie o usunięciu rezerwacji";

                    string managerBody = $"Dzień dobry,<br/><br/>" +
                                         $"Rezerwacja użytkownika <strong>{user.FirstName} {user.LastName}</strong> " +
                                         $"dla pojazdu o numerze rejestracyjnym " +
                                         $"<strong>{reservation.AssetTag}</strong> " +
                                         $"na okres od <strong>{reservation.PickupDate:yyyy-MM-dd HH:mm}</strong> " +
                                         $"do <strong>{reservation.ReturnDate:yyyy-MM-dd HH:mm}</strong> " +
                                         $"została usunięta.";

                    await _emailSender.SendEmailAsync(manager.Email, managerSubject, managerBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reservation deletion emails for reservation ID {Id}", reservation.Id);
            }
        }

        #endregion

        #region API CALLS

        /// <summary>
        /// Retrieves a JSON list of reservations for client-side processing.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(int? reservationId)
        {
            try
            {
                var reservations = await GetVisibleReservationsForCurrentUserAsync(reservationId);

                var reservationList = reservations.Select(r => new
                {
                    id = r.Id,
                    numberPlate = r.Asset.AssetTag,
                    pickupDate = r.PickupDate.ToString("yyyy-MM-dd HH:mm"),
                    returnDate = r.ReturnDate.ToString("yyyy-MM-dd HH:mm"),
                    destination = r.Destination,
                    user = $"{r.User.FirstName} {r.User.LastName}",
                    approval = r.Approval.ToString(),
                    approvedBy = r.ApprovedBy,
                    approvalDate = r.ApprovalDate,
                    returnMileage = r.ReturnMileage,
                    note = r.Note
                }).ToList();

                return Json(new { data = reservationList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load reservation list via API.");
                return Json(new { data = new List<object>() });
            }
        }

        /// <summary>
        /// Deletes a reservation by its ID.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _unitOfWork.Context.Reservation
                .Include(r => r.Asset)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return Json(new { success = false, message = "Błąd podczas usuwania: Rezerwacja nie znaleziona" });

            if (!await CanCurrentUserAccessReservationAsync(reservation))
                return Json(new { success = false, message = "Nie masz uprawnień do tej rezerwacji." });

            using (var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync())
            {
                try
                {
                    var feedbackLogs = await _unitOfWork.Context.Set<FeedbackLog>()
                        .Where(f => f.ReservationId == id)
                        .ToListAsync();

                    if (feedbackLogs.Any())
                    {
                        _unitOfWork.Context.Set<FeedbackLog>().RemoveRange(feedbackLogs);
                    }

                    _unitOfWork.Reservation.Remove(reservation);
                    _unitOfWork.Save();

                    await SendReservationDeletionEmailsAsync(reservation);

                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "Rezerwacja i powiązane opinie zostały usunięte pomyślnie" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete reservation {Id}", id);
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Error deleting reservation." });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateApproval(int id, string status)
        {
            var reservation = await _unitOfWork.Context.Reservation
                .Include(r => r.Asset)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return Json(new { success = false, message = "Rezerwacja nie znaleziona." });

            if (!await CanCurrentUserAccessReservationAsync(reservation))
                return Json(new { success = false, message = "Nie masz uprawnień do tej rezerwacji." });

            if (reservation.Approval != Approval.Oczekujace)
                return Json(new { success = false, message = "Rezerwacja została już przetworzona." });

            if (!Enum.TryParse<Approval>(status, out var newStatus))
                return Json(new { success = false, message = "Nieprawidłowy status." });

            var currentUser = await _userManager.GetUserAsync(User);
            reservation.Approval = newStatus;
            reservation.ApprovalDate = DateTime.Now;
            reservation.ApprovedBy = currentUser?.UserName;

            try
            {
                _unitOfWork.Reservation.Update(reservation);
                _unitOfWork.Save();

                if (newStatus == Approval.Zaakceptowane || newStatus == Approval.Odrzucone)
                {
                    await SendReservationEmailsAsync(reservation);
                }

                return Json(new { success = true, message = $"Rezerwacja została oznaczona jako {newStatus}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update approval for reservation {Id}", id);
                return Json(new { success = false, message = "Failed to update status." });
            }
        }

        #endregion

        #region GROUP ACCESS HELPERS

        private async Task<bool> IsCurrentUserAdminAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return false;

            return await _userManager.IsInRoleAsync(currentUser, "Admin");
        }

        private async Task<List<int>> GetCurrentManagerGroupIdsAsync()
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(currentUserId))
                return new List<int>();

            return await _unitOfWork.Context.UserGroupManagers
                .Where(x => x.ManagerId == currentUserId)
                .Select(x => x.UserGroupId)
                .Distinct()
                .ToListAsync();
        }

        private async Task<List<Reservation>> GetVisibleReservationsForCurrentUserAsync(int? reservationId = null)
        {
            var query = _unitOfWork.Context.Reservation
                .Include(r => r.Asset)
                .Include(r => r.User)
                .AsQueryable();

            if (reservationId.HasValue)
            {
                query = query.Where(r => r.Id == reservationId.Value);
            }

            if (await IsCurrentUserAdminAsync())
            {
                return await query
                    .OrderByDescending(r => r.PickupDate)
                    .ToListAsync();
            }

            var groupIds = await GetCurrentManagerGroupIdsAsync();

            if (!groupIds.Any())
                return new List<Reservation>();

            return await query
                .Where(r => r.User.UserGroupId != null && groupIds.Contains(r.User.UserGroupId.Value))
                .OrderByDescending(r => r.PickupDate)
                .ToListAsync();
        }

        private async Task<List<ApplicationUser>> GetVisibleUsersForCurrentUserAsync()
        {
            var query = _unitOfWork.Context.ApplicationUser.AsQueryable();

            if (await IsCurrentUserAdminAsync())
            {
                return await query
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToListAsync();
            }

            var groupIds = await GetCurrentManagerGroupIdsAsync();

            if (!groupIds.Any())
                return new List<ApplicationUser>();

            return await query
                .Where(u => u.UserGroupId != null && groupIds.Contains(u.UserGroupId.Value))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
        }

        private async Task<bool> CanCurrentUserAccessReservationAsync(Reservation reservation)
        {
            if (await IsCurrentUserAdminAsync())
                return true;

            var groupIds = await GetCurrentManagerGroupIdsAsync();

            if (!groupIds.Any())
                return false;

            var userGroupId = reservation.User?.UserGroupId;

            if (userGroupId == null)
            {
                userGroupId = await _unitOfWork.Context.ApplicationUser
                    .Where(u => u.Id == reservation.UserId)
                    .Select(u => u.UserGroupId)
                    .FirstOrDefaultAsync();
            }

            return userGroupId != null && groupIds.Contains(userGroupId.Value);
        }

        private async Task<List<ApplicationUser>> GetGroupManagersForUserAsync(string userId)
        {
            var user = await _unitOfWork.Context.ApplicationUser
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.UserGroupId == null)
                return new List<ApplicationUser>();

            return await _unitOfWork.Context.UserGroupManagers
                .Where(x => x.UserGroupId == user.UserGroupId)
                .Include(x => x.Manager)
                .Select(x => x.Manager)
                .ToListAsync();
        }

        #endregion

        #region EXCEL

        public async Task<FileResult> ExportToExcel(string? status, string? car, string? user, DateTime? dateFrom, DateTime? dateTo, int? reservationId)
        {
            try
            {
                var reservations = await GetVisibleReservationsForCurrentUserAsync(reservationId);
                var query = reservations.AsQueryable();

                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Approval>(status, out var st))
                    query = query.Where(r => r.Approval == st);

                if (!string.IsNullOrWhiteSpace(car))
                    query = query.Where(r => r.AssetTag == car);

                if (!string.IsNullOrWhiteSpace(user))
                    query = query.Where(r => (r.User.FirstName + " " + r.User.LastName) == user);

                if (dateFrom.HasValue)
                    query = query.Where(r => r.PickupDate.Date >= dateFrom.Value.Date);

                if (dateTo.HasValue)
                    query = query.Where(r => r.PickupDate.Date <= dateTo.Value.Date);

                var exportReservations = query.OrderBy(r => r.PickupDate).ToList();

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Rezerwacje");

                ws.Cell(1, 1).InsertTable(exportReservations.Select(r => new
                {
                    r.Id,
                    Samochód = r.AssetTag,
                    Kierowca = $"{r.User.FirstName} {r.User.LastName}",
                    Od = r.PickupDate,
                    Do = r.ReturnDate,
                    Cel = r.Destination,
                    Status = r.Approval.ToString(),
                    Przebieg_km = r.ReturnMileage
                }));

                ws.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                wb.SaveAs(stream);
                stream.Position = 0;

                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"rezerwacje_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export reservations to Excel.");
                throw;
            }
        }

        #endregion
    }
}