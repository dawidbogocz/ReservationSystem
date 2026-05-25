using CarReservation.DataAccess.Repository.IRepository;
using CarReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _unitOfWork.Context.ApplicationUser
                    .Include(u => u.UserGroup)
                    .ToListAsync();

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load user list");
                TempData["error"] = "Failed to load users.";
                return View(new List<ApplicationUser>());
            }
        }

        public async Task<IActionResult> Create()
        {
            await PopulateListsAsync();
            return View(new ApplicationUser());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user, string? role)
        {
            ModelState.Remove("Reservations");
            ModelState.Remove("UserGroup");

            if (!ModelState.IsValid)
            {
                await PopulateListsAsync(role, user.UserGroupId);
                return View(user);
            }

            var newUser = new ApplicationUser
            {
                UserName = user.Email,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                UserGroupId = user.UserGroupId
            };

            try
            {
                var result = await _userManager.CreateAsync(newUser);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    await PopulateListsAsync(role, user.UserGroupId);
                    return View(user);
                }

                if (!string.IsNullOrEmpty(role))
                    await _userManager.AddToRoleAsync(newUser, role);

                TempData["success"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user {Email}", user.Email);
                TempData["error"] = "An error occurred while creating the user.";
                await PopulateListsAsync(role, user.UserGroupId);
                return View(user);
            }
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var user = await _unitOfWork.Context.ApplicationUser
                    .Include(u => u.UserGroup)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    return NotFound();

                var roles = await _userManager.GetRolesAsync(user);
                var currentRole = roles.FirstOrDefault();

                await PopulateListsAsync(currentRole, user.UserGroupId);

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load edit view for user {UserId}", id);
                TempData["error"] = "Failed to load user data.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser user, string? role)
        {
            ModelState.Remove("Reservations");
            ModelState.Remove("UserGroup");

            if (!ModelState.IsValid)
            {
                await PopulateListsAsync(role, user.UserGroupId);
                return View(user);
            }

            try
            {
                var existingUser = await _unitOfWork.Context.ApplicationUser
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                if (existingUser == null)
                    return NotFound();

                var emailExists = await _unitOfWork.Context.ApplicationUser
                    .AnyAsync(u => u.Email == user.Email && u.Id != user.Id);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Ten adres e-mail jest już używany przez innego użytkownika.");
                    await PopulateListsAsync(role, user.UserGroupId);
                    return View(user);
                }

                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Email = user.Email;
                existingUser.UserName = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.UserGroupId = user.UserGroupId;

                _unitOfWork.ApplicationUser.Update(existingUser);
                _unitOfWork.Save();

                if (!string.IsNullOrEmpty(role))
                {
                    var currentRoles = await _userManager.GetRolesAsync(existingUser);

                    if (!currentRoles.Contains(role))
                    {
                        await _userManager.RemoveFromRolesAsync(existingUser, currentRoles.ToArray());
                        await _userManager.AddToRoleAsync(existingUser, role);
                    }
                }

                await _userManager.UpdateSecurityStampAsync(existingUser);

                TempData["success"] = "Użytkownik został zapisany pomyślnie.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}", user.Id);
                TempData["error"] = "An error occurred while saving user changes.";
                await PopulateListsAsync(role, user.UserGroupId);
                return View(user);
            }
        }

        private async Task PopulateListsAsync(string? selectedRole = null, int? selectedGroupId = null)
        {
            ViewBag.RoleList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Admin",
                    Value = "Admin",
                    Selected = selectedRole == "Admin"
                },
                new SelectListItem
                {
                    Text = "Manager",
                    Value = "Manager",
                    Selected = selectedRole == "Manager"
                },
                new SelectListItem
                {
                    Text = "Employee",
                    Value = "Employee",
                    Selected = selectedRole == "Employee"
                }
            };

            ViewBag.GroupList = await _unitOfWork.Context.UserGroups
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem
                {
                    Text = g.Name,
                    Value = g.Id.ToString(),
                    Selected = selectedGroupId.HasValue && selectedGroupId.Value == g.Id
                })
                .ToListAsync();
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _unitOfWork.Context.ApplicationUser
                    .Include(u => u.UserGroup)
                    .Select(u => new
                    {
                        id = u.Id,
                        username = u.UserName,
                        email = u.Email,
                        firstName = u.FirstName,
                        lastName = u.LastName,
                        phoneNumber = u.PhoneNumber,
                        groupName = u.UserGroup != null ? u.UserGroup.Name : ""
                    })
                    .ToListAsync();

                return Json(new { data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user list via API");
                return Json(new { data = new List<object>() });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                    return Json(new { success = false, message = "User not found." });

                var roles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, roles);

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                    return Json(new { success = false, message = "Failed to delete user." });

                return Json(new { success = true, message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the user." });
            }
        }

        #endregion
    }
}