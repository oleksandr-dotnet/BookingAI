## 1. Persistence and domain

- [x] 1.1 Add `Version` to `Apartment` domain entity and `ApartmentRecord` with EF configuration (default 1)
- [x] 1.2 Add EF migration `AddApartmentVersion` and verify applies on existing data

## 2. Application DTOs and abstractions

- [x] 2.1 Add `version` to apartment response/list DTOs; add `UpdateApartmentRequestDto` with required `version`
- [x] 2.2 Add `apartmentVersion` to `CreateBookingRequestDto` and validation when missing
- [x] 2.3 Extend `IApartmentRepository` with optimistic `UpdateAsync` (or equivalent) returning success/conflict/not-found
- [x] 2.4 Add `UpdateApartmentResult` / failure reasons on host apartment service interface

## 3. Application services

- [x] 3.1 Implement `HostApartmentService.UpdateAsync` (ownership, validation, version check, increment on success)
- [x] 3.2 Map `version` in create/list/catalog mappers; new apartments start at version 1
- [x] 3.3 Implement `BookingService` stale-version check before overlap persistence; add `ApartmentVersionConflict` result
- [x] 3.4 Map booking 409 to stable `code` (e.g. `apartmentUpdatedByHost`) and user-facing message in API layer

## 4. Infrastructure

- [x] 4.1 Implement repository update with `WHERE Version = @expected` and increment `Version` on match
- [x] 4.2 Update apartment record ↔ domain mapping to include `Version`

## 5. API endpoints

- [x] 5.1 Add `PUT /host/apartments/{id}` on `HostApartmentEndpoints` with result mapping (200/404/409/400)
- [x] 5.2 Update `BookingEndpoints` to pass `apartmentVersion` and map version conflict 409
- [x] 5.3 Update OpenAPI summaries for host update, catalog `version`, and booking body

## 6. Tests

- [x] 6.1 Unit tests: `HostApartmentService` update success, stale version, not owned
- [x] 6.2 Unit tests: `BookingService` rejects stale `apartmentVersion` with correct failure reason
- [x] 6.3 Integration tests: host update increments version; concurrent update 409; booking with stale version 409 and no row created
- [x] 6.4 Integration tests: catalog and host list include `version`; successful booking requires matching version

## 7. Verification

- [x] 7.1 Run `dotnet build` on solution
- [x] 7.2 Run `dotnet test` on Application.Tests and IntegrationTests
