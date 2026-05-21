## ADDED Requirements

### Requirement: Host documents presentation metadata

Host apartment create and update OpenAPI descriptions SHALL document optional metadata keys `propertyType`, `bedroomCount`, `bedCount`, `bathroomCount`, and `highlights` for Airbnb-style listing presentation on the public catalog.

#### Scenario: Host create with presentation metadata

- **WHEN** authenticated Host calls create with metadata containing valid presentation keys
- **THEN** the system returns 201 Created and the host response includes `metadata` with the submitted keys

#### Scenario: Host update merges presentation keys

- **WHEN** authenticated Host updates an apartment and supplies metadata with presentation keys
- **THEN** the system replaces metadata per existing JSONB replace semantics and public catalog reflects new projected fields after read
