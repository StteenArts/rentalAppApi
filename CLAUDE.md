# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

REST API for a short-term rental platform (Airbnb-style) built with ASP.NET Core (.NET 10) + PostgreSQL. Layered monolith: `Controllers → Services → Data (EF Core) → PostgreSQL`. No frontend — designed for external consumption.

## Commands

### Docker (preferred)
```bash
cp .env.example .env
docker compose up --build
```
API at `http://localhost:5000`, Swagger at `http://localhost:5000/swagger`.

### Local development (without Docker)
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=RentalAppDb;Username=postgres;Password=postgres"
dotnet user-secrets set "Jwt:Key" "un-secreto-largo-de-desarrollo"
dotnet user-secrets set "Crypto:AesKey" "12345678901234567890123456789012"
dotnet run
```

### EF Core migrations
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```
Migrations run automatically on startup (`db.Database.Migrate()` in `Program.cs`) — no manual step needed after adding.

### Build / restore
```bash
dotnet build
dotnet restore
```

## Architecture

### Layer responsibilities
- **Controllers** — HTTP in/out, extract `userId` from JWT claims via `ClaimsPrincipalExtensions.GetUserId()`, delegate all logic to services.
- **Services** — all business logic. Throw `DomainException(statusCode, message)` for expected domain errors.
- **Data/AppDbContext** — EF Core DbContext; relationships and indexes defined in `OnModelCreating`.
- **Models/Dtos** — input shapes; validators in `Models/Dtos/Validators/` (FluentValidation, auto-registered from assembly).
- **Middleware/GlobalExceptionHandler** — catches `DomainException` → serializes as `ProblemDetails`. All error responses are `ProblemDetails`, never raw stack traces.

### Auth
JWT Bearer. All endpoints require authentication by default (fallback policy in `Program.cs`). Exceptions are explicitly marked `[AllowAnonymous]` (property listing/search/detail). Roles: `Guest`, `Owner`, `Admin`. Admin role is provisioned directly in the DB — not via registration.

The user id is **always read from the JWT claims**, never from request body/query params.

### Key business rules
- **No double-booking**: overlap check against non-cancelled reservations in `ReservationService`.
- **Fixed check-in/out times**: 14:00 check-in, 12:00 check-out applied server-side, stored as UTC `timestamptz`.
- **KYC required to reserve**: user must have at least one `KycValidation` with `Status == KycStatus.Approved`.
- **KYC documents encrypted**: AES via `CryptoService` before persisting; overwritten on delete.

### Simulated / stub services
- `KycService` — simulates OCR/AI verdict based on file extension/size; replace internals without touching `KycController`.
- `EmailService` — logs "send" to console; `NotificationService` persists in-app notifications to DB.
- `Payment` model exists but no payment gateway integration.
- Images stored to `wwwroot/uploads` (local disk), not S3.

### Configuration
Secrets never in code or `appsettings.json`. Supplied via:
- Docker: environment variables from `.env` (see `.env.example`)
- Local: `dotnet user-secrets`

Required secrets: `ConnectionStrings:DefaultConnection`, `Jwt:Key`, `Crypto:AesKey`.
Optional config in `appsettings.json`: `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiresMinutes`, `Cors:AllowedOrigins`.

### Service interfaces
Only `IReservationService` and `IWishlistService` are extracted to interfaces (in `Services/Interfaces/`). Other services (`NotificationService`, `EmailService`, `CryptoService`, `TokenService`, `KycService`) are registered and injected as concrete types.
