# Reservation System

A role-based asset reservation management system built with ASP.NET Core MVC, Entity Framework Core, and ASP.NET Identity. Manage reservations, track faults/issues, provide feedback, send email notifications, and authenticate via SAML2 SSO.

## Quick Start

```bash
# Clone the repository
git clone https://github.com/dawidbogocz/ReservationSystem.git
cd ReservationSystem

# Configure connection string in appsettings.json

# Build and run
dotnet restore
dotnet build
dotnet run
```

Default admin: `admin@admin.com` / `Admin123*`

## Features

- **Asset Management** -- Add, edit, view, soft-delete assets with image upload support
- **Reservation Management** -- Create, approve, reject, cancel with conflict detection (no double-booking)
- **Fault/Issue Reporting** -- Report issues on assets with escalation notifications
- **Feedback Tracking** -- Pickup and return feedback workflows with expiry reminders
- **User Management** -- Full CRUD with roles (Admin, Manager, Employee)
- **Role-based Areas** -- Separate dashboards for employees, managers, and admins
- **Email Notifications** -- Configurable SMTP-based alerts
- **Excel Export** -- Export asset and reservation lists
- **SAML2 SSO** -- Configurable single sign-on (Azure AD / any SAML2 IdP)
- **Responsive UI** -- Bootstrap, DataTables, SweetAlert2, toastr

## Key Concepts

| Concept | Description |
|---------|-------------|
| **Reservations** | Users reserve assets. Each goes through approval: Pending -> Accepted/Rejected/Canceled. |
| **Feedback System** | At pickup/return, users report mileage, fuel, cleanliness, faults. 2-day window; expired feedback alerts managers. |
| **Fault Reporting** | Reported during feedback or separately. Managers mark as fixed. Minor faults flagged as drivable. |
| **User Groups** | Departments that scope manager visibility. Managers only see their group. |
| **Approval Lifecycle** | Pending -> Accepted / Rejected / Canceled |
| **Reminders** | Hangfire sends reminders 24h before pickup, expires feedback after 2 days. |

## Roles

| Role | Capabilities |
|------|-------------|
| **Employee** | Browse assets, create reservations, provide feedback, extend/cancel own reservations |
| **Manager** | Manage assets, reservations (own group), faults, feedback, approve/reject reservations |
| **Admin** | Full access: manage users, groups, all data |

## Scheduled Jobs (Hangfire)

| Job | Schedule | Description |
|-----|----------|-------------|
| `feedback-check` | Hourly | Creates feedback entries at pickup/return time, expires overdue feedback |
| `reservation-reminders` | Hourly | Sends email reminders 24h before an approved reservation starts |

## Project Structure

```
ReservationSystem/
  Areas/
    Admin/         User management, user group management
    Employee/      Home (reservations, feedback)
    Manager/       Asset, Reservation, Fault, Feedback management
    Identity/      ASP.NET Core Identity pages
  DataAccess/      DB context, repositories, services, DbInitializer
  Models/          Domain models, ViewModels, enums
  Utility/         Role constants, EmailSender
```

## Technology Stack

- **.NET 9** (ASP.NET Core MVC)
- **Entity Framework Core** with SQL Server
- **ASP.NET Core Identity** (roles + authentication)
- **Sustainsys.Saml2** (SAML2 SSO)
- **Hangfire** (background job scheduling)
- **ClosedXML** (Excel export)
- **MailKit** (email via SMTP)
- **Bootstrap 5** + **jQuery** + **DataTables** (UI)
- **SweetAlert2** + **Toastr** (notifications)

## Customization

This project uses `Asset` as the reservable entity with a flexible `AssetType` enum. To customize:

1. **Add new asset types** -- Edit `AssetType` enum in `Models/Asset.cs`
2. **Rename fields** -- `AssetTag`, `Make`/`Model` are all editable
3. **Localize** -- UI text is in Polish; replace labels as needed
4. **Extend** -- Add new properties or derived types

## Documentation

- [Architecture](docs/architecture.md) -- Architecture overview
- [Setup Guide](docs/setup.md) -- Configuration and deployment
- [Data Flow](docs/data-flow.md) -- How reservations, feedback, and faults interact
- [Roles & Permissions](docs/roles-and-permissions.md) -- RBAC reference