## Context

BookingSystemAI has JWT auth, Identity with `IdentityRole` tables, but no roles are seeded or assigned yet. This change adds apartments (host-owned), client bookings, and **Host** vs **Client** role enforcement on the appropriate routes.

## Goals / Non-Goals

**Goals:**

- Seed Identity roles `Host` and `Client` at startup.
- Registration assigns exactly one role from the request (`Host` or `Client`).
- JWT includes `role` claims for policy-based authorization.
- Host: create apartment, list own apartments (`hostId` = JWT user id).
- Client: public catalog browse/filter; authenticated book + list own bookings.
- Clean architecture: services, repositories, `.RequireAuthorization("Host")` / `.RequireAuthorization("Client")` on endpoints.

**Non-Goals:**

- Multiple roles per user, role switching, admin role.
- Host editing/deleting apartments, booking cancellation.
- UI changes in this change.

## Decisions

### 1. Roles and registration

**Choice:** Two fixed roles: `Host`, `Client`. `POST /auth/register` body adds required `role` (`"Host"` | `"Client"`). On success, assign user to that Identity role via `RoleManager` / `UserManager.AddToRoleAsync`. Invalid or missing role → 400.

**Rationale:** Explicit role at signup matches product personas; one role per user keeps authorization simple.

**Alternatives:** Separate register endpoints per role (deferred — single endpoint with role field is enough).

### 2. JWT role claims

**Choice:** `JwtTokenService` adds `ClaimTypes.Role` (or `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`) for each Identity role on login. Policies: `Host` policy requires role `Host`; `Client` policy requires role `Client`.

**Rationale:** Standard ASP.NET Core `RequireRole` / authorization policies.

### 3. Apartment ownership

**Choice:** `Apartment` entity includes `HostId` (string, Identity user id). Host create sets `HostId` from JWT — never from body. Host list filters `WHERE HostId = currentUserId`.

**Rationale:** Enforces ownership without separate host profile table.

### 4. Endpoint map and authorization

| Endpoint | Auth | Role | Purpose |
|----------|------|------|---------|
| `GET /apartments` | None | — | Public catalog + availability filters |
| `POST /host/apartments` | Bearer | Host | Create apartment for self |
| `GET /host/apartments` | Bearer | Host | List own apartments |
| `POST /bookings` | Bearer | Client | Create booking |
| `GET /bookings` | Bearer | Client | List own bookings |

Wrong role with valid JWT → **403 Forbidden**. No/invalid token on protected route → **401**.

Host calling `POST /bookings` → 403. Client calling `POST /host/apartments` → 403.

### 5. Time model and overlap (unchanged)

Half-open `[start, end)`; overlap: `existing.Start < new.End && existing.End > new.Start` per apartment.

### 6. Layering

- **Application:** `IIdentityUserManager` extended with `AssignRoleAsync`; `IHostApartmentService`, `IApartmentService` (catalog), `IBookingService`.
- **Infrastructure:** Role seeder (`IHostedService` or startup); repositories filter by `HostId` / `UserId`.
- **Api:** `HostApartmentEndpoints`, `ApartmentEndpoints`, `BookingEndpoints` with policy names.

### 7. Development data

Seed roles always; optional seed sample apartments tied to a dev Host user (or created via `.http` after host register).

### 8. Identity abstraction

**Choice:** Extend `IIdentityUserManager` with role assignment and `GetRolesAsync` for tests; adapter wraps `UserManager`/`RoleManager` only in Infrastructure.

## Project structure (after implementation)

```
BookingSystemAI.Domain/Entities/Apartment.cs, Booking.cs
BookingSystemAI.Application/
  Services/IHostApartmentService.cs, IApartmentService.cs, IBookingService.cs
BookingSystemAI.Api/Endpoints/HostApartmentEndpoints.cs, ApartmentEndpoints.cs, BookingEndpoints.cs
BookingSystemAI.Infrastructure/Identity/RoleSeeder.cs (or startup extension)
```

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| **BREAKING** register without `role` | Document in proposal; return 400 with clear message |
| User registers wrong role | No self-service role change in this change (future admin/onboarding) |
| Host books own apartment as conflict of interest | Out of scope; Host lacks Client role unless given both (we disallow multiple roles) |
| JWT stale after role change | Re-login required; acceptable for MVP |

## Migration Plan

1. Seed roles; extend register + JWT.
2. Add apartment/booking schema with `HostId`.
3. Implement services and endpoints with policies.
4. Tests for Host/Client happy paths and 403 cross-role.

## Open Questions

- _(none)_ — Register defaults to no implicit role; `role` is required.
