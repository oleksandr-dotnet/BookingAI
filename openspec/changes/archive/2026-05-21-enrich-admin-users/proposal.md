## Why

The admin panel already supports read-only user list and profile views, but operators cannot take support actions (lock accounts, adjust roles) or inspect a user's booking history. Real admin consoles combine identity oversight with activity context so admins can resolve disputes and account issues without switching tools.

## What Changes

- **API — user bookings:** `GET /admin/users/{userId}/bookings` returns that user's bookings with apartment display fields (same shape as client list items, plus `userId` where useful for admin tables).
- **API — global bookings (admin):** `GET /admin/bookings` — paginated list of all bookings with optional filters (`userId`, `status` date range optional MVP: `userId` only), sort by start date desc.
- **API — user management:**
  - `POST /admin/users/{userId}/lock` — enable lockout (set lockout end far future or max).
  - `POST /admin/users/{userId}/unlock` — clear lockout.
  - `PUT /admin/users/{userId}/roles` — replace role set with validated list; forbid removing the last `Admin`.
- **Application:** Extend `IAdminUserService` (or split `IAdminUserManagementService`) for bookings list and management operations; new `IAdminBookingQuery` for global booking list; extend `IIdentityUserManager` (or `IAdminIdentityOperations`) for lock/unlock and role replace — only in Infrastructure.
- **UI — user detail:** Tabbed layout (Profile | Bookings); action bar (Lock / Unlock, role editor with save); booking table on Bookings tab.
- **UI — users list:** Show booking count column; registered/created date when available; remove "read-only" messaging.
- **UI — admin nav:** Add **Bookings** tab at `/admin/bookings` for cross-user booking search/filter.
- **Tests:** Unit tests for role/lock guards; integration tests for admin bookings and management endpoints; Host/Client 403.

## Capabilities

### New Capabilities

- `admin-booking-oversight`: Admin APIs and UI to list all bookings and per-user bookings.

### Modified Capabilities

- `admin-user-management`: Add management actions (lock, unlock, set roles), enriched list/detail UI, booking count on list.

## Impact

- **API:** `AdminUserEndpoints.cs`, new `AdminBookingEndpoints.cs`, OpenAPI summaries.
- **Application:** New DTOs, service methods, abstractions for admin booking query and identity admin ops.
- **Infrastructure:** Identity adapter methods for lock/unlock/role replace; EF queries for admin booking list.
- **UI:** `AdminUserDetailPage`, `AdminUsersPage`, `AdminBookingsPage`, `AdminLayout`, `adminUsers.ts`, new `adminBookings.ts`.
- **Tests:** Application unit + Integration admin tests.
