## ADDED Requirements

### Requirement: Migrated apartment external identity

Apartments created by company data migration SHALL persist `SourceCompanyId` and `ExternalId` from the export. Apartments created via `POST /host/apartments` SHALL have null `SourceCompanyId` and null `ExternalId`.

#### Scenario: Migrated apartment external keys

- **WHEN** migration imports apartment `apt-101` under company `S`
- **THEN** the apartment row has `SourceCompanyId` `S` and `ExternalId` `apt-101`

#### Scenario: Host-created apartment has no external keys

- **WHEN** authenticated Host creates an apartment via API
- **THEN** `SourceCompanyId` and `ExternalId` are null

### Requirement: Migrated apartment host ownership

Migrated apartments SHALL use `hostId` equal to the migrated host's Identity user id resolved from the export host `externalId`, not from any value in the apartment JSON body.

#### Scenario: Ownership from export graph

- **WHEN** export places apartment `A` under host `H` and migration succeeds
- **THEN** apartment `A` has `hostId` matching migrated host `H` and `GET /host/apartments` for that host includes apartment `A` after login
