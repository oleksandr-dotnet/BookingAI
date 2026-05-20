## Why

The solution has unit and integration test projects but no automated verification or container publishing on push. A GitHub Actions pipeline is needed so every change is built, tested with visible reports in GitHub, and published as a Docker image to Docker Hub for deployment.

## What Changes

- Add GitHub Actions workflow(s) triggered on push/PR to `main` (and optionally tags).
- **Build** step: restore and build `BookingSystemAI.sln` on .NET 9.
- **Test** step: run `BookingSystemAI.Application.Tests` and `BookingSystemAI.IntegrationTests`; publish TRX + coverage/HTML so results appear in the GitHub Actions UI (Checks / Test Results summary).
- **Docker** step: add `Dockerfile` for `BookingSystemAI.Api` (multi-stage build); build image in CI.
- **Publish** step: push image to Docker Hub using repository secrets (`DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN`); tag with `latest` and commit SHA (and semver on release tags if configured).
- Document required GitHub secrets and expected deliverables (image URL, example workflow/test report screenshots in PR or change notes).

**Non-goals:** Kubernetes/Helm deploy, staging environments, signing/notary, multi-arch builds (unless trivial), UI (`booking-system-ui`) CI, database migration job in pipeline (runtime concern).

## Capabilities

### New Capabilities

- `github-cicd`: GitHub Actions pipeline — build, test with in-GitHub reports, Docker build, Docker Hub publish.

### Modified Capabilities

- _(none — no product API or domain requirement changes)_

## Impact

- **New files:** `.github/workflows/*.yml`, root `Dockerfile` (and optionally `.dockerignore`).
- **CI dependencies:** GitHub-hosted `ubuntu-latest`, .NET 9 SDK, Docker Buildx; integration tests use Testcontainers (Docker-in-Docker or service container strategy in design).
- **Secrets:** Docker Hub credentials in GitHub repository settings; no secrets committed to repo.
- **Deliverables for this change:** working pipeline, public/private image link on Docker Hub (`<username>/bookingsystemai-api:<tag>`), screenshots of green workflow run and test summary tab (captured after first successful run).

## Success criteria

- Push to default branch runs workflow to completion without manual steps (after secrets configured).
- Unit and integration test projects execute in CI; failed test fails the job; test report is viewable from the GitHub run (Summary / Tests).
- Docker image builds from API project and is pushed to the owner's Docker Hub repository.
- Documented image reference (e.g. `docker pull <user>/bookingsystemai-api:latest`) and evidence screenshots attached to implementation PR or `openspec/changes/add-github-cicd/` notes.
