using CarReservation.DataAccess.Data;
using CarReservation.Models;
using CarReservation.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarReservation.DataAccess.DbInitializer
{
    /// <summary>
    /// Initializes the database by applying pending migrations, creating default roles,
    /// and seeding an admin user if they do not already exist.
    /// </summary>
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbInitializer"/> class.
        /// </summary>
        /// <param name="userManager">The <see cref="UserManager{IdentityUser}"/> for managing user operations.</param>
        /// <param name="roleManager">The <see cref="RoleManager{IdentityRole}"/> for managing role operations.</param>
        /// <param name="db">The application database context.</param>
        public DbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        /// <summary>
        /// Applies pending migrations (if any) and seeds the database with default roles and an admin user.
        /// </summary>
        public void Initialize()
        {
            try
            {
                Console.WriteLine("Starting database initialization...");

                var pendingMigrations = _db.Database.GetPendingMigrations().ToList();

                if (pendingMigrations.Any())
                {
                    Console.WriteLine("Pending migrations detected:");
                    foreach (var m in pendingMigrations)
                    {
                        Console.WriteLine($"   ➜ {m}");
                    }

                    Console.WriteLine("Applying migrations...");
                    _db.Database.Migrate();
                    Console.WriteLine("Migrations applied successfully.");
                }
                else
                {
                    Console.WriteLine("No pending migrations.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("MIGRATION FAILED!");
                Console.WriteLine(ex.ToString());

                // CRITICAL: Re-throw so the app does NOT start silently broken
                throw;
            }

            // ---- ROLE & ADMIN SEEDING (unchanged logic, just safer) ----
            if (!_roleManager.RoleExistsAsync(SD.Role_Employee).GetAwaiter().GetResult())
            {
                Console.WriteLine("Seeding roles and admin user...");

                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Manager)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();

                var result = _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    FirstName = "Admin",
                    LastName = "Admin",
                    PhoneNumber = "1111111111",
                    EmailConfirmed = true
                }, "Admin123*").GetAwaiter().GetResult();

                if (!result.Succeeded)
                {
                    Console.WriteLine("Failed to create admin user:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"{error.Description}");
                    }
                }

                ApplicationUser user = _db.ApplicationUser.FirstOrDefault(u => u.Email == "admin@admin.com");
                if (user != null)
                {
                    _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
                    Console.WriteLine("Admin user seeded.");
                }
            }
        }

    }
}
