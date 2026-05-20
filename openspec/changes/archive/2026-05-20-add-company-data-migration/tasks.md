## 1. Data model and persistence

- [x] 1.1 Add `SourceCompanyId` and `ExternalId` to `ApplicationUser` and `ApartmentRecord` with EF configurations
- [x] 1.2 Add unique filtered indexes on `(SourceCompanyId, ExternalId)` for users and apartments
- [x] 1.3 Add optional fields to domain entities `User` and `Apartment` where exposed to Application layer
- [x] 1.4 Create and apply EF migration `AddCompanyMigrationExternalIds`

## 2. Application layer

- [x] 2.1 Add export DTOs (`CompanyExportDto`, host/client/apartment entries) in Application
- [x] 2.2 Add `ICompanyMigrationService` and `IExternalEntityLookup` (or repository methods) for import and lookup
- [x] 2.3 Extend `IIdentityUserManager` with migration user creation (email, default password, role, sourceCompanyId, externalId)
- [x] 2.4 Implement `CompanyMigrationService` orchestration (host map, apartments, conflict checks)
- [x] 2.5 Register services in `DependencyInjection.cs`

## 3. Infrastructure

- [x] 3.1 Implement streaming `JsonExportReader` using `Utf8JsonReader` / file stream
- [x] 3.2 Implement identity adapter methods for migrated user creation with external ids
- [x] 3.3 Implement apartment insert with `SourceCompanyId`, `ExternalId`, and resolved `HostId`
- [x] 3.4 Wrap import in `IDbContextTransaction` (atomic commit/rollback)
- [x] 3.5 Implement lookup queries by `(SourceCompanyId, ExternalId)` on users and apartments

## 4. Console migrator

- [x] 4.1 Create `BookingSystemAI.Migrator` project and add to solution
- [x] 4.2 Implement CLI (`--file`, `--default-password` or env fallback) and host bootstrap
- [x] 4.3 Wire migrator to `ICompanyMigrationService` and set exit codes
- [x] 4.4 Add `SampleData/acquired-company-export.sample.json` per design schema

## 5. Tests and verification

- [x] 5.1 Unit tests for JSON parsing edge cases and migration validation (duplicate external id)
- [x] 5.2 Integration test: run migration with sample JSON on Testcontainers PostgreSQL; assert atomic rollback on failure
- [x] 5.3 Integration test: successful migration; login host/client with default password; host lists own apartment
- [x] 5.4 Manual check: `dotnet run --project BookingSystemAI.Migrator` against local DB with sample file
