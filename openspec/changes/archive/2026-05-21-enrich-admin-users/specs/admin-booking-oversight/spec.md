## ADDED Requirements

### Requirement: Admin list user bookings

The system SHALL expose `GET /admin/users/{userId}/bookings` for authenticated users with the `Admin` role. The response SHALL be a JSON array of booking objects with the same fields as client booking list items (`id`, `apartmentId`, `start`, `end`, `pricePerNight`, `guestCount`, `amenities`, `apartmentName`, `city`, `imageUrl` or `imageUrls`). When `userId` does not exist, the system SHALL return 404 Not Found.

#### Scenario: Admin lists bookings for client

- **WHEN** authenticated Admin calls `GET /admin/users/{userId}/bookings` for a user with at least one booking
- **THEN** the system returns 200 OK with a non-empty array including apartment display fields

#### Scenario: Admin lists bookings for user with none

- **WHEN** authenticated Admin calls `GET /admin/users/{userId}/bookings` for a user with zero bookings
- **THEN** the system returns 200 OK with an empty array

#### Scenario: Unknown user for bookings

- **WHEN** authenticated Admin calls `GET /admin/users/{userId}/bookings` for a non-existent user id
- **THEN** the system returns 404 Not Found

#### Scenario: Host denied user bookings

- **WHEN** authenticated Host calls `GET /admin/users/{userId}/bookings`
- **THEN** the system returns 403 Forbidden

### Requirement: Admin global bookings list

The system SHALL expose `GET /admin/bookings` for authenticated Admin users. The response SHALL be paginated with `items`, `page`, `pageSize`, and `totalCount`. Each item SHALL include `bookingId`, `userId`, `userEmail`, `apartmentId`, `apartmentName`, `city`, `start`, `end`, and `pricePerNight`. Optional query `userId` SHALL filter to that user's bookings. `page` defaults to 1; `pageSize` defaults to 20 with maximum 100.

#### Scenario: Admin lists all bookings

- **WHEN** authenticated Admin calls `GET /admin/bookings` without filters
- **THEN** the system returns 200 OK with paginated booking items sorted by `start` descending

#### Scenario: Admin filters bookings by user

- **WHEN** authenticated Admin calls `GET /admin/bookings?userId={userId}`
- **THEN** the system returns 200 OK where every item has matching `userId`

#### Scenario: Client denied global bookings

- **WHEN** authenticated Client calls `GET /admin/bookings`
- **THEN** the system returns 403 Forbidden

### Requirement: Admin bookings UI

The admin panel SHALL provide a Bookings page at `/admin/bookings` accessible only to Admin role users. The page SHALL load data from `GET /admin/bookings` and display a paginated table. The page SHALL provide an optional user id filter that passes `userId` to the API.

#### Scenario: Admin opens bookings page

- **WHEN** authenticated Admin navigates to `/admin/bookings`
- **THEN** the UI shows the bookings table with pagination

#### Scenario: Admin filters bookings by user in UI

- **WHEN** Admin enters a user id in the filter and applies it
- **THEN** the UI requests `GET /admin/bookings?userId=...` and displays matching rows

### Requirement: Admin user detail bookings tab

The user detail page at `/admin/users/{userId}` SHALL include a Bookings tab that loads `GET /admin/users/{userId}/bookings` and displays a table of that user's bookings.

#### Scenario: Admin views user bookings tab

- **WHEN** Admin opens the Bookings tab on a user profile
- **THEN** the UI shows bookings for that user or an empty state
