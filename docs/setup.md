# Setup Guide

## Prerequisites

- .NET 9 SDK (or .NET 8 -- check the project file)
- SQL Server (local, Docker, or cloud)
- Optional: SMTP server for email notifications
- Optional: SAML2 identity provider for SSO

## Configuration

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ReservationSystemDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "EmailSettings": {
    "SmtpServer": "",
    "SmtpPort": 587,
    "SmtpEmail": "",
    "SmtpUser": "",
    "SmtpPass": ""
  },
  "Saml2": {
    "EntityId": "https://localhost:5001",
    "ReturnUrl": "https://localhost:5001/Saml2/Acs",
    "MetadataLocation": "",
    "IdpEntityId": "",
    "CertificatePath": ""
  },
  "FeedbackSettings": {
    "ExpirationDays": 2
  }
}
```

Copy `appsettings.json` to `appsettings.Development.json` for local development -- this file is gitignored and never committed.

## Database Setup

### Migrations

```bash
dotnet ef database update
```

Migrations run automatically on application startup (`DbInitializer.Initialize()`).

### Seed Data

On first startup, the `DbInitializer` creates:

- Three roles: **Employee**, **Manager**, **Admin**
- Default admin account: `admin@admin.com` / `Admin123*`
- Sample seed data (defined in `ApplicationDbContext.OnModelCreating()`)

## Running the Application

```bash
# Development
dotnet run

# Production
dotnet publish --configuration Release
dotnet ReservationApp.dll
```

## Customizing for Your Domain

This project uses `Asset` as the reservable entity with a flexible `AssetType` enum.

1. **Add new asset types** -- Edit `AssetType` enum in `Models/Asset.cs`
2. **Rename fields** -- `AssetTag` (identifier), `Make`/`Model` (category/subcategory) are all editable
3. **Localize** -- UI text is in Polish; replace labels and messages as needed
4. **Extend** -- Add new properties to `Asset` or create derived types

## Troubleshooting

### SAML2 Authentication Fails
- Verify certificate path and permissions
- Check that `EntityId` matches the URL the app is served on

### Hangfire Jobs Not Running
- Verify SQL Server connection string
- Check Hangfire dashboard at `/hangfire`

### Email Sending Fails
- Verify SMTP credentials in `EmailSettings`
- Email sending is optional -- app works without it

### Identity Login Issues
- Default admin: `admin@admin.com` / `Admin123*`
- New users must be assigned a role by Admin
- Security stamp validation runs every 1 minute