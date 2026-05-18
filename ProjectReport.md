# SofraLink Project Report

## Submission Links

- GitHub repository: https://github.com/burakmtn/CateringWebsiteProject
- Unlisted YouTube demo video: https://www.youtube.com/watch?v=JR44BO6Tr44

## 1. Project Overview

SofraLink is a full-stack catering website built with ASP.NET Core MVC. The system allows users to browse nearby catering menu items, customize orders, use a simulated payment flow, receive order documents, submit ratings, and review order history. Caretakers manage their own menu items and service locations. Admin users monitor platform data, logs, users, caterers, orders, and ratings.

The project focuses on role-based access control, relational database design, order management, location-based filtering, dynamic document generation, email integration, logging, filtering, pagination, and custom branding.

## 2. Technologies Used

- ASP.NET Core MVC (.NET 9)
- Entity Framework Core
- SQL Server LocalDB
- ASP.NET Core Identity
- Razor Views
- Bootstrap
- Custom CSS
- Google Distance Matrix API integration
- SMTP email integration
- EF Core Code First migrations

## 3. Architecture

The application follows an MVC-based architecture with a clear separation between controllers, models, view models, data access, and services.

- Controllers handle HTTP requests and role-specific workflows.
- Models represent persistent database entities.
- View models shape data for Razor pages.
- `ApplicationDbContext` manages EF Core database access.
- Services handle external or reusable logic such as Google Maps distance checks, email sending, PDF generation, and system logging.
- ASP.NET Core Identity handles authentication, password storage, roles, and authorization.

Main service classes:

- `GoogleDistanceService`
- `SmtpOrderEmailService`
- `OrderDocumentService`
- `SystemLogService`

## 4. Roles and Permissions

The system supports three roles:

- `Admin`
- `Caretaker`
- `User`

Role-based authorization is enforced with controller-level `[Authorize]` attributes. Users cannot access caretaker-only or admin-only pages. Caretakers can manage only their own menu items. Users can only access their own cart, orders, reviews, and order documents.

The default admin account is seeded at startup:

- Email: `admin@sofralink.local`
- Password: `Admin123!`

## 5. Database Design

The database is built with EF Core Code First migrations. It includes Identity tables and application-specific tables.

Main entities:

- `ApplicationUser`
- `MenuItem`
- `MenuCustomizationOption`
- `Order`
- `OrderItem`
- `OrderItemOption`
- `OrderItemReview`
- `SystemLog`

Important relationships:

- A caretaker owns many menu items.
- A menu item has many customization options.
- A user has many orders.
- An order has many order items.
- An order item stores selected customization options.
- A completed order item can have one review.
- Reviews are linked to users, menu items, caretakers, and order items.
- System logs can optionally be linked to an application user.

The schema supports users, caterers, menu items, customization options, orders, ratings, and logs as required.

## 6. Main Features

### Authentication and Roles

Users can register and log in through custom MVC views. Registered users select either the User or Caretaker role. Admin is seeded by the application. Login attempts, successful logins, failed logins, registrations, and logout events are logged.

### Caretaker Menu Management

Caretakers can create, edit, delete, and filter their own menu items. Menu items include:

- name
- description
- price
- image upload
- removable ingredients
- optional additions
- option groups

Caretaker dashboard statistics show menu count, received orders, completed orders, revenue, average rating, and review count.

### User Menu Browsing

Users can set a delivery location and browse menus within the configured distance. Nearby filtering uses the Google Distance Matrix API. Menu and caretaker ratings are calculated dynamically from completed-order reviews.

User dashboard statistics show nearby menus, purchase count, total spent, and submitted review count.

### Cart and Payment Flow

Users can:

- add menu items to cart
- select customization options
- update quantities
- remove items
- view item subtotals
- view total order amount

The payment page collects simulated card information, shows the full order summary, displays customization details, and confirms payment without storing real card data.

After successful payment, the system triggers:

- order creation
- receipt generation
- agreement PDF generation
- email notification
- logging

### Ratings and Comments

Users can rate completed order items only. Each order item can be reviewed once. Reviews include:

- menu rating
- caretaker rating
- comment

Ratings are tied to completed orders and are used to calculate menu and caretaker rating summaries.

### Email System

After order creation, the application attempts to send order emails to the user and relevant caretaker accounts. SMTP settings are loaded from configuration. Credentials are not hardcoded. If SMTP is not configured, email sending is skipped without breaking the order flow.

### Dynamic PDF Generation

For every order, the system generates:

- receipt PDF
- agreement PDF

PDF documents are generated programmatically and contain order-specific customer, caretaker, item, customization, total price, status, and date information.

### Logging and Admin Monitoring

The system stores important events in the `SystemLogs` table. Logged events include:

- login attempts
- successful logins
- failed logins
- order creation
- payment actions
- rating submissions
- email sending events
- admin actions
- errors and exceptions

Admins can view logs through a dedicated admin logging page.

### Filtering and Pagination

Major tables support filtering and pagination:

- caretaker menu table
- cart table
- user order history
- admin logs
- admin users
- admin caterers
- admin orders
- admin ratings/comments

## 7. External Services

### Google Maps

The Google Distance Matrix API is used for location-based filtering. Users and caretakers store address information. The application calculates whether a caretaker's menu is within the configured nearby distance.

The API key is read from configuration:

```json
"GoogleMaps": {
  "ApiKey": "",
  "NearbyDistanceKm": 20
}
```

If the API key is missing, the application displays a clear message and avoids failing.

### SMTP Email

SMTP settings are read from:

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

Sensitive values should be configured with user secrets, environment variables, or local configuration outside source control.

## 8. Security and Validation

Security-related implementation details:

- ASP.NET Core Identity stores user credentials securely.
- Role-based authorization protects controllers.
- Users can access only their own orders and documents.
- Caretakers can edit/delete only their own menu items.
- Real card data is not stored.
- Uploaded images are restricted by extension and size.
- Anti-forgery tokens are used on form posts.
- Errors are logged through the system logging service.

## 9. Branding and UI

The project uses the SofraLink brand identity. The interface includes custom layout, colors, cards, navigation, tables, dashboard sections, responsive styling, and branded empty states. The design avoids the plain default MVC template appearance.

## 10. Database Reproducibility

The project includes EF Core migrations and a generated SQL script:

- `DatabaseScripts/CateringWebsiteDatabase.sql`

This script can be run against SQL Server to create or update the schema. The application also seeds required roles and the default admin account at startup through `SeedData.InitializeAsync`.

## 11. How to Run

1. Open the solution:

```bash
dotnet build CateringWebsiteProject.sln
```

2. Apply database migrations:

```bash
dotnet ef database update --project CateringWebsite
```

3. Run the application:

```bash
dotnet run --project CateringWebsite
```

4. Open the local URL shown by the application.

## 12. Demo Notes

Default admin:

- Email: `admin@sofralink.local`
- Password: `Admin123!`

For full Google Maps functionality, configure a valid Google Maps API key. For real email sending, configure SMTP settings. Without these settings, the related workflows are handled safely without crashing.

## 13. Limitations

The live call system is listed as a bonus feature in the project description and was not implemented in the required feature set. Payment is intentionally simulated, and no real payment provider or real card storage is used.
