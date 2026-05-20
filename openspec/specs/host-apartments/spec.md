# host-apartments Specification

## Purpose
TBD - created by archiving change add-apartment-booking. Update Purpose after archive.
## Requirements
### Requirement: Host creates apartment

The system SHALL allow a user with the Host role to create an apartment via `POST /host/apartments` with body `name` and `description`.

#### Scenario: Successful create

- **WHEN** authenticated Host posts valid `name` and `description`
- **THEN** the system returns 201 Created with apartment `id`, `name`, `description`, and persists `hostId` equal to the Host's user identifier from the JWT

#### Scenario: Unauthenticated create

- **WHEN** client calls `POST /host/apartments` without a valid bearer token
- **THEN** the system returns 401 Unauthorized

#### Scenario: Client role denied

- **WHEN** authenticated user with only the Client role calls `POST /host/apartments`
- **THEN** the system returns 403 Forbidden

#### Scenario: Invalid create input

- **WHEN** Host omits `name` or sends empty `name`
- **THEN** the system returns 400 Bad Request

### Requirement: Host lists own apartments

The system SHALL expose `GET /host/apartments` for users with the Host role returning only apartments where `hostId` matches the caller.

#### Scenario: List my apartments as Host

- **WHEN** authenticated Host calls `GET /host/apartments`
- **THEN** the system returns 200 OK with apartments owned by that Host (each with `id`, `name`, `description`)

#### Scenario: No cross-host leakage

- **WHEN** Host A is authenticated and Host B owns apartments
- **THEN** Host A's response does not include Host B's apartments

#### Scenario: Empty list for new Host

- **WHEN** authenticated Host has created no apartments
- **THEN** the system returns 200 OK with an empty array

#### Scenario: Client role denied on host list

- **WHEN** authenticated user with only the Client role calls `GET /host/apartments`
- **THEN** the system returns 403 Forbidden

#### Scenario: Unauthenticated host list

- **WHEN** client calls `GET /host/apartments` without a valid bearer token
- **THEN** the system returns 401 Unauthorized

### Requirement: Host cannot set owner in request

The system MUST NOT accept `hostId` in the create-apartment request body; ownership is derived only from the JWT.

#### Scenario: Host id from token only

- **WHEN** Host creates an apartment
- **THEN** the stored `hostId` matches the JWT NameIdentifier and ignores any `hostId` in the body if present

### Requirement: Migrated apartment external identity

Apartments created by company data migration SHALL persist `SourceCompanyId` and `ExternalId` from the export. Apartments created via `POST /host/apartments` SHALL have null `SourceCompanyId` and null `ExternalId`.

#### Scenario: Migrated apartment external keys

- **WHEN** migration imports apartment `apt-101` under company `S`
- **THEN** the apartment row has `SourceCompanyId` `S` and `ExternalId` `apt-101`

#### Scenario: Host-created apartment has no external keys

- **WHEN** authenticated Host creates an apartment via API
- **THEN** `SourceCompanyId` and `ExternalId` are null

### Requirement: Migrated apartment host ownership

Migrated apartments SHALL use `hostId` equal to the migrated host's Identity user id resolved from the export host `externalId`, not from any value in the apartment JSON body.

#### Scenario: Ownership from export graph

- **WHEN** export places apartment `A` under host `H` and migration succeeds
- **THEN** apartment `A` has `hostId` matching migrated host `H` and `GET /host/apartments` for that host includes apartment `A` after login

