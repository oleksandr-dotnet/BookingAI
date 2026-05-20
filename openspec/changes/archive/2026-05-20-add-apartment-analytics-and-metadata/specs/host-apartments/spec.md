## MODIFIED Requirements

### Requirement: Host creates apartment

The system SHALL allow a user with the Host role to create an apartment via `POST /host/apartments` with body `name`, `description`, `pricePerNight`, `guestCount`, `amenities`, and optional `metadata`.

#### Scenario: Successful create

- **WHEN** authenticated Host posts valid `name`, `description`, `pricePerNight`, `guestCount`, and `amenities`
- **THEN** the system returns 201 Created with apartment `id`, `name`, `description`, `pricePerNight`, `guestCount`, `amenities`, optional `metadata`, and persists `hostId` equal to the Host's user identifier from the JWT

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

The system SHALL expose `GET /host/apartments` for users with the Host role returning only apartments where `hostId` matches the caller, including `metadata` for each apartment.

#### Scenario: List my apartments as Host

- **WHEN** authenticated Host calls `GET /host/apartments`
- **THEN** the system returns 200 OK with apartments owned by that Host (each with `id`, `name`, `description`, `pricePerNight`, `guestCount`, `amenities`, and `metadata`)

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
