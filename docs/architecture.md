# Architecture

## Overview

ReservationSystem is a generic **ASP.NET Core MVC** application using **Entity Framework Core** with SQL Server and **ASP.NET Core Identity** for authentication and role-based authorization. It uses an **Area-based** structure to separate concerns between Employee, Manager, and Admin functionality.

## High-Level Layers

```
+--------------------------------------------------+
|              Web Layer (MVC)                       |
|  ReservationApp/                                   |
|    Areas/Employee/Controllers/HomeController        |
|    Areas/Manager/Controllers/                       |
|      AssetController, ReservationController         |
|      FaultController, FeedbackController            |
|    Areas/Admin/Controllers/                         |
|      UserController, UserGroupController            |
|    Areas/Identity/Pages/                            |
+--------------------------------------------------+
|           Service / Repository Layer                |
|  ReservationApp.DataAccess/                         |
|    Repository/ -- Repository + UnitOfWork           |
|    Services/                                        |
|      ReservationService -- Hangfire background jobs |
|      DepartmentNotificationService                  |
|    DbInitializer/ -- Seed + migrate on startup      |
|    ApplicationDbContext -- EF Core context           |
+--------------------------------------------------+
|           Domain Layer (Models)                     |
|  ReservationApp.Models/                             |
|    Asset, Reservation, Fault, FeedbackLog           |
|    ApplicationUser, UserGroup, UserGroupManager     |
|    ViewModels: AssetVM, ReservationVM, FaultVM      |
|    Enums: Approval, AssetType, FeedbackKind         |
|           FeedbackStatus                            |
+--------------------------------------------------+
|  Utility Layer                                      |
|  ReservationApp.Utility/                            |
|    SD -- role constants                             |
|    EmailSender -- SMTP email                        |
+--------------------------------------------------+
```

## External Integrations

### SAML2 (Optional)
- SAML2 authentication via Sustainsys.Saml2 (configurable)
- Configured via `Saml2:*` in `appsettings.json`
- Works with any SAML2-compatible identity provider (Azure AD, Okta, etc.)

### SMTP Email (Optional)
- Configured via `EmailSettings:*` in `appsettings.json`
- Sends notifications for reservation creation, approval, rejection, reminders

### Hangfire Dashboard
- Available at `/hangfire` for monitoring background jobs
- Access is unrestricted by default (secure in production via firewall/IP whitelist)

## Background Services (Hangfire)

Two recurring jobs run **hourly** via Hangfire:

1. **SendUpcomingReservationReminders** (job: `reservation-reminders`)
   - Finds approved reservations starting within 24 hours
   - Sends email reminder to the user
   - Uses optimistic concurrency to avoid double-sending

2. **CheckAndSendFeedbackReminders** (job: `feedback-check`)
   - Creates `FeedbackLog` entries at pickup/return time
   - After configured expiration period (default: 2 days), expires overdue feedback
   - Sends expired-feedback alerts to group managers and admins

## Database Schema (Core Entities)

### Asset
- `AssetTag` (PK, string) -- unique identifier for the asset
- `AssetType` (enum: Car, Lift, etc.)
- `Make`, `Model`
- `Inspection` (DateOnly), `Service` (DateOnly)
- `ImageUrl` (nullable)
- `IsDirty`, `HasVideotolling`, `IsDeleted` (boolean flags)
- `Mileage` (int), `FuelLevel` (int, percentage)
- Navigation: `Faults`, `Reservations`

### Reservation
- `Id` (PK, int)
- `AssetTag` (FK -> Asset)
- `UserId` (FK -> ApplicationUser)
- `PickupDate`, `ReturnDate` (DateTime)
- `Destination`
- `Approval` (enum: Pending / Accepted / Rejected / Canceled)
- Feedback fields, approval audit fields
- Navigation: `Asset`, `User`

### Fault
- `Id` (PK, int)
- `AssetTag` (FK -> Asset)
- `UserId` (FK -> ApplicationUser)
- `Description`, `DateReported`
- `IsFixed`, `FixDescription`, `FixDate`
- `IsDrivable`, `DrivableComment`

### FeedbackLog
- `Id` (PK, int)
- `ReservationId` (FK -> Reservation)
- `AssetTag`, `UserId`
- `Kind` (enum: Pickup / Return)
- `Status` (enum: Pending / Provided / Expired)
- Mileage, fuel, dirty, faults tracking

### ApplicationUser (extends IdentityUser)
- `FirstName`, `LastName`
- `UserGroupId` (FK -> UserGroup, nullable)
- `IsDeleted` (soft-delete)

### UserGroup
- `Id` (PK, int)
- `Name`
- Navigation: `Users`, `Managers`

## Identity and Authorization

- ASP.NET Core Identity with 3 roles: Employee, Manager, Admin
- Two policies:
  - `Anyone` -- requires any authenticated role
  - `AdminManager` -- requires Manager or Admin
- Global query filters for soft-delete on Asset and ApplicationUser