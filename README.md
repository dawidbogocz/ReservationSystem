# Reservation System

A role-based asset reservation management system built with ASP.NET Core MVC, Entity Framework Core, and ASP.NET Identity. Manage reservations, track faults/issues, provide feedback, send email notifications, and authenticate via SAML2 SSO.

## Features

- **Asset Management** -- Add, edit, view, soft-delete assets with image upload support
- **Reservation Management** -- Create, approve, reject, cancel reservations with conflict detection (no double-booking)
- **Fault/Issue Reporting** -- Report issues on assets with escalation notifications
- **Feedback Tracking** -- Pickup and return feedback workflows with expiry reminders
- **User Management** -- Full CRUD with roles (Admin, Manager, Employee) via admin panel
- **Role-based Areas** -- Separate dashboards for employees, managers, and admins
- **Email Notifications** -- Configurable SMTP-based alerts for reservations, feedback, and faults
- **Excel Export** -- Export asset and reservation lists to Excel
- **SAML2 SSO** -- Configurable single sign-on (Azure AD / any SAML2 IdP)
- **Responsive UI** -- Razor Views with Bootstrap, DataTables, SweetAlert2, toastr

## Architecture

```
ReservationSystem/
  |- Areas/
  |   |- Admin/         User management
  |   |- Manager/       Asset, reservation, fault, feedback management
  |   |- Employee/      Employee dashboard, reservation workflow
  |   - Identity/       Scaffolded Identity pages
  |- DataAccess/
  |   |- Data/          EF Core DbContext + migrations
  |   |- Repository/    Repository + Unit of Work pattern
  |   - Services/       Business logic (notifications, feedback checks)
  |- Models/
  |   - Models, enums, ViewModels
  |- Utility/
  |   - EmailSender, role constants
  |- wwwroot/           Static assets (JS, CSS, libs)
```

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local, Docker, or cloud)

### Setup

1. **Clone the repo:**
   ```bash
   git clone https://github.com/dawidbogocz/ReservationSystem.git
   cd ReservationSystem
   ```

2. **Configure the database:**
   Copy `appsettings.example.json` to `appsettings.json` and update the connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=ReservationSystemDb;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. **Build and run:**
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

4. **Seed data:** On first run, the app automatically applies migrations, creates the default roles (Admin, Manager, Employee), and seeds an admin user:
   - Email: `admin@admin.com`
   - Password: `Admin123*`

### Configuration

| Setting | Section | Description |
|---------|---------|-------------|
| Connection string | `ConnectionStrings.DefaultConnection` | SQL Server connection |
| SMTP | `EmailSettings` | Mail server for notifications |
| SAML2 | `Saml2` | SSO identity provider config |
| Feedback expiry | `FeedbackSettings.ExpirationDays` | Days before feedback auto-expires |
| Dev login bypass | `DevLogin` | Enable/disable dev login (dev only) |

## Roles

- **Admin** -- Full access: manage users, user groups, all assets
- **Manager** -- Manage assets, reservations, faults, feedback history
- **Employee** -- Make reservations, view assets, provide feedback

## Customization

This project uses `Asset` as the reservable entity with a flexible `AssetType` enum (Car, Lift, etc.). To customize for your domain:

1. **Add new asset types** -- Edit `AssetType` enum in `Models/Asset.cs`
2. **Rename fields** -- `AssetTag` (identifier), `Make`/`Model` (category/subcategory), maintenance dates, and status flags are all editable
3. **Localize** -- The UI text is in Polish; replace labels, validation messages, and display names as needed
4. **Extend** -- Add new properties to `Asset` or create derived types

## Tech Stack

- .NET 9 MVC + Razor Pages
- Entity Framework Core (SQL Server)
- ASP.NET Core Identity
- Bootstrap 5, DataTables, SweetAlert2, toastr
- MailKit (email)
- ClosedXML (Excel export)
- Hangfire (background jobs)
- Sustainsys.Saml2 (SSO)