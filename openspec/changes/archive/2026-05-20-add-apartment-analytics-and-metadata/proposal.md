## Why

Hosts and integrations need richer apartment and booking data (pricing, capacity, amenities) plus a place to store partner-specific metadata. Operators also need SQL-level reporting (aggregates, grouping, HAVING, quantiles) without pushing analytics through EF. A raw-SQL upsert path and Dapper-backed queries keep advanced persistence separate from the existing EF repositories while extending the catalog and booking model.

## What Changes

- Extend **Apartment** and **Booking** with `pricePerNight`, `guestCount`, and `amenities` (enum set: LargeBed, Microwave, Bath, Shower).
- Add PostgreSQL **JSONB** column `metadata` on apartments for owner/integration custom key-value data.
- Add **SQL upsert** for apartments (insert or update by id / external identity) executed via Dapper, not EF.
- Add **five read-only statistics endpoints** backed by Dapper and `.sql` scripts in embedded resources:
  1. Aggregate — e.g. total bookings, revenue sum, average price per night across catalog.
  2. **GROUP BY** — e.g. booking counts or revenue per apartment or per host.
  3. **HAVING** — e.g. apartments/hosts with booking count above a threshold.
  4. **Quantile** — e.g. percentile of `pricePerNight` or booking duration (PostgreSQL `percentile_cont`).
  5. Combined report using aggregates + group + having in one query (documented in design).
- Introduce **Dapper** + **SqlScripts** resource pattern in Infrastructure for all new SQL (upsert + stats).
- Update catalog, host create/list, and booking create/list DTOs and responses to include new fields.
- EF migration for new columns; existing rows get sensible defaults (e.g. `guestCount` minimum 1, empty amenities).
- Unit tests for amenity validation and application mapping; integration tests for upsert, metadata round-trip, and at least one stats endpoint.

**Non-goals:** Payments/checkout, dynamic pricing rules, amenity search/filter on catalog (unless trivial), editing bookings after create, admin UI, replacing EF repositories for normal CRUD.

## Capabilities

### New Capabilities

- `apartment-analytics`: Five protected analytics endpoints (aggregates, GROUP BY, HAVING, quantiles, combined) using Dapper + embedded SQL.
- `apartment-upsert`: Host or integration upsert of apartment rows via raw SQL, including JSONB metadata merge/replace semantics.

### Modified Capabilities

- `apartments`: Catalog and responses include `pricePerNight`, `guestCount`, `amenities`; optional `metadata` on host-owned detail where appropriate.
- `host-apartments`: Create and list include new listing fields; optional `metadata` on create; upsert route for Host.
- `bookings`: Create and list include `pricePerNight`, `guestCount`, `amenities` (snapshot at booking time from apartment or request per design).

## Impact

- **API surface:** `GET /analytics/...` (five routes, Host role); `PUT /host/apartments/{id}` or `POST /host/apartments/upsert` for SQL upsert; extended bodies on `POST /host/apartments`, `POST /bookings`, and list responses on `GET /apartments`, `GET /host/apartments`, `GET /bookings`.
- **BREAKING:** `POST /host/apartments` and `POST /bookings` require new fields (or server defaults documented in specs); clients must send `pricePerNight`, `guestCount`, and `amenities` (or accept documented defaults).
- **Dependencies:** Dapper, Npgsql; embedded `.sql` resources in Infrastructure.
- **Code:** Domain entities, Application DTOs/services, Infrastructure (Dapper gateway, SQL resources, EF migration, repository mapping), Api endpoints, OpenAPI summaries, Application + Integration tests.
- **Persistence:** New columns on `Apartments` and `Bookings`; `metadata jsonb` on `Apartments`.

## Success criteria

- Apartments and bookings persist and return `pricePerNight`, `guestCount`, and validated `amenities`.
- Host can upsert an apartment via SQL path; JSONB `metadata` round-trips on read for host-owned apartment.
- Five analytics endpoints return correct shapes for seeded data; SQL demonstrates aggregate, GROUP BY, HAVING, and quantile functions.
- All new SQL lives in resource files executed through Dapper; normal list/create still works via EF.
- `dotnet build` and integration tests pass.
