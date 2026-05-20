## ADDED Requirements

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
