# host-apartments Specification

## Purpose
Host-owned apartment management with listing economics, optional JSONB metadata, optimistic concurrency via `version`, and SQL upsert for integrations.
## Requirements
### Requirement: Apartment version on host responses

The system SHALL include integer `version` on each apartment returned from `POST /host/apartments` (201 body) and `GET /host/apartments`. New apartments SHALL start at `version` 1.

#### Scenario: Create returns initial version

- **WHEN** authenticated Host successfully creates an apartment
- **THEN** the response body includes `version` equal to 1

#### Scenario: List includes version

- **WHEN** authenticated Host calls `GET /host/apartments`
- **THEN** each apartment in the array includes `version`

### Requirement: Host updates own apartment

The system SHALL allow a user with the Host role to update an apartment they own via `PUT /host/apartments/{id}` with body `name`, `description`, `pricePerNight`, `guestCount`, `amenities`, optional `metadata`, and required integer `version` matching the current stored value.

#### Scenario: Successful update

- **WHEN** authenticated Host sends valid fields and `version` equal to the apartment's current version
- **THEN** the system returns 200 OK with updated apartment fields and `version` incremented by 1

#### Scenario: Stale version on update

- **WHEN** authenticated Host sends `version` less than the current stored version
- **THEN** the system returns 409 Conflict and does not change apartment fields

#### Scenario: Update non-owned apartment

- **WHEN** authenticated Host A calls `PUT /host/apartments/{id}` for an apartment owned by Host B
- **THEN** the system returns 404 Not Found

#### Scenario: Update missing apartment

- **WHEN** authenticated Host calls `PUT /host/apartments/{id}` for a non-existent id
- **THEN** the system returns 404 Not Found

#### Scenario: Unauthenticated update

- **WHEN** client calls `PUT /host/apartments/{id}` without a valid bearer token
- **THEN** the system returns 401 Unauthorized

#### Scenario: Client role denied on update

- **WHEN** authenticated user with only the Client role calls `PUT /host/apartments/{id}`
- **THEN** the system returns 403 Forbidden

#### Scenario: Invalid update input

- **WHEN** Host omits `version`, sends empty `name`, or violates listing economics validation
- **THEN** the system returns 400 Bad Request

### Requirement: Host creates apartment

The system SHALL allow a user with the Host role to create an apartment via `POST /host/apartments` with body `name`, `description`, `pricePerNight`, `guestCount`, `amenities`, and optional `metadata`.

#### Scenario: Successful create

- **WHEN** authenticated Host posts valid `name`, `description`, `pricePerNight`, `guestCount`, and `amenities`
- **THEN** the system returns 201 Created with apartment `id`, `name`, `description`, `pricePerNight`, `guestCount`, `amenities`, optional `metadata`, `version` 1, and persists `hostId` equal to the Host's user identifier from the JWT

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

The system SHALL expose `GET /host/apartments` for users with the Host role returning only apartments where `hostId` matches the caller, including `metadata` and `version` for each apartment.

#### Scenario: List my apartments as Host

- **WHEN** authenticated Host calls `GET /host/apartments`
- **THEN** the system returns 200 OK with apartments owned by that Host (each with `id`, `name`, `description`, `pricePerNight`, `guestCount`, `amenities`, `metadata`, and `version`)

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

### Requirement: Host documents presentation metadata

Host apartment create and update OpenAPI descriptions SHALL document optional metadata keys `propertyType`, `bedroomCount`, `bedCount`, `bathroomCount`, and `highlights` for Airbnb-style listing presentation on the public catalog.

#### Scenario: Host create with presentation metadata

- **WHEN** authenticated Host calls create with metadata containing valid presentation keys
- **THEN** the system returns 201 Created and the host response includes `metadata` with the submitted keys

#### Scenario: Host update merges presentation keys

- **WHEN** authenticated Host updates an apartment and supplies metadata with presentation keys
- **THEN** the system replaces metadata per existing JSONB replace semantics and public catalog reflects new projected fields after read

