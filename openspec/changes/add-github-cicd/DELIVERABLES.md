# CI/CD deliverables (operator checklist)

Complete these steps after merging the workflow. Replace `<DOCKERHUB_USERNAME>` with your Docker Hub account.

## 1. GitHub repository

- [ ] Push this repository to GitHub (fix `git safe.directory` locally if needed).
- [ ] Ensure default branch is `main`.

## 2. Docker Hub

- [ ] Create repository: `bookingsystemai-api`
- [ ] Generate access token (read/write) at https://hub.docker.com/settings/security

## 3. GitHub Actions secrets

| Secret | Value |
|--------|--------|
| `DOCKERHUB_USERNAME` | Your Hub username |
| `DOCKERHUB_TOKEN` | Access token |

## 4. First green run

- [ ] Push to `main` and wait for **CI** workflow to complete
- [ ] Confirm **build-and-test** and **docker** jobs are green

## 5. Image pull link

```text
docker pull docker.io/<DOCKERHUB_USERNAME>/bookingsystemai-api:latest
```

Example: `docker pull docker.io/janedoe/bookingsystemai-api:latest`

## 6. Screenshots

Save under `openspec/changes/add-github-cicd/evidence/`:

- `workflow-run.png` — Actions tab, successful run
- `test-report.png` — .NET Tests check or job summary
- `dockerhub-tags.png` — Hub repo tags showing `latest` and commit SHA
