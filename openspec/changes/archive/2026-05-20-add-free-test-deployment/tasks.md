## 1. API runtime for hosted test environment



- [x] 1.1 Add `GET /health` endpoint (anonymous, HTTP 200) in `BookingSystemAI.Api`

- [x] 1.2 Add `Cors:AllowedOrigins` configuration and `Deployed` CORS policy; apply in `Staging`, keep `Development` localhost policy unchanged

- [x] 1.3 Run `MigrateDatabaseAsync()` when environment is `Staging` (mirror Development/Testing guard in `Program.cs`)

- [x] 1.4 Align HTTPS redirection behavior for `Staging` with hosted TLS (avoid redirect loops on Render)

- [x] 1.5 Add `appsettings.Staging.json` template (no secrets) documenting expected keys



## 2. Deployment artifacts



- [x] 2.1 Add optional `render.yaml` Blueprint (API web service + static site; comment that Neon supplies Postgres)

- [x] 2.2 Write `docs/deployment.md`: Neon signup, connection string, Render API service env vars, Render static site build with `VITE_API_URL`, CORS update, smoke test checklist

- [x] 2.3 Add `booking-system-ui/.env.example` with `VITE_API_URL` placeholder for deployed builds

- [x] 2.4 Link deployment guide from root `README.md`



## 3. Verification



- [x] 3.1 `dotnet build BookingSystemAI.sln -c Release`

- [x] 3.2 Operator: one-time Neon + Render services + GitHub secrets; run **Deploy test environment** workflow; confirm `/health`, register/login from UI, and data persists after re-run

- [x] 3.3 Capture deployed API and UI URLs in `openspec/changes/add-free-test-deployment/DEPLOYED_URLS.md` (placeholders until first deploy)

