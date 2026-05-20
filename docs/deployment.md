# Free test deployment (Neon + Render + GitHub Actions)

Host the API, React UI, and PostgreSQL on **$0** free tiers. **Deploy from GitHub** with a manual workflow—no repeated clicks in Render after one-time setup.

| Component | Provider |
|-----------|----------|
| PostgreSQL 16 | [Neon](https://neon.tech) |
| API + UI | [Render](https://render.com) free tier |
| Deploy trigger | GitHub Actions → **Deploy test environment** (`workflow_dispatch`) |

Workflow file: [`.github/workflows/deploy-test.yml`](../.github/workflows/deploy-test.yml)

## Deploy from GitHub (normal path)

1. Open **Actions** → **Deploy test environment** → **Run workflow**.
2. Defaults are fine (sync env, deploy API + UI, wait for `/health`).
3. Watch the job; the summary lists what ran. Cold start can take several minutes.

The workflow will:

1. Push secrets from GitHub to Render env vars (API + UI).
2. Trigger Render deploys for API (Docker) and UI (static site).
3. Poll `GET {TEST_API_URL}/health` until healthy.

### Required GitHub secrets

**Settings → Secrets and variables → Actions → New repository secret**

| Secret | Example / notes |
|--------|-----------------|
| `RENDER_API_KEY` | From [Render Account → API Keys](https://dashboard.render.com/u/settings#api-keys) |
| `RENDER_API_SERVICE_ID` | Service ID (`srv-...`) from API service URL or `GET /v1/services` |
| `RENDER_UI_SERVICE_ID` | Static site service ID (`srv-...`) |
| `NEON_CONNECTION_STRING` | Neon pooled connection string (`SSL Mode=Require`) |
| `JWT_KEY` | Random 32+ characters (signing key; rotate invalidates tokens) |
| `TEST_API_URL` | `https://bookingsystemai-api.onrender.com` (no trailing slash) |
| `TEST_UI_URL` | `https://booking-system-ui.onrender.com` (exact CORS origin) |

Optional:

| Secret | Purpose |
|--------|---------|
| `ADMIN_EMAIL` | Seed admin user on API startup |
| `ADMIN_PASSWORD` | Admin password (Identity rules) |

### List Render service IDs

```bash
curl -sS -H "Authorization: Bearer $RENDER_API_KEY" \
  "https://api.render.com/v1/services?limit=20" | jq '.[].service | {name, id}'
```

## One-time infrastructure setup

Do this once per repository (or when recreating services).

### 1. PostgreSQL on Neon

1. Create a project at [neon.tech](https://neon.tech).
2. Copy the **pooled** connection string.
3. Save it as GitHub secret `NEON_CONNECTION_STRING`.

### 2. Render services

Create two services from this repo (or apply [`render.yaml`](../render.yaml) as a Blueprint):

**API — Web Service**

- Runtime: **Docker**, `Dockerfile` at repo root
- Plan: Free
- Health check: `/health`
- **Disable auto-deploy** (recommended) so only GitHub Actions deploys
- Branch: `main`

**UI — Static Site**

- Root: `booking-system-ui`
- Build: `npm ci && npm run build`
- Publish: `dist`
- **Disable auto-deploy** (recommended)

Note the public URLs and service IDs for GitHub secrets `TEST_API_URL`, `TEST_UI_URL`, `RENDER_API_SERVICE_ID`, `RENDER_UI_SERVICE_ID`.

Env vars on Render can stay empty initially; the workflow syncs them from GitHub on each deploy when **sync_environment** is enabled.

### 3. First deploy

1. Add all required GitHub secrets.
2. Run **Deploy test environment** on `main`.
3. Update [DEPLOYED_URLS.md](../openspec/changes/add-free-test-deployment/DEPLOYED_URLS.md) with your URLs.

### 4. Smoke test

- [ ] Workflow finishes; `/health` step passes
- [ ] Register / login from UI
- [ ] Re-run workflow; data still present in Neon

## Workflow inputs

| Input | Default | Meaning |
|-------|---------|---------|
| `sync_environment` | true | Push GitHub secrets to Render before deploy |
| `deploy_api` | true | Trigger API deploy |
| `deploy_ui` | true | Trigger UI deploy |
| `wait_for_health` | true | Poll API `/health` after API deploy |

Deploy UI only: turn off `deploy_api` and `wait_for_health`. Config-only sync: enable `sync_environment`, disable both deploy flags.

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| Missing secret error at start | Add all required secrets in GitHub |
| `401` from Render API | Regenerate `RENDER_API_KEY` |
| `404` on deploy | Wrong `RENDER_*_SERVICE_ID` |
| CORS errors in browser | `TEST_UI_URL` must match static site URL exactly |
| Health check timeout | Free tier cold start; re-run workflow or wait longer |
| Startup fails on DB | Check `NEON_CONNECTION_STRING` and SSL |

## Manual Render dashboard deploy

You can still use **Manual Deploy** on Render, but the intended path is the GitHub workflow so env vars and API/UI stay in sync.

## Optional Blueprint

[`render.yaml`](../render.yaml) documents service shape. Postgres remains on Neon; connection string flows via `NEON_CONNECTION_STRING` in GitHub.

## Secrets safety

Never commit `JWT_KEY`, Neon credentials, or `RENDER_API_KEY`. GitHub Actions secrets and Render env vars only.
