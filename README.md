# SofraLink Catering Website

SofraLink is a full-stack catering website built with ASP.NET Core MVC. It supports role-based access for admins, caretakers, and users; menu management; location-based menu browsing; cart and simulated payment flows; ratings; email notifications; dynamic order documents; and system logging.

## Project Structure

- `CateringWebsite/`: ASP.NET Core MVC application source code.
- `DatabaseScripts/`: SQL Server database generation script and database setup notes.
- `ProjectReport.md`: project report with architecture, features, database design, and limitations.
- `DemoVideoPlan.md`: suggested demo video flow.
- `References.md`: references and resources used during development.
- `Full-Stack-Catering-Website-Project.md`: original project requirements.
- `CateringWebsiteProject.sln`: Visual Studio / .NET solution file.

## Technologies

- .NET 9
- ASP.NET Core MVC
- ASP.NET Core Identity
- Entity Framework Core
- SQL Server LocalDB
- Razor Views
- Bootstrap and custom CSS
- Google Distance Matrix API integration
- SMTP email integration

## Main Features

- Authentication and role-based authorization.
- Admin, caretaker, and user dashboards.
- Dynamic menu item and customization option management.
- User location setup and nearby menu filtering.
- Shopping cart with customizable items and simulated payment.
- Dynamic receipt and agreement PDF generation.
- Order email notifications for users and caretakers.
- Ratings and comments tied to completed order items.
- System event logging and admin log monitoring.
- Filtered and paginated tables across major data views.

## Default Accounts

The application seeds the default admin account on startup:

- Email: `admin@sofralink.local`
- Password: `Admin123!`

Regular users and caretakers can be created through the registration page.

## Prerequisites

- .NET 9 SDK
- SQL Server LocalDB or another SQL Server instance
- Optional: Google Maps API key for distance-based filtering
- Optional: SMTP credentials for real email sending

## Setup and Run

1. Restore and build the solution:

```bash
dotnet build CateringWebsiteProject.sln
```

2. Apply EF Core migrations:

```bash
dotnet ef database update --project CateringWebsite
```

The application also runs migrations automatically during startup through `SeedData.InitializeAsync`.

3. Run the website:

```bash
dotnet run --project CateringWebsite
```

4. Open the URL printed by the application. The default launch profile uses:

- `https://localhost:7222`
- `http://localhost:5185`

## Configuration

The default connection string is defined in `CateringWebsite/appsettings.json` and uses SQL Server LocalDB.

Google Maps settings:

```json
"GoogleMaps": {
  "ApiKey": "",
  "NearbyDistanceKm": 20
}
```

SMTP settings:

```json
"Email": {
  "Smtp": {
    "Host": "",
    "Port": 587,
    "EnableSsl": true,
    "UserName": "",
    "Password": "",
    "SenderEmail": "",
    "SenderName": "SofraLink"
  }
}
```

Sensitive values should be stored with user secrets, environment variables, or local configuration outside source control.

## Database Script

The database can also be created or updated with:

- `DatabaseScripts/CateringWebsiteDatabase.sql`

See `DatabaseScripts/README.md` for script-specific usage notes.

## Documentation

- `ProjectReport.md` explains the implemented system and design decisions.
- `DemoVideoPlan.md` gives a structured demo recording plan.
- `References.md` lists external resources.
- `CateringWebsite/README.md` documents the application folder in more technical detail.

