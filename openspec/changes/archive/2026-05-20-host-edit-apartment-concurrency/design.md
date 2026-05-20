## Context

Apartments are persisted via EF Core (`ApartmentRecord`) and exposed through `IHostApartmentService` (create/list) and `IApartmentService` (public catalog). Bookings snapshot listing fields at create time via `BookingService` + `IApartmentRepository.GetByIdAsync`. There is no update endpoint and no concurrency token; `PUT /host/apartments/upsert` targets integration/migration, not ordinary host edits.

## Goals / Non-Goals

**Goals:**

- Host can update listing fields on apartments they own.
- Every apartment exposes a `version` integer clients pass when booking.
- Stale `version` on host update or booking create returns 409 with actionable messaging for booking conflicts.
- Existing bookings remain immutable snapshots when apartment changes.

**Non-Goals:**

- Changing upsert SQL semantics or external-id merge rules.
- Row-level locking or distributed locks; optimistic check only.
- Pessimistic blocking of bookings while host edits.
- UI implementation in `booking-system-ui/` (document API contract only).

## Decisions

### 1. Integer `Version` column on `Apartments`

**Choice:** Add `Version int NOT NULL DEFAULT 1`, increment on each successful host update.

**Alternatives:** `rowversion`/`xmin` (opaque to clients), ETag header only (harder for JSON bodies), timestamp (clock skew).

**Rationale:** Matches common optimistic concurrency; easy to display in SPA and send as `apartmentVersion` on booking.

### 2. `PUT /host/apartments/{id}` with body `version`

**Choice:** Dedicated update route on existing host group; body mirrors create fields plus required `version`. Repository update uses `WHERE Id = @id AND HostId = @hostId AND Version = @expectedVersion`; zero rows → conflict.

**Alternatives:** PATCH partial updates (more complex validation); reuse upsert by id (blurs integration vs host edit).

**Rationale:** Clear ownership check, symmetric with create DTO, keeps upsert for migrations.

### 3. Booking create requires `apartmentVersion`

**Choice:** Extend `CreateBookingRequestDto` with required `apartmentVersion`. After loading apartment, if `apartment.Version != request.ApartmentVersion`, return 409 before overlap check.

**Alternatives:** Optional version (allows silent stale bookings — rejected by product goal); If-Match header (less discoverable in OpenAPI).

**Rationale:** Forces client to refresh catalog after host change; snapshot still taken at successful commit.

### 4. 409 response shape for stale booking

**Choice:** 409 Conflict with problem details or JSON body including stable machine-readable `code` (e.g. `apartmentUpdatedByHost`) and human message: apartment was updated by the host; review listing and create booking again.

**Rationale:** UI can branch on `code` without parsing free text.

### 5. Application-layer ownership and results

**Choice:** `HostApartmentService.UpdateAsync(hostId, apartmentId, dto)` returns `UpdateApartmentResult` with `Success`, `NotFound`, `Conflict`, `Validation`. `BookingService` adds `CreateBookingFailureReason.ApartmentVersionConflict`.

**Rationale:** Keeps endpoints thin; testable in `Application.Tests`.

### 6. Catalog and host list expose `version`

**Choice:** Add `version` to `ApartmentListItemDto`, `ApartmentResponseDto`, and host list responses.

**Rationale:** Single source for client-held token through browse → book flow.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| **BREAKING** clients omitting `apartmentVersion` | Document in OpenAPI; 400 validation if missing; update integration tests |
| Race: two clients book with same version, one wins overlap | Existing 409 overlap handling unchanged |
| Upsert path bypasses version rules | Out of scope; upsert remains integration-only; document non-goal |
| Migration on large DB | Simple `ADD COLUMN` with default 1 |

## Migration Plan

1. Add EF migration `AddApartmentVersion` with `Version` default 1 for existing rows.
2. Deploy API before UI, or deploy together; old UI without `apartmentVersion` will get 400 until updated.
3. Rollback: revert migration only if no host edits incremented versions (otherwise clients may hold stale tokens).

## Open Questions

- None for MVP. Future: include current apartment summary in 409 body to reduce round-trips (optional enhancement, not in scope).
