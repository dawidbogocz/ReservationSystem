using System.Diagnostics;
using System.Security.Claims;
using ReservationApp.DataAccess.Repository.IRepository;
using ReservationApp.DataAccess.Services;
using ReservationApp.Models;
using ReservationApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ReservationApp.Areas.Employee.Controllers
{
    /// <summary>
    /// Manages operations for employee users, including viewing available vehicles,
    /// creating reservations, and providing feedback.
    /// </summary>
    [Area("Employee")]
    [Authorize(Policy = "Anyone")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;
        private readonly IReservationService _reservationService;
        private readonly IDepartmentNotificationService _departmentNotifications;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            IConfiguration config,
            IReservationService reservationService,
            IDepartmentNotificationService departmentNotifications,
            ILogger<HomeController> logger)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _config = config;
            _reservationService = reservationService;
            _departmentNotifications = departmentNotifications;
            _logger = logger;
        }

        /// <summary>
        /// Returns the generic term for an asset.
        /// </summary>
        private (string nominative, string genitive, string instrumental) GetVehicleWord(AssetType type)
        {
            return ("asset", "asset", "asset");
        }

        /// <summary>
        /// Displays the main page with available vehicles and manages reminders
        /// for upcoming reservations.
        /// </summary>
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var start = startDate ?? DateTime.Now.Date;
                var end = endDate ?? DateTime.Now.Date.AddDays(1).AddTicks(-1);

                var assets = await _unitOfWork.Asset.GetAllAsync(c => !c.IsDeleted, "Reservations.User,Faults");

                var upcomingReservations = (await _unitOfWork.Reservation.GetAllAsync(null, "Asset"))
                    .Where(r => r.UserId == userId
                                && r.Approval == Approval.Accepted
                                && r.PickupDate > DateTime.Now
                                && r.PickupDate <= DateTime.Now.AddDays(2))
                    .ToList();

                var reminders = new List<string>();

                foreach (var reservation in upcomingReservations)
                {
                    var words = GetVehicleWord(reservation.Asset.AssetType);
                    reminders.Add($"Reminder: You have a reservation of asset {reservation.AssetTag} on {reservation.PickupDate:dd-MM-yyyy HH:mm}");
                }

                ViewBag.ReminderNotifications = reminders;

                var pendingPickup = (await _unitOfWork.Reservation.GetAllAsync(r => r.Asset.IsDeleted == false, "Asset"))
                    .FirstOrDefault(r => r.UserId == userId
                        && r.Approval == Approval.Accepted
                        && DateTime.Now >= r.PickupDate
                        && r.IsAssetDamagedAtPickup == null
                        && DateTime.Now < r.ReturnDate);

                var pendingReturn = (await _unitOfWork.Reservation.GetAllAsync(r => r.Asset.IsDeleted == false, "Asset"))
                    .FirstOrDefault(r => r.UserId == userId
                        && r.Approval == Approval.Accepted
                        && DateTime.Now >= r.ReturnDate
                        && r.IsAssetDamagedAtReturn == null);

                ViewBag.PendingPickupReservationId = pendingPickup?.Id;
                ViewBag.PendingReturnReservationId = pendingReturn?.Id;
                ViewBag.StartDate = start;
                ViewBag.EndDate = end;

                return View(assets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Employee Home index view.");

                ViewBag.ReminderNotifications = new List<string>();
                ViewBag.PendingPickupReservationId = null;
                ViewBag.PendingReturnReservationId = null;
                ViewBag.StartDate = startDate ?? DateTime.Now.Date;
                ViewBag.EndDate = endDate ?? DateTime.Now.Date.AddDays(1).AddTicks(-1);

                TempData["error"] = "An error occurred while loading the home page.";
                return View(new List<Asset>());
            }
        }

        /// <summary>
        /// Displays the reservation creation form for a specific asset.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(string assetTag, DateTime startDate, DateTime endDate)
        {
            try
            {
                var asset = (await _unitOfWork.Asset.GetAllAsync(c => !c.IsDeleted))
                    .FirstOrDefault(c => c.AssetTag == assetTag);

                if (asset == null)
                {
                    TempData["error"] = "Asset not found!";
                    return RedirectToAction("Index");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var vm = new ReservationVM
                {
                    Reservation = new Reservation
                    {
                        AssetTag = asset.AssetTag,
                        PickupDate = startDate,
                        ReturnDate = endDate,
                        Asset = asset,
                        UserId = userId
                    },
                    AssetList = new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Text = asset.AssetTag,
                            Value = asset.AssetTag,
                            Disabled = true
                        }
                    }
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reservation create view for asset {AssetTag}", assetTag);
                TempData["error"] = "An error occurred while preparing the reservation form.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Processes a new reservation submission, sending notifications and preventing double-booking.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(ReservationVM obj)
        {
            try
            {
                var asset = await _unitOfWork.Asset.GetAsync(c => c.AssetTag == obj.Reservation.AssetTag);
                if (asset == null)
                {
                    TempData["error"] = "Asset not found!";
                    obj.AssetList = await GetAssetSelectListAsync();
                    return View(obj);
                }

                if (!obj.Reservation.AcceptStatute)
                {
                    ModelState.AddModelError("AcceptStatute", "You must accept the terms before reserving the asset.");
                }

                if (!ModelState.IsValid)
                {
                    TempData["error"] = "An error occurred while creating the reservation.";
                    obj.AssetList = await GetAssetSelectListAsync();
                    return View(obj);
                }

                obj.Reservation.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                using (var transaction = _unitOfWork.Context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    var allReservations = await _unitOfWork.Reservation.GetAllAsync();

                    bool conflictExists = allReservations.Any(r =>
                        r.AssetTag == obj.Reservation.AssetTag &&
                        (r.Approval == Approval.Pending || r.Approval == Approval.Accepted) &&
                        r.PickupDate < obj.Reservation.ReturnDate &&
                        r.ReturnDate > obj.Reservation.PickupDate);

                    if (conflictExists)
                    {
                        TempData["error"] = "Asset was reserved while filling the form. Please choose different dates or another asset.";
                        transaction.Rollback();
                        return RedirectToAction("Index");
                    }

                    obj.Reservation.Approval = Approval.Pending;
                    await _unitOfWork.Reservation.AddAsync(obj.Reservation);
                    _unitOfWork.Save();

                    transaction.Commit();
                }

                TempData["success"] = "Reservation created successfully!";

                var user = await _unitOfWork.ApplicationUser.GetAsync(u => u.Id == obj.Reservation.UserId);
                var myReservationsLink = _config["ReservationLinks:MyReservations"];

                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    string userSubject = "Your reservation has been sent for approval";

                    string userBody =
                        $"Hello {user.FirstName},<br/><br/>" +
                        $"Your reservation for asset <strong>{obj.Reservation.AssetTag}</strong> has been sent for approval.<br/>" +
                        $"You will receive a notification when the reservation is accepted or rejected.<br/><br/>" +
                        $"You can track its status <a href='{myReservationsLink}'>here</a>.";

                    await _emailSender.SendEmailAsync(user.Email, userSubject, userBody);
                }

                var approvalLink = Url.Action(
                    "Index",
                    "Reservation",
                    new { area = "Manager", reservationId = obj.Reservation.Id },
                    Request.Scheme) ?? _config["ReservationLinks:ManagerApproval"];

                string managerSubject = $"New reservation for approval #{obj.Reservation.Id}";

                string managerBody =
                    $"Hello,<br/><br/>" +
                    $"A new reservation has been submitted for approval.<br/><br/>" +
                    "<ul>" +
                    $"<li><strong>Reservation ID:</strong> {obj.Reservation.Id}</li>" +
                    $"<li><strong>Asset:</strong> {obj.Reservation.AssetTag}</li>" +
                    $"<li><strong>User:</strong> {user?.FirstName} {user?.LastName}</li>" +
                    $"<li><strong>User email:</strong> {user?.Email}</li>" +
                    $"<li><strong>Pickup date:</strong> {obj.Reservation.PickupDate:dd-MM-yyyy HH:mm}</li>" +
                    $"<li><strong>Return date:</strong> {obj.Reservation.ReturnDate:dd-MM-yyyy HH:mm}</li>" +
                    $"<li><strong>Destination:</strong> {(string.IsNullOrWhiteSpace(obj.Reservation.Destination) ? "Not provided" : obj.Reservation.Destination)}</li>" +
                    "</ul>" +
                    $"<a href='{approvalLink}'>Open reservation #{obj.Reservation.Id}</a>";

                await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                    obj.Reservation.UserId,
                    managerSubject,
                    managerBody);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating reservation for asset {AssetTag} and user {UserId}",
                    obj.Reservation?.AssetTag,
                    obj.Reservation?.UserId);

                TempData["error"] = "An error occurred while creating the reservation.";
                obj.AssetList = await GetAssetSelectListAsync();
                return View(obj);
            }
        }

        #region Reservations

        /// <summary>
        /// Displays the extend reservation form.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExtendReservation(int id)
        {
            try
            {
                var reservation = await GetOwnReservationAsync(id, includeProperties: "Asset");
                if (reservation == null)
                {
                    TempData["error"] = "Reservation not found.";
                    return RedirectToAction("MyReservations");
                }

                if (DateTime.Now > reservation.ReturnDate)
                {
                    TempData["error"] = "Cannot extend a reservation after it has ended.";
                    return RedirectToAction("MyReservations");
                }

                return View(new ReservationVM { Reservation = reservation });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading extend reservation view for reservation {ReservationId}", id);
                TempData["error"] = "An error occurred while loading the extension form.";
                return RedirectToAction("MyReservations");
            }
        }

        /// <summary>
        /// Processes the extension of a reservation.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ExtendReservation(ReservationVM vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "An error occurred. Please check the entered data.";
                    return View(vm);
                }

                var reservation = await GetOwnReservationAsync(vm.Reservation.Id, includeProperties: "Asset,User");
                if (reservation == null)
                {
                    TempData["error"] = "Reservation not found.";
                    return RedirectToAction("MyReservations");
                }

                if (vm.Reservation.ReturnDate <= reservation.ReturnDate)
                {
                    TempData["error"] = "The new return date must be later than the current one.";
                    return View(vm);
                }

                if (DateTime.Now > reservation.ReturnDate)
                {
                    TempData["error"] = "Cannot extend a reservation after it has ended.";
                    return RedirectToAction("MyReservations");
                }

                using (var transaction = _unitOfWork.Context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    var allReservations = await _unitOfWork.Reservation.GetAllAsync();

                    bool conflictExists = allReservations.Any(r =>
                        r.AssetTag == reservation.AssetTag &&
                        r.Id != reservation.Id &&
                        (r.Approval == Approval.Pending || r.Approval == Approval.Accepted) &&
                        r.PickupDate < vm.Reservation.ReturnDate &&
                        r.ReturnDate > reservation.ReturnDate);

                    if (conflictExists)
                    {
                        TempData["error"] = "Cannot extend the reservation, the asset is reserved in the selected time period.";
                        transaction.Rollback();
                        return RedirectToAction("MyReservations");
                    }

                    reservation.ReturnDate = vm.Reservation.ReturnDate;
                    reservation.Approval = Approval.Pending;

                    _unitOfWork.Reservation.Update(reservation);
                    _unitOfWork.Save();

                    transaction.Commit();
                }

                string subject = $"Reservation extension for approval #{reservation.Id}";
                string body =
                    $"Hello,<br/><br/>" +
                    $"User <strong>{reservation.User?.FirstName} {reservation.User?.LastName}</strong> has extended a reservation and it requires re-approval.<br/><br/>" +
                    "<ul>" +
                    $"<li><strong>Reservation ID:</strong> {reservation.Id}</li>" +
                    $"<li><strong>Asset:</strong> {reservation.AssetTag}</li>" +
                    $"<li><strong>New return date:</strong> {reservation.ReturnDate:dd-MM-yyyy HH:mm}</li>" +
                    "</ul>";

                await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                    reservation.UserId,
                    subject,
                    body);

                TempData["success"] = "Reservation has been successfully extended!";
                return RedirectToAction("MyReservations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending reservation {ReservationId}", vm.Reservation?.Id);
                TempData["error"] = "An error occurred while extending the reservation.";
                return RedirectToAction("MyReservations");
            }
        }

        /// <summary>
        /// Ends an active reservation early.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> EndEarlyReservation(int id)
        {
            try
            {
                var reservation = await GetOwnReservationAsync(id, includeProperties: "Asset,User");
                if (reservation == null)
                    return Json(new { success = false, message = "Reservation not found." });

                if (DateTime.Now < reservation.PickupDate || DateTime.Now >= reservation.ReturnDate)
                    return Json(new { success = false, message = "The reservation is not currently active and cannot be ended early." });

                reservation.ReturnDate = DateTime.Now;

                _unitOfWork.Reservation.Update(reservation);
                _unitOfWork.Save();

                string subject = $"Reservation ended early #{reservation.Id}";
                string body =
                    $"Hello,<br/><br/>" +
                    $"User <strong>{reservation.User?.FirstName} {reservation.User?.LastName}</strong> has ended a reservation early.<br/><br/>" +
                    "<ul>" +
                    $"<li><strong>Reservation ID:</strong> {reservation.Id}</li>" +
                    $"<li><strong>Asset:</strong> {reservation.AssetTag}</li>" +
                    $"<li><strong>New return date:</strong> {reservation.ReturnDate:dd-MM-yyyy HH:mm}</li>" +
                    "</ul>";

                await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                    reservation.UserId,
                    subject,
                    body);

                return Json(new { success = true, message = "Reservation has been ended early." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending reservation early {ReservationId}", id);
                return Json(new { success = false, message = "An error occurred while ending the reservation early." });
            }
        }

        /// <summary>
        /// Displays the current user's reservations.
        /// </summary>
        public async Task<IActionResult> MyReservations()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var reservations = (await _unitOfWork.Reservation.GetAllAsync(null, "Asset"))
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.PickupDate);

                return View(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading MyReservations view.");
                TempData["error"] = "An error occurred while loading your reservations.";
                return View(Enumerable.Empty<Reservation>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelReservation(int id, bool isAssetDamaged, bool hasFaults, string? faultsDescription, string? otherReason)
        {
            try
            {
                var reservation = await GetOwnReservationAsync(id, includeProperties: "Asset,User");

                if (reservation == null || DateTime.Now >= reservation.PickupDate)
                    return Json(new { success = false, message = "Reservation cannot be cancelled." });

                reservation.Approval = Approval.Cancelled;

                var asset = reservation.Asset;

                if (isAssetDamaged && asset != null)
                {
                    asset.IsDamaged = true;
                    _unitOfWork.Asset.Update(asset);
                }

                if (hasFaults && !string.IsNullOrWhiteSpace(faultsDescription))
                {
                    await _unitOfWork.Fault.AddAsync(new Fault
                    {
                        AssetTag = reservation.AssetTag,
                        UserId = reservation.UserId,
                        Description = faultsDescription.Trim(),
                        DateReported = DateTime.Now,
                        IsFixed = false,
                        IsDrivable = false
                    });
                }

                var notes = new List<string>();

                if (isAssetDamaged)
                    notes.Add("User reported that the asset was damaged");

                if (hasFaults && !string.IsNullOrWhiteSpace(faultsDescription))
                    notes.Add($"Faults: {faultsDescription}");

                if (!string.IsNullOrWhiteSpace(otherReason))
                    notes.Add($"Other reason: {otherReason}");

                reservation.Note = string.Join(" | ", notes);

                _unitOfWork.Reservation.Update(reservation);
                _unitOfWork.Save();

                string subject = $"Reservation cancelled #{reservation.Id}";
                string body =
                    $"Hello,<br/><br/>" +
                    $"User <strong>{reservation.User?.FirstName} {reservation.User?.LastName}</strong> has cancelled a reservation.<br/><br/>" +
                    "<ul>" +
                    $"<li><strong>Reservation ID:</strong> {reservation.Id}</li>" +
                    $"<li><strong>Asset:</strong> {reservation.AssetTag}</li>" +
                    $"<li><strong>From:</strong> {reservation.PickupDate:dd-MM-yyyy HH:mm}</li>" +
                    $"<li><strong>To:</strong> {reservation.ReturnDate:dd-MM-yyyy HH:mm}</li>" +
                    $"<li><strong>Note:</strong> {(string.IsNullOrWhiteSpace(reservation.Note) ? "None" : reservation.Note)}</li>" +
                    "</ul>";

                await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                    reservation.UserId,
                    subject,
                    body);

                return Json(new { success = true, message = "Reservation has been cancelled." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling reservation {ReservationId}", id);
                return Json(new { success = false, message = "An error occurred while cancelling the reservation." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Reserved()
        {
            try
            {
                var assets = await _unitOfWork.Asset.GetAllAsync(null, "Reservations.User");
                ViewBag.StartDate = DateTime.Today;
                return View(assets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Reserved view.");
                TempData["error"] = "An error occurred while loading reserved assets.";
                return View(new List<Asset>());
            }
        }

        #endregion

        #region Feedback

        /// <summary>
        /// Displays the pickup feedback form for a reservation.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PickupFeedback(int reservationId)
        {
            try
            {
                var reservation = await GetOwnReservationAsync(reservationId, includeProperties: "Asset,User");
                if (reservation == null)
                {
                    TempData["error"] = "Reservation not found.";
                    return RedirectToAction("MyReservations");
                }

                if (DateTime.Now > reservation.ReturnDate)
                {
                    TempData["error"] = "The reservation has already ended. Pickup feedback is no longer possible.";
                    return RedirectToAction("MyReservations");
                }

                ViewBag.FeedbackType = "Pickup";
                return View("Feedback", reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pickup feedback view for reservation {ReservationId}", reservationId);
                TempData["error"] = "An error occurred while loading pickup feedback.";
                return RedirectToAction("MyReservations");
            }
        }

        /// <summary>
        /// Processes the pickup feedback submission.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PickupFeedback(int reservationId, bool isAssetDamaged, bool hasFaults, string? faults, int pickupMileage)
        {
            try
            {
                var reservation = await GetOwnReservationAsync(reservationId, includeProperties: "Asset,User");
                if (reservation == null)
                {
                    TempData["error"] = "Reservation not found.";
                    return RedirectToAction("MyReservations");
                }

                var asset = reservation.Asset;
                var previousReturn = (await _unitOfWork.Reservation.GetAllAsync())
                    .Where(r => r.AssetTag == reservation.AssetTag
                                && r.Id != reservation.Id
                                && r.ReturnDate <= reservation.PickupDate
                                && r.ReturnMileage.HasValue)
                    .OrderByDescending(r => r.ReturnDate)
                    .FirstOrDefault();

                if (previousReturn != null && pickupMileage < previousReturn.ReturnMileage!.Value - 50)
                {
                    TempData["error"] =
                        $"Pickup mileage ({pickupMileage:N0} km) is over 50 km less than " +
                        $"the last return mileage ({previousReturn.ReturnMileage:N0} km).";

                    return RedirectToAction("PickupFeedback", new { reservationId });
                }

                var diff = Math.Abs(pickupMileage - asset.UsageCount);

                if (diff > 10)
                {
                    string link = string.Format(_config["ReservationLinks:AssetEdit"], asset.AssetTag);

                    string subject = $"[ALERT] Mileage discrepancy – {asset.AssetTag}";
                    string body =
                        "Hello,<br/><br/>" +
                        $"For asset <strong>{asset.AssetTag}</strong> a mileage discrepancy greater than ±10 km was recorded.<br/><br/>" +
                        "<ul>" +
                        $"<li><strong>Previous recorded mileage:</strong> {asset.UsageCount:N0} km</li>" +
                        $"<li><strong>Mileage provided at pickup:</strong> {pickupMileage:N0} km</li>" +
                        $"<li><strong>Difference:</strong> {diff:N0} km</li>" +
                        $"<li><strong>Driver:</strong> {reservation.User?.FirstName} {reservation.User?.LastName}</li>" +
                        "</ul>" +
                        $"Please verify and correct if necessary in the system: <a href=\"{link}\">Edit asset</a>";

                    await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                        reservation.UserId,
                        subject,
                        body);
                }

                asset.UsageCount = pickupMileage;
                _unitOfWork.Asset.Update(asset);

                reservation.PickupMileage = pickupMileage;
                reservation.IsAssetDamagedAtPickup = isAssetDamaged;
                reservation.PickupFaults = hasFaults ? faults : null;
                reservation.PickupFeedbackDate = DateTime.Now;

                _unitOfWork.Reservation.Update(reservation);

                if (hasFaults)
                {
                    await _unitOfWork.Fault.AddAsync(new Fault
                    {
                        AssetTag = asset.AssetTag,
                        UserId = reservation.UserId,
                        Description = string.IsNullOrWhiteSpace(faults) ? "(no description)" : faults.Trim(),
                        DateReported = DateTime.Now,
                        IsFixed = false,
                        IsDrivable = false
                    });
                }

                await UpsertFeedbackLogAsync(
                    reservation,
                    FeedbackKind.Pickup,
                    pickupMileage,
                    null,
                    isAssetDamaged,
                    hasFaults,
                    faults);

                _unitOfWork.Save();

                if (isAssetDamaged || hasFaults)
                {
                    string subject = $"Pickup issue report #{reservation.Id}";
                    string body =
                        $"Hello,<br/><br/>" +
                        $"User <strong>{reservation.User?.FirstName} {reservation.User?.LastName}</strong> reported an issue during asset pickup.<br/><br/>" +
                        "<ul>" +
                        $"<li><strong>Reservation ID:</strong> {reservation.Id}</li>" +
                        $"<li><strong>Asset:</strong> {reservation.AssetTag}</li>" +
                        $"<li><strong>Damaged asset:</strong> {(isAssetDamaged ? "Yes" : "No")}</li>" +
                        $"<li><strong>Faults:</strong> {(hasFaults ? (string.IsNullOrWhiteSpace(faults) ? "(no description)" : faults) : "No")}</li>" +
                        $"<li><strong>Mileage:</strong> {pickupMileage:N0} km</li>" +
                        "</ul>";

                    await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                        reservation.UserId,
                        subject,
                        body);
                }

                await _reservationService.CheckAndSendFeedbackReminders();

                TempData["success"] = "Pickup feedback saved.";
                return RedirectToAction("MyReservations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving pickup feedback for reservation {ReservationId}", reservationId);
                TempData["error"] = "An error occurred while saving pickup feedback.";
                return RedirectToAction("MyReservations");
            }
        }

        /// <summary>
        /// Displays the return feedback form for a reservation.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ReturnFeedback(int reservationId)
        {
            try
            {
                var reservation = await GetOwnReservationAsync(reservationId, includeProperties: "Asset,User");
                if (reservation == null)
                {
                    TempData["error"] = "Reservation not found.";
                    return RedirectToAction("MyReservations");
                }

                if (DateTime.Now < reservation.ReturnDate)
                {
                    TempData["error"] = "The reservation has not ended yet.";
                    return RedirectToAction("MyReservations");
                }

                ViewBag.FeedbackType = "Return";
                return View("Feedback", reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading return feedback view for reservation {ReservationId}", reservationId);
                TempData["error"] = "An error occurred while loading return feedback.";
                return RedirectToAction("MyReservations");
            }
        }

        /// <summary>
        /// Processes the return feedback submission.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnFeedback(int reservationId, bool isAssetDamaged, bool hasFaults, string? faults, int returnMileage, int fuelLevel)
        {
            try
            {
                var reservation = await GetOwnReservationAsync(reservationId, includeProperties: "Asset,User");
                if (reservation == null)
                {
                    TempData["error"] = "Reservation not found.";
                    return RedirectToAction("MyReservations");
                }

                if (reservation.PickupMileage.HasValue && returnMileage < reservation.PickupMileage.Value)
                {
                    TempData["error"] =
                        $"Return mileage ({returnMileage:N0} km) cannot be less than pickup mileage ({reservation.PickupMileage:N0} km).";
                    return RedirectToAction("ReturnFeedback", new { reservationId });
                }

                var asset = reservation.Asset;
                if (asset == null)
                {
TempData["error"] = "Reservation not found.";
                    return RedirectToAction("MyReservations");
                }

                reservation.IsAssetDamagedAtReturn = isAssetDamaged;
                reservation.ReturnFaults = hasFaults ? faults : null;
                reservation.ReturnMileage = returnMileage;
                reservation.ReturnFeedbackDate = DateTime.Now;

                _unitOfWork.Reservation.Update(reservation);

                asset.IsDamaged = isAssetDamaged;
                asset.UsageCount = returnMileage;
                asset.ConditionLevel = fuelLevel;

                _unitOfWork.Asset.Update(asset);

                if (hasFaults)
                {
                    await _unitOfWork.Fault.AddAsync(new Fault
                    {
                        AssetTag = asset.AssetTag,
                        UserId = reservation.UserId,
                        Description = string.IsNullOrWhiteSpace(faults) ? "(no description)" : faults.Trim(),
                        DateReported = DateTime.Now,
                        IsFixed = false,
                        IsDrivable = false
                    });
                }

                await UpsertFeedbackLogAsync(
                    reservation,
                    FeedbackKind.Return,
                    returnMileage,
                    fuelLevel,
                    isAssetDamaged,
                    hasFaults,
                    faults);

                _unitOfWork.Save();

                if (isAssetDamaged || hasFaults)
                {
                    string subject = $"Return report for asset #{reservation.Id}";
                    string body =
                        $"Hello,<br/><br/>" +
                        $"User <strong>{reservation.User?.FirstName} {reservation.User?.LastName}</strong> reported an issue during asset return.<br/><br/>" +
                        "<ul>" +
                        $"<li><strong>Reservation ID:</strong> {reservation.Id}</li>" +
                        $"<li><strong>Asset:</strong> {reservation.AssetTag}</li>" +
                        $"<li><strong>Damaged asset:</strong> {(isAssetDamaged ? "Yes" : "No")}</li>" +
                        $"<li><strong>Faults:</strong> {(hasFaults ? (string.IsNullOrWhiteSpace(faults) ? "(no description)" : faults) : "No")}</li>" +
                        $"<li><strong>Mileage:</strong> {returnMileage:N0} km</li>" +
                        $"<li><strong>Fuel level:</strong> {fuelLevel}%</li>" +
                        "</ul>";

                    await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                        reservation.UserId,
                        subject,
                        body);
                }

                await _reservationService.CheckAndSendFeedbackReminders();

                TempData["success"] = "Return feedback saved.";
                return RedirectToAction("MyReservations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving return feedback for reservation {ReservationId}", reservationId);
                TempData["error"] = "An error occurred while saving return feedback.";
                return RedirectToAction("MyReservations");
            }
        }

        #endregion

        [HttpGet]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<IEnumerable<SelectListItem>> GetAssetSelectListAsync()
        {
            return (await _unitOfWork.Asset.GetAllAsync(c => !c.IsDeleted))
                .Select(i => new SelectListItem
                {
                    Text = i.AssetTag,
                    Value = i.AssetTag
                });
        }

        private async Task<Reservation?> GetOwnReservationAsync(int reservationId, string? includeProperties = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await _unitOfWork.Reservation.GetAsync(
                r => r.Id == reservationId && r.UserId == userId,
                includeProperties);
        }

        private async Task UpsertFeedbackLogAsync(
            Reservation reservation,
            FeedbackKind kind,
            int mileage,
            int? fuelLevel,
            bool isAssetDamaged,
            bool hasFaults,
            string? faults)
        {
            var set = _unitOfWork.Context.Set<FeedbackLog>();

            var log = await set.FirstOrDefaultAsync(f =>
                f.ReservationId == reservation.Id &&
                f.Kind == kind);

            if (log != null)
            {
                log.Status = FeedbackStatus.Provided;
                log.Mileage = mileage;
                log.FuelLevel = fuelLevel;
                log.IsAssetDamaged = isAssetDamaged;
                log.HasFaults = hasFaults;
                log.Faults = hasFaults ? faults : null;
            }
            else
            {
                await set.AddAsync(new FeedbackLog
                {
                    ReservationId = reservation.Id,
                    AssetTag = reservation.AssetTag,
                    UserId = reservation.UserId,
                    Kind = kind,
                    Status = FeedbackStatus.Provided,
                    CreatedAt = DateTime.Now,
                    Mileage = mileage,
                    FuelLevel = fuelLevel,
                    IsAssetDamaged = isAssetDamaged,
                    HasFaults = hasFaults,
                    Faults = hasFaults ? faults : null
                });
            }
        }
    }

    /// <summary>
    /// Helper extensions for text formatting.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Capitalizes the first letter of the given string.
        /// </summary>
        public static string Capitalize(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}