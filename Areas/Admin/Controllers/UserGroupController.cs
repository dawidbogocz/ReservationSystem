using CarReservation.DataAccess.Repository.IRepository;
using CarReservation.Models;
using CarReservation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserGroupController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserGroupController> _logger;

        public UserGroupController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<UserGroupController> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var groups = await _unitOfWork.Context.UserGroups
                .Include(g => g.Users)
                .Include(g => g.Managers)
                    .ThenInclude(m => m.Manager)
                .OrderBy(g => g.Name)
                .ToListAsync();

            return View(groups);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            UserGroup group;

            if (id.HasValue && id.Value > 0)
            {
                group = await _unitOfWork.Context.UserGroups
                    .Include(g => g.Managers)
                    .Include(g => g.Users)
                    .FirstOrDefaultAsync(g => g.Id == id.Value);

                if (group == null)
                    return NotFound();
            }
            else
            {
                group = new UserGroup();
            }

            var vm = new UserGroupVM
            {
                UserGroup = group,
                SelectedManagerIds = group.Managers.Select(m => m.ManagerId).ToList(),
                SelectedEmployeeIds = group.Users.Select(u => u.Id).ToList()
            };

            await PopulateListsAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(UserGroupVM vm)
        {
            ModelState.Remove("UserGroup.Users");
            ModelState.Remove("UserGroup.Managers");
            ModelState.Remove("ManagerList");
            ModelState.Remove("EmployeeList");

            if (!ModelState.IsValid)
            {
                await PopulateListsAsync(vm);
                return View(vm);
            }

            try
            {
                UserGroup group;

                if (vm.UserGroup.Id == 0)
                {
                    group = new UserGroup
                    {
                        Name = vm.UserGroup.Name
                    };

                    await _unitOfWork.Context.UserGroups.AddAsync(group);
                    await _unitOfWork.Context.SaveChangesAsync();
                }
                else
                {
                    group = await _unitOfWork.Context.UserGroups
                        .Include(g => g.Managers)
                        .Include(g => g.Users)
                        .FirstOrDefaultAsync(g => g.Id == vm.UserGroup.Id);

                    if (group == null)
                        return NotFound();

                    group.Name = vm.UserGroup.Name;

                    _unitOfWork.Context.UserGroupManagers.RemoveRange(group.Managers);
                    await _unitOfWork.Context.SaveChangesAsync();
                }

                foreach (var managerId in vm.SelectedManagerIds.Distinct())
                {
                    await _unitOfWork.Context.UserGroupManagers.AddAsync(new UserGroupManager
                    {
                        UserGroupId = group.Id,
                        ManagerId = managerId
                    });
                }

                var allUsersPreviouslyInGroup = await _unitOfWork.Context.ApplicationUser
                    .Where(u => u.UserGroupId == group.Id)
                    .ToListAsync();

                foreach (var user in allUsersPreviouslyInGroup)
                {
                    user.UserGroupId = null;
                }

                var selectedUsers = await _unitOfWork.Context.ApplicationUser
                    .Where(u => vm.SelectedEmployeeIds.Contains(u.Id))
                    .ToListAsync();

                foreach (var user in selectedUsers)
                {
                    user.UserGroupId = group.Id;
                }

                await _unitOfWork.Context.SaveChangesAsync();

                TempData["success"] = "Grupa została zapisana.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save user group.");
                TempData["error"] = "Nie udało się zapisać grupy.";

                await PopulateListsAsync(vm);
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _unitOfWork.Context.UserGroups
                .Include(g => g.Managers)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            var users = await _unitOfWork.Context.ApplicationUser
                .Where(u => u.UserGroupId == id)
                .ToListAsync();

            foreach (var user in users)
            {
                user.UserGroupId = null;
            }

            _unitOfWork.Context.UserGroupManagers.RemoveRange(group.Managers);
            _unitOfWork.Context.UserGroups.Remove(group);

            await _unitOfWork.Context.SaveChangesAsync();

            TempData["success"] = "Grupa została usunięta.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateListsAsync(UserGroupVM vm)
        {
            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

            var managerOrAdminIds = managerUsers
                .Concat(adminUsers)
                .Select(u => u.Id)
                .Distinct()
                .ToList();

            vm.ManagerList = await _unitOfWork.Context.ApplicationUser
                .Where(u => managerOrAdminIds.Contains(u.Id))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new SelectListItem
                {
                    Text = $"{u.FirstName} {u.LastName} ({u.Email})",
                    Value = u.Id,
                    Selected = vm.SelectedManagerIds.Contains(u.Id)
                })
                .ToListAsync();

            vm.EmployeeList = await _unitOfWork.Context.ApplicationUser
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new SelectListItem
                {
                    Text = $"{u.FirstName} {u.LastName} ({u.Email})",
                    Value = u.Id,
                    Selected = vm.SelectedEmployeeIds.Contains(u.Id)
                })
                .ToListAsync();
        }
    }
}