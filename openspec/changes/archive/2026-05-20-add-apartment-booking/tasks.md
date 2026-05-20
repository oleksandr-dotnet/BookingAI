## 1. Roles and auth

- [x] 1.1 Seed Identity roles `Host` and `Client` on application startup
- [x] 1.2 Extend `RegisterRequestDto` with required `role`; validate `Host` | `Client` in `UserService`
- [x] 1.3 Extend `IIdentityUserManager` + adapter with role assignment after user create
- [x] 1.4 Add role claims to `JwtTokenService` on login
- [x] 1.5 Register authorization policies `Host` and `Client` in `Program.cs`
- [x] 1.6 Update auth unit/integration tests for register role and JWT role claims

## 2. Domain and persistence models

- [x] 2.1 Add `Apartment` (with `HostId`) and `Booking` entities in Domain
- [x] 2.2 Add EF configurations, `DbSet`s, migration `AddApartmentsAndBookings`

## 3. Application layer

- [x] 3.1 Add DTOs and result types for catalog, host apartments, and bookings
- [x] 3.2 Add repository abstractions (list all, list by host, overlap, insert booking, list by client user)
- [x] 3.3 Implement `IApartmentService` (public catalog + availability filters)
- [x] 3.4 Implement `IHostApartmentService` (create, list mine by host id)
- [x] 3.5 Implement `IBookingService` (create with overlap, list by client user id)
- [x] 3.6 Register services and repositories in DI

## 4. Infrastructure repositories

- [x] 4.1 Implement apartment repository (all, by host id, add)
- [x] 4.2 Implement booking repository (overlap, insert, list by user) with transaction on create

## 5. API endpoints

- [x] 5.1 `ApartmentEndpoints` — `GET /apartments` (public, query filters)
- [x] 5.2 `HostApartmentEndpoints` — `POST /host/apartments`, `GET /host/apartments` with Host policy
- [x] 5.3 `BookingEndpoints` — `POST /bookings`, `GET /bookings` with Client policy
- [x] 5.4 Map groups in `Program.cs`; OpenAPI summaries for roles and 403 cases

## 6. Tests and verification

- [x] 6.1 Unit tests: overlap, availability validation, register role validation
- [x] 6.2 Integration tests: Host create/list own apartments; Client book/list; 403 cross-role; public catalog
- [x] 6.3 Update `BookingSystemAI.http` with Host register, Client register, host apartments, client booking flows
- [x] 6.4 Run `dotnet build` and integration tests
