## Context

BookingSystemAI is a .NET 9 minimal API with a single unprotected `GET /weatherforecast` endpoint. The project has no database or auth packages yet. This change introduces the security foundation for future booking endpoints.

## Goals / Non-Goals

**Goals:**

- ASP.NET Core Identity for user storage and password hashing
- JWT bearer authentication for stateless API access
- `POST /auth/register`, `POST /auth/login`, protected `GET /weatherforecast`
- SQLite + EF Core for local development persistence
- OpenAPI bearer security scheme in Development

**Non-Goals:**

- Refresh tokens, OAuth/social login, email confirmation, password reset
- Roles/policies beyond requiring authentication
- Production key management (document User Secrets / env vars only)

## Decisions

### 1. Identity + EF Core SQLite

**Choice:** `ApplicationUser` extending `IdentityUser`, `ApplicationDbContext` extending `IdentityDbContext<ApplicationUser>`, SQLite connection string in `appsettings.Development.json`.

**Rationale:** Standard Microsoft stack, minimal setup for greenfield API, easy local dev without SQL Server.

**Alternatives:** In-memory store (rejected — data lost on restart); SQL Server (deferred until deployment target is defined).

### 2. JWT bearer alongside Identity

**Choice:** On login, `UserManager` validates credentials; a dedicated `IJwtTokenService` builds the JWT using `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationMinutes`. `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` validates tokens on protected routes.

**Rationale:** Identity handles passwords; JWT fits SPA/mobile clients and matches project conventions.

**Alternatives:** Cookie authentication (rejected for API-first); Identity bearer token package only (JWT is explicit requirement).

### 3. Minimal API endpoint grouping

**Choice:** Map auth routes in `Endpoints/AuthEndpoints.cs` (static extension `MapAuthEndpoints`), keep weather map in `Program.cs` with `.RequireAuthorization()`.

**Rationale:** Keeps `Program.cs` readable as auth grows; aligns with config guidance for layered growth without full MVC.

**Alternatives:** Controllers (deferred until endpoint count justifies it).

### 4. DTOs and responses

| Endpoint | Request | Success response |
|----------|---------|------------------|
| `POST /auth/register` | `{ "email", "password" }` | 201 + `{ "userId" }` |
| `POST /auth/login` | `{ "email", "password" }` | 200 + `{ "accessToken", "expiresIn" }` |
| `GET /weatherforecast` | Bearer token | 200 + forecast array (unchanged shape) |

Validation errors: 400 with `ValidationProblemDetails` or Identity error dictionary.

### 5. Database migration on startup (Development)

**Choice:** Apply pending EF migrations when `IsDevelopment()` at startup (or document `dotnet ef database update`).

**Rationale:** Frictionless first run for the user.

**Risk:** Auto-migrate in Production is unsafe — gated to Development only.

### 6. OpenAPI

**Choice:** Add document transformer or `AddOpenApi` security requirement for HTTP bearer scheme so Swagger UI can authorize requests in Development.

## Project structure (after implementation)

```
BookingSystemAI/
  Data/ApplicationDbContext.cs
  Models/ApplicationUser.cs
  Models/AuthDtos.cs
  Services/IJwtTokenService.cs
  Services/JwtTokenService.cs
  Endpoints/AuthEndpoints.cs
  Program.cs
  appsettings.json          # Jwt section (no secret key in base file)
  appsettings.Development.json  # Jwt:Key for dev
```

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Weak dev signing key committed to git | Put `Jwt:Key` only in Development config / User Secrets; document production env var |
| **BREAKING** weather clients without token | Document in proposal; update `BookingSystemAI.http` with login + bearer example |
| SQLite not suitable for production | Accept for this change; note SQL Server swap in future deployment change |
| Password rules frustrate dev testers | Use sensible Identity options; document minimum requirements in API errors |

## Migration Plan

1. Add NuGet packages and `Jwt` configuration.
2. Add DbContext, user model, EF migration.
3. Wire Identity + JWT in `Program.cs`.
4. Add auth endpoints; add `.RequireAuthorization()` to weather endpoint.
5. Verify with `dotnet build` and manual register → login → weather with bearer token.

**Rollback:** Revert commit; remove SQLite db file if created.

## Open Questions

- _(none blocking)_ — Token lifetime default 60 minutes unless user prefers shorter during `/opsx:apply`.
