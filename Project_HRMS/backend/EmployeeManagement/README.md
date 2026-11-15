# Employee Management System

Production-oriented REST API for managing employee master data and employee records (header + family, address,
education and experience details). Built with ASP.NET Core (.NET 10), Clean Architecture and CQRS.

## Features

- CRUD for 7 master tables: departments, designations, employee types, relationships, countries, states, cities
- Employee create / update with multiple detail records, saved in one database transaction
- Update supports add / update / remove of detail rows in a single call (id present = update, no id = add, missing = remove)
- Employee listing with paging, sorting (whitelisted columns) and filtering
- Soft delete, activate / deactivate, optimistic concurrency (`rowVersion`), audit fields
- JWT authentication, role-based authorization (Admin, HR, Manager, Employee), login lockout and rate limiting
- Global exception handling, uniform `{ success, message, data, errors, traceId }` responses
- Serilog structured logging, health check (`/health`), Swagger / OpenAPI
- Docker + docker-compose, Azure App Service ready

## Technology stack

.NET 10 · C# 14 · EF Core 10 (SQL Server) · MediatR 12 · FluentValidation 11 · Serilog · Swashbuckle ·
xUnit · Moq · EF Core InMemory + `WebApplicationFactory` for integration tests

## Architecture

```
EmployeeManagement/
├── src/
│   ├── EmployeeManagement.API/             Controllers, middleware, auth/Swagger/CORS/rate-limit setup, Program.cs
│   ├── EmployeeManagement.Application/     Commands, queries, handlers, validators, DTOs, pipeline behaviors
│   ├── EmployeeManagement.Domain/          Entities, enums, constants (no dependencies)
│   └── EmployeeManagement.Infrastructure/  DbContext, EF configurations, seeding, JWT + password services
├── tests/EmployeeManagement.UnitTests/     Unit tests (xUnit + Moq) and API integration tests
├── database/                               Migration helper scripts
├── docker-compose.yml
└── EmployeeManagement.sln
```

Dependencies point inward: API → Application, Infrastructure; Infrastructure → Application; Application → Domain.
Handlers use `IAppDbContext` directly (no generic repository layer). Pipeline: logging → validation → handler.
The seven master tables share one generic accessor, so their CRUD behaviour is identical and defined once.

## Database

| Table | Notes |
|---|---|
| Departments, Designations, EmployeeTypes, Relationships, Countries | Unique `Name` (among non-deleted rows) |
| States (→ Countries), Cities (→ States) | Unique `Name` per parent |
| Employees | Unique `EmployeeCode` and `Email`; FKs to Department, Designation, EmployeeType; self-FK `ReportingManagerId` |
| EmployeeFamilies / EmployeeAddresses / EmployeeEducations / EmployeeExperiences | One-to-many from Employee (cascade); FKs to Relationship / Country / State / City (restrict) |
| Users | Login accounts (PBKDF2 password hash, lockout counters) |

Every table has `Id`, `IsActive`, `IsDeleted`, `CreatedAt/By`, `UpdatedAt/By`, `DeletedAt/By` and a `RowVersion`
concurrency token. A global query filter hides soft-deleted rows; unique indexes are filtered on `IsDeleted = 0`.

## Prerequisites

- .NET 10 SDK
- SQL Server (local, or `docker compose up sqlserver`)
- `dotnet tool install --global dotnet-ef`
- Docker (optional)

## Setup

```bash
# 1. start SQL Server (or point ConnectionStrings:DefaultConnection at your own instance)
docker compose up -d sqlserver

# 2. create the initial migration (once) and commit it
./database/add-migration.sh InitialCreate

# 3. run the API (Development applies migrations + seeds on startup)
dotnet run --project src/EmployeeManagement.API
```

Swagger UI: http://localhost:5080/swagger

### Migration commands

```bash
dotnet ef migrations add <Name> --project src/EmployeeManagement.Infrastructure --startup-project src/EmployeeManagement.API --output-dir Persistence/Migrations
dotnet ef database update      --project src/EmployeeManagement.Infrastructure --startup-project src/EmployeeManagement.API
dotnet ef migrations script --idempotent --project src/EmployeeManagement.Infrastructure --startup-project src/EmployeeManagement.API -o database/migrations.sql
```

`dotnet ef` uses a design-time factory; set `EMS_CONNECTION` to target a specific server.
If no migration exists yet, startup falls back to `EnsureCreated` (local development only).

### Seed data

On startup (`Database:SeedOnStartup`, default true) the API idempotently seeds departments, designations, employee
types, relationships, countries, states and cities, plus the first administrator when `Seed:AdminEmail` and
`Seed:AdminPassword` are configured and no admin exists.

## Configuration

Use environment variables in deployed environments (`:` becomes `__`).

