## ADDED Requirements

### Requirement: Free PostgreSQL for test data

The deployment documentation SHALL describe provisioning a free PostgreSQL 16-compatible database (Neon free tier) and configuring the API with `ConnectionStrings__DefaultConnection` using SSL.

#### Scenario: Operator connects API to Neon

- **WHEN** the operator creates a Neon project and copies the pooled connection string into the API host environment as `ConnectionStrings__DefaultConnection`
- **THEN** the API process starts without connection configuration errors
- **AND** EF Core can reach the database over TLS

#### Scenario: Missing connection string

- **WHEN** the API starts without `ConnectionStrings__DefaultConnection`
- **THEN** startup fails with a clear configuration error (existing behavior)

### Requirement: API hosted as container on free tier

The system SHALL support deploying `BookingSystemAI.Api` via the repository root `Dockerfile` on a free container web service (Render) listening on port 8080.

#### Scenario: Health check without authentication

- **WHEN** an unauthenticated client sends `GET /health`
- **THEN** the API responds with HTTP 200 and a minimal healthy payload

#### Scenario: Platform deploy from GitHub

- **WHEN** the operator links the GitHub repository to the web service and pushes to the configured branch
- **THEN** the platform builds the Docker image and replaces the running service with the new version

### Requirement: Schema applied on deployed startup

When `ASPNETCORE_ENVIRONMENT` is `Staging`, the API SHALL apply EF Core migrations (and existing seed logic invoked from the migration path) before serving traffic.

#### Scenario: First deploy to empty database

- **WHEN** the API starts in `Staging` against an empty Neon database
- **THEN** migrations run successfully
- **AND** auth and booking endpoints are usable without manual `dotnet ef` commands

#### Scenario: Repeat deploy with pending migrations

- **WHEN** a new migration exists in the deployed build
- **THEN** startup applies pending migrations before accepting requests

### Requirement: CORS for deployed UI origin

The API SHALL allow browser calls from origins configured in `Cors:AllowedOrigins` when running in `Staging`, and SHALL continue to allow localhost origins only in `Development`.

#### Scenario: UI calls API from deployed static site

- **WHEN** the browser loads the UI from the configured static-site URL
- **AND** `Cors__AllowedOrigins` includes that exact origin (scheme + host, no path)
- **THEN** preflight and API requests from the UI succeed

#### Scenario: Wrong origin configured

- **WHEN** `Cors__AllowedOrigins` does not match the UI URL
- **THEN** the browser blocks cross-origin API calls (CORS failure)

### Requirement: React UI on free static hosting

The deployment documentation SHALL describe building `booking-system-ui` with `VITE_API_URL` set to the public API base URL and publishing the `dist` folder to a free static site (Render).

#### Scenario: UI uses deployed API

- **WHEN** the static site is built with `VITE_API_URL=https://<api-host>`
- **THEN** login and authenticated API calls from the UI target the deployed API

#### Scenario: Local UI unchanged

- **WHEN** a developer runs `npm run dev` with `.env` pointing to `http://localhost:5070`
- **THEN** local development behavior is unchanged

### Requirement: Secrets not in repository

Deployment instructions SHALL list required secrets (`Jwt__Key`, database connection string) and SHALL NOT commit real secret values to git.

#### Scenario: Repository scan

- **WHEN** the change is implemented
- **THEN** no production JWT key or Neon password appears in tracked files
- **AND** only placeholder examples exist in documentation

### Requirement: Operator documentation

The repository SHALL include `docs/deployment.md` with ordered steps: Neon → API on Render → UI on Render → CORS verification → smoke test (register, login, list apartments).

#### Scenario: New operator follows guide

- **WHEN** the operator follows `docs/deployment.md` without prior Render/Neon accounts
- **THEN** they obtain working HTTPS URLs for API and UI within one session
- **AND** total cost remains $0 on documented free tiers
