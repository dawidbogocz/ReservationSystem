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
        /// Returns the correct Polish word forms for the given vehicle type
        /// (mianownik, dopełniacz, narzędnik).
        /// </summary>
        private (string mianownik, string dopełniacz, string narzędnik) GetVehicleWord(AssetType type)
        {
            return type == AssetType.Lift
                ? ("podnośnik", "podnośnika", "podnośnikiem")
                : ("samochód", "samochodu", "samochodem");
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
                                && r.Approval == Approval.Zaakceptowane
                                && r.PickupDate > DateTime.Now
                                && r.PickupDate <= DateTime.Now.AddDays(2))
                    .ToList();

                var reminders = new List<string>();

                foreach (var reservation in upcomingReservations)
                {
                    var words = GetVehicleWord(reservation.Asset.AssetType);
                    reminders.Add($"Przypomnienie: Masz rezerwację {words.dopełniacz} {reservation.AssetTag} w dniu {reservation.PickupDate:dd-MM-yyyy HH:mm}");
                }

                ViewBag.ReminderNotifications = reminders;

                var pendingPickup = (await _unitOfWork.Reservation.GetAllAsync(r => r.Asset.IsDeleted == false, "Asset"))
                    .FirstOrDefault(r => r.UserId == userId
                        && r.Approval == Approval.Zaakceptowane
                        && DateTime.Now >= r.PickupDate
                        && r.IsCarDirtyAtPickup == null
                        && DateTime.Now < r.ReturnDate);

                var pendingReturn = (await _unitOfWork.Reservation.GetAllAsync(r => r.Asset.IsDeleted == false, "Asset"))
                    .FirstOrDefault(r => r.UserId == userId
                        && r.Approval == Approval.Zaakceptowane
                        && DateTime.Now >= r.ReturnDate
                        && r.IsCarDirtyAtReturn == null);

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
        /// Displays the reservation creation form for a specific vehicle.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(string carNumberPlate, DateTime startDate, DateTime endDate)
        {
            try
            {
                var car = (await _unitOfWork.Asset.GetAllAsync(c => !c.IsDeleted))
                    .FirstOrDefault(c => c.AssetTag == carNumberPlate);

                if (car == null)
                {
                    TempData["error"] = "Pojazd nie został znaleziony!";
                    return RedirectToAction("Index");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var vm = new ReservationVM
                {
                    Reservation = new Reservation
                    {
                        AssetTag = car.AssetTag,
                        PickupDate = startDate,
                        ReturnDate = endDate,
                        Asset = car,
                        UserId = userId
                    },
                    AssetList = new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Text = car.AssetTag,
                            Value = car.AssetTag,
                            Disabled = true
                        }
                    }
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reservation create view for car {AssetTag}", carNumberPlate);
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
                var car = await _unitOfWork.Asset.GetAsync(c => c.AssetTag == obj.Reservation.AssetTag);
                if (car == null)
                {
                    TempData["error"] = "Pojazd nie został znaleziony!";
                    obj.AssetList = await GetCarSelectListAsync();
                    return View(obj);
                }

                var words = GetVehicleWord(car.AssetType);

                if (!obj.Reservation.AcceptStatute)
                {
                    ModelState.AddModelError("AcceptStatute", $"Musisz zaakceptować regulamin przed rezerwacją {words.dopełniacz}.");
                }

                if (!ModelState.IsValid)
                {
                    TempData["error"] = "Wystąpił błąd podczas tworzenia rezerwacji.";
                    obj.AssetList = await GetCarSelectListAsync();
                    return View(obj);
                }

                obj.Reservation.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                using (var transaction = _unitOfWork.Context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    var allReservations = await _unitOfWork.Reservation.GetAllAsync();

                    bool conflictExists = allReservations.Any(r =>
                        r.AssetTag == obj.Reservation.AssetTag &&
                        (r.Approval == Approval.Oczekujace || r.Approval == Approval.Zaakceptowane) &&
                        r.PickupDate < obj.Reservation.ReturnDate &&
                        r.ReturnDate > obj.Reservation.PickupDate);

                    if (conflictExists)
                    {
                        TempData["error"] = $"{words.mianownik.Capitalize()} został zarezerwowany podczas wypełniania formularza. Proszę wybrać inne daty lub inny {words.mianownik}.";
                        transaction.Rollback();
                        return RedirectToAction("Index");
                    }

                    obj.Reservation.Approval = Approval.Oczekujace;
                    await _unitOfWork.Reservation.AddAsync(obj.Reservation);
                    _unitOfWork.Save();

                    transaction.Commit();
                }

                TempData["success"] = "Rezerwacja została pomyślnie utworzona!";

                var user = await _unitOfWork.ApplicationUser.GetAsync(u => u.Id == obj.Reservation.UserId);
                var myReservationsLink = _config["ReservationLinks:MyReservations"];

                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    string userSubject = "Twoja rezerwacja została wysłana do zatwierdzenia";

                    string userBody =
                        $"Witaj {user.FirstName},<br/><br/>" +
                        $"Twoja rezerwacja pojazdu <strong>{obj.Reservation.AssetTag}</strong> została wysłana do zatwierdzenia.<br/>" +
                        $"Otrzymasz powiadomienie, gdy rezerwacja zostanie zaakceptowana lub odrzucona.<br/><br/>" +
                        $"Możesz śledzić jej status <a href='{myReservationsLink}'>tutaj</a>.";

                    await _emailSender.SendEmailAsync(user.Email, userSubject, userBody);
                }

                var approvalLink = Url.Action(
                    "Index",
                    "Reservation",
                    new { area = "Manager", reservationId = obj.Reservation.Id },
                    Request.Scheme) ?? _config["ReservationLinks:ManagerApproval"];

                string managerSubject = $"Nowa rezerwacja do zatwierdzenia #{obj.Reservation.Id}";

                string managerBody =
                    $"Dzień dobry,<br/><br/>" +
                    $"Pojawiła się nowa rezerwacja do zatwierdzenia.<br/><br/>" +
                    "<ul>" +
                    $"<li><strong>ID rezerwacji:</strong> {obj.Reservation.Id}</li>" +
                    $"<li><strong>Pojazd:</strong> {obj.Reservation.AssetTag}</li>" +
                    $"<li><strong>Użytkownik:</strong> {user?.FirstName} {user?.LastName}</li>" +
                    $"<li><strong>Email użytkownika:</strong> {user?.Email}</li>" +
                    $"<li><strong>Data odbioru:</strong> {obj.Reservation.PickupDate:dd-MM-yyyy HH:mm}</li>" +
                    $"<li><strong>Data zwrotu:</strong> {obj.Reservation.ReturnDate:dd-MM-yyyy HH:mm}</li>" +
                    $"<li><strong>Cel:</strong> {(string.IsNullOrWhiteSpace(obj.Reservation.Destination) ? "Nie podano" : obj.Reservation.Destination)}</li>" +
                    "</ul>" +
                    $"<a href='{approvalLink}'>Otwórz rezerwację #{obj.Reservation.Id}</a>";

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
                    "Error creating reservation for car {AssetTag} and user {UserId}",
                    obj.Reservation?.AssetTag,
                    obj.Reservation?.UserId);

                TempData["error"] = "An error occurred while creating the reservation.";
                obj.AssetList = await GetCarSelectListAsync();
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
                    TempData["error"] = "Nie znaleziono rezerwacji.";
                    return RedirectToAction("MyReservations");
                }

                if (DateTime.Now > reservation.ReturnDate)
                {
                    TempData["error"] = "Nie można przedłużyć rezerwacji po jej zakończeniu.";
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
                    TempData["error"] = "Wystąpił błąd. Sprawdź poprawność wprowadzonych danych.";
                    return View(vm);
                }

                var reservation = await GetOwnReservationAsync(vm.Reservation.Id, includeProperties: "Asset,User");
                if (reservation == null)
                {
                    TempData["error"] = "Nie znaleziono rezerwacji.";
                    return RedirectToAction("MyReservations");
                }

                if (vm.Reservation.ReturnDate <= reservation.ReturnDate)
                {
                    TempData["error"] = "Nowa data zwrotu musi być późniejsza niż aktualna.";
                    return View(vm);
                }

                if (DateTime.Now > reservation.ReturnDate)
                {
                    TempData["error"] = "Nie można przedłużyć rezerwacji po jej zakończeniu.";
                    return RedirectToAction("MyReservations");
                }

                using (var transaction = _unitOfWork.Context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    var allReservations = await _unitOfWork.Reservation.GetAllAsync();

                    bool conflictExists = allReservations.Any(r =>
                        r.AssetTag == reservation.AssetTag &&
                        r.Id != reservation.Id &&
                        (r.Approval == Approval.Oczekujace || r.Approval == Approval.Zaakceptowane) &&
                        r.PickupDate < vm.Reservation.ReturnDate &&
                        r.ReturnDate > reservation.ReturnDate);

                    if (conflictExists)
                    {
                        TempData["error"] = "Nie można przedłużyć rezerwacji, samochód jest zarezerwowany w wybranym przedziale czasowym.";
                        transaction.Rollback();
                        return RedirectToAction("MyReservations");
                    }

                    reservation.ReturnDate = vm.Reservation.ReturnDate;
                    reservation.Approval = Approval.Oczekujace;

                    _unitOfWork.Reservation.Update(reservation);
                    _unitOfWork.Save();

                    transaction.Commit();
                }

                string subject = $"Przedłużenie rezerwacji do zatwierdzenia #{reservation.Id}";
                string body =
                    $"Dzień dobry,<br/><br/>" +
                    $"Użytkownik <strong>{reservation.User?.FirstName} {reservation.User?.LastName}</strong> przedłużył rezerwację i wymaga ona ponownego zatwierdzenia.<br/><br/>" +
                    "<ul>" +
                    $"<li><strong>ID rezerwacji:</strong> {reservation.Id}</li>" +
                    $"<li><strong>Pojazd:</strong> {reservation.AssetTag}</li>" +
                    $"<li><strong>Nowa data zwrotu:</strong> {reservation.ReturnDate:dd-MM-yyyy HH:mm}</li>" +
                    "</ul>";

                await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                    reservation.UserId,
                    subject,
                    body);

                TempData["success"] = "Rezerwacja została pomyślnie przedłużona!";
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
                    return Json(new { success = false, message = "Nie znaleziono rezerwacji." });

                if (DateTime.Now < reservation.PickupDate || DateTime.Now >= reservation.ReturnDate)
                    return Json(new { success = false, message = "Rezerwacja nie jest aktualnie aktywna i nie może być zakończona wcześniej." });

                reservation.ReturnDate = DateTime.Now;

                _unitOfWork.Reservation.Update(reservation);
                _unitOfWork.Save();

                string subject = $"Rezerwacja zakończona wcześniej #{reservation.Id}";
                string body =
                    $"Dzień dobry,<br/><br/>" +
                    $"Użytkownik <strong>{reservation.User?.FirstName} {reservation.User?.LastName}</strong> zakończył rezerwację wcześniej.<br/><br/>" +
                    "<ul>" +
                    $"<li><strong>ID rezerwacji:</strong> {reservation.Id}</li>" +
                    $"<li><strong>Pojazd:</strong> {reservation.AssetTag}</li>" +
                    $"<li><strong>Nowa data zwrotu:</strong> {reservation.ReturnDate:dd-MM-yyyy HH:mm}</li>" +
                    "</ul>";

                await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                    reservation.UserId,
                    subject,
                    body);

                return Json(new { success = true, message = "Rezerwacja została zakończona wcześniej." });
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
        public async Task<IActionResult> CancelReservation(int id, bool isCarDirty, bool hasFaults, string? faultsDescription, string? otherReason)
        {
            try
            {
                var reservation = await GetOwnReservationAsync(id, includeProperties: "Asset,User");

                if (reservation == null || DateTime.Now >= reservation.PickupDate)
                    return Json(new { success = false, message = "Rezerwacja nie może być anulowana." });

                reservation.Approval = Approval.Anulowana;

                var car = reservation.Asset;

                if (isCarDirty && car != null)
                {
                    car.IsDamaged = true;
                    _unitOfWork.Asset.Update(car);
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

                if (isCarDirty)
                    notes.Add("Użytkownik zgłosił, że samochód był brudny");

                if (hasFaults && !string.IsNullOrWhiteSpace(faultsDescription))
                    notes.Add($"Usterki: {faultsDescription}");

                if (!string.IsNullOrWhiteSpace(otherReason))
                    notes.Add($"Inny powód: {otherReason}");

                reservation.Note = string.Join(" | ", notes);

                _unitOfWork.Reservation.Update(reservation);
                _unitOfWork.Save();

                string subject = $"Rezerwacja anulowana #{reservation.Id}";
                string body =
                    $"Dzień dobry,<br/><br/>" +
                    $"Użytkownik <strong>{reservation.User?.FirstName} {reservation.User?.LastName}</strong> anulował rezerwację.<br/><br/>" +
                    "<ul>" +
                    $"<li><strong>ID rezerwacji:</strong> {reservation.Id}</li>" +
                    $"<li><strong>Pojazd:</strong> {reservation.AssetTag}</li>" +
                    $"<li><strong>Od:</strong> {reservation.PickupDate:dd-MM-yyyy HH:mm}</li>" +
                    $"<li><strong>Do:</strong> {reservation.ReturnDate:dd-MM-yyyy HH:mm}</li>" +
                    $"<li><strong>Notatka:</strong> {(string.IsNullOrWhiteSpace(reservation.Note) ? "Brak" : reservation.Note)}</li>" +
                    "</ul>";

                await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                    reservation.UserId,
                    subject,
                    body);

                return Json(new { success = true, message = "Rezerwacja została anulowana." });
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
                TempData["error"] = "An error occurred while loading reserved vehicles.";
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
                    TempData["error"] = "Rezerwacja nie została znaleziona.";
                    return RedirectToAction("MyReservations");
                }

                if (DateTime.Now > reservation.ReturnDate)
                {
                    TempData["error"] = "Rezerwacja już się zakończyła. Feedback przy odbiorze nie jest już możliwy.";
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
        public async Task<IActionResult> PickupFeedback(int reservationId, bool isCarDirty, bool hasFaults, string? faults, int pickupMileage)
        {
            try
            {
                var reservation = await GetOwnReservationAsync(reservationId, includeProperties: "Asset,User");
                if (reservation == null)
                {
                    TempData["error"] = "Nie znaleziono rezerwacji.";
                    return RedirectToAction("MyReservations");
                }

                var car = reservation.Asset;
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
                        $"Przebieg przy odbiorze ({pickupMileage:N0} km) jest o ponad 50 km mniejszy niż " +
                        $"ostatni przebieg przy zwrocie ({previousReturn.ReturnMileage:N0} km).";

                    return RedirectToAction("PickupFeedback", new { reservationId });
                }

                var diff = Math.Abs(pickupMileage - car.Mileage);

                if (diff > 10)
                {
                    string link = string.Format(_config["ReservationLinks:CarEdit"], car.AssetTag);

                    string subject = $"[ALERT] Rozbieżność przebiegu – {car.AssetTag}";
                    string body =
                        "Dzień dobry,<br/><br/>" +
                        $"Dla pojazdu <strong>{car.AssetTag}</strong> zarejestrowano rozbieżność przebiegu większą niż ±10 km.<br/><br/>" +
                        "<ul>" +
                        $"<li><strong>Poprzedni zapisany przebieg:</strong> {car.Mileage:N0} km</li>" +
                        $"<li><strong>Przebieg podany przy odbiorze:</strong> {pickupMileage:N0} km</li>" +
                        $"<li><strong>Różnica:</strong> {diff:N0} km</li>" +
                        $"<li><strong>Kierowca:</strong> {reservation.User?.FirstName} {reservation.User?.LastName}</li>" +
                        "</ul>" +
                        $"Prosimy o weryfikację i ewentualną korektę w systemie: <a href=\"{link}\">Edycja pojazdu</a>";

                    await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                        reservation.UserId,
                        subject,
                        body);
                }

                car.Mileage = pickupMileage;
                _unitOfWork.Asset.Update(car);

                reservation.PickupMileage = pickupMileage;
                reservation.IsCarDirtyAtPickup = isCarDirty;
                reservation.PickupFaults = hasFaults ? faults : null;
                reservation.PickupFeedbackDate = DateTime.Now;

                _unitOfWork.Reservation.Update(reservation);

                if (hasFaults)
                {
                    await _unitOfWork.Fault.AddAsync(new Fault
                    {
                        AssetTag = car.AssetTag,
                        UserId = reservation.UserId,
                        Description = string.IsNullOrWhiteSpace(faults) ? "(brak opisu)" : faults.Trim(),
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
                    isCarDirty,
                    hasFaults,
                    faults);

                _unitOfWork.Save();

                if (isCarDirty || hasFaults)
                {
                    string subject = $"Zgłoszenie przy odbiorze pojazdu #{reservation.Id}";
                    string body =
                        $"Dzień dobry,<br/><br/>" +
                        $"Użytkownik <strong>{reservation.User?.FirstName} {reservation.User?.LastName}</strong> zgłosił problem przy odbiorze pojazdu.<br/><br/>" +
                        "<ul>" +
                        $"<li><strong>ID rezerwacji:</strong> {reservation.Id}</li>" +
                        $"<li><strong>Pojazd:</strong> {reservation.AssetTag}</li>" +
                        $"<li><strong>Brudny pojazd:</strong> {(isCarDirty ? "Tak" : "Nie")}</li>" +
                        $"<li><strong>Usterki:</strong> {(hasFaults ? (string.IsNullOrWhiteSpace(faults) ? "(brak opisu)" : faults) : "Nie")}</li>" +
                        $"<li><strong>Przebieg:</strong> {pickupMileage:N0} km</li>" +
                        "</ul>";

                    await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                        reservation.UserId,
                        subject,
                        body);
                }

                await _reservationService.CheckAndSendFeedbackReminders();

                TempData["success"] = "Feedback przy odbiorze zapisany.";
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
                    TempData["error"] = "Rezerwacja nie została znaleziona.";
                    return RedirectToAction("MyReservations");
                }

                if (DateTime.Now < reservation.ReturnDate)
                {
                    TempData["error"] = "Rezerwacja jeszcze się nie zakończyła.";
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
        public async Task<IActionResult> ReturnFeedback(int reservationId, bool isCarDirty, bool hasFaults, string? faults, int returnMileage, int fuelLevel)
        {
            try
            {
                var reservation = await GetOwnReservationAsync(reservationId, includeProperties: "Asset,User");
                if (reservation == null)
                {
                    TempData["error"] = "Nie znaleziono rezerwacji.";
                    return RedirectToAction("MyReservations");
                }

                if (reservation.PickupMileage.HasValue && returnMileage < reservation.PickupMileage.Value)
                {
                    TempData["error"] =
                        $"Przebieg przy zwrocie ({returnMileage:N0} km) nie może być mniejszy niż przy odbiorze ({reservation.PickupMileage:N0} km).";
                    return RedirectToAction("ReturnFeedback", new { reservationId });
                }

                var car = reservation.Asset;
                if (car == null)
                {
                    TempData["error"] = "Nie znaleziono pojazdu.";
                    return RedirectToAction("MyReservations");
                }

                reservation.IsCarDirtyAtReturn = isCarDirty;
                reservation.ReturnFaults = hasFaults ? faults : null;
                reservation.ReturnMileage = returnMileage;
                reservation.ReturnFeedbackDate = DateTime.Now;

                _unitOfWork.Reservation.Update(reservation);

                car.IsDamaged = isCarDirty;
                car.Mileage = returnMileage;
                car.FuelLevel = fuelLevel;

                _unitOfWork.Asset.Update(car);

                if (hasFaults)
                {
                    await _unitOfWork.Fault.AddAsync(new Fault
                    {
                        AssetTag = car.AssetTag,
                        UserId = reservation.UserId,
                        Description = string.IsNullOrWhiteSpace(faults) ? "(brak opisu)" : faults.Trim(),
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
                    isCarDirty,
                    hasFaults,
                    faults);

                _unitOfWork.Save();

                if (isCarDirty || hasFaults)
                {
                    string subject = $"Zgłoszenie przy zwrocie pojazdu #{reservation.Id}";
                    string body =
                        $"Dzień dobry,<br/><br/>" +
                        $"Użytkownik <strong>{reservation.User?.FirstName} {reservation.User?.LastName}</strong> zgłosił problem przy zwrocie pojazdu.<br/><br/>" +
                        "<ul>" +
                        $"<li><strong>ID rezerwacji:</strong> {reservation.Id}</li>" +
                        $"<li><strong>Pojazd:</strong> {reservation.AssetTag}</li>" +
                        $"<li><strong>Brudny pojazd:</strong> {(isCarDirty ? "Tak" : "Nie")}</li>" +
                        $"<li><strong>Usterki:</strong> {(hasFaults ? (string.IsNullOrWhiteSpace(faults) ? "(brak opisu)" : faults) : "Nie")}</li>" +
                        $"<li><strong>Przebieg:</strong> {returnMileage:N0} km</li>" +
                        $"<li><strong>Poziom paliwa:</strong> {fuelLevel}%</li>" +
                        "</ul>";

                    await _departmentNotifications.SendToGroupManagersAndAdminsAsync(
                        reservation.UserId,
                        subject,
                        body);
                }

                await _reservationService.CheckAndSendFeedbackReminders();

                TempData["success"] = "Feedback przy zwrocie zapisany.";
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

        private async Task<IEnumerable<SelectListItem>> GetCarSelectListAsync()
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
            bool isCarDirty,
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
                log.IsCarDirty = isCarDirty;
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
                    IsCarDirty = isCarDirty,
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