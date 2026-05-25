using ReservationApp.DataAccess.Repository.IRepository;
using ReservationApp.Models;
using ReservationApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ReservationApp.Areas.Manager.Controllers
{
    /// <summary>
    /// Manages fault-related operations within the Manager area.
    /// </summary>
    [Area("Manager")]
    [Authorize(Policy = "AdminManager")]
    public class FaultController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FaultController> _logger;

        public FaultController(IUnitOfWork unitOfWork, ILogger<FaultController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Displays a list of faults.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var faults = (await _unitOfWork.Fault.GetAllAsync(null, "User")).OrderByDescending(f => f.Id).ToList();
                ViewBag.CarPlates = faults.Select(f => f.AssetTag).Distinct().OrderBy(p => p).ToList();
                ViewBag.Users = faults.Select(f => f.User.FirstName + " " + f.User.LastName).Distinct().OrderBy(u => u).ToList();
                return View(faults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Fault Index page.");
                TempData["error"] = "Failed to load faults.";
                return View(new List<Fault>());
            }
        }

        /// <summary>
        /// Displays the upsert view for creating or editing a fault.
        /// </summary>
        public async Task<IActionResult> Upsert(string carNumberPlate, string userId, int? id)
        {
            try
            {
                var faultVM = new FaultVM
                {
                    Fault = new Fault(),
                    AssetList = (await _unitOfWork.Asset.GetAllAsync())
                                .Select(c => new SelectListItem
                                {
                                    Text = c.AssetTag,
                                    Value = c.AssetTag
                                }),
                    UserList = (await _unitOfWork.ApplicationUser.GetAllAsync())
                                .Select(u => new SelectListItem
                                {
                                    Text = $"{u.FirstName} {u.LastName}",
                                    Value = u.Id.ToString()
                                })
                };

                if (id == null || id == 0)
                {
                    faultVM.Fault.AssetTag = carNumberPlate;
                    faultVM.Fault.UserId = userId;
                    return View(faultVM);
                }

                faultVM.Fault = await _unitOfWork.Fault.GetAsync(f => f.Id == id, "Asset,User,FixedByUser");
                if (faultVM.Fault == null)
                    return NotFound();

                return View(faultVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Fault Upsert view. Fault ID {Id}", id);
                TempData["error"] = "Failed to load fault details.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Processes the upsert form submission for creating or updating a fault.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(FaultVM faultVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (faultVM.Fault.Id == 0)
                    {
                        faultVM.Fault.DateReported = DateTime.Now;

                        if (faultVM.Fault.IsFixed)
                            faultVM.Fault.FixDate ??= DateTime.Now;
                        else
                            faultVM.Fault.FixDate = null;

                        await _unitOfWork.Fault.AddAsync(faultVM.Fault);
                        TempData["success"] = "Fault successfully created.";
                    }
                    else
                    {
                        var existing = await _unitOfWork.Fault.GetAsync(f => f.Id == faultVM.Fault.Id);
                        if (existing == null) return NotFound();

                        var wasFixed = existing.IsFixed;

                        existing.AssetTag = faultVM.Fault.AssetTag;
                        existing.UserId = faultVM.Fault.UserId;
                        existing.Description = faultVM.Fault.Description;
                        existing.IsDrivable = faultVM.Fault.IsDrivable;
                        existing.DrivableComment = faultVM.Fault.DrivableComment;
                        existing.FixDescription = faultVM.Fault.FixDescription;
                        existing.IsFixed = faultVM.Fault.IsFixed;

                        var now = DateTime.Now;
                        if (!wasFixed && existing.IsFixed)
                            existing.FixDate = faultVM.Fault.FixDate ?? now;
                        else if (wasFixed && existing.IsFixed)
                            existing.FixDate = faultVM.Fault.FixDate ?? existing.FixDate;
                        else if (!existing.IsFixed){
                            existing.FixDate = null;
                            existing.FixDescription = null;
                        }

                        _unitOfWork.Fault.Update(existing);
                        TempData["success"] = "Fault successfully updated.";
                    }

                    _unitOfWork.Save();
                    return RedirectToAction(nameof(Index));
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create/update fault. Fault ID {Id}", faultVM.Fault.Id);
                TempData["error"] = "An error occurred while saving the fault.";
            }

            if (faultVM.Fault.IsDrivable && string.IsNullOrWhiteSpace(faultVM.Fault.DrivableComment))
            {
                ModelState.AddModelError("Fault.DrivableComment",
                    "Podaj komentarz, dlaczego można użytkować auto mimo usterki.");
            }

            faultVM.AssetList = (await _unitOfWork.Asset.GetAllAsync())
                .Select(c => new SelectListItem
                {
                    Text = c.AssetTag,
                    Value = c.AssetTag
                });
            faultVM.UserList = (await _unitOfWork.ApplicationUser.GetAllAsync())
                .Select(u => new SelectListItem
                {
                    Text = $"{u.FirstName} {u.LastName}",
                    Value = u.Id.ToString()
                });
            return View(faultVM);
        }

        /// <summary>
        /// Deletes a fault by its ID.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteFault(int id)
        {
            try
            {

                var fault = await _unitOfWork.Fault.GetAsync(u => u.Id == id);
                if (fault == null)
                    return Json(new { success = false, message = "Nie znaleziono usterki." });

                _unitOfWork.Fault.Remove(fault);
                _unitOfWork.Save();

                return Json(new { success = true, message = "Usterka usunięta pomyślnie." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Fault ID {Id}", id);
                return Json(new { success = false, message = "Error deleting fault." });
            }
        }

        /// <summary>
        /// Retrieves a JSON list of faults with detailed information.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFaults()
        {
            try
            {
                var faults = (await _unitOfWork.Fault.GetAllAsync(null, "User,FixedByUser"))
                    .OrderByDescending(f => f.Id)
                    .Select(f => new
                    {
                        f.Id,
                        f.AssetTag,
                        f.Description,
                        User = f.User.FirstName + " " + f.User.LastName,
                        isDrivable = (f.IsFixed || f.IsDrivable) ? "Tak" : "Nie",
                        comment = f.DrivableComment,
                        Status = f.IsFixed ? "Usterka naprawiona" : "Usterka nie naprawiona",
                        isFixed = f.IsFixed,
                        f.FixDescription,
                        dateReported = f.DateReported,
                        fixDate = f.FixDate,

                        fixedByUser = f.FixedByUser == null ? null : new
                        {
                            fullName = f.FixedByUser.FirstName + " " + f.FixedByUser.LastName,
                            email = f.FixedByUser.Email
                        }
                    })
                    .ToList();
                return Json(new { data = faults });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Faults via API.");
                return Json(new { data = new List<object>() });
            }
        }

        public async Task<FileResult> ExportToExcel(string? status, string? drive, string? car, string? user)
        {
            try
            {
                var q = (await _unitOfWork.Fault.GetAllAsync(null, "User,FixedByUser")).AsQueryable();

                if (!string.IsNullOrWhiteSpace(status))
                    q = q.Where(f => status == "naprawiona" ? f.IsFixed : !f.IsFixed);

                if (!string.IsNullOrWhiteSpace(drive))
                    q = q.Where(f => ((f.IsFixed || f.IsDrivable) ? "Tak" : "Nie") == drive);

                if (!string.IsNullOrWhiteSpace(car))
                    q = q.Where(f => f.AssetTag == car);

                if (!string.IsNullOrWhiteSpace(user))
                    q = q.Where(f => (f.User.FirstName + " " + f.User.LastName) == user);

                var rows = q.OrderByDescending(f => f.Id).ToList()
                            .Select(f => new
                            {
                                f.Id,
                                f.AssetTag,
                                ZgloszonePrzez = $"{f.User.FirstName} {f.User.LastName}",
                                DataZgloszenia = f.DateReported,
                                DataNaprawy = f.FixDate,
                                f.Description,
                                Status = f.IsFixed ? "Naprawiona" : "Nienaprawiona",
                                Jezdne = (f.IsFixed || f.IsDrivable) ? "Tak" : "Nie",
                                f.DrivableComment,
                                f.FixDescription
                            });

                using var wb = new ClosedXML.Excel.XLWorkbook();
                wb.Worksheets.Add("Usterki").FirstCell().InsertTable(rows);
                wb.Worksheet(1).Columns().AdjustToContents();

                using var ms = new MemoryStream();
                wb.SaveAs(ms); ms.Position = 0;
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"usterki_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export faults to Excel.");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFixed(int id)
        {
            try
            {
                var fault = await _unitOfWork.Fault.GetAsync(f => f.Id == id);

                if (fault == null)
                    return Json(new { success = false, message = "Nie znaleziono usterki." });

                var now = DateTime.Now;

                if (!fault.IsFixed)
                {
                    // Mark as fixed
                    fault.IsFixed = true;
                    fault.FixDate = now;
                }
                else
                {
                    // Unfix → reset date
                    fault.IsFixed = false;
                    fault.FixDate = null;
                    fault.FixDescription = null;
                }

                _unitOfWork.Fault.Update(fault);
                _unitOfWork.Save();

                return Json(new
                {
                    success = true,
                    status = fault.IsFixed ? "Usterka naprawiona" : "Usterka nie naprawiona",
                    fixDate = fault.FixDate
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Błąd podczas aktualizacji statusu." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkFixed(int id, string description, string date)
        {
            var fault = await _unitOfWork.Fault.GetAsync(f => f.Id == id);

            if (fault == null)
                return Json(new { success = false });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!DateTime.TryParse(date, out var parsedDate))
                return Json(new { success = false, message = "Niepoprawny format daty." });

            fault.IsFixed = true;
            fault.FixDescription = description;
            fault.FixDate = parsedDate;
            fault.FixedByUserId = userId;

            _unitOfWork.Fault.Update(fault);
            _unitOfWork.Save();

            return Json(new
            {
                success = true,
                fixDate = fault.FixDate?.ToString("yyyy-MM-dd")
            });
        }

    }
}
