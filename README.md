# BookingSystemAI

Booking system API (.NET 9, clean architecture, PostgreSQL, JWT).

## Local development

- Database: `podman compose up -d` (see `compose.yaml`)
- API: `dotnet run --project BookingSystemAI.Api/BookingSystemAI.Api.csproj --launch-profile http`
- OpenAPI (Development): `/scalar`

## CI/CD

See [docs/ci.md](docs/ci.md) for GitHub Actions, Docker Hub image, test reports, and required secrets.

## Free test deployment

See [docs/deployment.md](docs/deployment.md) for hosting on Render + Neon ($0 tiers). Deploy via **Actions → Deploy test environment** (manual workflow).
