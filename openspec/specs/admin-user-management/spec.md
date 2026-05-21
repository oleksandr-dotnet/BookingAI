# admin-user-management Specification

## Purpose
TBD - created by archiving change add-admin-user-management. Update Purpose after archive.
## Requirements
### Requirement: Admin user list

The system SHALL expose `GET /admin/users` for authenticated users with the `Admin` role. The response SHALL be a paginated JSON object with `items`, `page`, `pageSize`, and `totalCount`. Each item SHALL include `userId`, `email`, `userName`, `roles` (array of strings), `emailConfirmed`, `lockoutEnabled`, optional `lockoutEnd` (ISO 8601 when set), `firstName`, `lastName`, optional `dateOfBirth`, `profileImageUrl`, and `displayName`. The endpoint SHALL support optional query parameters: `role` (`Host`, `Client`, or `Admin`), `search` (case-insensitive email substring), `page` (default 1), `pageSize` (default 20, maximum 100), and `sort` (`email` or `created`, default `email`). The system MUST NOT include password or security-sensitive fields in the response.

#### Scenario: Admin lists all users

- **WHEN** authenticated Admin calls `GET /admin/users` without filters
- **THEN** the system returns 200 OK with paginated user items including profile display fields and `totalCount` at least the number of seeded users

#### Scenario: Admin filters by Host role

- **WHEN** authenticated Admin calls `GET /admin/users?role=Host`
- **THEN** the system returns 200 OK where every item includes `Host` in `roles`

#### Scenario: Admin searches by email

- **WHEN** authenticated Admin calls `GET /admin/users?search=admin@`
- **THEN** the system returns 200 OK with items whose `email` contains the search substring (case-insensitive)

#### Scenario: Invalid role filter

- **WHEN** authenticated Admin calls `GET /admin/users?role=SuperUser`
- **THEN** the system returns 400 Bad Request with validation errors

#### Scenario: Host denied list access

- **WHEN** authenticated user with only the Host role calls `GET /admin/users`
- **THEN** the system returns 403 Forbidden

#### Scenario: Client denied list access

- **WHEN** authenticated user with only the Client role calls `GET /admin/users`
- **THEN** the system returns 403 Forbidden

#### Scenario: Unauthenticated list access

- **WHEN** client calls `GET /admin/users` without a bearer token
- **THEN** the system returns 401 Unauthorized

### Requirement: Admin user detail

The system SHALL expose `GET /admin/users/{userId}` for authenticated users with the `Admin` role. The response SHALL include the same profile fields as list items plus optional `sourceCompanyId` and `externalId` when present on the Identity user. The system MUST NOT return password or reset-token fields.

#### Scenario: Admin views user profile

- **WHEN** authenticated Admin requests `GET /admin/users/{userId}` for an existing user
- **THEN** the system returns 200 OK with `userId`, `email`, `userName`, `roles`, `emailConfirmed`, lockout fields, migration metadata when set, and profile fields (`firstName`, `lastName`, `dateOfBirth`, `profileImageUrl`, `displayName`)

#### Scenario: Unknown user id

- **WHEN** authenticated Admin requests `GET /admin/users/{userId}` for a non-existent id
- **THEN** the system returns 404 Not Found

#### Scenario: Host denied detail access

- **WHEN** authenticated Host requests `GET /admin/users/{userId}` for any id
- **THEN** the system returns 403 Forbidden

### Requirement: Admin panel user list UI

The admin panel SHALL provide a Users page at `/admin/users` accessible only to users with the Admin role. The page SHALL display a table of users from `GET /admin/users` with columns for avatar, display name (or email), email, username, and roles. The page SHALL provide a role filter control that passes the selected role to the API. Each row SHALL link to the user detail page when the admin clicks the user's email, username, or user id.

#### Scenario: Admin opens users list

- **WHEN** authenticated Admin navigates to `/admin/users`
- **THEN** the UI shows the user table with avatars and role filter

#### Scenario: Admin filters users in UI

