# ClaimsModule API

Backend for the Claims Module — a fullstack insurance claims management system. Built with .NET 9, Clean Architecture, CQRS, EF Core, and Hangfire.

---

## Live Application

| Resource | URL |
|---|---|
| **Swagger UI** | https://claims-module-api-ada7hxafdse6arc5.polandcentral-01.azurewebsites.net/swagger |

---

## Tech Stack

- .NET 9 / ASP.NET Core
- MediatR (CQRS)
- EF Core 9 + SQL Server
- FluentValidation
- Hangfire
- Azure Blob Storage
- Swagger / OpenAPI

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9)
- SQL Server 2022 or LocalDB
- [dotnet-ef CLI tool](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

---

## Project Structure

```
ClaimsModule/
├── ClaimsModule.Domain/          # Entities, enums, exceptions, domain events
├── ClaimsModule.Application/     # CQRS commands/queries, validators, interfaces
├── ClaimsModule.Infrastructure/  # Storage services, Hangfire jobs
├── ClaimsModule.Persistence/     # EF Core DbContext, migrations, seed data
└── ClaimsModule.API/             # Controllers, middleware, DI wiring
```

---

## Running Locally

### 1. Clone the repository

```bash
git clone https://github.com/drmitia/ClaimsModule-API.git
cd ClaimsModule-API
```

### 2. Configure the connection string

Open `ClaimsModule.API/appsettings.json` and set your local SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ClaimsModule;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Storage": {
    "Provider": "Local",
    "LocalBasePath": "uploads"
  }
}
```

For a named SQL Server instance replace the server value:
```
Server=YOUR_PC_NAME\\SQLEXPRESS
```

### 3. Install EF Core tools (if not already installed)

```bash
dotnet tool install --global dotnet-ef
```

### 4. Run database migrations

```bash
dotnet ef database update \
  --project ClaimsModule.Persistence/ClaimsModule.Persistence.csproj \
  --startup-project ClaimsModule.API/ClaimsModule.API.csproj
```

This creates the `ClaimsModule` database with all tables and seeds:
- 10 cause of loss codes
- 5 sample policies
- Database sequence for claim number generation

### 5. Run the API

```bash
cd ClaimsModule.API
dotnet run
```

Or open the solution in Rider / Visual Studio and run `ClaimsModule.API`.

The API starts at `https://localhost:7222` (port may vary — check terminal output).

### 6. Open Swagger

```
https://localhost:7222/swagger
```

Click **Authorize** and enter one of: `handler`, `supervisor`, or `manager` to set the mock user role.

### 7. (Optional) Load test data

Run `seed_test_data.sql` against your local database in SSMS to populate 5 realistic test claims.

---

## Mock Authentication

The API uses mock authentication via the `X-User` request header. No real JWT tokens are required.

| Header Value | Role | Authority |
|---|---|---|
| `handler` | Handler | Create claims, submit reserves |
| `supervisor` | Supervisor | Approve/reject reserves up to $100,000 |
| `manager` | Manager | Approve/reject reserves of any amount |

In Swagger click **Authorize** and enter the header value. In a real HTTP client add the header:
```
X-User: supervisor
```

---

## Configuration Reference

### appsettings.json

| Key | Description | Example |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | `Server=(localdb)\\MSSQLLocalDB;...` |
| `Storage:Provider` | Storage provider (`Local` or `Azure`) | `Local` |
| `Storage:LocalBasePath` | Local uploads folder path | `uploads` |
| `Storage:AzureConnectionString` | Azure Blob Storage connection string | `DefaultEndpointsProtocol=https;...` |
| `Storage:AzureContainerName` | Azure Blob container name | `claim-documents` |

### Environment variables (Azure App Service)

| Name | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Azure SQL connection string |
| `Storage__Provider` | `Azure` |
| `Storage__AzureConnectionString` | Storage account connection string |
| `Storage__AzureContainerName` | `claim-documents` |

---

## Running Migrations

Apply all pending migrations:
```bash
dotnet ef database update \
  --project ClaimsModule.Persistence/ClaimsModule.Persistence.csproj \
  --startup-project ClaimsModule.API/ClaimsModule.API.csproj
```

Add a new migration:
```bash
dotnet ef migrations add MigrationName \
  --project ClaimsModule.Persistence/ClaimsModule.Persistence.csproj \
  --startup-project ClaimsModule.API/ClaimsModule.API.csproj
```

---

## Azure Deployment

Deployed via GitHub Actions — see `.github/workflows/deploy-api.yml`.

To trigger manually: **Actions** → **Deploy API to Azure** → **Run workflow**

The pipeline:
1. Builds the solution
2. Runs EF Core migrations against Azure SQL
3. Publishes and deploys to Azure App Service

---

## Key Features

- **FNOL** — atomic claim number generation (`CLM-YYYY-0000001`), loss event, initial reserve
- **State machine** — 7 statuses, 11 valid transitions, closure pre-flight checks
- **Reserve workflow** — 3-tier authority, self-approval prevention, $10M aggregate cap
- **Hangfire jobs** — GL posting simulation, SLA monitoring every 15 minutes
- **Audit log** — immutable, append-only, 15+ event types with CorrelationId
- **Storage** — local filesystem or Azure Blob, switchable via config