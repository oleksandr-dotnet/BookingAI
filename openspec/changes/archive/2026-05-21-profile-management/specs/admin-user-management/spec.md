## MODIFIED Requirements

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
