using ReservationApp.DataAccess.Repository.IRepository;
using ReservationApp.Models;
using ReservationApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ReservationApp.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Policy = "AdminManager")]
    public class AssetController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AssetController> _logger;

        public AssetController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, ILogger<AssetController> logger)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// Displays the list of assets.
        /// </summary>
        /// <returns>The Index view with a list of assets.</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                var assets = await _unitOfWork.Asset.GetAllAsync(c => !c.IsDeleted, "Faults");
                return View(assets.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load asset list.");
                TempData["error"] = "Failed to load assets.";
                return View(new List<Asset>());
            }
        }

        /// <summary>
        /// Displays the create asset form.
        /// </summary>
        /// <returns>The Create view with an empty AssetVM.</returns>
        public IActionResult Create() => View(new AssetVM { Asset = new Asset() });

        /// <summary>
        /// Processes the create asset form submission asynchronously.
        /// </summary>
        /// <param name="obj">The asset view model containing asset details.</param>
        /// <param name="file">An optional image file for the car.</param>
        /// <returns>
        /// Redirects to the Index view on success; otherwise, redisplays the form.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Create(AssetVM obj, IFormFile? file)
        {
            try
            {
                // Check if an asset with the same tag already exists.
                var existingAsset = await _unitOfWork.Asset.GetAsync(u => u.AssetTag == obj.Asset.AssetTag && !u.IsDeleted);
                if (existingAsset != null)
                {
                    TempData["error"] = "Samochód o tym numerze rejestracyjnym już istnieje.";
                    obj.Asset.AssetTag = string.Empty;
                    return View(obj);
                }

                if (ModelState.IsValid)
                {
                    obj.Asset.ImageUrl = SaveImage(file);
                    await _unitOfWork.Asset.AddAsync(obj.Asset);
                    _unitOfWork.Save();
                    TempData["success"] = "Dodano nowy samochód!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create asset {Tag}.", obj.Asset?.AssetTag);
                TempData["error"] = "Failed to create car.";
            }
            return View(obj);
        }

        /// <summary>
        /// Displays the edit form for an existing car.
        /// </summary>
        /// <param name="id">The tag of the asset to edit.</param>
        /// <returns>The Edit view with asset details.</returns>
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var asset = await _unitOfWork.Asset.GetAsync(u => u.AssetTag == id && !u.IsDeleted);
                if (asset == null)
                    return NotFound();

                return View(new AssetVM { Asset = asset });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load edit form for asset {Tag}.", id);
                TempData["error"] = "Failed to load asset details.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Processes the edit asset form submission asynchronously.
        /// </summary>
        /// <param name="obj">The asset view model with updated details.</param>
        /// <param name="file">An optional new image file for the car.</param>
        /// <returns>
        /// Redirects to the Index view upon success; otherwise, redisplays the form.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Edit(AssetVM obj, IFormFile? file)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (file != null)
                    {
                        if (!string.IsNullOrEmpty(obj.Asset.ImageUrl))
                            DeleteImage(obj.Asset.ImageUrl);
                        obj.Asset.ImageUrl = SaveImage(file);
                    }
                    _unitOfWork.Asset.Update(obj.Asset);
                    _unitOfWork.Save();
                    TempData["success"] = "Zaktualizowano samochód";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update asset {Tag}.", obj.Asset?.AssetTag);
                TempData["error"] = "Failed to update car.";
            }
            return View(obj);
        }

        /// <summary>
        /// Deletes an asset asynchronously based on its tag.
        /// </summary>
        /// <param name="id">The tag of the asset to delete.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteAsset(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Nie podano numeru rejestracyjnego." });

                var asset = await _unitOfWork.Asset.GetAsync(u => u.AssetTag == id && !u.IsDeleted);
                if (asset == null)
                    return Json(new { success = false, message = "Nie znaleziono samochodu." });

                if (!string.IsNullOrEmpty(asset.ImageUrl))
                    DeleteImage(asset.ImageUrl);

                asset.IsDeleted = true;
                _unitOfWork.Asset.Update(asset);
                _unitOfWork.Save();

                return Json(new { success = true, message = "Samochód usunięty pomyślnie." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete asset {Tag}.", id);
                return Json(new { success = false, message = "Error deleting car." });
            }
        }
        public async Task<FileResult> ExportToExcel(string? make, string? dirt, string? vt)
        {
            try
            {
                var q = _unitOfWork.Asset.GetAllQueryable(c => !c.IsDeleted, "Faults");

                if (!string.IsNullOrWhiteSpace(make))
                    q = q.Where(c => c.Make == make);

                var list = q.OrderBy(c => c.AssetTag).ToList()
                            .Where(c => (string.IsNullOrWhiteSpace(dirt) || (c.IsDamaged ? "Brudny" : "Czysty") == dirt) &&
                                        (string.IsNullOrWhiteSpace(vt) || (c.HasTracking ? "Tak" : "Nie") == vt))
                            .Select(c => new
                            {
                                c.AssetTag,
                                c.Make,
                                Typ = c.AssetType.ToString(),
                                c.Model,
                                Przeglad = c.InspectionDate,
                                Serwis = c.ServiceDate,
                                Stan = c.IsDamaged ? "Brudny" : "Czysty",
                                Tracking = c.HasTracking ? "Tak" : "Nie",
                                c.Mileage,
                                Poziom = c.AssetType == AssetType.Car ? $"{c.FuelLevel} %" : $"{c.FuelLevel} % SoC"
                            });

                using var wb = new ClosedXML.Excel.XLWorkbook();
                wb.Worksheets.Add("Samochody").FirstCell().InsertTable(list);
                wb.Worksheet(1).Columns().AdjustToContents();

                using var ms = new MemoryStream();
                wb.SaveAs(ms); ms.Position = 0;
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"samochody_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export asset list to Excel.");
                throw;
            }
        }


        #region Helpers

        /// <summary>
        /// Saves an uploaded image file to the server.
        /// </summary>
        /// <param name="file">The image file to save.</param>
        /// <returns>The relative path to the saved image.</returns>
        private string SaveImage(IFormFile file)
        {
            if (file == null)
                return string.Empty;

            try
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string path = Path.Combine(wwwRootPath, @"images/car/" + fileName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                return @"/images/car/" + fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save uploaded asset image.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Deletes an image file from the server.
        /// </summary>
        /// <param name="imageUrl">The relative path to the image file.</param>
        private void DeleteImage(string imageUrl)
        {
            try
            {
                string root = _webHostEnvironment.WebRootPath;
                string path = Path.Combine(root, imageUrl.TrimStart('/', '\\'));

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image file {ImageUrl}.", imageUrl);
            }
        }

        #endregion
    }
}