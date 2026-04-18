# FamLedger Backend

ASP.NET Core Web API for FamLedger. The backend provides authentication, family management, and income APIs with JWT-based authorization.

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- AutoMapper
- JWT Bearer Authentication

## Project Structure

- `FamLedger.Api` - API host, controllers, DI, auth, swagger
- `FamLedger.Application` - business logic, DTOs, services, interfaces
- `FamLedger.Domain` - entities and enums
- `FamLedger.Infrastructure` - EF DbContext, repositories, migrations

## Prerequisites

- .NET SDK 8.x
- PostgreSQL running locally

## Configuration

Main config file: `FamLedger.Api/appsettings.json` (gitignored — never commit real secrets).

A non-secret template is provided at `FamLedger.Api/appsettings.Example.json`. First-time setup:

```bash
cp FamLedger.Api/appsettings.Example.json FamLedger.Api/appsettings.json
```

Then edit `appsettings.json` and fill in the real values. Important keys:

- `ConnectionStrings:DefaultConnection` — local Postgres connection string
- `JWT:Key` — generate with `openssl rand -base64 64 | tr -d '\n'` (must be ≥ 32 chars)
- `JWT:Issuer`, `JWT:Audience`, `JWT:ExpireMinutes`
- `Cors:AllowedOrigins`

### Alternative: User Secrets (recommended for dev)

Instead of putting secrets in `appsettings.json`, use .NET User Secrets (stored outside the repo):

```bash
cd FamLedger.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=FamLedgerDb;Username=postgres;Password=YOUR_PASSWORD;"
dotnet user-secrets set "JWT:Key" "YOUR_GENERATED_KEY"
```

### EF Core CLI (migrations)

The design-time factory reads the same connection string. You can also override via environment variable:

```bash
export FAMLEDGER_DB_CONNECTION="Host=localhost;Port=5432;Database=FamLedgerDb;Username=postgres;Password=YOUR_PASSWORD;"
```

## Run Locally

From repo root:

```bash
dotnet restore
dotnet build FamLedger.Api.sln
dotnet run --project FamLedger.Api
```

Swagger is available in development environment.

## Income Module Notes

- Authenticated APIs use JWT claims via `IUserContext`.
- Income create flow uses trusted user context from token (does not trust client-sent identity for authorization decisions).
- Income list response is ordered by `UpdatedOn DESC`, then `CreatedOn DESC`.
- Add income returns `201 Created` using named route resolution (`CreatedAtRoute`) for income detail lookup.

## Current Important Income Endpoints

- `GET /api/families/{familyId}/incomes`
- `POST /api/income`
- `GET /api/families/{familyId}/incomes/{incomeId}/{type}`
- `GET /api/income/categories`

