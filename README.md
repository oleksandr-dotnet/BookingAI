# BookingSystemAI

Booking system API (.NET 9, clean architecture, PostgreSQL, JWT).

## Local development

- Database: `podman compose up -d` (see `compose.yaml`)
- API: `dotnet run --project BookingSystemAI.Api/BookingSystemAI.Api.csproj --launch-profile http`
- OpenAPI (Development): `/scalar`

## CI/CD

See [docs/ci.md](docs/ci.md) for GitHub Actions, Docker Hub image, test reports, and required secrets.
