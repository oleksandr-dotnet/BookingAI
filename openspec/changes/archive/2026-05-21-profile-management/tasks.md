## 1. Domain and persistence

- [x] 1.1 Add profile fields to `ApplicationUser` and EF migration (`FirstName`, `LastName`, `DateOfBirth`, `ProfileImageUrl`)
- [x] 1.2 Extend `User` domain entity and Identity adapter read/update methods for profile

## 2. Application layer

- [x] 2.1 Add profile DTOs (`UserProfileDto`, `UpdateUserProfileRequestDto`, `UserDisplayDto`)
- [x] 2.2 Add `IUserProfileService` / `UserProfileService` with role-based validation and `displayName` / `profileComplete` logic
- [x] 2.3 Extend `AdminUserRecord` and admin DTOs/mapping with profile fields

## 3. API layer

- [x] 3.1 Add `ProfileEndpoints` (`GET/PUT /profile/me`, `GET /users/{userId}/display`)
- [x] 3.2 Register endpoints in `Program.cs`
- [x] 3.3 Update admin user query mapping for profile fields

## 4. Tests

- [x] 4.1 Application unit tests for `UserProfileService` validation (Client, Host, Admin)
- [x] 4.2 Integration tests for profile and display endpoints

## 5. Frontend

- [x] 5.1 Add API types and `profile` API client functions
- [x] 5.2 Add `UserAvatar` component and profile context/banner for completeness
- [x] 5.3 Add `ProfilePage` at `/profile` with Cloudinary upload
- [x] 5.4 Update `Layout` nav with avatar link to profile
- [x] 5.5 Update admin users list and detail with avatars and name fields

## 6. Verification

- [x] 6.1 Run `dotnet test` (Application + Integration)
- [x] 6.2 Run `npm run build` in `booking-system-ui`
