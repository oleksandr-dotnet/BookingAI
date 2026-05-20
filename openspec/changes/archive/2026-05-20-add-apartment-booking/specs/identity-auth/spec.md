## ADDED Requirements

### Requirement: Application roles

The system SHALL define Identity roles `Host` and `Client` and ensure both exist in the database before handling registration.

#### Scenario: Roles seeded at startup

- **WHEN** the application starts
- **THEN** roles `Host` and `Client` exist in the Identity role store

### Requirement: Role selection at registration

The system SHALL require `role` on `POST /auth/register` with value `Host` or `Client` and assign the new user to that single Identity role.

#### Scenario: Register as Host

- **WHEN** client sends valid email, password, and `"role": "Host"`
- **THEN** the system creates the user, assigns the Host role, and returns 201 Created with the user identifier

#### Scenario: Register as Client

- **WHEN** client sends valid email, password, and `"role": "Client"`
- **THEN** the system creates the user, assigns the Client role, and returns 201 Created with the user identifier

#### Scenario: Invalid role

- **WHEN** client sends a `role` value other than `Host` or `Client`, or omits `role`
- **THEN** the system returns 400 Bad Request with validation errors
