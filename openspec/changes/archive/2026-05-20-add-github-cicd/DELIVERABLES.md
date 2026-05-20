# CI/CD deliverables (operator checklist)

Complete these steps after merging the workflow. Replace `<DOCKERHUB_USERNAME>` with your Docker Hub account.

## 1. GitHub repository

Remote: https://github.com/oleksandr-dotnet/BookingAI.git (branch `main`)

- [x] Pushed to https://github.com/oleksandr-dotnet/BookingAI (`main`)
- [x] Git authenticated as `oleksandr-dotnet` (Git Credential Manager)

## 2. Docker Hub

- [ ] Create repository: `bookingsystemai-api`
- [ ] Generate access token (read/write) at https://hub.docker.com/settings/security

## 3. GitHub Actions secrets

| Secret | Value |
|--------|--------|
| `DOCKERHUB_USERNAME` | Your Hub username |
| `DOCKERHUB_TOKEN` | Access token |

## 4. First green run

- [x] Push to `main` and **CI** workflow completed successfully
- [x] **build-and-test** and **docker** jobs green

## 5. Image pull link

```text
docker pull docker.io/oleksandr-dotnet/bookingsystemai-api:latest
```

Repository: https://github.com/oleksandr-dotnet/BookingAI

## 6. Screenshots

Optional: save under archived change `evidence/` if capturing for documentation.
