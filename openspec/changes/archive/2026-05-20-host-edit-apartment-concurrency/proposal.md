## Why

Hosts can create and list apartments but cannot update listing details after publication. Clients book against whatever the catalog shows at submit time; if a host changes price or capacity while a client still has a stale view, the booking could be created on outdated terms. Optimistic concurrency with an apartment version lets the API reject stale booking attempts and tell the client to refresh and book again.

## What Changes

- Add a monotonically increasing `version` (or equivalent concurrency token) on apartments, returned on catalog and host list responses.
- Add `PUT /host/apartments/{id}` for Host to update own apartment fields (`name`, `description`, `pricePerNight`, `guestCount`, `amenities`, optional `metadata`) with required `version` in the body; mismatch → **409 Conflict**.
- Extend `POST /bookings` request with required `apartmentVersion`; if it does not match the current apartment version → **409 Conflict** with a clear message that the apartment was updated by the host and the client must review listing details and create the booking again.
- Include `version` on public `GET /apartments` items so the UI can hold the token through the booking flow.
- EF migration for `Apartments.Version`; existing rows backfilled to `1`.
- Unit and integration tests for host update, version conflict on update, and stale-version booking rejection.

**Non-goals:** editing migrated apartments via external identity upsert semantics change, cancel/modify bookings, WebSocket push notifications, automatic merge of booking form fields, UI implementation (API contract only; UI may follow separately).

## Capabilities

### New Capabilities

_None — behavior extends existing host and booking flows._

### Modified Capabilities

- `host-apartments`: Host can update an owned apartment with optimistic concurrency on `version`.
- `apartments`: Public catalog responses expose `version` for client booking flows.
- `bookings`: Create booking requires `apartmentVersion`; stale version returns 409 with host-updated guidance.

## Impact

- **API:** `PUT /host/apartments/{id}` (Host); `version` on apartment DTOs; `apartmentVersion` on `POST /bookings` body; new 409 scenarios for host update and booking create.
- **Persistence:** `Apartments` table column `Version` (int, default 1); EF configuration and migration.
- **Code:** Domain `Apartment`, Application DTOs/services (`HostApartmentService`, `BookingService`, `ApartmentService`), `IApartmentRepository` update path, Infrastructure repository/mapping, Api endpoint mapping and OpenAPI descriptions.
- **Breaking:** **BREAKING** — `POST /bookings` requires `apartmentVersion`; clients that omit it receive 400. Apartment list/create responses gain `version` field (additive for readers that ignore unknown fields).

## Success criteria

- Host can update own apartment; successful update increments `version` and returns the new value.
- Host updating with an outdated `version` receives 409 and no partial write.
- Client creating a booking with matching `apartmentVersion` succeeds as today (snapshot still taken at commit time).
- Client creating a booking with stale `apartmentVersion` receives 409; response indicates the apartment was updated by the host and the client should refresh and book again.
- Host cannot update another host's apartment (404 or 403 per existing ownership pattern).
- Integration tests cover host edit, concurrent edit conflict, and stale booking attempt.
