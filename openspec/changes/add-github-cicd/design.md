## Context

BookingSystemAI is a .NET 9 solution with `BookingSystemAI.Application.Tests` (unit) and `BookingSystemAI.IntegrationTests` (API + Testcontainers PostgreSQL). There is no `Dockerfile` or `.github/workflows` yet. Local DB runs via `compose.yaml` (Postgres only). The user wants GitHub Actions: build, test with visible reports in GitHub, Docker image build, push to personal Docker Hub, plus screenshots and image link as proof.

## Goals / Non-Goals

**Goals:**

- Single primary workflow (e.g. `ci.yml`) on `push` and `pull_request` to `main`.
- Restore/build entire solution; fail fast on compile errors.
- Run both test projects; surface TRX results in GitHub (Actions job summary + optional `dorny/test-reporter` or built-in `actions/upload-artifact` + publish step).
- Build production-oriented API Docker image; push to Docker Hub with immutable (`sha`) and floating (`latest`) tags on `main`.
- Document required secrets and image naming convention.

**Non-Goals:**

- Deploy to cloud/K8s, blue-green, or environment-specific compose stacks in CI.
- CI for React UI or Migrator console as separate jobs (can be added later).
- Code coverage gates or SonarQube (optional follow-up).
- Multi-platform (`linux/arm64`) images unless needed later.

## Decisions

### 1. Workflow layout: one job vs matrix

**Choice:** Two jobs — `build-and-test` then `docker` (depends on tests passing).

**Rationale:** Keeps Docker publish off failed builds; allows Testcontainers/Docker only in test job while publish job reuses login.

**Alternative:** Single job — simpler but longer feedback loop on Docker failures mixed with test failures.

### 2. .NET SDK and build

**Choice:** `actions/setup-dotnet@v4` with `dotnet-version: '9.0.x'`; `dotnet restore` and `dotnet build BookingSystemAI.sln -c Release --no-restore`.

**Rationale:** Matches project TFM; Release matches container publish.

### 3. Test execution and GitHub-visible reports

**Choice:**

- `dotnet test BookingSystemAI.sln -c Release --no-build --logger "trx;LogFileName=test-results.trx" --results-directory ./TestResults`
- Upload TRX as artifact (`actions/upload-artifact@v4`, name `test-results`).
- `dorny/test-reporter@v1` with `reporter: dotnet-trx` on `**/test-results.trx` so failed/passed tests appear in the PR Checks "Tests" tab and job summary.

**Rationale:** Native TRX is standard for .NET; test-reporter integrates with GitHub UI without third-party SaaS.

**Alternative:** Publish HTML report only — viewable via artifact download but not embedded in Checks summary.

### 4. Integration tests + Testcontainers on GitHub

**Choice:** Run integration tests on `ubuntu-latest` with Docker available (default on GitHub-hosted runners). Ensure job has no restriction on Docker socket; Testcontainers pulls `postgres:16-alpine` at runtime.

**Rationale:** `BookingSystemAI.IntegrationTests` already uses `Testcontainers.PostgreSql`; no workflow change to test code required.

**Risk:** Flaky pulls or rate limits → pin image digest in test fixture later if needed.

### 5. Dockerfile location and shape

**Choice:** Root `Dockerfile` multi-stage:

1. `mcr.microsoft.com/dotnet/sdk:9.0` — restore/build/publish `BookingSystemAI.Api`
2. `mcr.microsoft.com/dotnet/aspnet:9.0` — runtime, `ENTRYPOINT` `dotnet BookingSystemAI.Api.dll`, expose 8080, `ASPNETCORE_URLS=http://+:8080`

Add `.dockerignore` excluding `**/bin`, `**/obj`, `.git`, `booking-system-ui`, test projects.

**Rationale:** Standard ASP.NET container pattern; API is the deployable unit.

### 6. Docker Hub publish

**Choice:** `docker/login-action@v3` with secrets `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN` (Docker Hub access token, not password in logs). `docker/build-push-action@v6` push tags:

- `docker.io/${{ secrets.DOCKERHUB_USERNAME }}/bookingsystemai-api:latest` (only on `main` push)
- `docker.io/${{ secrets.DOCKERHUB_USERNAME }}/bookingsystemai-api:${{ github.sha }}`

Image name is configurable via workflow `env` for user-specific Hub namespace.

**Rationale:** User asked for personal Docker Hub; SHA tag traceability; `latest` for simple pulls.

**Alternative:** GitHub Container Registry — not requested.

### 7. Triggers and secrets

**Choice:** `on: push: branches: [main]` and `pull_request: branches: [main]`. Docker push only on `push` to `main` (not PR forks without secrets).

**Rationale:** PRs get build+test; publish avoids leaking credentials on untrusted forks.

### 8. Deliverables (screenshots + link)

**Choice:** After first green run, capture:

- Actions run overview (all steps green)
- Test Reporter / Checks test summary
- Docker Hub repository page showing pushed tag

Store under `openspec/changes/add-github-cicd/evidence/` or attach to implementation PR description.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Missing Docker Hub secrets → publish fails | Document setup in workflow comment + tasks; skip push job if secrets absent only in fork PRs |
| Testcontainers slow on cold start | Accept; cache NuGet via `actions/cache` |
| Integration tests fail without Docker | Job runs on ubuntu-latest with Docker preinstalled |
| JWT/connection strings in container | Runtime env vars at deploy time; CI image build does not embed production secrets |
| User_info said repo may not be git yet | Workflow still valid once pushed to GitHub |

## Migration Plan

1. Add `Dockerfile` + `.dockerignore`.
2. Add `.github/workflows/ci.yml`.
3. Create Docker Hub repo `bookingsystemai-api` (or chosen name).
4. Add GitHub repository secrets.
5. Push to GitHub `main`; verify workflow.
6. Add evidence screenshots and `docker pull` link to PR/change notes.

**Rollback:** Disable workflow or revert workflow file; delete bad image tags on Hub manually.

## Open Questions

- Exact Docker Hub username / image repository name (workflow `env.IMAGE_NAME` — confirm during `/opsx:apply`).
- Whether to run workflow on all branches or only `main` (default: `main` + PRs targeting `main`).
