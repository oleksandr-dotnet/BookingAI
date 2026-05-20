## ADDED Requirements

### Requirement: Stale apartment version on booking create

When `apartmentVersion` does not match the apartment's current `version`, the system SHALL return 409 Conflict with a response that indicates the apartment was updated by the host and the client must review current listing details and create the booking again. The system MUST NOT create a booking in this case.

#### Scenario: Host updated apartment before client booked

- **WHEN** authenticated Client posts `POST /bookings` with `apartmentVersion` less than the apartment's current `version` and otherwise valid fields
- **THEN** the system returns 409 Conflict with a machine-readable code identifying apartment updated by host and does not persist a booking

#### Scenario: Missing apartment version

- **WHEN** authenticated Client posts `POST /bookings` without `apartmentVersion`
- **THEN** the system returns 400 Bad Request

## MODIFIED Requirements

### Requirement: Create booking for available apartment

The system SHALL allow an authenticated Client to create a booking via `POST /bookings` with body fields `apartmentId`, `apartmentVersion`, `start`, and `end` (ISO 8601 `DateTimeOffset`), using half-open interval `[start, end)`. The client-supplied `apartmentVersion` MUST equal the apartment's current `version` at commit time. The created booking SHALL snapshot `pricePerNight`, `guestCount`, and `amenities` from the target apartment at booking time.

#### Scenario: Successful booking

- **WHEN** authenticated Client posts a valid future window, matching `apartmentVersion`, for an existing apartment with no overlapping booking
- **THEN** the system returns 201 Created with booking `id`, `apartmentId`, `start`, `end`, `pricePerNight`, `guestCount`, and `amenities` copied from the apartment, and does not expose other users' data

#### Scenario: Unauthenticated create

- **WHEN** client calls `POST /bookings` without a valid bearer token
- **THEN** the system returns 401 Unauthorized

#### Scenario: Apartment not found

- **WHEN** authenticated Client posts a non-existent `apartmentId`
- **THEN** the system returns 404 Not Found

#### Scenario: Overlapping booking conflict

- **WHEN** authenticated Client posts a window that overlaps an existing booking for the same apartment
- **THEN** the system returns 409 Conflict and does not create a second booking

#### Scenario: Invalid booking interval

- **WHEN** authenticated Client posts `end` less than or equal to `start`, or omits required fields
- **THEN** the system returns 400 Bad Request with validation errors

#### Scenario: Booking in the past

- **WHEN** authenticated Client posts a window where `start` is before the current UTC instant
- **THEN** the system returns 400 Bad Request
