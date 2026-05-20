## Why

We are acquiring a smaller booking company and must import their exported customer and apartment data (JSON, up to ~5 GB) into BookingSystemAI without manual re-entry. Imported entities must remain traceable to the source company and source-system identifiers so support and future sync can resolve records reliably. A dedicated, atomic migration path reduces risk of partial imports and inconsistent host–apartment ownership.

## What Changes

- Add **external identity** fields on migrated users and apartments: `SourceCompanyId`, `ExternalId`, with a **unique composite index** `(SourceCompanyId, ExternalId)` for lookup.
- Persist **host → apartment references** from the export so each apartment remains tied to the correct migrated host.
- New **console migrator** project (`BookingSystemAI.Migrator`) that reads a JSON export file and runs a **single atomic database transaction** (all-or-nothing).
- **Register migrated users** via Identity with a configurable **default password**; assign roles from export (`Host` / `Client`).
- Application/Infrastructure support for **lookup by** `(SourceCompanyId, ExternalId)` on users and apartments (for migrator idempotency checks and future API use).
- Ship a **sample JSON export** file documenting the expected schema (small fixture for dev/tests).
- EF Core migration for new columns, indexes, and constraints.

**Non-goals:** HTTP API endpoint to trigger migration; booking history import; incremental/delta sync; password reset email flow; editing migrated records in UI; parallel multi-file merge; real 5 GB fixture in repo.

## Capabilities

### New Capabilities

- `company-data-migration`: JSON import schema, atomic migrator workflow, default-password user provisioning, host–apartment linkage, `SourceCompanyId` + `ExternalId` persistence and indexed lookup.

### Modified Capabilities

- `identity-auth`: Migrated users are created through the same Identity stack with assigned roles; requirement documents default-password provisioning and external-id storage (no change to public register/login contract).
- `host-apartments`: Migrated apartments store `SourceCompanyId` and `ExternalId`; host ownership uses migrated host’s new Identity id while preserving export linkage.

## Impact

- **New project:** `BookingSystemAI.Migrator` (console, references Application + Infrastructure).
- **Domain/Application:** optional external-id fields on entities/DTOs; `IMigrationService` or dedicated migrator orchestration; repository methods `FindByExternalAsync(sourceCompanyId, externalId)`.
- **Infrastructure:** `ApplicationUser`, `ApartmentRecord` columns; unique index; transactional import; streaming JSON reader for large files.
- **Database:** new EF migration; PostgreSQL unique constraint on `(SourceCompanyId, ExternalId)` per entity type.
- **Tests:** unit tests for mapping/validation; integration test with sample JSON against Testcontainers DB.
- **API:** no new routes in this change (lookup helpers only unless later change adds endpoints).

## Success criteria

- Running migrator with valid sample JSON creates all hosts, clients, and apartments in one transaction; re-run after success fails or no-ops per idempotency rules (documented in spec).
- Each migrated user and apartment has `SourceCompanyId` and `ExternalId`; duplicate `(SourceCompanyId, ExternalId)` within same entity type is rejected.
- Host’s apartments in DB match export references (`hostExternalId` → apartment list).
- Migrated users can log in with configured default password and correct role.
- Lookup by `(SourceCompanyId, ExternalId)` returns the correct internal id.
- Invalid JSON or referential errors roll back entire migration (no partial rows).
