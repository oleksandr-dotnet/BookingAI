# jwt-bearer Specification

## Purpose
TBD - created by archiving change add-jwt-identity-auth. Update Purpose after archive.
## Requirements
### Requirement: JWT access token issuance

The system SHALL issue a signed JWT access token on successful login containing at minimum: subject (`sub` or name identifier), email claim, unique token id (`jti`), issued-at and expiration times.

#### Scenario: Token contains required claims

- **WHEN** user logs in successfully
- **THEN** the returned JWT is valid for the configured issuer and audience and expires after the configured lifetime (default 60 minutes unless configured otherwise)

### Requirement: JWT bearer authentication

The system SHALL validate JWT bearer tokens on protected endpoints using the configured signing key, issuer, and audience.

#### Scenario: Valid bearer token

- **WHEN** client sends `Authorization: Bearer <valid-token>` to a protected endpoint
- **THEN** the request is authenticated and the endpoint handler runs

#### Scenario: Missing authorization header

- **WHEN** client calls a protected endpoint without an `Authorization` header
- **THEN** the system returns 401 Unauthorized

#### Scenario: Invalid or expired token

- **WHEN** client sends a malformed, tampered, or expired JWT
- **THEN** the system returns 401 Unauthorized

### Requirement: JWT configuration

The system SHALL read JWT settings (issuer, audience, signing key, expiration minutes) from configuration (`Jwt` section in appsettings).

#### Scenario: Development configuration

- **WHEN** running in Development
- **THEN** a development signing key is present in configuration (User Secrets or appsettings.Development.json) and MUST NOT be committed as a production secret

### Requirement: Role claims in JWT

The system SHALL include the user's Identity role names as role claims in the JWT issued on successful login.

#### Scenario: Host token contains Host role

- **WHEN** a user with the Host role logs in successfully
- **THEN** the JWT contains a role claim with value `Host` usable by authorization policies

#### Scenario: Client token contains Client role

- **WHEN** a user with the Client role logs in successfully
- **THEN** the JWT contains a role claim with value `Client` usable by authorization policies

