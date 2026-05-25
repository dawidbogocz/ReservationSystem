using CarReservation.DataAccess.Repository.IRepository;
using CarReservation.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarReservation.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Policy = "AdminManager")]
    public class FeedbackController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(IUnitOfWork uow, ILogger<FeedbackController> logger)
        {
            _unitOfWork = uow;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var baseQuery = _unitOfWork.Context.Set<FeedbackLog>()
                    .Include(f => f.User)
                    .AsNoTracking();

                var logs = await baseQuery.ToListAsync();

                ViewBag.CarPlates = logs.Select(l => l.AssetTag)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();

                ViewBag.Users = logs.Select(l => $"{l.User.FirstName} {l.User.LastName}")
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();

                ViewBag.Statuses = Enum.GetNames(typeof(FeedbackStatus))
                    .Select(s => new { Value = s, Text = s == "Provided" ? "Dostarczona" : s == "Expired" ? "Wygasła" : "Oczekująca" })
                    .ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load feedback index page.");
                TempData["error"] = "Failed to load feedback logs.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = (await _unitOfWork.Context.Set<FeedbackLog>()
                    .Include(f => f.User)
                    .Include(f => f.Reservation)
                    .AsNoTracking()
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync())
                    .Select(f => new
                    {
                        id = f.Id,
                        when = f.CreatedAt,
                        kind = f.Kind.ToString(),
                        status = f.Status.ToString(),
                        car = f.AssetTag,
                        user = f.User.FirstName + " " + f.User.LastName,
                        reservationId = f.ReservationId,
                        mileage = f.Mileage,
                        fuel = f.FuelLevel,
                        dirty = f.IsCarDirty ?? false,
                        faults = f.HasFaults == true ? (string.IsNullOrWhiteSpace(f.Faults) ? "(brak)" : f.Faults) : "-"
                    });

                return Json(new { data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load feedback logs via API.");
                return Json(new { data = new List<object>() });
            }
        }

        [HttpGet]
        public async Task<FileResult> ExportToExcel(string? kind, string? status, string? car, string? user, DateTime? dateFrom, DateTime? dateTo)
        {
            try
            {
                var query = _unitOfWork.Context.Set<FeedbackLog>()
                    .Include(f => f.User)
                    .Include(f => f.Reservation)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(kind) && Enum.TryParse<FeedbackKind>(kind, true, out var k))
                    query = query.Where(f => f.Kind == k);

                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FeedbackStatus>(status, true, out var s))
                    query = query.Where(f => f.Status == s);

                if (!string.IsNullOrWhiteSpace(car))
                    query = query.Where(f => f.AssetTag == car);

                if (!string.IsNullOrWhiteSpace(user))
                    query = query.Where(f => (f.User.FirstName + " " + f.User.LastName) == user);

                if (dateFrom.HasValue)
                    query = query.Where(f => f.CreatedAt.Date >= dateFrom.Value.Date);

                if (dateTo.HasValue)
                    query = query.Where(f => f.CreatedAt.Date <= dateTo.Value.Date);

                var rows = await query.OrderByDescending(f => f.CreatedAt).ToListAsync();

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Feedback");

                var tableData = rows.Select(f => new
                {
                    Data = f.CreatedAt,
                    Typ = f.Kind.ToString(),
                    Status = f.Status == FeedbackStatus.Provided ? "Dostarczona" :
                             f.Status == FeedbackStatus.Expired ? "Wygasła" : "Oczekująca",
                    Samochód = f.AssetTag,
                    Użytkownik = $"{f.User.FirstName} {f.User.LastName}",
                    Rezerwacja = f.ReservationId,
                    Przebieg_km = f.Mileage,
                    Paliwo_pct = f.FuelLevel,
                    Czystość = (f.IsCarDirty ?? false) ? "Brudny" : "Czysty",
                    Usterki = f.HasFaults == true ? (string.IsNullOrWhiteSpace(f.Faults) ? "(brak)" : f.Faults) : "-"
                });

                ws.Cell(1, 1).InsertTable(tableData);
                ws.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                wb.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"feedback_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export feedback logs to Excel.");
                throw;
            }
        }
    }
}