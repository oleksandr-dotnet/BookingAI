# github-cicd Specification

## Purpose

GitHub Actions CI/CD for BookingSystemAI: solution build, unit and integration tests with visible reports in GitHub, and Docker Hub publish of the API image.

## Requirements

### Requirement: CI builds the solution on every qualifying push

The repository SHALL include a GitHub Actions workflow that restores and builds `BookingSystemAI.sln` in Release configuration on pushes and pull requests to the default branch.

#### Scenario: Successful build on pull request

- **WHEN** a pull request targets `main`
- **THEN** the workflow runs a build job that completes with success if the solution compiles

#### Scenario: Failed build fails the workflow

- **WHEN** the solution fails to compile
- **THEN** the build job fails and subsequent publish steps do not run

### Requirement: CI runs unit and integration tests with GitHub-visible results

The workflow SHALL execute `BookingSystemAI.Application.Tests` and `BookingSystemAI.IntegrationTests`, emit TRX test results, and publish them so outcomes are visible in the GitHub Actions UI (job summary and/or Checks test report).

#### Scenario: All tests pass

- **WHEN** all unit and integration tests pass
- **THEN** the test job succeeds and passed tests are listed in the GitHub test report or job summary

#### Scenario: Test failure fails the pipeline

- **WHEN** any test fails
- **THEN** the test job fails and the Docker publish job does not run

#### Scenario: Test report artifact

- **WHEN** the test job completes
- **THEN** TRX files are uploaded as workflow artifacts or reported via a GitHub-integrated test reporter action

### Requirement: CI builds and publishes API Docker image to Docker Hub

On push to `main`, after tests pass, the workflow SHALL build a Docker image from the API `Dockerfile` and push it to Docker Hub using repository secrets, tagged with the commit SHA and `latest`.

#### Scenario: Image published on main push

- **WHEN** code is pushed to `main` and tests pass and Docker Hub secrets are configured
- **THEN** the image is pushed to `docker.io/<DOCKERHUB_USERNAME>/bookingsystemai-api` with tags including `latest` and the commit SHA

#### Scenario: Pull request does not publish

- **WHEN** a pull request is opened without publish credentials on the fork
- **THEN** build and test run but image push to Docker Hub is skipped or not executed

### Requirement: CI configuration is documented for operators

The change SHALL document required GitHub secrets (`DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN`), image pull command, and where to find workflow and test report screenshots after the first successful run.

#### Scenario: Operator can pull the published image

- **WHEN** the pipeline has succeeded on `main` with secrets configured
- **THEN** documentation provides `docker pull docker.io/<username>/bookingsystemai-api:latest` (or equivalent documented name)
