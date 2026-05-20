## ADDED Requirements

### Requirement: Client role for booking endpoints

The system SHALL require the Client role for `POST /bookings` and `GET /bookings`.

#### Scenario: Host denied on create booking

- **WHEN** authenticated user with only the Host role calls `POST /bookings`
- **THEN** the system returns 403 Forbidden

#### Scenario: Host denied on list bookings

- **WHEN** authenticated user with only the Host role calls `GET /bookings`
- **THEN** the system returns 403 Forbidden

### Requirement: Create booking for available apartment

The system SHALL allow an authenticated Client to create a booking via `POST /bookings` with body fields `apartmentId`, `start`, and `end` (ISO 8601 `DateTimeOffset`), using half-open interval `[start, end)`.

#### Scenario: Successful booking

- **WHEN** authenticated Client posts a valid future window for an existing apartment with no overlapping booking
- **THEN** the system returns 201 Created with booking `id`, `apartmentId`, `start`, `end`, and does not expose other users' data

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

### Requirement: Booking ownership from token

The system SHALL associate each new booking with the authenticated Client's identifier from the JWT; the request body MUST NOT supply `userId`.

#### Scenario: User id from JWT

- **WHEN** authenticated Client creates a booking
- **THEN** the persisted booking's user identifier matches the JWT NameIdentifier claim

### Requirement: List own bookings only

The system SHALL expose `GET /bookings` for authenticated Clients returning only bookings owned by the caller.

#### Scenario: List my bookings

- **WHEN** authenticated Client calls `GET /bookings`
- **THEN** the system returns 200 OK with a JSON array of that user's bookings (each including `id`, `apartmentId`, `start`, `end`)

#### Scenario: No cross-user leakage

- **WHEN** Client A is authenticated and Client B has existing bookings
- **THEN** Client A's `GET /bookings` response does not include Client B's bookings

#### Scenario: Unauthenticated list

- **WHEN** client calls `GET /bookings` without a valid bearer token
- **THEN** the system returns 401 Unauthorized

#### Scenario: Empty list for new Client

- **WHEN** authenticated Client has no bookings
- **THEN** the system returns 200 OK with an empty array

### Requirement: Booking persistence

The system SHALL persist bookings in PostgreSQL via Entity Framework Core alongside existing Identity data.

#### Scenario: Booking survives restart

- **WHEN** a booking is created and the application restarts
- **THEN** the booking is still returned for the owning Client on `GET /bookings`
