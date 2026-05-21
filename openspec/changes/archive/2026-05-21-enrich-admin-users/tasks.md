## 1. Application layer

- [x] 1.1 Add `AdminBookingListItemDto`, `AdminBookingListQuery`, `SetUserRolesRequestDto`; extend `AdminUserListItemDto` with `BookingCount`
- [x] 1.2 Add `IAdminIdentityOperations` and `IAdminBookingQuery` abstractions
- [x] 1.3 Extend `IAdminUserService` with `ListUserBookingsAsync`, `LockUserAsync`, `UnlockUserAsync`, `SetRolesAsync`; add `IAdminBookingService` for global list
- [x] 1.4 Implement service methods with last-admin guard and validation
- [x] 1.5 Register new services in `AddApplication()`

## 2. Infrastructure layer

- [x] 2.1 Implement `IdentityAdminOperationsAdapter` (lock, unlock, set roles)
- [x] 2.2 Implement `EfAdminBookingQuery` and extend `IdentityAdminUserQuery` for `bookingCount` on list
- [x] 2.3 Register adapters in `DependencyInjection.cs`

## 3. API layer

- [x] 3.1 Extend `AdminUserEndpoints` with bookings, lock, unlock, roles routes
- [x] 3.2 Add `AdminBookingEndpoints` with `GET /admin/bookings`
- [x] 3.3 Map endpoints in `Program.cs` with OpenAPI summaries

## 4. Tests

- [x] 4.1 Unit tests: `AdminUserService` lock/roles/last-admin; `AdminBookingService` pagination
- [x] 4.2 Integration tests: admin bookings, user bookings, lock/unlock, roles 409, Host 403

## 5. React admin UI

- [x] 5.1 Extend `adminUsers.ts` and add `adminBookings.ts` API clients
- [x] 5.2 Update types for booking count and admin booking rows
- [x] 5.3 Add `AdminBookingsPage`; extend `AdminLayout` with Bookings tab
- [x] 5.4 Enrich `AdminUsersPage` (booking count, copy)
- [x] 5.5 Enrich `AdminUserDetailPage` (tabs, lock/unlock, roles, bookings table)
- [x] 5.6 Wire routes in `App.tsx`

## 6. Verification

- [x] 6.1 `dotnet test` Application + Integration tests
- [x] 6.2 `npm run build` in booking-system-ui
- [x] 6.3 Browser check: admin users detail actions and bookings pages
