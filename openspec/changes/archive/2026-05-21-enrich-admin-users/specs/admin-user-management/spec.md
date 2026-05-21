## ADDED Requirements

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

## MODIFIED Requirements

### Requirement: Admin panel user detail UI

The admin panel SHALL provide a user detail page at `/admin/users/{userId}` accessible only to Admin role users. The page SHALL load data from `GET /admin/users/{userId}` and display profile fields and roles. The page SHALL include navigation back to the users list. The page SHALL provide Profile and Bookings tabs. The page SHALL include account management actions (lock, unlock, role editor).

#### Scenario: Admin opens profile from list

- **WHEN** Admin clicks a user's email in the users table
- **THEN** the UI navigates to `/admin/users/{userId}` and shows that user's profile on the Profile tab

#### Scenario: Admin views unknown user

- **WHEN** Admin navigates to `/admin/users/{userId}` for an id that does not exist
- **THEN** the UI shows a not-found message without exposing other users' data

#### Scenario: Admin opens bookings tab

- **WHEN** Admin selects the Bookings tab on user detail
- **THEN** the UI loads and displays that user's bookings

### Requirement: Admin panel user list UI

The admin panel SHALL provide a Users page at `/admin/users` accessible only to users with the Admin role. The page SHALL display a table of users from `GET /admin/users` with columns for identifier, email, username, roles, booking count, and status. The page SHALL provide a role filter control that passes the selected role to the API. Each row SHALL link to the user detail page when the admin clicks the user's email, username, or user id.

#### Scenario: Admin opens users list

- **WHEN** authenticated Admin navigates to `/admin/users`
- **THEN** the UI shows the user table and role filter

#### Scenario: Admin filters users in UI

- **WHEN** Admin selects role `Client` in the filter and applies it
- **THEN** the UI requests `GET /admin/users?role=Client` and displays only matching users

#### Scenario: Host cannot open users list

- **WHEN** authenticated Host navigates to `/admin/users`
- **THEN** the UI shows a permission error and does not render user data
