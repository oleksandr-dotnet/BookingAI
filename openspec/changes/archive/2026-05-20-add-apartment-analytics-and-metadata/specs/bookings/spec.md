## MODIFIED Requirements

### Requirement: Create booking for available apartment

The system SHALL allow an authenticated Client to create a booking via `POST /bookings` with body fields `apartmentId`, `start`, and `end` (ISO 8601 `DateTimeOffset`), using half-open interval `[start, end)`. The created booking SHALL snapshot `pricePerNight`, `guestCount`, and `amenities` from the target apartment at booking time.

#### Scenario: Successful booking

- **WHEN** authenticated Client posts a valid future window for an existing apartment with no overlapping booking
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

### Requirement: List own bookings only

The system SHALL expose `GET /bookings` for authenticated Clients returning only bookings owned by the caller.

#### Scenario: List my bookings

- **WHEN** authenticated Client calls `GET /bookings`
- **THEN** the system returns 200 OK with a JSON array of that user's bookings (each including `id`, `apartmentId`, `start`, `end`, `pricePerNight`, `guestCount`, and `amenities`)

#### Scenario: No cross-user leakage

- **WHEN** Client A is authenticated and Client B has existing bookings
- **THEN** Client A's `GET /bookings` response does not include Client B's bookings

#### Scenario: Unauthenticated list

- **WHEN** client calls `GET /bookings` without a valid bearer token
- **THEN** the system returns 401 Unauthorized

#### Scenario: Empty list for new Client

- **WHEN** authenticated Client has no bookings
- **THEN** the system returns 200 OK with an empty array

## ADDED Requirements

### Requirement: Booking snapshot immutability

The system SHALL NOT update `pricePerNight`, `guestCount`, or `amenities` on an existing booking when the parent apartment is later modified.

#### Scenario: Apartment price change does not alter past booking

- **WHEN** a booking exists and the host later changes the apartment `pricePerNight`
- **THEN** `GET /bookings` for the client still returns the original snapshot values on that booking
