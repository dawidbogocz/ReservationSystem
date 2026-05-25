using System.Security.Cryptography.X509Certificates;
using ReservationApp.DataAccess.Data;
using ReservationApp.DataAccess.DbInitializer;
using ReservationApp.DataAccess.Repository;
using ReservationApp.DataAccess.Repository.IRepository;
using ReservationApp.DataAccess.Services;
using ReservationApp.Models;
using ReservationApp.Utility;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Metadata;
using Sustainsys.Saml2.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC & Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 2. Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 4. SAML2 Authentication
var saml2Settings = builder.Configuration.GetSection("Saml2");
var entityId = saml2Settings["EntityId"];
var returnUrl = saml2Settings["ReturnUrl"];
var metadataLocation = saml2Settings["MetadataLocation"];
var certPath = saml2Settings["CertificatePath"];
var idpEntityId = saml2Settings["IdpEntityId"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddSaml2(options =>
{
    options.SignInScheme = IdentityConstants.ExternalScheme;
    options.SPOptions.EntityId = new EntityId(entityId);
    options.SPOptions.ReturnUrl = new Uri(returnUrl);

    var idp = new IdentityProvider(new EntityId(idpEntityId), options.SPOptions)
    {
        LoadMetadata = true,
        MetadataLocation = metadataLocation,
        AllowUnsolicitedAuthnResponse = true,
    };

    var azureCert = new X509Certificate2(certPath);
    idp.SigningKeys.AddConfiguredKey(new X509RawDataKeyIdentifierClause(azureCert));
    options.IdentityProviders.Add(idp);
});

// 5. Cookie config
builder.Services.ConfigureApplicationCookie(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    }
    else
    {
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    }
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.ExternalScheme, options =>
{
    options.Cookie.Name = ".AspNetCore.External";
    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    }
    else
    {
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    }
});

// 6. Security Stamp Validator
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(1);
});

// 7. Dependency Injection
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IDepartmentNotificationService, DepartmentNotificationService>();

// 8. Authorization Policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Anyone", policy =>
        policy.RequireRole("Admin", "Employee", "Manager"))
    .AddPolicy("AdminManager", policy =>
        policy.RequireRole("Admin", "Manager"));

// 9. Hangfire configuration
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHangfireServer();

// 10. Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// 11. Seed database
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    dbInitializer.Initialize();
}

// 12. Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 13. Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

// 14. Schedule recurring jobs
RecurringJob.AddOrUpdate<IReservationService>(
    "feedback-check",
    x => x.CheckAndSendFeedbackReminders(),
    Cron.Hourly);

RecurringJob.AddOrUpdate<IReservationService>(
    "reservation-reminders",
    x => x.SendUpcomingReservationReminders(),
    Cron.Hourly);

// 15. Routing
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Employee}/{controller=Home}/{action=Index}/{id?}");

app.Run();