- **WHEN** Admin selects role `Client` in the filter and applies it
- **THEN** the UI requests `GET /admin/users?role=Client` and displays only matching users

#### Scenario: Host cannot open users list

- **WHEN** authenticated Host navigates to `/admin/users`
- **THEN** the UI shows a permission error and does not render user data

### Requirement: Admin panel user detail UI

The admin panel SHALL provide a user detail page at `/admin/users/{userId}` accessible only to Admin role users. The page SHALL load data from `GET /admin/users/{userId}` and display profile fields including avatar, legal name, birth date when set, and roles. The page SHALL include navigation back to the users list.

#### Scenario: Admin opens profile from list

- **WHEN** Admin clicks a user's email in the users table
- **THEN** the UI navigates to `/admin/users/{userId}` and shows that user's profile including avatar

#### Scenario: Admin views unknown user

- **WHEN** Admin navigates to `/admin/users/{userId}` for an id that does not exist
- **THEN** the UI shows a not-found message without exposing other users' data

### Requirement: Admin lock user account

The system SHALL expose `POST /admin/users/{userId}/lock` for authenticated Admin users. The operation SHALL enable account lockout so the user cannot sign in until unlocked. When `userId` does not exist, the system SHALL return 404 Not Found.

#### Scenario: Admin locks user

- **WHEN** authenticated Admin calls `POST /admin/users/{userId}/lock` for an existing unlocked user
- **THEN** the system returns 204 No Content and subsequent `GET /admin/users/{userId}` shows lockout active

#### Scenario: Host cannot lock user

- **WHEN** authenticated Host calls `POST /admin/users/{userId}/lock`
- **THEN** the system returns 403 Forbidden

### Requirement: Admin unlock user account

The system SHALL expose `POST /admin/users/{userId}/unlock` for authenticated Admin users. The operation SHALL clear lockout so the user may sign in again.

#### Scenario: Admin unlocks user

- **WHEN** authenticated Admin calls `POST /admin/users/{userId}/unlock` for a locked user
- **THEN** the system returns 204 No Content and lockout is cleared on profile

### Requirement: Admin set user roles

The system SHALL expose `PUT /admin/users/{userId}/roles` for authenticated Admin users with JSON body `{ "roles": string[] }`. Each role MUST be one of `Host`, `Client`, or `Admin`. The body MUST contain at least one role. The system SHALL replace the user's roles with the provided set. When the change would remove the last user with the `Admin` role from the system, the system SHALL return 409 Conflict and SHALL NOT apply the change.

#### Scenario: Admin adds Host role

- **WHEN** authenticated Admin sends `PUT /admin/users/{userId}/roles` with `["Client","Host"]` for a user who only had `Client`
- **THEN** the system returns 200 OK with updated roles including `Host`

#### Scenario: Admin cannot demote last admin

- **WHEN** only one Admin exists and Admin sends roles without `Admin` for that user
- **THEN** the system returns 409 Conflict

#### Scenario: Invalid role in body

- **WHEN** Admin sends a role name not in `Host`, `Client`, `Admin`
- **THEN** the system returns 400 Bad Request

### Requirement: Admin user list booking count

`GET /admin/users` list items SHALL include `bookingCount` (non-negative integer) for each user.

#### Scenario: List includes booking counts

- **WHEN** authenticated Admin calls `GET /admin/users`
- **THEN** each item includes `bookingCount` reflecting that user's bookings

### Requirement: Admin user management UI

The admin Users list and detail pages SHALL support account management. The detail page SHALL provide Lock and Unlock actions calling the lock/unlock endpoints. The detail page SHALL provide role selection and Save calling `PUT /admin/users/{userId}/roles`. The list page SHALL display `bookingCount` and SHALL NOT describe the area as read-only only.

#### Scenario: Admin locks user from UI

- **WHEN** Admin clicks Lock on a user profile and confirms
- **THEN** the UI calls lock endpoint and refreshes profile showing locked state

#### Scenario: Admin updates roles from UI

- **WHEN** Admin changes role checkboxes and saves
- **THEN** the UI calls roles endpoint and shows updated role badges

