# Database Generation Scripts

This folder contains the SQL Server schema script for the SofraLink catering website.

## Files

- `CateringWebsiteDatabase.sql`: idempotent EF Core migration script for creating or updating the database schema.

## How to Use

1. Create a SQL Server database or use the LocalDB database configured in `CateringWebsite/appsettings.json`.
2. Run `CateringWebsiteDatabase.sql` against that database.
3. Start the ASP.NET Core application.

The application also runs `SeedData.InitializeAsync` on startup, which creates the required roles and default admin account if they do not already exist.

Default admin:

- Email: `admin@sofralink.local`
- Password: `Admin123!`

The script is generated from EF Core migrations and includes Identity, menu, customization, order, review, and system log tables.
