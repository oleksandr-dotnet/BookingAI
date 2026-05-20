# apartments Specification

## Purpose
Public apartment catalog with listing economics (price, capacity, amenities) for client discovery and availability filtering.

## Requirements
### Requirement: Public catalog for clients

The system SHALL expose `GET /apartments` returning all bookable apartments without requiring authentication, so Clients (and anonymous browsers) can discover inventory before sign-in. Each apartment in the response SHALL include `pricePerNight`, `guestCount`, and `amenities` in addition to `id`, `name`, and `description`. The public catalog MUST NOT include `metadata`.

#### Scenario: List all apartments without filters

- **WHEN** caller invokes `GET /apartments` without query parameters
- **THEN** the system returns 200 OK with a JSON array of apartments, each including `id`, `name`, `description`, `pricePerNight`, `guestCount`, and `amenities`

#### Scenario: Catalog includes host-created apartments

- **WHEN** a Host has created apartments
- **THEN** those apartments appear in `GET /apartments` alongside any others

### Requirement: Filter apartments by time window

The system SHALL accept optional `from` and `to` query parameters (ISO 8601 `DateTimeOffset`) on `GET /apartments` to evaluate availability for each apartment across the half-open interval `[from, to)`.

#### Scenario: Availability flag when window provided

- **WHEN** caller requests `GET /apartments?from={from}&to={to}` with `to` strictly after `from`
- **THEN** the system returns 200 OK with all apartments and each item includes `isAvailable` indicating whether the apartment has no overlapping booking in that window, plus `pricePerNight`, `guestCount`, and `amenities`

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

### Requirement: Filter to available apartments only

The system SHALL accept optional `availableOnly=true` together with both `from` and `to` to return only apartments free for the entire requested window.

#### Scenario: Available only filter

- **WHEN** caller requests `GET /apartments?from={from}&to={to}&availableOnly=true`
- **THEN** the system returns 200 OK containing only apartments whose `isAvailable` is true for that window

#### Scenario: Available only without full window

- **WHEN** caller sets `availableOnly=true` without both `from` and `to`
- **THEN** the system returns 400 Bad Request

### Requirement: Supported amenities vocabulary

The system SHALL accept apartment `amenities` as a JSON array of strings drawn only from: `LargeBed`, `Microwave`, `Bath`, `Shower` (full bathing tub, not merely a bathroom), where `Bath` denotes a full bathtub for bathing.

#### Scenario: Valid amenities accepted

- **WHEN** host or upsert payload includes `amenities` `["LargeBed", "Shower"]`
- **THEN** the system persists and returns the same values

#### Scenario: Unknown amenity rejected

- **WHEN** caller includes an amenity string not in the vocabulary
- **THEN** the system returns 400 Bad Request

#### Scenario: Duplicate amenities deduplicated

- **WHEN** caller sends duplicate amenity entries
- **THEN** the system stores a de-duplicated list

### Requirement: Listing economics validation

The system SHALL require `pricePerNight` as a non-negative decimal and `guestCount` as an integer greater than or equal to 1 on apartment create and upsert paths.

#### Scenario: Invalid guest count

- **WHEN** caller sets `guestCount` to 0 or negative
- **THEN** the system returns 400 Bad Request

#### Scenario: Negative price rejected

- **WHEN** caller sets `pricePerNight` less than zero
- **THEN** the system returns 400 Bad Request
