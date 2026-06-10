# FamLedger — Backend API

> A shared finance workspace for families: track income, expenses, and debts/EMIs together, with role-based family workspaces and a single source of truth for the household.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-EF%20Core-336791)
![Architecture](https://img.shields.io/badge/Clean-Architecture-2ea44f)
![Status](https://img.shields.io/badge/status-beta-f59e0b)

🔗 **Live app:** https://famledger-frontend.vercel.app · **API:** https://famledger-api.onrender.com
🖥️ **Frontend repo:** [Famledger-frontend](https://github.com/Vikyy98/Famledger-frontend)

> ⚠️ **Early beta.** Please don't store real bank credentials or highly sensitive financial data yet.

---

## Why FamLedger

Most expense trackers are built for one person — but a household's money is shared. FamLedger gives
a whole family one workspace: shared income, expenses, and debts, with a dashboard that shows where
the money actually goes, instead of finances scattered across separate apps and "did you pay that?" texts.

## Features

- **JWT authentication** — register, log in; passwords hashed with ASP.NET Core `PasswordHasher`.
- **Family workspaces** — create a family, invite members with single-use hashed invite codes, manage roles (Admin / Member) with a "can't remove the last admin" guard.
- **Income** — one-time and recurring, with categories.
- **Expenses** — one-time and recurring, with categories.
- **Debts & EMIs** — track loans, auto-generate a linked recurring EMI expense, compute next-EMI dates, upcoming-EMI windows, and category breakdowns.
- **Security hardening** — uniform login responses (no account enumeration), disabled-account block, and per-IP rate limiting on auth endpoints.

## Tech Stack

.NET 8 Web API · Entity Framework Core · PostgreSQL · AutoMapper · JWT Bearer auth · Swagger · Docker

## Architecture (Clean Architecture)

```
FamLedger.Api            API host — controllers, DI, auth, Swagger, rate limiting
FamLedger.Application    Business logic — services, DTOs, interfaces, AutoMapper profiles
FamLedger.Domain         Entities and enums (no external dependencies)
FamLedger.Infrastructure EF Core DbContext, repositories, migrations
```

Dependencies point inward: `Domain` knows nothing of the outside; `Application` defines interfaces;
`Infrastructure` implements them; `Api` wires everything via DI.

## Getting Started

### Prerequisites
- .NET SDK 8.x
- PostgreSQL running locally

### Configuration
The runtime config (`FamLedger.Api/appsettings.json`) is gitignored — never commit secrets. Copy the template:

```bash
cp FamLedger.Api/appsettings.Example.json FamLedger.Api/appsettings.json
```

Then fill in:
- `ConnectionStrings:DefaultConnection` — local Postgres connection string
- `JWT:Key` — generate with `openssl rand -base64 64 | tr -d '\n'` (≥ 32 chars), plus `Issuer`, `Audience`, `ExpireMinutes`
- `Cors:AllowedOrigins` — include your frontend origin(s)

**Recommended for dev — .NET User Secrets** (kept out of the repo):

```bash
cd FamLedger.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=FamLedgerDb;Username=postgres;Password=YOUR_PASSWORD;"
dotnet user-secrets set "JWT:Key" "YOUR_GENERATED_KEY"
```

### Database migrations

```bash
dotnet ef database update --project FamLedger.Infrastructure --startup-project FamLedger.Api
```

The design-time factory reads the same connection string (override with `FAMLEDGER_DB_CONNECTION` if needed).

### Run

```bash
dotnet restore
dotnet build FamLedger.Api.sln
dotnet run --project FamLedger.Api
```

Swagger UI is available in the Development environment. A public `GET /health` liveness probe is always on.

## API Overview

| Area | Endpoints |
|------|-----------|
| Auth | `POST /api/auth/token` · `POST /api/users` (register) |
| Families | `POST /api/families` · `GET /api/families/{id}` · `POST /api/families/{id}/invitation` · `GET/DELETE /api/families/{id}/members[...]` |
| Income | `GET /api/families/{familyId}/incomes` · `POST /api/income` · `GET /api/income/categories` |
| Expenses | `GET /api/families/{familyId}/expenses` · `POST /api/expenses` · `GET /api/expenses/categories` |
| Debts | `GET /api/families/{familyId}/debts` · `POST /api/debts` · `GET /api/debts/categories` |

Authenticated endpoints derive the caller's identity and family from JWT claims (`IUserContext`) — client-sent
identity is never trusted for authorization. Family-scoped operations verify the token's family matches the route.

## Deployment

Containerized via the multi-stage [`Dockerfile`](Dockerfile) and deployed on **Render** (see [`render.yaml`](render.yaml)).
The platform terminates TLS at the edge; the container binds to `$PORT`. Secrets are provided as Render
environment variables (never committed).

## Roadmap

Budgets · AI-assisted categorization · automatic expense capture from SMS · net-worth/assets · data export.

---

Built by [@Vikyy98](https://github.com/Vikyy98) as a learning + real-product project.
