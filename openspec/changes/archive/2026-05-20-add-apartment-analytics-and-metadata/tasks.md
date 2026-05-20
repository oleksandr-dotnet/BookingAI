## 1. Domain and schema

- [x] 1.1 Add `Amenity` enum (`LargeBed`, `Microwave`, `Bath`, `Shower`) in Domain
- [x] 1.2 Extend `Apartment` and `Booking` entities with `PricePerNight`, `GuestCount`, `Amenities`; add `Metadata` on `Apartment` only
- [x] 1.3 Update `ApartmentRecord` / `BookingRecord`, EF configurations (`jsonb` for metadata, array conversion for amenities)
- [x] 1.4 Add EF migration `AddApartmentListingAndBookingSnapshotFields` with backfill defaults

## 2. Dapper infrastructure

- [x] 2.1 Add `Dapper` package to Infrastructure; register `IDbConnection` factory (Npgsql)
- [x] 2.2 Add `SqlScripts/` folder with embedded resources: `UpsertApartment.sql` + five analytics scripts
- [x] 2.3 Implement `ISqlScriptLoader` and `IApartmentSqlGateway` (Dapper executor)
- [x] 2.4 Wire gateway in `DependencyInjection.cs`

## 3. Application layer

- [x] 3.1 Add DTOs for listing fields, metadata, analytics responses, upsert request/result
- [x] 3.2 Add amenity and economics validation helpers (guest count, price, vocabulary, metadata size)
- [x] 3.3 Update `IApartmentService` / `IHostApartmentService` / `IBookingService` and implementations for new fields and booking snapshot
- [x] 3.4 Add `IApartmentUpsertService` and `IApartmentAnalyticsService` with implementations delegating to SQL gateway
- [x] 3.5 Register new services in Application DI

## 4. EF repositories

- [x] 4.1 Update apartment and booking repository mapping for new columns
- [x] 4.2 Ensure host create and catalog queries project amenities and economics

## 5. API endpoints

- [x] 5.1 Extend `POST /host/apartments`, `GET /host/apartments`, `GET /apartments` responses (no public metadata)
- [x] 5.2 Add `PUT /host/apartments/upsert` with Host policy
- [x] 5.3 Add `AnalyticsEndpoints` — five `GET /analytics/...` routes with Host policy and query params
- [x] 5.4 Map endpoints in `Program.cs`; OpenAPI summaries for analytics and upsert

## 6. Tests and verification

- [x] 6.1 Unit tests: amenity validation, economics validation, booking snapshot mapping
- [x] 6.2 Integration tests: upsert insert/update + metadata round-trip; one analytics endpoint; create booking includes snapshot fields
- [x] 6.3 Update `BookingSystemAI.http` with upsert and analytics examples
- [x] 6.4 Run `dotnet build` and integration tests
