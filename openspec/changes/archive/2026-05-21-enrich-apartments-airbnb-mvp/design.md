## Context

Apartments already expose city, multi-image gallery, amenities, pricing, and availability filtering. Host-only `metadata` JSON exists but is hidden from public APIs. The UI uses a functional catalog grid and a single-column detail page with an inline booking block.

## Goals / Non-Goals

**Goals:**

- Project stable presentation fields on public list/detail DTOs without exposing full `metadata`.
- Validate presentation keys on host create/update inside existing metadata size/object rules.
- Refresh catalog and detail UX toward Airbnb layout patterns while keeping current booking and payment flows.

**Non-Goals:**

- New DB columns, reviews, maps API, host profile pages, fee breakdowns, or cancellation policy engines.

## Decisions

### 1. Store presentation in `metadata`, project on read

**Choice:** Host writes `propertyType`, `bedroomCount`, `bedCount`, `bathroomCount`, `highlights` as top-level metadata keys. `ApartmentDtoMapper` maps them to camelCase public DTO properties; omits nulls when unset.

**Alternatives:** New columns — clearer schema but requires migration and host form/API churn for MVP.

**Rationale:** Reuses JSONB column and host endpoints; public contract stays explicit without leaking arbitrary metadata.

### 2. `propertyType` vocabulary

Allowed: `Apartment`, `House`, `Studio`, `Room`. Invalid or missing → omitted on public responses; host write returns 400.

### 3. `highlights`

Array of strings, max 5 items, each max 40 characters, trimmed, de-duplicated case-insensitively.

### 4. Counts

`bedroomCount`, `bedCount`, `bathroomCount`: optional integers 0–20; omitted when not set.

### 5. UI layout

- **Catalog:** `.catalog-search` row mimicking Airbnb (Where / Check-in / Check-out / Search). Cards show subtitle line: `{propertyType} in {city}` and meta `{beds} beds · {baths} baths · {guests} guests`.
- **Detail:** CSS grid `.apartment-detail-layout` — main column (gallery, title, about, amenities, location placeholder, things to know) + sticky `.apartment-booking-card` on wide screens.
- **Rating:** Static UI copy “New · No reviews yet” (no API field).

### 6. Demo defaults

On Development startup or one-time seed helper, patch apartments with empty presentation metadata to sample values (does not overwrite host-set keys).

## Risks / Trade-offs

- **[Risk] Metadata key collision with analytics/partner keys** → Use documented reserved keys only; validation ignores unknown keys in presentation validator.
- **[Risk] Public projection diverges from stored metadata** → Single mapper module; unit tests for round-trip host write → public read.
- **[Risk] Sticky booking card on mobile** → Booking card stacks below content under 900px breakpoint.

## Migration Plan

No EF migration. Deploy API + UI together. Existing apartments without keys render without presentation lines until host edits or dev seed runs.

## Open Questions

None for MVP scope.
