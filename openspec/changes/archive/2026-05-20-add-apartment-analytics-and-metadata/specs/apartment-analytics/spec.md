## ADDED Requirements

### Requirement: Host-only booking aggregate summary

The system SHALL expose `GET /analytics/bookings/summary` requiring the Host role, returning global aggregate metrics over all bookings using SQL aggregate functions (`COUNT`, `SUM`, `AVG`).

#### Scenario: Summary with data

- **WHEN** authenticated Host calls `GET /analytics/bookings/summary` and bookings exist
- **THEN** the system returns 200 OK with JSON including `totalBookings`, `totalRevenue`, and `averagePricePerNight` computed from persisted booking/apartment data

#### Scenario: Client denied

- **WHEN** authenticated user with only the Client role calls `GET /analytics/bookings/summary`
- **THEN** the system returns 403 Forbidden

#### Scenario: Unauthenticated

- **WHEN** caller invokes the endpoint without a valid bearer token
- **THEN** the system returns 401 Unauthorized

### Requirement: Bookings grouped by apartment

The system SHALL expose `GET /analytics/bookings/by-apartment` requiring the Host role, returning per-apartment metrics using SQL `GROUP BY` apartment identifier.

#### Scenario: Per-apartment rows

- **WHEN** authenticated Host calls `GET /analytics/bookings/by-apartment`
- **THEN** the system returns 200 OK with an array where each item includes `apartmentId`, `bookingCount`, and `revenueSum`

#### Scenario: Empty database

- **WHEN** no bookings exist
- **THEN** the system returns 200 OK with an empty array

### Requirement: Active hosts filtered with HAVING

The system SHALL expose `GET /analytics/hosts/active` requiring the Host role, accepting optional query `minBookings` (integer, default 1), and returning hosts whose booking count satisfies `HAVING COUNT(*) >= minBookings`.

#### Scenario: Filter by minimum bookings

- **WHEN** authenticated Host calls `GET /analytics/hosts/active?minBookings=2` and only some hosts have at least two bookings
- **THEN** the system returns 200 OK listing only those hosts with `hostId` and `bookingCount`

#### Scenario: Invalid minBookings

- **WHEN** caller provides `minBookings` less than 1
- **THEN** the system returns 400 Bad Request

### Requirement: Price per night quantiles

The system SHALL expose `GET /analytics/apartments/price-quantiles` requiring the Host role, returning percentile values of apartment `pricePerNight` using PostgreSQL ordered-set aggregate (`percentile_cont`).

#### Scenario: Quantile response shape

- **WHEN** authenticated Host calls `GET /analytics/apartments/price-quantiles` and apartments exist
- **THEN** the system returns 200 OK with `p25`, `p50`, and `p75` decimal values

#### Scenario: No apartments

- **WHEN** no apartments exist
- **THEN** the system returns 200 OK with null quantile fields

### Requirement: Combined occupancy report

The system SHALL expose `GET /analytics/apartments/occupancy` requiring the Host role, executing SQL that combines aggregates, `GROUP BY` apartment, and `HAVING` to return apartments whose average booked nights exceed optional query `minAvgNights` (default 0).

#### Scenario: Occupancy rows returned

- **WHEN** authenticated Host calls `GET /analytics/apartments/occupancy`
- **THEN** the system returns 200 OK with an array of items including `apartmentId`, `bookingCount`, and `averageNightsBooked`

#### Scenario: HAVING filter applied

- **WHEN** authenticated Host calls `GET /analytics/apartments/occupancy?minAvgNights=1` and some apartments average below one night
- **THEN** those apartments are excluded from the response

### Requirement: Analytics SQL via Dapper and embedded scripts

The system SHALL execute all analytics queries through Dapper using SQL loaded from embedded resource files in Infrastructure, not inline SQL strings in Application or Api layers.

#### Scenario: Script resource present

- **WHEN** the application starts
- **THEN** each analytics endpoint maps to a distinct embedded `.sql` script name documented in the design artifact
