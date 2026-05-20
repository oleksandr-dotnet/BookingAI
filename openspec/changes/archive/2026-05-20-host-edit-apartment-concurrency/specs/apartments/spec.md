## MODIFIED Requirements

### Requirement: Public catalog for clients

The system SHALL expose `GET /apartments` returning all bookable apartments without requiring authentication, so Clients (and anonymous browsers) can discover inventory before sign-in. Each apartment in the response SHALL include `pricePerNight`, `guestCount`, `amenities`, and integer `version` in addition to `id`, `name`, and `description`. The public catalog MUST NOT include `metadata`.

#### Scenario: List all apartments without filters

- **WHEN** caller invokes `GET /apartments` without query parameters
- **THEN** the system returns 200 OK with a JSON array of apartments, each including `id`, `name`, `description`, `pricePerNight`, `guestCount`, `amenities`, and `version`

#### Scenario: Catalog includes host-created apartments

- **WHEN** a Host has created apartments
- **THEN** those apartments appear in `GET /apartments` alongside any others

### Requirement: Filter apartments by time window

The system SHALL accept optional `from` and `to` query parameters (ISO 8601 `DateTimeOffset`) on `GET /apartments` to evaluate availability for each apartment across the half-open interval `[from, to)`.

#### Scenario: Availability flag when window provided

- **WHEN** caller requests `GET /apartments?from={from}&to={to}` with `to` strictly after `from`
- **THEN** the system returns 200 OK with all apartments and each item includes `isAvailable`, `pricePerNight`, `guestCount`, `amenities`, and `version`

#### Scenario: Apartment unavailable due to existing booking

- **WHEN** caller requests a window that overlaps an existing booking for an apartment
- **THEN** that apartment's `isAvailable` is false

#### Scenario: Apartment available with no overlap

- **WHEN** caller requests a window with no overlapping booking for an apartment
- **THEN** that apartment's `isAvailable` is true

#### Scenario: Invalid partial time range

- **WHEN** caller provides only one of `from` or `to`
- **THEN** the system returns 400 Bad Request

#### Scenario: Invalid time range order

- **WHEN** caller provides `from` greater than or equal to `to`
- **THEN** the system returns 400 Bad Request
