## ADDED Requirements

### Requirement: Current user profile read

The system SHALL expose `GET /profile/me` for any authenticated user (Client, Host, or Admin). The response SHALL include `userId`, `email`, `userName`, `roles`, `firstName`, `lastName`, `dateOfBirth` (ISO 8601 date or null), `profileImageUrl` (or null), `displayName`, and `profileComplete` (boolean).

#### Scenario: Authenticated user reads own profile

- **WHEN** authenticated user calls `GET /profile/me`
- **THEN** the system returns 200 OK with the user's profile fields and `profileComplete` reflecting role rules

#### Scenario: Unauthenticated profile read

- **WHEN** client calls `GET /profile/me` without a bearer token
- **THEN** the system returns 401 Unauthorized

### Requirement: Current user profile update

The system SHALL expose `PUT /profile/me` for any authenticated user. The body SHALL accept optional `firstName`, `lastName`, `dateOfBirth`, and `profileImageUrl`. Validation SHALL depend on the caller's roles:

- Users with **Client** or **Host** role (and not overriding as Admin-only): `firstName`, `lastName`, and `dateOfBirth` are required on save; `dateOfBirth` MUST NOT be in the future and MUST imply age at least 13 years.
- Users with **Admin** role only (no Client/Host): `firstName` and `lastName` are required on save; `dateOfBirth` is optional.
- Users with multiple roles SHALL use the stricter Client/Host rules when they have Client or Host.

`profileImageUrl` when provided MUST be a valid HTTPS URL with length at most 2048.

#### Scenario: Client updates complete profile

- **WHEN** authenticated Client sends valid `firstName`, `lastName`, `dateOfBirth`, and `profileImageUrl`
- **THEN** the system returns 200 OK with updated profile and `profileComplete` true

#### Scenario: Host missing birth date

- **WHEN** authenticated Host sends `PUT /profile/me` without `dateOfBirth`
- **THEN** the system returns 400 Bad Request with field validation errors

#### Scenario: Admin updates without birth date

- **WHEN** authenticated Admin sends valid `firstName` and `lastName` without `dateOfBirth`
- **THEN** the system returns 200 OK and `profileComplete` true

#### Scenario: Invalid image URL

- **WHEN** client sends `profileImageUrl` that is not HTTPS or exceeds max length
- **THEN** the system returns 400 Bad Request with validation errors

### Requirement: User display card for references

The system SHALL expose `GET /users/{userId}/display` for any authenticated user. The response SHALL include `userId`, `displayName`, `profileImageUrl` (or null), and `initials` (one or two characters derived from name or email).

#### Scenario: Display card for existing user

- **WHEN** authenticated user requests `GET /users/{userId}/display` for an existing user id
- **THEN** the system returns 200 OK with display fields and no email unless the requested user is the caller (optional: omit email always for privacy)

#### Scenario: Unknown user for display

- **WHEN** authenticated user requests display for a non-existent id
- **THEN** the system returns 404 Not Found

### Requirement: Profile management UI

The application SHALL provide a profile page at `/profile` for authenticated Client, Host, and Admin users. The page SHALL load `GET /profile/me`, allow editing names, birth date (hidden or optional for Admin-only accounts), and profile photo via Cloudinary upload (reusing existing upload config). Saving SHALL call `PUT /profile/me`. The page SHALL show email as read-only.

#### Scenario: User opens profile page

- **WHEN** authenticated user navigates to `/profile`
- **THEN** the UI shows current profile fields and avatar preview

#### Scenario: User saves profile

- **WHEN** user submits valid profile data
- **THEN** the UI calls `PUT /profile/me` and updates the displayed avatar and names

### Requirement: Avatar shown across application

The UI SHALL render a shared avatar component using `profileImageUrl` when set, otherwise initials, wherever the current user or another user is referenced (header account area, admin user list, admin user detail).

#### Scenario: Header shows avatar after profile save

- **WHEN** user saves a profile image URL
- **THEN** the header account area shows the uploaded image on subsequent navigation

#### Scenario: Admin list shows user avatars

- **WHEN** Admin opens `/admin/users`
- **THEN** each row shows avatar (or initials) alongside email and display name when available

### Requirement: Profile completeness prompt

When `GET /profile/me` returns `profileComplete` false, the UI SHALL show a non-blocking banner linking to `/profile` until the profile is complete.

#### Scenario: Incomplete client sees banner

- **WHEN** Client logs in with empty profile fields
- **THEN** the UI shows a completion banner on protected pages
