## Context

BookingSystemAI ships a .NET 9 API (`Dockerfile` on port 8080), React Vite UI (`booking-system-ui/`), and PostgreSQL via EF Core. Local dev uses Podman Compose and Development-only CORS (`http://localhost:5173`). Migrations run automatically only in `Development` and `Testing`. GitHub Actions (`add-github-cicd`) builds/tests and can push a Docker image, but nothing hosts the stack for browser testing.

The owner wants the **simplest $0 setup** for personal QA—not production SLAs.

## Goals / Non-Goals

**Goals:**

- One written path (`docs/deployment.md`) to stand up API + UI + Postgres with free tiers and no credit card where possible.
- Prefer **two vendors, three clicks**: Neon (Postgres) + Render (API web service + static site)—both have generous free tiers and GitHub integration.
- API reachable over HTTPS; UI calls API via `VITE_API_URL`.
- Schema applied automatically on deploy without manual `dotnet ef` on a laptop.
- CORS allows only the deployed UI origin (plus localhost for local dev unchanged).

**Non-Goals:**

- High availability, autoscaling, custom domain, WAF, observability stacks, IaC beyond optional `render.yaml`.
- Hosting `BookingSystemAI.Migrator` or company-import jobs in the cloud.
- Replacing or duplicating full CI (tests stay in GitHub Actions).

## Decisions

### 1. Platform: Neon + Render (not all-in-one Railway/Fly)

**Choice:** **Neon** (free PostgreSQL 16, connection string with SSL) + **Render** (free Web Service for API from repo `Dockerfile`, free Static Site for UI).

**Rationale:**

- Neon’s free tier is stable for hobby Postgres; no container to operate.
- Render deploys from GitHub with minimal YAML; static sites are first-class for Vite `dist/`.
- Single-vendor alternatives (Railway credits, Fly.io + volume, Supabase) add billing/credit-card friction or more moving parts.

**Alternatives considered:**

| Option | Why not default |
|--------|------------------|
| Railway (all services) | Free credits expire; often requires card |
| Render Postgres | Free DB tier removed/limited; Neon is simpler for Postgres-only |
| Vercel (UI) + separate API | Two configs; API on Vercel needs adapterless Docker elsewhere |
| Docker Hub image only | Still need a host and DB; extra step vs Render build from repo |

### 2. API hosting: Render Web Service (Docker)

**Choice:** Render **Web Service**, environment `Staging`, `dockerfilePath: ./Dockerfile`, health check `GET /weatherforecast` (authorized) or a new anonymous `GET /health` endpoint.

**Rationale:** Reuses existing multi-stage `Dockerfile`; `ASPNETCORE_URLS` already binds `8080`.

**Env vars (Render dashboard):**

| Variable | Source |
|----------|--------|
| `ASPNETCORE_ENVIRONMENT` | `Staging` |
| `ConnectionStrings__DefaultConnection` | Neon pooled connection string (`?sslmode=require`) |
| `Jwt__Key` | Render secret (32+ char random) |
| `Jwt__Issuer` / `Jwt__Audience` | Defaults or explicit |
| `Cors__AllowedOrigins` | `https://<ui-service>.onrender.com` |

### 3. Database migrations on deploy

**Choice:** Run `MigrateDatabaseAsync()` when `ASPNETCORE_ENVIRONMENT` is `Staging` (same pattern as Development), including Identity seeders if already invoked from migration path.

**Rationale:** Simplest operator experience—push to `main`, Render redeploys, schema updates. Acceptable for a personal test environment.

**Alternative:** One-off Render Job or local `dotnet ef database update`—rejected as extra manual steps.

### 4. CORS configuration

**Choice:** Add configuration section `Cors:AllowedOrigins` (comma-separated). Register policy `Deployed` when not Development; use it in `Staging` instead of hard-coded localhost list.

**Rationale:** No code change per redeploy; set UI URL once in Render env.

Local Development keeps existing `Development` policy unchanged.

### 5. UI hosting: Render Static Site

**Choice:** Root directory `booking-system-ui`, build `npm ci && npm run build`, publish `dist`, env `VITE_API_URL=https://<api>.onrender.com`.

**Rationale:** Vite requires API URL at build time; static hosting is free and CDN-backed on Render.

### 6. Optional `render.yaml` Blueprint

**Choice:** Provide a **documented** `render.yaml` with two services (api, ui) and a comment that Postgres URL comes from Neon (not provisioned in YAML).

**Rationale:** One-click Blueprint for repeatability; Neon stays separate because it’s the simplest free Postgres signup.

### 7. HTTPS and JWT

**Choice:** Rely on Render TLS termination; disable HTTPS redirection middleware when environment is `Staging` (same as Testing) **or** keep redirection—Render terminates TLS at edge, container sees HTTP.

**Rationale:** Match existing `Testing` behavior if redirect causes loop; verify during apply.

### 8. Health endpoint

**Choice:** Add `GET /health` returning `200` without auth for Render health checks.

**Rationale:** `/weatherforecast` requires JWT and is unsuitable for platform probes.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Render free web service **spins down** after inactivity (~50s cold start) | Document expectation; acceptable for personal testing |
| Neon free tier **storage/branch limits** | Sufficient for test data; document reset via Neon console |
| Free tier **sleep** causes first UI login to timeout | Retry or wait; note in docs |
| `Jwt__Key` rotation invalidates tokens | Expected; re-login after rotate |
| CORS misconfiguration blocks browser calls | Checklist in `docs/deployment.md` with exact origin (no trailing slash) |
| Migrations on startup race during multi-instance | Free tier is single instance; N/A |

## Migration Plan

1. Create Neon project → copy `postgresql://...` connection string.
2. Generate `Jwt__Key` secret locally; store in password manager.
3. Connect GitHub repo on Render → create Web Service (Docker, branch `main`).
4. Set env vars; deploy API; confirm `GET /health` returns 200.
5. Create Static Site; set `VITE_API_URL`; deploy UI.
6. Update API `Cors__AllowedOrigins` with final UI URL; redeploy API.
7. Register test user via UI; smoke-test booking flow.

**Rollback:** Suspend/delete Render services; delete Neon branch. No production data commitment.

## Open Questions

- Whether to use environment name `Staging` vs `Production` on Render (proposal uses `Staging` to gate migrations + CORS without enabling Scalar in prod).
- Optional: trigger Render deploy from GitHub Actions `workflow_dispatch` (defer unless user wants push-button deploy).
