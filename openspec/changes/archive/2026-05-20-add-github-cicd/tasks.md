## 1. Container build assets

- [x] 1.1 Add root `Dockerfile` (multi-stage SDK → aspnet) publishing `BookingSystemAI.Api`
- [x] 1.2 Add `.dockerignore` excluding bin/obj, tests, UI, and git metadata
- [x] 1.3 Verify local `docker build` produces a runnable API image on port 8080

## 2. GitHub Actions workflow

- [x] 2.1 Create `.github/workflows/ci.yml` with triggers on `push` and `pull_request` to `main`
- [x] 2.2 Add `build-and-test` job: setup .NET 9, cache NuGet, `dotnet restore`, `dotnet build` Release
- [x] 2.3 Add test step: `dotnet test` with TRX logger to `./TestResults`
- [x] 2.4 Upload TRX artifacts and configure `dorny/test-reporter` (or equivalent) for GitHub Checks test summary
- [x] 2.5 Add `docker` job (needs tests): login to Docker Hub, build-push API image with `latest` + `github.sha` tags on `main` only

## 3. Secrets and documentation

- [x] 3.1 Document required GitHub secrets: `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN` (access token)
- [x] 3.2 Set workflow `env` for image name (`bookingsystemai-api`) aligned with Docker Hub repo
- [x] 3.3 Add short CI section to project README (or `docs/ci.md`): how to read test reports and pull the image

## 4. Verification and deliverables

- [x] 4.1 Initialize git remote / push to GitHub if not already done
- [x] 4.2 Configure Docker Hub repository and GitHub secrets; run workflow to green on `main`
- [x] 4.3 Capture screenshots: workflow run, test report tab, Docker Hub tags page → `openspec/changes/add-github-cicd/evidence/`
- [x] 4.4 Record `docker pull docker.io/<username>/bookingsystemai-api:latest` link in PR or change notes
