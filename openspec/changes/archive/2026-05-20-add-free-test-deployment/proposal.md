## Why

The app runs locally (Podman Postgres + `dotnet run` + Vite dev server) but there is no hosted environment for manual end-to-end testing without a laptop. A minimal, zero-cost deployment of the API, React UI, and PostgreSQL is needed so the owner can share URLs and test bookings/auth against a real database.

## What Changes

- Document and automate the **simplest free stack**: hosted PostgreSQL (Neon free tier) + API on Render free web service (Docker from repo) + UI on Render free static site (Vite build).
- Add deployment configuration artifacts: `render.yaml` (optional Blueprint), `docs/deployment.md` (step-by-step manual setup), and environment variable templates.
- **API runtime:** configurable CORS for the deployed UI origin; apply EF migrations on startup in a dedicated `Staging` (or production-deploy) environment so the remote DB schema stays current without a separate migrator job.
- **UI build:** bake `VITE_API_URL` at build time to point at the public API URL.
- Wire GitHub → Render auto-deploy on `main` (complements existing Docker Hub CI from `github-cicd`).

**Non-goals:** Custom domains/SSL beyond platform defaults, HA/scaling, paid tiers, Kubernetes, Migrator console as a hosted service, production hardening (WAF, secrets rotation automation), multi-region, or CI changes beyond optional deploy hook docs.

## Capabilities

### New Capabilities

- `free-test-deployment`: Hosted test environment — free Postgres, API container/service, static UI, env/secrets, CORS, migrations, and operator documentation.

### Modified Capabilities

- _(none — no booking/auth API contract changes)_

## Impact

- **New/updated files:** `render.yaml`, `docs/deployment.md`, possibly `.env.example` for UI; small `Program.cs` / CORS / migration guard changes in `BookingSystemAI.Api`.
- **External services:** Neon (database), Render (API + static site); GitHub repo connection on Render.
- **Secrets (platform dashboards only):** `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Cors__AllowedOrigins` (or equivalent), Render/UI build env `VITE_API_URL`.
- **Depends on:** existing root `Dockerfile` and GitHub Actions from `add-github-cicd` (image build optional if Render builds from Dockerfile in repo).

## Success criteria

- Operator can follow `docs/deployment.md` once and obtain three public URLs: API health/login works, UI loads and calls API with JWT flow, Postgres persists data across API restarts.
- Total recurring cost is **$0** on free tiers (accepting cold-start/sleep limits on Render free web service).
- No secrets committed to the repository; only documented variable names and examples.
