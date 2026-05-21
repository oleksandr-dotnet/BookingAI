## 1. Application — presentation metadata

- [x] 1.1 Add `ListingPresentation` validation and projection helpers
- [x] 1.2 Extend `ApartmentListItemDto` with presentation fields; map in `ApartmentDtoMapper`
- [x] 1.3 Call presentation validation from `ListingValidation.ValidateMetadata`

## 2. Tests

- [x] 2.1 Unit tests for presentation metadata validation and projection
- [x] 2.2 Integration test: public list/detail include projected fields after host metadata set

## 3. UI — catalog and detail

- [x] 3.1 Update API types and `ApartmentCard` with subtitle, capacity line, highlight chips
- [x] 3.2 Airbnb-style catalog search row and client-side city filter
- [x] 3.3 Two-column detail layout, sections, sticky booking card, static reviews line
- [x] 3.4 CSS for catalog search, enriched cards, and detail layout

## 4. UI — host form

- [x] 4.1 Host create/edit inputs for presentation fields mapped to metadata JSON

## 5. Verify

- [x] 5.1 `dotnet test` Application + Integration projects
- [x] 5.2 `npm run build` in booking-system-ui
