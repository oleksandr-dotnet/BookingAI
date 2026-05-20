## ADDED Requirements

### Requirement: Role claims in JWT

The system SHALL include the user's Identity role names as role claims in the JWT issued on successful login.

#### Scenario: Host token contains Host role

- **WHEN** a user with the Host role logs in successfully
- **THEN** the JWT contains a role claim with value `Host` usable by authorization policies

#### Scenario: Client token contains Client role

- **WHEN** a user with the Client role logs in successfully
- **THEN** the JWT contains a role claim with value `Client` usable by authorization policies
