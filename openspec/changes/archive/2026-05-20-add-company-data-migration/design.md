## Context

BookingSystemAI stores users in ASP.NET Identity and apartments with `HostId` (Identity user id). There is no `ExternalId` or `SourceCompanyId` today. An acquired company delivers a JSON export (potentially up to ~5 GB) listing hosts, their apartments, and clients. We need a one-shot, atomic import that preserves source identifiers and host–apartment relationships without exposing migration through the HTTP API.

## Goals / Non-Goals

**Goals:**

- Console app `BookingSystemAI.Migrator` reads export JSON and imports in **one EF Core transaction**.
- Store `SourceCompanyId` + `ExternalId` on users and apartments; **unique index** per table on `(SourceCompanyId, ExternalId)`.
- Map export host `externalId` → new Identity user id; attach apartments with correct `HostId`.
- Create users with configurable **default password** and roles from export.
- **Streaming JSON** deserialization suitable for large files (no full-file string load).
- Repository/application helpers to resolve internal ids by `(SourceCompanyId, ExternalId)`.
- Sample export JSON under migrator project for manual and integration tests.

**Non-Goals:**

- REST endpoint to upload/trigger migration.
- Importing bookings or payments.
- Background job scheduler, S3 ingestion, or multi-tenant admin UI.
- Forcing password change on first login (can be a follow-up).
- Idempotent “upsert” re-import of the same company (first version: fail if any external id already exists for that `SourceCompanyId`).

## Decisions

### 1. Export JSON shape

**Choice:** Single root object:

```json
{
  "sourceCompanyId": "<guid>",
  "hosts": [
    {
      "externalId": "<string>",
      "email": "<string>",
      "apartments": [
        { "externalId": "<string>", "name": "<string>", "description": "<string>" }
      ]
    }
  ],
  "clients": [
    { "externalId": "<string>", "email": "<string>" }
  ]
}
```

Hosts imply role `Host`; clients imply role `Client`. Apartment ownership is explicit under each host (reference preserved in export structure).

**Rationale:** Matches “save references for hosts and their apartments”; one file carries company id and full graph.

**Alternatives:** NDJSON line-per-entity (better for 5 GB append-only) — deferred unless profiling shows root-array parsing is insufficient; can add `hosts`/`clients` as separate line-delimited files later.

### 2. Large-file reading

**Choice:** `System.Text.Json` with `Utf8JsonReader` / async stream from `FileStream` (buffered). Deserialize in passes: read `sourceCompanyId`, then stream `hosts` array elements one host at a time (and nested apartments per host).

**Rationale:** Avoids loading 5 GB into memory; built-in, no extra package.

**Alternatives:** `JsonSerializer.DeserializeAsyncEnumerable` if schema is flattened to NDJSON — not needed for v1 sample.

### 3. Atomicity

**Choice:** `await dbContext.Database.BeginTransactionAsync()` before any writes; create users (Identity) and apartments inside the same transaction; `CommitAsync` on success, `RollbackAsync` on any failure.

**Rationale:** User requirement “atomic migration”; PostgreSQL supports wrapping Identity + custom tables when enlisted in one transaction (same `ApplicationDbContext`).

**Note:** Identity user creation uses the same DbContext; if a host fails mid-batch, entire import rolls back.

### 4. External identity columns

**Choice:**

| Entity | Columns | Index |
|--------|---------|-------|
| `ApplicationUser` | `SourceCompanyId` (nullable Guid), `ExternalId` (nullable string, max 128) | Unique filtered: both NOT NULL |
| `ApartmentRecord` | same | Unique filtered: both NOT NULL |

Non-migrated rows keep both null (unaffected by unique index).

**Rationale:** Supports future lookup; nulls for organically registered users.

### 5. Default password

**Choice:** Required CLI flag `--default-password` (or environment variable `MIGRATION_DEFAULT_PASSWORD` as fallback). Same password for all migrated users in one run; not stored in JSON.

**Rationale:** Security — secret not in export file; ops supplies at run time.

### 6. Layering and project layout

**Choice:**

- `BookingSystemAI.Application`: `ICompanyMigrationService`, DTOs for export model, `IExternalEntityLookup` (or extend repositories).
- `BookingSystemAI.Infrastructure`: `CompanyMigrationService`, streaming parser in `Migration/JsonExportReader.cs`, uses `IIdentityUserManager` for user create + role assign.
- `BookingSystemAI.Migrator`: `Program.cs` — parse CLI, build host (`Host.CreateApplicationBuilder`), `AddApplication()`, `AddInfrastructure()`, invoke migration once, exit code 0/1.

**Rationale:** Keeps business rules testable; console stays thin.

**Alternatives:** All logic in console — rejected (violates clean architecture).

### 7. Duplicate / idempotency policy (v1)

**Choice:** Before insert, if any user or apartment already exists for the same `(SourceCompanyId, ExternalId)`, abort with clear error and rollback. No update-in-place.

**Rationale:** Safer for first acquisition; avoids silent partial state.

### 8. Host–apartment linkage

**Choice:** Two-phase within transaction: (1) insert all hosts, build `Dictionary<externalId, identityUserId>`; (2) insert apartments using mapped `HostId` and apartment `ExternalId`.

**Rationale:** Apartments require real Identity ids for `HostId` FK semantics.

### 9. Sample JSON location

**Choice:** `BookingSystemAI.Migrator/SampleData/acquired-company-export.sample.json` (small, committed). Integration test copies or references this file.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| 5 GB JSON with huge single array still memory-heavy if parsed wrongly | Stream per host element; document export format; optional NDJSON in future |
| Identity + EF transaction edge cases | Integration test on PostgreSQL (Testcontainers); verify rollback leaves zero migrated rows |
| Duplicate emails across companies | Unique email in Identity — migration fails with descriptive error |
| Default password shared by all users | Document ops must force reset in follow-up; not in scope |
| Long-running transaction locks tables | Acceptable for offline migration window; batch commits out of scope |

## Migration Plan

1. Deploy EF migration adding columns and unique indexes.
2. Deploy API (no behavior change for existing users).
3. Run `BookingSystemAI.Migrator` against target DB with export file path and default password (maintenance window).
4. Verify sample login for one host and one client; spot-check apartment ownership via `GET /host/apartments` after host login.

**Rollback:** Restore DB snapshot taken before migrator run; v1 does not support automatic undo.

## Open Questions

- Should acquired company `sourceCompanyId` be validated against a known registry table? **Deferred** — trust JSON + CLI for v1.
- NDJSON export variant for 5 GB production files? **Monitor** after first real export profile.