| Setting | Env variable | Description |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | SQL Server connection string (required) |
| `Jwt:Key` | `Jwt__Key` | HMAC signing key, ≥ 32 chars (required; startup fails without it) |
| `Jwt:Issuer` / `Jwt:Audience` / `Jwt:ExpiryMinutes` | `Jwt__Issuer` … | Token settings |
| `Cors:AllowedOrigins` | `Cors__AllowedOrigins__0` … | Allowed browser origins |
| `Database:ApplyMigrationsOnStartup` | `Database__ApplyMigrationsOnStartup` | Apply migrations at startup (default false) |
| `Database:SeedOnStartup` | `Database__SeedOnStartup` | Seed master data/admin (default true) |
| `Seed:AdminEmail` / `Seed:AdminPassword` | `Seed__AdminEmail` … | First admin account |
| `Security:UseHttpsRedirection` | `Security__UseHttpsRedirection` | Disable behind TLS-terminating proxies |
| `Swagger:Enabled` | `Swagger__Enabled` | Expose Swagger outside Development |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | same | Set `true` behind App Service / reverse proxies |

Development defaults live in `appsettings.Development.json` (admin `admin@ems.local` / `Admin@12345`, dev-only JWT key).
**Never reuse these values outside local development.**

## API endpoints

All endpoints except `POST /api/auth/login` and `GET /health` require `Authorization: Bearer <token>`.

| Method | Route | Roles | Description |
|---|---|---|---|
| POST | `/api/auth/login` | anonymous | Get a JWT |
| POST | `/api/auth/register` | Admin | Create a user |
| GET | `/api/auth/me` | any | Current identity |
| GET | `/api/masters/{master}` | any | List (`search`, `parentId`, `isActive`, `page`, `pageSize`) |
| GET | `/api/masters/{master}/{id}` | any | Get one |
| POST | `/api/masters/{master}` | Admin, HR | Create |
| PUT | `/api/masters/{master}/{id}` | Admin, HR | Update (needs `rowVersion`) |
| PATCH | `/api/masters/{master}/{id}/status` | Admin, HR | Activate / deactivate |
| DELETE | `/api/masters/{master}/{id}` | Admin | Soft delete (409 if in use) |
| GET | `/api/employees` | Admin, HR, Manager | Paged list (`search`, `departmentId`, `designationId`, `employeeTypeId`, `reportingManagerId`, `gender`, `isActive`, `sortBy`, `sortDirection`, `page`, `pageSize`) |
| GET | `/api/employees/{id}` | Admin, HR, Manager | Employee with all details |
| POST | `/api/employees` | Admin, HR | Create with details (transactional) |
| PUT | `/api/employees/{id}` | Admin, HR | Update with add/update/remove of details (needs `rowVersion`) |
| PATCH | `/api/employees/{id}/status` | Admin, HR | Activate / deactivate |
| DELETE | `/api/employees/{id}` | Admin | Soft delete |
| GET | `/health` | anonymous | Liveness + database check |

`{master}` is one of `departments`, `designations`, `employee-types`, `relationships`, `countries`, `states`, `cities`.
For `states` send `parentId` = country id; for `cities` send `parentId` = state id.
`sortBy`: `employeeCode`, `firstName`, `lastName`, `email`, `joiningDate`, `department`, `designation`, `createdAt`.

Status codes: 400 validation, 401/403 auth, 404 not found, 409 duplicate / in use / concurrency, 429 rate limited, 500 unexpected.

## Authentication

```bash
curl -s -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@ems.local","password":"Admin@12345"}'
```

Use `data.accessToken` as `Authorization: Bearer <token>` (in Swagger: **Authorize**, paste the token).
Five failed logins lock the account for 15 minutes; the login endpoint is rate limited.

## Running and testing

```bash
dotnet build
dotnet test
```

Unit tests use xUnit, Moq and an in-memory EF provider. Integration tests start the real API pipeline
(`WebApplicationFactory`) against an in-memory database and cover auth, validation, the employee lifecycle and
error formats. They do not exercise SQL Server specifics (filtered unique indexes, rowversion, transactions);
run the Docker stack for that.

## Docker

```bash
docker compose up --build        # API on http://localhost:8080 (Swagger at /swagger), SQL Server on 1433
```

Override the dev defaults with a `.env` file (`SA_PASSWORD`, `JWT_KEY`, `ADMIN_EMAIL`, `ADMIN_PASSWORD`).
Standalone image: `docker build -f src/EmployeeManagement.API/Dockerfile -t employee-management-api .`

## Azure App Service

1. Create an Azure SQL database and an App Service (Linux, .NET 10) or deploy the container image.
2. App settings: `ConnectionStrings__DefaultConnection`, `Jwt__Key` (Key Vault reference recommended),
   `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`, `Cors__AllowedOrigins__0`, optionally `Seed__AdminEmail/Password` for first run.
3. Apply the schema with `./database/script-migrations.sh` (reviewed idempotent SQL) or temporarily set
   `Database__ApplyMigrationsOnStartup=true`.
4. Point the App Service health check at `/health`. Console logs flow to the App Service log stream.
5. CI: `.github/workflows/ci.yml` builds and tests on every push / PR.

## Security notes

- Passwords are hashed with PBKDF2 (ASP.NET Core Identity hasher); secrets come from configuration, never source control.
- Listing responses omit date of birth and mobile; full records require Admin, HR or Manager roles.
- Request payloads are never logged; error responses never expose stack traces.
- `Cache-Control: no-store` on API responses, security headers, HSTS outside Development.
- Enable Azure SQL transparent data encryption and TLS-only connections for data at rest and in transit.
