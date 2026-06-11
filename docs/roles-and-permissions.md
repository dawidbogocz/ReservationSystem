# Roles & Permissions

## Overview

Three roles exist in the system. They are **cumulative** -- each higher role includes all permissions of the lower ones.

| Role | Level | Capabilities |
|------|-------|-------------|
| **Employee** | Basic | Browse assets, create reservations, provide feedback, extend/cancel own reservations |
| **Manager** | Elevated | Manage assets, reservations (scoped to own user group), faults, feedback, approve/reject reservations |
| **Admin** | Full | User management, user group management, full system access (unscoped) |

## Authorization Policies

Defined in `Program.cs`:

```csharp
options.AddPolicy("Anyone", policy =>
    policy.RequireRole("Admin", "Employee", "Manager"));

options.AddPolicy("AdminManager", policy =>
    policy.RequireRole("Admin", "Manager"));
```

## Permissions by Area

### Employee Area (`Anyone` policy)

| Action | Description |
|--------|-------------|
| Index | View available assets with reservation calendar |
| Create | Create new reservation (status: Pending) |
| MyReservations | View own reservations |
| ExtendReservation | Extend own reservation (re-approval required) |
| CancelReservation | Cancel upcoming reservation |
| PickupFeedback | Provide pickup feedback |
| ReturnFeedback | Provide return feedback |

### Manager Area (`AdminManager` policy)

| Controller | Actions |
|------------|--------|
| AssetController | CRUD assets + image upload + export Excel |
| ReservationController | List, Upsert, Approve/Reject, Delete, Export Excel |
| FaultController | CRUD faults, Toggle/Mark fixed, Export Excel |
| FeedbackController | List feedback logs, Export Excel |

### Admin Area (`Admin` role only)

| Controller | Actions |
|------------|--------|
| UserController | List, Create, Edit, Soft-delete, Restore users |
| UserGroupController | List, Create/Edit, Delete user groups |

## Group-Based Scoping

| Entity | Manager Visibility |
|--------|-------------------|
| Reservations | Only for users in manager's assigned groups |
| Users | Only users in manager's assigned groups |
| Assets | All assets (not group-scoped) |
| Faults | All faults (not group-scoped) |
| Feedback | All feedback (not group-scoped) |

Admins bypass all scoping.

## Customization

The project is designed to be generic. To customize for your domain:

1. Add new asset types in `AssetType` enum (`Models/Asset.cs`)
2. Rename `AssetTag` to your identifier (serial number, barcode, etc.)
3. Add or remove asset properties as needed
4. Localize UI text by replacing Polish labels and validation messages
5. Change the default admin credentials in `appsettings.json` or `DbInitializer.cs`