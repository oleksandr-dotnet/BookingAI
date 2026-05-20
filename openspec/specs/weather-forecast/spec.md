# weather-forecast Specification

## Purpose
TBD - created by archiving change add-jwt-identity-auth. Update Purpose after archive.
## Requirements
### Requirement: Protected weather forecast

The system SHALL expose `GET /weatherforecast` that returns a JSON array of forecast entries, but ONLY to authenticated users.

#### Scenario: Authenticated access

- **WHEN** authenticated client calls `GET /weatherforecast` with a valid bearer token
- **THEN** the system returns 200 OK with an array of forecast objects (date, temperature Celsius, summary, temperature Fahrenheit)

#### Scenario: Unauthenticated access

- **WHEN** client calls `GET /weatherforecast` without a valid bearer token
- **THEN** the system returns 401 Unauthorized and does not return forecast data

### Requirement: Public auth endpoints

The system SHALL NOT require authentication for `POST /auth/register` and `POST /auth/login`.

#### Scenario: Register without token

- **WHEN** client calls `POST /auth/register` without authorization
- **THEN** the request is processed according to identity-auth requirements

#### Scenario: Login without token

- **WHEN** client calls `POST /auth/login` without authorization
- **THEN** the request is processed according to identity-auth requirements

