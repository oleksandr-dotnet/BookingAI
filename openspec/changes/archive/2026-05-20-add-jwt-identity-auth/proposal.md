## Why

The API currently exposes `/weatherforecast` without authentication. Before adding booking features, we need a standard way for clients to register, sign in, and call protected endpoints using JWT bearer tokens. ASP.NET Core Identity provides user management; JWT keeps the API stateless for future clients.

## What Changes

- Add ASP.NET Core Identity with a local user store (EF Core + SQLite for development).
- Issue JWT access tokens on successful login; validate JWT on protected routes.
- Add `POST /auth/register` and `POST /auth/login` endpoints.
- Protect `GET /weatherforecast` — require a valid bearer token; return 401 when missing or invalid.
- Add configuration for JWT issuer, audience, signing key, and token lifetime.
- Extend OpenAPI (Development) to document bearer security scheme.

**Non-goals (this change):** refresh tokens, external OAuth providers, role-based authorization beyond “authenticated user”, email confirmation, password reset, production secrets management.

## Capabilities

### New Capabilities

- `identity-auth`: User registration, login, password validation, and Identity persistence.
- `jwt-bearer`: JWT generation at login and bearer authentication middleware for protected endpoints.
- `weather-forecast`: Existing weather endpoint behavior, now with an authentication requirement.

### Modified Capabilities

- _(none — no prior specs in `openspec/specs/`)_

## Impact

- **API surface:** New routes `POST /auth/register`, `POST /auth/login`; `GET /weatherforecast` becomes 401 without token.
- **Dependencies:** `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.AspNetCore.Authentication.JwtBearer`, EF Core tools for initial migration.
- **Code:** `Program.cs`, new `Data/`, `Models/`, `Endpoints/` or grouped minimal API maps; `appsettings.json` JWT section.
- **Breaking:** **BREAKING** — clients calling `/weatherforecast` without a token will receive 401 Unauthorized.

## Success criteria

- User can register with email and password; duplicate email returns a clear error.
- User can login and receive a JWT; invalid credentials return 401.
- Valid JWT grants access to `/weatherforecast`; no/invalid token returns 401.
- Solution builds; auth flows are testable via `.http` file or Swagger with bearer token.
