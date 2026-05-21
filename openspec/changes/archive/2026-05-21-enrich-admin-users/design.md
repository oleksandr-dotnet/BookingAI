## Context

`add-admin-user-management` delivered read-only `GET /admin/users` and `/admin/users/{userId}` plus list/detail UI. Bookings exist on `IBookingRepository.ListByUserIdWithApartmentAsync` but are only exposed to the owning Client via `/bookings`. Identity write operations today are limited to register/migrate (`IIdentityUserManager`).

## Goals / Non-Goals

**Goals:**

- Admin can view any user's bookings and browse all bookings with optional `userId` filter.
- Admin can lock/unlock accounts and set roles with guardrails (last admin, valid roles).
- User detail and list UI feel like a production admin console (tabs, actions, booking counts).
- New admin booking routes under `/admin/bookings` and user sub-routes under `/admin/users`.

**Non-Goals:**

- Admin creating/cancelling bookings on behalf of users.
- Password reset or impersonation.
- Audit log table (future).
- Email send / confirm-email override (MVP).

## Decisions

### 1. API layout

**Choice:**

- `GET /admin/users/{userId}/bookings` — reuse `BookingResponseDto` list mapping.
- `GET /admin/bookings?page&pageSize&userId` — paginated `AdminBookingListItemDto` (booking id, userId, user email, apartment name, city, start, end, price).
- `POST /admin/users/{userId}/lock`, `POST .../unlock`, `PUT .../roles` body `{ "roles": ["Client","Host"] }`.

**Rationale:** Keeps user-centric and global views separate; matches existing `/admin/users` group.

### 2. Identity operations abstraction

**Choice:** Add `IAdminIdentityOperations` in Application with `LockUserAsync`, `UnlockUserAsync`, `SetRolesAsync`. Implement in `IdentityAdminOperationsAdapter` using `UserManager<ApplicationUser>` only in Infrastructure.

**Rationale:** Avoid bloating `IIdentityUserManager` used by public auth flows.

**Alternatives:** Extend `IIdentityUserManager` — couples registration adapter to admin ops.

### 3. Last-admin guard

**Choice:** In `AdminUserService.SetRolesAsync`, if resulting roles omit `Admin`, query count of users with Admin role; if target user is the only admin, return 409 Conflict with clear message.

**Rationale:** Prevents lockout from admin panel.

### 4. Lock implementation

**Choice:** `LockUserAsync` sets `LockoutEnabled = true` and `LockoutEnd = DateTimeOffset.UtcNow.AddYears(100)`. `UnlockUserAsync` clears `LockoutEnd` and optionally sets `LockoutEnabled = false`.

**Rationale:** Standard Identity pattern; UI shows locked state from existing detail fields.

### 5. Role replace semantics

**Choice:** `PUT` replaces entire role set: remove roles not in body, add missing roles. Body must include at least one role from `ApplicationRoles.All`.

**Rationale:** Simpler admin UI than per-role PATCH endpoints.

### 6. Booking queries

**Choice:** `IAdminBookingQuery` in Application; `EfAdminBookingQuery` joins Bookings + Apartments + Users (email) for global list; per-user list delegates to existing `IBookingRepository.ListByUserIdWithApartmentAsync` via service.

**Rationale:** Reuse tested booking+apartment join; separate query for cross-user pagination.

### 7. UI structure

**Choice:**

- `AdminLayout` tabs: Analytics | Users | Bookings.
- `AdminUserDetailPage`: sub-tabs Profile | Bookings; sticky actions (Lock/Unlock, role checkboxes + Save).
- `AdminUsersPage`: columns — email, roles, bookings count, status, created (if on record).
- `AdminBookingsPage`: filter by user id (text), paginated table.

**Rationale:** Familiar admin patterns; incremental over existing pages.

## Risks / Trade-offs

- **[Risk] Role replace removes Host from dual-role user unexpectedly** → UI shows current roles before save; confirm dialog optional MVP skip.
- **[Risk] Large booking table slow** → Paginate global list (default 20, max 100).
- **[Risk] Lock does not invalidate JWTs** → Document: lock affects next login; tokens expire naturally (acceptable MVP).

## Migration Plan

No schema change required unless `ApplicationUser` lacks `CreatedAt` for list column — if missing, add migration in Infrastructure (same as prior admin change pattern).

Deploy: API + UI together; no data backfill.

## Open Questions

None — scope fixed for MVP enrichment.
