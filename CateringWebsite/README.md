# CateringWebsite Application

This folder contains the ASP.NET Core MVC application for SofraLink.

## Application Responsibilities

- User registration and login with ASP.NET Core Identity.
- Role management for `Admin`, `Caretaker`, and `User`.
- Menu browsing, details, filtering, and ratings.
- Caretaker menu item and customization option management.
- Cart, payment simulation, order creation, and order history.
- Dynamic receipt and agreement document generation.
- SMTP-based order emails.
- Google Distance Matrix API distance checks.
- System logging and admin monitoring screens.

## Important Folders

- `Controllers/`: MVC controllers for account, admin, cart, caretaker, home, orders, profile, and user workflows.
- `Data/`: EF Core database context, migrations, role constants, and startup seed data.
- `Models/`: persistent entities and view models.
- `Services/`: reusable business services for distance checks, email, order documents, PDF generation, and logging.
- `Views/`: Razor views grouped by controller.
- `wwwroot/`: static assets, custom CSS, JavaScript, Bootstrap, jQuery, and validation libraries.
- `Properties/launchSettings.json`: local development launch profiles.

## Configuration Files

- `appsettings.json`: default connection string, Google Maps options, SMTP options, logging, and allowed hosts.
- `appsettings.Development.json`: development-specific configuration.
- `CateringWebsite.csproj`: .NET 9 project file and NuGet package references.

## Running from This Folder

Build:

```bash
dotnet build
```

Apply migrations:

```bash
dotnet ef database update
```

Run:

```bash
dotnet run
```

Default development URLs:

- `https://localhost:7222`
- `http://localhost:5185`

## Database Startup Behavior

`Program.cs` calls `SeedData.InitializeAsync(app.Services)` during startup. This method:

- applies pending EF Core migrations,
- creates the required roles if missing,
- creates the default admin user if missing,
- assigns the admin role to the default admin user.

Default admin:

- Email: `admin@sofralink.local`
- Password: `Admin123!`

## External Service Notes

Google Maps is optional for local startup, but distance-based menu filtering requires a valid API key for full behavior.

SMTP is optional for local startup. If SMTP settings are empty, the order flow continues without crashing, but real emails are not sent.

## Security Notes

- Real card data is not stored; payment is a simulation.
- Users can access only their own cart, orders, reviews, and documents.
- Caretakers can manage only their own menu items.
- Admin-only screens are protected by role-based authorization.
- Sensitive API keys and SMTP credentials should not be committed to source control.

