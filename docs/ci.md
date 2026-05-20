# Continuous Integration

GitHub Actions workflow: [`.github/workflows/ci.yml`](../.github/workflows/ci.yml)

## Pipeline

| Job | When | Steps |
|-----|------|--------|
| `build-and-test` | Every push/PR to `main` | Restore, Release build, unit + integration tests, TRX upload, test report in Checks |
| `docker` | Push to `main` only (after tests pass) | Build API image, push to Docker Hub |

## GitHub secrets

Configure under **Settings → Secrets and variables → Actions**:

| Secret | Description |
|--------|-------------|
| `DOCKERHUB_USERNAME` | Docker Hub username |
| `DOCKERHUB_TOKEN` | Docker Hub [access token](https://docs.docker.com/security/for-developers/access-tokens/) (not account password) |

Create a Docker Hub repository named `bookingsystemai-api` (or change `IMAGE_NAME` in the workflow).

## Test reports in GitHub

1. Open the workflow run on the **Actions** tab.
2. Open the **build-and-test** job.
3. View the **.NET Tests** check (from `dorny/test-reporter`) for pass/fail per test.
4. Download **test-results** artifact for raw TRX files.

## Docker image

After a successful run on `main`:

```bash
docker pull docker.io/<DOCKERHUB_USERNAME>/bookingsystemai-api:latest
docker pull docker.io/<DOCKERHUB_USERNAME>/bookingsystemai-api:<commit-sha>
```

Run the container (provide PostgreSQL and JWT at runtime):

```bash
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=booking;Username=...;Password=..." \
  -e Jwt__Key="<at-least-32-char-signing-key>" \
  docker.io/<DOCKERHUB_USERNAME>/bookingsystemai-api:latest
```

Migrations run automatically only in `Development` and `Testing` environments. For production images, apply EF migrations separately (e.g. Migrator job) before or during deploy.

## Local verification

```powershell
docker build -t bookingsystemai-api:local .
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development ...
```

## Evidence (screenshots)

After the first green CI run, add screenshots to `openspec/changes/add-github-cicd/evidence/`:

- Actions workflow run (all steps green)
- Test report / Checks summary
- Docker Hub repository tags page

See `openspec/changes/add-github-cicd/DELIVERABLES.md` for the image pull link template.
