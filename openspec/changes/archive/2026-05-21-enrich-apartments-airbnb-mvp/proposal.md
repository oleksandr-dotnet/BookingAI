## Why

The apartment catalog and detail pages work for booking, but they still feel like an admin list rather than a stay marketplace. A light Airbnb-inspired presentation (search bar, richer cards, two-column detail with sticky booking) improves demo quality without building reviews, maps, messaging, or full host profiles.

## What Changes

- **API:** Public catalog and `GET /apartments/{id}` expose optional listing presentation fields: `propertyType`, `bedroomCount`, `bedCount`, `bathroomCount`, `highlights` (string array, max 5). Values are stored in apartment `metadata` under stable keys and projected on read; raw `metadata` remains host-only.
- **API:** Host create/update accept the same keys inside `metadata` with validation (known `propertyType` enum, non-negative counts, highlight length limits).
- **UI — Catalog:** Airbnb-style compact search row (location + dates + search); cards show property type, capacity summary, and highlight chips; improved grid spacing.
- **UI — Detail:** Two-column layout with sticky booking card; sections for about, amenities, location placeholder, and static “Things to know”; demo rating line (“New listing”).
- **UI — Host:** Optional fields on create/edit for property type, room counts, and highlights (mapped to metadata).
- **Seed/demo:** Existing dev apartments get sensible default presentation metadata where empty.

**Non-goals:** Real maps, reviews/ratings backend, host avatars, instant book rules, pricing breakdowns (cleaning/service fees), wishlists, compare, or new booking/payment behavior.

## Capabilities

### New Capabilities

_None — extends existing capabilities._

### Modified Capabilities

- `apartments`: Public list/detail include projected listing presentation fields; validation for metadata keys on host write paths.
- `host-apartments`: Create/update document and validate presentation metadata keys.
- `client-booking-ux`: Catalog search hero, enriched cards, Airbnb-style detail layout and host listing form fields.

## Impact

- **Application:** DTOs, `ApartmentDtoMapper`, `ListingValidation` for presentation metadata keys.
- **Infrastructure:** Optional data seed or migration script for demo defaults (no new columns if using metadata projection).
- **UI:** `CatalogPage`, `ApartmentDetailPage`, `ApartmentCard`, `HostApartmentsPage`, `index.css`, API types.
- **Tests:** Unit tests for presentation metadata validation; integration tests for public projection on list/detail.

## Success criteria

- Anonymous user sees Airbnb-like catalog search and richer cards; can open detail with sticky booking widget and sectioned content.
- Host can set property type, beds/baths/bedrooms, and highlights; values appear on public listing.
- `dotnet test` passes; UI `npm run build` passes; browser happy path on catalog → detail → reserve still works for Client.
