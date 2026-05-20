# apartment-upsert Specification

## Purpose
Host SQL upsert for apartments with optional JSONB metadata for integrations.

## Requirements
### Requirement: Host upserts apartment via SQL

The system SHALL expose `PUT /host/apartments/upsert` for users with the Host role, persisting apartment data using raw SQL (`INSERT ... ON CONFLICT DO UPDATE`) executed via Dapper, not Entity Framework.

#### Scenario: Insert new apartment

- **WHEN** authenticated Host sends valid body without existing `id` or external keys
- **THEN** the system returns 200 OK (or 201 Created per implementation) with apartment `id`, core fields, `metadata`, and `hostId` from the JWT

#### Scenario: Update by id

- **WHEN** authenticated Host sends body with `id` of an apartment they own
- **THEN** the system updates the row and returns 200 OK with updated fields

#### Scenario: Update by external identity

- **WHEN** authenticated Host sends `sourceCompanyId` and `externalId` matching an existing row they own
- **THEN** the system upserts on the unique external key and returns 200 OK

#### Scenario: Not owner

- **WHEN** Host sends `id` belonging to another host
- **THEN** the system returns 404 Not Found and does not modify the row

#### Scenario: Client denied

- **WHEN** authenticated Client calls `PUT /host/apartments/upsert`
- **THEN** the system returns 403 Forbidden

### Requirement: JSONB metadata on upsert

The system SHALL accept optional `metadata` JSON object on upsert and persist it to PostgreSQL `jsonb` column on `Apartments`.

#### Scenario: Metadata stored

- **WHEN** Host upserts with `metadata` `{ "channel": "partner-x", "listingCode": "LX-9" }`
- **THEN** subsequent `GET /host/apartments` for that host includes the same `metadata` object for the apartment

#### Scenario: Metadata omitted on update

- **WHEN** Host upserts an existing apartment without `metadata` in the body
- **THEN** the previous `metadata` value remains unchanged

#### Scenario: Metadata replaced when provided

- **WHEN** Host upserts with a new `metadata` object for an existing apartment
- **THEN** the stored object is replaced entirely by the new value

#### Scenario: Metadata too large

- **WHEN** Host sends `metadata` exceeding 16 KB serialized
- **THEN** the system returns 400 Bad Request

### Requirement: Upsert SQL via embedded script

The system SHALL load upsert SQL from an embedded resource file (e.g. `UpsertApartment.sql`) and execute it through Dapper.

#### Scenario: No inline upsert SQL in Api

- **WHEN** reviewing Application and Api projects
- **THEN** upsert SQL text is not defined in those layers
