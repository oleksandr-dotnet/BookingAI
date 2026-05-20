## Context

BookingSystemAI persists apartments and bookings through EF Core and Npgsql. There is no Dapper or raw SQL layer yet. Hosts manage apartments; clients browse and book. Company migration already stores `SourceCompanyId` / `ExternalId` on apartments. This change adds listing economics (price, capacity, amenities), JSONB partner metadata, a Dapper-based SQL upsert, and five analytics queries for teaching/reporting SQL features (aggregates, `GROUP BY`, `HAVING`, quantiles).

## Goals / Non-Goals

**Goals:**

- Model `pricePerNight` (decimal), `guestCount` (int, min 1), and `amenities` on apartments and bookings.
- Store apartment `metadata` as PostgreSQL `jsonb`; expose on host APIs; merge on upsert.
- Implement apartment upsert with `INSERT ... ON CONFLICT DO UPDATE` via Dapper.
- Five Host-only analytics endpoints, each backed by one embedded `.sql` file.
- Keep clean architecture: Application defines `IApartmentAnalyticsService`, `IApartmentUpsertService`, DTOs; Infrastructure implements with Dapper + `ISqlScriptLoader`.
- EF migration for schema; EF repositories updated for normal CRUD paths.

**Non-Goals:**

- Replacing EF for standard create/list booking flows.
- Public analytics without auth.
- Full-text search on metadata.
- Amenity-based catalog filtering (future).

## Decisions

### 1. Amenity model

**Choice:** Domain enum `Amenity`: `LargeBed`, `Microwave`, `Bath`, `Shower`. Persist as PostgreSQL `text[]` or `integer[]` via EF value conversion; API uses string array matching enum names (PascalCase JSON).

**Rationale:** Fixed vocabulary matches product list; invalid values → 400.

**Alternatives:** Free-form strings (rejected — no validation).

### 2. Booking field snapshot

**Choice:** On `POST /bookings`, copy apartment's `pricePerNight`, `guestCount`, and `amenities` onto the booking row at create time (client does not override price). Request may optionally validate `guestCount` does not exceed apartment capacity.

**Rationale:** Historical record if apartment price changes later.

### 3. JSONB metadata

**Choice:** Column `Metadata` (`jsonb`, nullable, default `{}`). API: `metadata` object in JSON. Host upsert: replace entire metadata object when provided; omit field to leave unchanged on update path.

**Rationale:** Simple contract for integrations; no partial JSON patch in v1.

### 4. Dapper + SQL resources

**Choice:**

- Package: `Dapper` on Infrastructure.
- Folder: `BookingSystemAI.Infrastructure/SqlScripts/` with files:
  - `UpsertApartment.sql`
  - `StatsBookingAggregates.sql`
  - `StatsBookingsByApartment.sql` (GROUP BY)
  - `StatsHostsWithMinBookings.sql` (HAVING)
  - `StatsPricePerNightQuantiles.sql` (quantile)
  - `StatsApartmentOccupancySummary.sql` (combined aggregates + group + having)
- Embed as `EmbeddedResource` in `.csproj`; load via `ISqlScriptLoader` (reads manifest stream by logical name).
- `IDbConnection` factory using `NpgsqlConnection` + connection string from options (same as EF).

**Rationale:** User requirement; keeps SQL reviewable and versioned separately from C#.

### 5. Upsert semantics

**Choice:** `PUT /host/apartments/upsert` (Host role). Body includes optional `id`, or (`sourceCompanyId`, `externalId`) for migration-style keys, plus core fields and `metadata`. SQL upsert conflict target:

- Primary: `Id` when provided.
- Alternate: unique index on `(SourceCompanyId, ExternalId)` when both set.

`hostId` always from JWT on insert; on update, SQL `WHERE` ensures row belongs to caller's `hostId` (return 404 if no row updated).

**Alternatives:** EF upsert (rejected — user asked for SQL).

### 6. Analytics endpoints (Host only)

| Route | SQL feature | Example output |
|-------|-------------|----------------|
| `GET /analytics/bookings/summary` | `COUNT`, `SUM`, `AVG` | totals across all bookings |
| `GET /analytics/bookings/by-apartment` | `GROUP BY` apartment | count + revenue per apartment |
| `GET /analytics/hosts/active` | `GROUP BY` + `HAVING` | hosts with `COUNT(*) >= minBookings` query param (default 1) |
| `GET /analytics/apartments/price-quantiles` | `percentile_cont` | p25/p50/p75 of `price_per_night` |
| `GET /analytics/apartments/occupancy` | combined | apartments with avg nights booked + filter via HAVING |

All require `Host` policy. Query params documented in OpenAPI.

### 7. Layering

```
Application/
  Abstractions/IApartmentSqlGateway.cs  (or split Analytics + Upsert interfaces)
  Services/ApartmentAnalyticsService.cs
  Services/ApartmentUpsertService.cs
  DTOs, Amenity enum usage
Infrastructure/
  Sql/DapperApartmentSqlGateway.cs
  Sql/SqlScriptLoader.cs
  SqlScripts/*.sql
  Data: EF configs for new columns
Api/
  AnalyticsEndpoints.cs
  HostApartmentEndpoints: upsert route
```

Endpoints stay thin; no Dapper in Api.

### 8. EF migration defaults

- `PricePerNight`: `numeric(18,2)`, not null, default `0` for backfill then remove default if desired.
- `GuestCount`: not null, default `1`.
- `Amenities`: empty array default.
- `Metadata`: `'{}'::jsonb`.

Bookings: same three fields (no metadata on booking).

## Risks / Trade-offs

- **[Dual persistence paths]** EF and Dapper can drift → Mitigation: upsert updates same columns EF uses; integration test compares upsert then EF read.
- **[Breaking API]** New required fields on create → Mitigation: document defaults in specs; migration backfill.
- **[Analytics cost]** Full-table scans acceptable for demo → Mitigation: Host-only; note index on `ApartmentId` for bookings in Open Questions.
- **[Metadata size]** Unbounded JSON → Mitigation: max 16 KB validation in Application layer.

## Migration Plan

1. Add EF migration for new columns.
2. Deploy API; existing apartments/bookings get defaults.
3. Hosts update listings via `PUT /host/apartments/upsert` or future PATCH (out of scope).
4. Rollback: revert migration (drops columns) only if no production dependency on metadata.

## Open Questions

- Should `GET /apartments` expose `metadata` publicly? **Proposal:** omit from public catalog; include on `GET /host/apartments` only.
- Should Client `POST /bookings` reject when `guestCount` on apartment is less than party size? **Proposal:** validate `guestCount` in request ≤ apartment `guestCount` if request includes party size; otherwise copy apartment value only.
