# Hotel Management System (HMS)

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core Web API](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-5C2D91?logo=dotnet&logoColor=white)
![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-0F766E)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![Swagger](https://img.shields.io/badge/API%20Docs-Swagger-85EA2D?logo=swagger&logoColor=black)

A modern backend API for managing hotels, rooms, reservations, and users, built with **.NET 10**, **ASP.NET Core Web API**, and **Clean Architecture**.

---

## Table of Contents

- [Project Description](#project-description)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Features](#features)
- [NuGet Packages](#nuget-packages)
- [API Documentation](#api-documentation)
- [Getting Started](#getting-started)
- [Environment Configuration](#environment-configuration)
- [Authentication](#authentication)
- [Database](#database)
- [Development Practices](#development-practices)
- [Future Improvements](#future-improvements)
- [Author](#author)
- [License](#license)

---

## Project Description

The **Hotel Management System (HMS)** is a backend API designed to support core hotel operations through a secure, maintainable, and scalable architecture. It centralizes hotel management, room inventory, reservations, guest records, and user accounts in a single service-oriented platform.

The goal of this project is to provide a solid backend foundation that can power administrative dashboards, staff tools, mobile applications, or third-party integrations. The codebase emphasizes clean boundaries, extensibility, and production-oriented design practices such as role-based authorization, structured validation, database migrations, and API documentation.

---

## Architecture

This project follows **Clean Architecture**, which separates business rules from framework and infrastructure concerns. The result is a codebase that is easier to test, maintain, and evolve over time.

### Layer Overview

- **Domain**
  Contains the core business entities and identity models. This layer represents the heart of the system and should remain independent of infrastructure and UI concerns.
- **Application**
  Contains use-case logic, service contracts, repository contracts, DTOs, mapping configuration, validation rules, and application-specific exceptions.
- **Infrastructure**
  Contains implementation details such as Entity Framework Core, SQL Server persistence, repositories, database migrations, and development data seeding.
- **API**
  Exposes the application through HTTP endpoints. This layer handles controllers, middleware, Swagger configuration, authentication/authorization setup, and dependency injection wiring.

### Dependency Direction

```text
hms.Api -----------------------> hms.Application -----------------> hms.Domain
   |                                      ^
   |                                      |
   +-------------> hms.Infrastructure ----+
```

This structure ensures that the business core stays isolated from external technologies.

---

## Tech Stack

### Backend

- **.NET 10**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **ASP.NET Core Identity**
- **JWT Authentication**

### Architecture

- **Clean Architecture**
- **Repository Pattern**
- **Dependency Injection**

### Libraries / Tools

- **Mapster** for object mapping
- **FluentValidation** for request validation
- **Swagger / OpenAPI** for API documentation
- **Microsoft Identity** for authentication and user management
- **EF Core** for ORM and database access

### Database

- **SQL Server**

### Version Control

- **Git**
- **GitHub**

---

## Project Structure

The solution is split into separate projects to keep responsibilities clear and maintainable.

```text
Hotel-Management-System/
|-- hms.Api/
|-- hms.Application/
|-- hms.Domain/
|-- hms.Infrastructure/
`-- hms.slnx
```

### Project Purpose

- **`hms.Api`**
  Hosts the ASP.NET Core Web API, controllers, Swagger setup, middleware, filters, and application bootstrap code.
- **`hms.Application`**
  Contains DTOs, service contracts, repository abstractions, validation, mapping configuration, and business-focused application services.
- **`hms.Domain`**
  Contains domain entities and identity models that represent the core business rules and data structures.
- **`hms.Infrastructure`**
  Contains EF Core persistence, repository implementations, database configurations, migrations, and seed logic.

---

## Features

- :memo: User registration
- :lock: User authentication with JWT
- :shield: Role-based authorization
- :hotel: Hotel management
- :bed: Room management
- :calendar: Reservation system
- :busts_in_silhouette: Guest management
- :closed_lock_with_key: Secure password hashing via ASP.NET Identity
- :rotating_light: Global error handling middleware
- :blue_book: Swagger / OpenAPI documentation
- :card_file_box: EF Core migrations and development seeding

---

## NuGet Packages

Below are the main NuGet packages used in the solution and their responsibilities.

| Package                                             | Purpose                                                                    |
| --------------------------------------------------- | -------------------------------------------------------------------------- |
| `Microsoft.EntityFrameworkCore`                     | Core ORM used for data access and entity tracking.                         |
| `Microsoft.EntityFrameworkCore.SqlServer`           | SQL Server provider for Entity Framework Core.                             |
| `Microsoft.EntityFrameworkCore.Design`              | Design-time support for EF Core tooling and migrations.                    |
| `Microsoft.EntityFrameworkCore.Tools`               | Command-line tooling support for managing migrations and database updates. |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Integrates ASP.NET Identity with EF Core-backed stores.                    |
| `Microsoft.AspNetCore.Authentication.JwtBearer`     | Enables JWT bearer token authentication for secured API endpoints.         |
| `System.IdentityModel.Tokens.Jwt`                   | Provides JWT token creation and token handling support.                    |
| `Mapster`                                           | Maps entities to DTOs and request/response models.                         |
| `Mapster.DependencyInjection`                       | Integrates Mapster with the dependency injection container.                |
| `FluentValidation`                                  | Defines validation rules for application requests and models.              |
| `FluentValidation.AspNetCore`                       | Adds FluentValidation integration to ASP.NET Core.                         |
| `Swashbuckle.AspNetCore`                            | Generates Swagger / OpenAPI documentation and UI.                          |

---

## API Documentation

The API uses **Swagger / OpenAPI** for interactive documentation and endpoint testing.

When the application is running in the `Development` environment, Swagger UI is available at:

```text
http://localhost:5009/swagger
https://localhost:8000/swagger
```

Swagger can be used to:

- Inspect available endpoints
- Test requests directly from the browser
- Authorize with a JWT bearer token
- Review request and response contracts

---

## Getting Started

### Prerequisites

Before running the project locally, make sure you have:

- **.NET 10 SDK**
- **SQL Server**
- **Entity Framework Core CLI tools**

If `dotnet ef` is not installed, run:

```bash
dotnet tool install --global dotnet-ef
```

### Run Locally

1. Clone the repository:

```bash
git clone <repository-url>
```

2. Navigate to the project folder:

```bash
cd Hotel-Management-System
```

3. Restore dependencies:

```bash
dotnet restore hms.slnx
```

4. Apply database migrations:

```bash
dotnet ef database update --project hms.Infrastructure --startup-project hms.Api
```

5. Run the application:

```bash
dotnet run --project hms.Api
```

### Notes

- In `Development`, the API also applies pending migrations and seeds development data on startup.
- The default development URLs are configured in `hms.Api/Properties/launchSettings.json`.

---

## Environment Configuration

Application settings are stored in:

```text
hms.Api/appsettings.json
```

The most important configuration values are:

- **Connection String**
- **JWT Secret Key**
- **JWT Issuer**
- **JWT Audience**
- **Token Expiration**

Example configuration:

```json
{
  "JwtOptions": {
    "Secret": "your-super-secure-secret-key",
    "Issuer": "HMS_API",
    "Audience": "HMS_CLIENT",
    "ExpiryMinutes": 60
  },
  "ConnectionStrings": {
    "HmsDb": "Server=YOUR_SERVER;Database=HotelsManagementSystem;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

For production environments, sensitive values should be stored outside source control by using:

- Environment variables
- Secret managers
- `.NET User Secrets`

---

## Authentication

Authentication is implemented using:

- **JWT tokens**
- **ASP.NET Core Identity**

### High-Level Login Flow

1. A user registers or is created with an application role such as `Admin`, `Manager`, or `Guest`.
2. The user submits credentials to the login endpoint.
3. ASP.NET Identity validates the credentials and user data.
4. A signed JWT token is generated and returned to the client.
5. The client sends the token in the `Authorization` header for protected endpoints.

Example authorization header:

```http
Authorization: Bearer <your-jwt-token>
```

The API currently supports role-based access for key operations such as hotel management, room management, manager assignment, guest operations, and reservation workflows.

---

## Database

The project uses:

- **Entity Framework Core**
- **Code First approach**
- **Migrations**
- **SQL Server**

### Database Strategy

- The database is managed through EF Core migrations.
- Migrations are stored in `hms.Infrastructure/Migrations`.
- `HmsDbContext` is configured in the infrastructure layer.
- Development startup includes migration execution and sample data seeding.

This approach keeps the schema versioned alongside the application code and makes local setup more consistent across environments.

---

## Development Practices

This project is built around modern backend engineering practices, including:

- **Clean Architecture**
- **SOLID principles**
- **Dependency Injection**
- **Separation of Concerns**
- **Repository Pattern**
- **Validation with FluentValidation**
- **Object mapping with Mapster**
- **Centralized error handling**

These practices help keep the codebase modular, testable, and easier to extend as the system grows.

---

## Future Improvements

Possible future enhancements for the project include:

- Unit testing
- Integration testing
- Docker support
- CI/CD pipelines
- Structured logging system
- Caching
- Rate limiting

---

## Author

**Nika Ebralidze**  
Full Stack Developer
