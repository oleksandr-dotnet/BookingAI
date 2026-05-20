# company-data-migration Specification

## Purpose
Import acquired-company JSON exports into BookingSystemAI via an atomic console migrator, preserving external identifiers and host–apartment relationships.

## Requirements
### Requirement: Company export JSON schema

The migrator SHALL accept a UTF-8 JSON file whose root contains `sourceCompanyId` (GUID), `hosts` (array), and `clients` (array). Each host entry SHALL include `externalId`, `email`, and `apartments` (array). Each apartment entry SHALL include `externalId`, `name`, and `description`. Each client entry SHALL include `externalId` and `email`.

#### Scenario: Valid minimal export

- **WHEN** migrator reads a file matching the schema with one host (one apartment) and one client
- **THEN** parsing succeeds and yields structured migration input without loading the entire file into a single string buffer

#### Scenario: Missing source company id

- **WHEN** root `sourceCompanyId` is missing or not a valid GUID
- **THEN** migrator exits with error and performs no database writes

#### Scenario: Invalid host apartment reference

- **WHEN** a host entry omits `externalId` or an apartment under that host omits required fields
- **THEN** migrator exits with error and performs no database writes

### Requirement: Atomic company migration

The system SHALL import all hosts, clients, and apartments from one export file inside a single database transaction. On any failure, no migrated users or apartments from that run SHALL remain committed.

#### Scenario: Successful full import

- **WHEN** migrator runs with valid export and no conflicting existing external ids
- **THEN** all hosts, clients, and apartments are persisted and the transaction commits

#### Scenario: Failure mid-import

- **WHEN** user creation fails for one host after earlier hosts were inserted in the same run
- **THEN** the transaction rolls back and the database has no partial data from that run

#### Scenario: Duplicate external id in same company

- **WHEN** a user or apartment with the same `sourceCompanyId` and `externalId` already exists
- **THEN** migrator aborts, rolls back, and reports a conflict error

### Requirement: Host and apartment reference preservation

The system SHALL assign each imported apartment a `hostId` equal to the Identity id of the host whose `externalId` matches the host entry containing that apartment in the export.

#### Scenario: Apartment owned by correct host

- **WHEN** export lists host `H1` with apartment `A1`
- **THEN** persisted apartment `A1` has `hostId` equal to the internal id of migrated host `H1`

#### Scenario: Multiple apartments per host

- **WHEN** one host entry lists multiple apartments
- **THEN** all apartments reference that host's internal id and distinct apartment `externalId` values

### Requirement: Migrated user provisioning

The migrator SHALL create Identity users for every host and client with email from export, password from CLI configuration (not from JSON), and role `Host` or `Client` respectively.

#### Scenario: Host can authenticate after migration

- **WHEN** migration completes for a host with email `host@acquired.test`
- **THEN** `POST /auth/login` with that email and the configured default password returns 200 OK with a JWT

#### Scenario: Client receives Client role

- **WHEN** migration imports a client entry
- **THEN** the user has only the Client role in Identity

### Requirement: External identity storage and lookup

The system SHALL persist `SourceCompanyId` and `ExternalId` on migrated users and apartments and SHALL support lookup of internal identifiers by `(SourceCompanyId, ExternalId)`.

#### Scenario: Unique external identity per company

- **WHEN** two migrated apartments share the same `SourceCompanyId` and `ExternalId`
- **THEN** the second insert is rejected by the database or migration logic

#### Scenario: Lookup apartment by external key

- **WHEN** application code queries by `sourceCompanyId` and apartment `externalId` from the export
- **THEN** the system returns the corresponding internal apartment id

#### Scenario: Lookup user by external key

- **WHEN** application code queries by `sourceCompanyId` and user `externalId` from the export
- **THEN** the system returns the corresponding Identity user id

#### Scenario: Organic users unaffected

- **WHEN** a user registers via `POST /auth/register` without migration
- **THEN** `SourceCompanyId` and `ExternalId` remain null and existing auth behavior is unchanged

### Requirement: Console migrator application

The solution SHALL include `BookingSystemAI.Migrator`, a console executable that accepts the export file path and default password, runs the migration once, and exits with code 0 on success or non-zero on failure.

#### Scenario: CLI success

- **WHEN** operator runs migrator with `--file <path>` and `--default-password <secret>` against a configured database
- **THEN** migration runs and process exits with code 0

#### Scenario: CLI missing password

- **WHEN** operator omits default password configuration
- **THEN** migrator prints usage or error and exits without database changes

### Requirement: Sample export fixture

The repository SHALL include a small sample JSON export file documenting the schema for development and automated tests.

#### Scenario: Sample file present

- **WHEN** a developer opens the migrator project sample data folder
- **THEN** a sample export file exists with at least one host (with apartment) and one client sharing one `sourceCompanyId`
