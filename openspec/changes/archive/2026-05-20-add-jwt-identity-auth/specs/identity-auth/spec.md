## ADDED Requirements

### Requirement: User registration

The system SHALL allow new users to register with an email and password via `POST /auth/register`.

#### Scenario: Successful registration

- **WHEN** client sends a valid email and password meeting Identity password rules
- **THEN** the system creates the user account and returns 201 Created with the user identifier (no password in response)

#### Scenario: Duplicate email

- **WHEN** client registers with an email already in use
- **THEN** the system returns 400 Bad Request with validation errors describing the conflict

#### Scenario: Invalid registration input

- **WHEN** client sends missing email, invalid email format, or password that fails Identity rules
- **THEN** the system returns 400 Bad Request with field-level validation errors

### Requirement: User login

The system SHALL authenticate registered users via `POST /auth/login` with email and password.

#### Scenario: Successful login

- **WHEN** client sends correct email and password for an existing user
- **THEN** the system returns 200 OK with a JWT access token and token metadata (expires in seconds)

#### Scenario: Invalid credentials

- **WHEN** client sends wrong password or unknown email
- **THEN** the system returns 401 Unauthorized without revealing whether the email exists

#### Scenario: Missing login fields

- **WHEN** client omits email or password
- **THEN** the system returns 400 Bad Request

### Requirement: User persistence

The system SHALL persist Identity users using Entity Framework Core with a database appropriate for development (SQLite).

#### Scenario: Application startup

- **WHEN** the application starts in Development
- **THEN** the database schema is applied (migrate or ensure created) so auth endpoints are usable without manual setup
