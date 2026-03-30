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

Main config file: `FamLedger.Api/appsettings.json`

Important keys:

- `ConnectionStrings:DefaultConnection`
- `JWT:Key`, `JWT:Issuer`, `JWT:Audience`, `JWT:ExpireMinutes`
- `Cors:AllowedOrigins`

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

