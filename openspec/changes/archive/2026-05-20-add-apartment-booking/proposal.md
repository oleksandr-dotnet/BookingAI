## Why

JWT authentication is in place, but the API has no booking domain and no Host vs Client separation. Hosts need to list and create their own apartments; clients need to browse the catalog, filter by availability, book free slots, and see only their reservations. Role-based authorization keeps each workflow isolated.

## What Changes

- Add Identity roles `Host` and `Client`; seed roles on startup; assign exactly one role at registration.
- Include role claims in JWT; enforce role policies on booking and host endpoints.
- Add domain entities and persistence for apartments (with `hostId` owner) and bookings (PostgreSQL via EF Core).
- **Host:** `POST /host/apartments` to create an apartment; `GET /host/apartments` to list only apartments owned by the authenticated host.
- **Client (public catalog):** `GET /apartments` with optional `from`, `to`, `availableOnly` (no role required for browsing).
- **Client (authenticated):** `POST /bookings`, `GET /bookings` — require `Client` role; overlap → 409.
- Application-layer services, repository abstractions, thin minimal API endpoints.
- Unit and integration tests for roles, host apartment ownership, client booking, and 403 when wrong role.

**Non-goals:** users with both roles, admin/superuser, edit/delete apartments, cancel/modify bookings, payments, UI changes, refresh tokens.

## Capabilities

### New Capabilities

- `apartments`: Public catalog listing with optional time-range and availability filtering (client discovery).
- `host-apartments`: Host-only create apartment and list own apartments.
- `bookings`: Client-only create reservation and list own bookings.

### Modified Capabilities

- `identity-auth`: Registration selects `Host` or `Client` role; user is assigned that Identity role.
- `jwt-bearer`: Access token includes role claims for authorization policies.

## Impact

- **API surface:** `GET /apartments` (public); `POST /host/apartments`, `GET /host/apartments` (Host); `POST /bookings`, `GET /bookings` (Client); `POST /auth/register` accepts `role`.
- **Status codes:** 403 Forbidden when authenticated user lacks required role; existing 401/400/404/409 behavior unchanged where applicable.
- **Code:** Domain, Application, Infrastructure, Api endpoints, `JwtTokenService`, `UserService`, `IIdentityUserManager` role APIs, role seeding, authorization policies in `Program.cs`.
- **Breaking:** Registration request gains required `role` field (existing API clients must send `Host` or `Client`).

## Success criteria

- User registering as Host receives Host role; JWT allows host endpoints and returns 403 on client-only booking routes.
- User registering as Client receives Client role; can book and list own bookings; receives 403 on host apartment management routes.
- Host sees only own apartments on `GET /host/apartments`; cannot create booking via client endpoints without Client role.
- Client can browse/filter apartments; can book when available; sees only own bookings.
- Integration tests cover role isolation and 403 for wrong role.
