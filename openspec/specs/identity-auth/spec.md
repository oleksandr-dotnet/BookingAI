# identity-auth Specification

## Purpose
TBD - created by archiving change add-jwt-identity-auth. Update Purpose after archive.
## Requirements
### Requirement: User registration

The system SHALL allow new users to register with an email and password via `POST /auth/register`.

#### Scenario: Successful registration

- **WHEN** client sends a valid email and password meeting Identity password rules
- **THEN** the system creates the user account and returns 201 Created with the user identifier (no password in response)

#### Scenario: Duplicate email

- **WHEN** client registers with an email already in use
- **THEN** the system returns 400 Bad Request with validation errors describing the conflict

#### Scenario: Invalid registration input

- **WHEN** client sends missing email, invalid email format, or password that fails Identity rules
- **THEN** the system returns 400 Bad Request with field-level validation errors

### Requirement: User login

The system SHALL authenticate registered users via `POST /auth/login` with email and password.

#### Scenario: Successful login

- **WHEN** client sends correct email and password for an existing user
- **THEN** the system returns 200 OK with a JWT access token and token metadata (expires in seconds)

#### Scenario: Invalid credentials

- **WHEN** client sends wrong password or unknown email
- **THEN** the system returns 401 Unauthorized without revealing whether the email exists

#### Scenario: Missing login fields

- **WHEN** client omits email or password
- **THEN** the system returns 400 Bad Request

### Requirement: User persistence

The system SHALL persist Identity users using Entity Framework Core with a database appropriate for development (SQLite).

#### Scenario: Application startup

- **WHEN** the application starts in Development
- **THEN** the database schema is applied (migrate or ensure created) so auth endpoints are usable without manual setup

### Requirement: Application roles

The system SHALL define Identity roles `Host` and `Client` and ensure both exist in the database before handling registration.

#### Scenario: Roles seeded at startup

- **WHEN** the application starts
- **THEN** roles `Host` and `Client` exist in the Identity role store

### Requirement: Role selection at registration

The system SHALL require `role` on `POST /auth/register` with value `Host` or `Client` and assign the new user to that single Identity role.

#### Scenario: Register as Host

- **WHEN** client sends valid email, password, and `"role": "Host"`
- **THEN** the system creates the user, assigns the Host role, and returns 201 Created with the user identifier

#### Scenario: Register as Client

- **WHEN** client sends valid email, password, and `"role": "Client"`
- **THEN** the system creates the user, assigns the Client role, and returns 201 Created with the user identifier

#### Scenario: Invalid role

- **WHEN** client sends a `role` value other than `Host` or `Client`, or omits `role`
- **THEN** the system returns 400 Bad Request with validation errors

### Requirement: Migrated user external identity

The system SHALL store optional `SourceCompanyId` (GUID) and `ExternalId` (string) on Identity users created by company data migration. Values SHALL be null for users created only through `POST /auth/register`.

#### Scenario: Migrated user has external keys

- **WHEN** company migration creates a host user with export `externalId` `host-001` for `sourceCompanyId` `S`
- **THEN** the persisted user has `SourceCompanyId` `S` and `ExternalId` `host-001`

#### Scenario: Registered user has no external keys

- **WHEN** client registers via `POST /auth/register`
- **THEN** the new user has null `SourceCompanyId` and null `ExternalId`

### Requirement: Migrated user default password

Users created by the company migrator SHALL be provisioned with the operator-supplied default password and SHALL be able to log in via `POST /auth/login` using that password until changed through a future password-management feature.

#### Scenario: Login with default password

- **WHEN** migration used default password `TempPass123!` for imported client `client@acquired.test`
- **THEN** login with that email and password succeeds

#### Scenario: Public register unchanged

- **WHEN** client registers via `POST /auth/register` with their chosen password
- **THEN** login works with the chosen password and not the migrator default

