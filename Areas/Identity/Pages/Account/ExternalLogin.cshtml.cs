using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CarReservation.DataAccess.Data;
using CarReservation.Models;
using CarReservation.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarReservation.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly ApplicationDbContext _dbContext;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext dbContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            _dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }
        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            public string? SelectedUserId { get; set; }

            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }

            public string? PhoneNumber { get; set; }

            public List<ApplicationUser>? MatchingUsers { get; set; }
        }

        public IActionResult OnGet(string? returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl = returnUrl ?? Url.Content("~/") });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return new ChallengeResult("Saml2", properties);
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return new ChallengeResult("Saml2", properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (!authResult.Succeeded)
            {
                ErrorMessage = "Authentication failed.";

                var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
                var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

                return Challenge(properties, "Saml2");
            }

            var email = authResult.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
                var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

                return Challenge(properties, "Saml2");
            }

            var matchingUsers = await _dbContext.Users
                .OfType<ApplicationUser>()
                .Where(u => u.Email == email)
                .ToListAsync();

            ProviderDisplayName = "Azure AD";
            Input = new InputModel
            {
                Email = email,
                MatchingUsers = matchingUsers
            };

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ApplicationUser selectedUser = null;

            if (!string.IsNullOrEmpty(Input.SelectedUserId))
            {
                selectedUser = await _dbContext.Users
                    .OfType<ApplicationUser>()
                    .FirstOrDefaultAsync(u => u.Id == Input.SelectedUserId);
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    await ReloadUsers();
                    return Page();
                }

                selectedUser = new ApplicationUser
                {
                    Email = Input.Email,
                    UserName = $"{Input.Email}_{Input.FirstName}{Input.LastName}",
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    PhoneNumber = Input.PhoneNumber
                };

                var result = await _userManager.CreateAsync(selectedUser);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    await ReloadUsers();
                    return Page();
                }

                await _userManager.AddToRoleAsync(selectedUser, SD.Role_Employee);
            }

            await _signInManager.SignInAsync(selectedUser, isPersistent: false);
            return LocalRedirect(returnUrl);
        }

        private async Task ReloadUsers()
        {
            Input.MatchingUsers = await _dbContext.Users
                .OfType<ApplicationUser>()
                .Where(u => u.Email == Input.Email)
                .ToListAsync();
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }

            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}