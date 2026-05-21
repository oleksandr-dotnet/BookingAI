## Why

Users today are identified only by email in the UI and admin tools. Hosts, clients, and admins need a self-service profile area with names, optional birth date (role-dependent), and a profile photo shown consistently wherever a user is referenced.

## What Changes

- **Persistence:** Extend Identity `ApplicationUser` with `FirstName`, `LastName`, optional `DateOfBirth`, and `ProfileImageUrl` (HTTPS URL from Cloudinary upload, same pattern as apartment photos).
- **API (authenticated):**
  - `GET /profile/me` — current user's full profile including roles and completeness flags.
  - `PUT /profile/me` — update profile fields; role-based validation (Client/Host require first name, last name, birth date; Admin requires first and last name only; birth date optional for Admin).
- **API (display):** `GET /users/{userId}/display` — minimal public card (`userId`, `displayName`, `profileImageUrl`, `initials`) for any authenticated caller; used when showing another user in lists or detail views.
- **Admin APIs:** Extend `GET /admin/users` and `GET /admin/users/{userId}` list/detail DTOs with profile fields and avatar.
- **UI:** Shared `UserAvatar` component; `/profile` page for Client, Host, and Admin with edit form and Cloudinary image upload; nav shows avatar; admin user list/detail show photos and names.
- **Extras:** `displayName` computed from names (fallback to email); `profileComplete` flag on `GET /profile/me`; prompt incomplete profiles on first visit to protected areas.

## Capabilities

### New Capabilities

- `user-profile-management`: Self-service profile APIs, display endpoint, persistence, profile UI, and avatar display across the app.

### Modified Capabilities

- `admin-user-management`: Admin list/detail responses and UI include profile image, first name, last name, and birth date when present.

## Impact

- **Domain/Application:** `User` entity fields; `IUserProfileService`, DTOs, validation by role.
- **Infrastructure:** EF migration; `IIdentityUserManager` / admin query extensions for profile fields.
- **Api:** `ProfileEndpoints`, extend admin DTO mapping.
- **UI:** `ProfilePage`, `UserAvatar`, API client, routes, admin table columns, layout account link.
- **Tests:** Application unit tests for role-based validation; integration tests for profile and display endpoints.

## Non-Goals

- Server-side file upload (client → Cloudinary → URL, existing pattern).
- Changing email or password from profile (future).
- Admin editing other users' profiles (read-only in admin panel for others).
- Profile visibility to anonymous users.

## Success Criteria

- Client/Host/Admin can open `/profile`, upload a photo, save names (and birth date per role), and see the avatar in the header.
- Admin user list and detail show avatar and legal name fields.
- `GET /users/{userId}/display` returns display data for users shown in the app.
- Incomplete Client/Host profiles are blocked or warned until required fields are saved.
