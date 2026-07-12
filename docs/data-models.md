<!--
DEPRECATED — redirect to canonical home.
This file is a `bmad-document-project --mode deep` output from
2026-07-12. It is on a milestone-boundary retirement schedule
(see `.specify/memory/constitution.md` v1.1.0 § "Documentation
Systems and Source of Truth — Retirement schedule").

Canonical home: `AGENTS.md` § 3.3 "Database / DBTools integration" +
`.specs/1 - modernization-roadmap/design.md` "DBTools_SQL Integration".
-->

# Data Models — ControlEasy Reborn

**Part:** `api`
**Database:** MySQL 8.0
**Data access:** DBTools NuGet 1.4.3 — `IAsyncSqlClient` + `Linq<TModel>` via `TenantAwareLinqFactory`
**Schema source:** `docker/mysql/init/*.sql` (ordered `00-…` through `11-…`)
**Analysis date:** 2026-07-12

> **Source of truth:** the SQL init scripts in `docker/mysql/init/`. This document lists the **table families and the dual-column tenancy pattern** so an AI agent can navigate to the right repository.

## Schema Layout (per init script)

| File | Purpose |
|------|---------|
| `00-schema.sql` | Base database, root user, charset, extensions |
| `01-tenants-schema.sql` | Tenants registry (platform table, **not** tenant-scoped) |
| `02a-residents-schema.sql` | Residents, apartments (Resident ↔ Apartment association) |
| `03-tenant-backfill.sql` | Adds `tenant_id` to legacy tables; **CI-enforced marker sync** via `SchemaBackfillSyncTests` |
| `04-visits-schema.sql` | Visits, open visits view |
| `05-security-schema.sql` | Users, AttendantProfiles, RefreshTokens, Permissions |
| `06-service-providers-schema.sql` | Service providers |
| `07-vehicles-schema.sql` | Vehicles (linked to apartments) |
| `08-reports-schema.sql` | Read-side views / projections |
| `09-administration-schema.sql` | Audit log, configuration |
| `10-apartments-schema.sql` | (Phase 6) Apartments module, inline residents |
| `11-demo-seed.sql` | Demo data: sample tenants, residents, etc. |

**`SchemaBackfillSyncTests`** in `tests/ControlEasyReborn.ArchitectureTests/` enforces that every new business table appears as a commented `-- ALTER TABLE …` line in `03-tenant-backfill.sql`. Adding a module table without updating the backfill fails CI.

## Dual-Column Tenancy Pattern (Tech Debt)

Every tenant-scoped table carries **both** columns:

- `TenantId` (PascalCase, used by DBTools LINQ for inserts and by entity classes)
- `tenant_id` (snake_case, used by the `TenantFilterInterceptor` for query rewriting)

Repositories must populate **both** on insert; mismatch silently leaks across tenants.

| File | Why this pattern exists |
|------|-------------------------|
| `docker/mysql/init/02a-residents-schema.sql` | Strangler migration from legacy schema while adding interceptor-based filtering |
| `docker/mysql/init/07-vehicles-schema.sql` | Same |
| `docker/mysql/init/09-administration-schema.sql` | Same |
| `src/Modules/Tenants/.../Persistence/TenantAdminRepository.cs` | |

**Fix approach** (from `CONCERNS.md`): pick one canonical column, migrate data, update interceptor and repositories, drop the duplicate in a dedicated schema phase.

## Key Tables (families)

### Platform tables (bypass tenant filter)
- `Tenants` — tenant registry
- `Users` — auth identity (`Modules/Security/.../Persistence/UserRepository.cs` uses raw `IAsyncSqlClient`)
- `RefreshTokens` — JWT refresh tokens
- `Permissions` — fine-grained permissions catalog

### Tenant-scoped tables (use `TenantAwareLinqFactory`)
- `Residents`
- `Apartments` (with inline `ApartmentResidents` join)
- `Visits`
- `Vehicles`
- `ServiceProviders`
- `AttendantProfiles`
- `AuditLog` (Administration module)

## Entity Conventions

- **C# entity classes** live in `Modules/{Module}/.../Domain/Entities/`.
- Convention: `sealed class` with private setters and explicit constructors.
- ID: `Guid` everywhere.
- Timestamps: `DateTime.UtcNow` (e.g. `CreatedAtUtc`, `UpdatedAtUtc`).
- Tenant property: `TenantId` (PascalCase) on every tenant-scoped entity.

## Repository Pattern

```
Application/Abstractions/I{Entity}Repository.cs        # interface
Infrastructure/Persistence/{Entity}Repository.cs        # implementation
```

- Repository constructor takes `ITenantContext _ctx` + `ITenantAwareLinqFactory _factory`.
- Reads/writes go through `_factory.Create(_ctx)` which returns an `IAsyncSqlClient` wrapped in `TenantFilterInterceptor`.
- **No `IAsyncSqlClient` injection in handlers** (architecture anti-pattern).
- **No manual `WHERE tenant_id =` in repositories** (rely on interceptor).

## Migrations

- **Strategy:** Ordered SQL init scripts in `docker/mysql/init/` — run once on first MySQL start with an empty volume.
- **No migration framework** (no EF Migrations, no Flyway, no DBMate). To change the schema, write a new `*.sql` with the next sequence number and update `03-tenant-backfill.sql` markers.
- **Volume reset:** `docker compose -f docker/docker-compose.yml down -v` clears the DB; first restart re-runs all init scripts.

## Tenant Backup

- `src/Modules/Tenants/.../Persistence/TenantBackupService.cs` invokes `mysqldump` + `gzip`.
- Writes to `Backup:Path` (default `./backups/tenants/`).
- **Concerns:** (a) dumps the **entire** schema, not tenant-scoped data; (b) password passed on CLI (visible in process list). Requires host binaries (not present in current `docker/api.Dockerfile`).
- **Exposed via** `POST /api/v1/admin/backups` (PlatformAdmin only).

## Test Isolation

- `tests/ControlEasyReborn.IntegrationTests/MySqlContainerFixture.cs` spins a real `mysql:8.0` container per test class via Testcontainers.
- Replays all `docker/mysql/init/*.sql` scripts in order to materialize the schema.
- Tests share via `[Collection("MySql Collection")]`.

## Data Quality Concerns (from `CONCERNS.md`)

1. **Dual column pattern** (see above).
2. **`tenant_id` ambiguity in JOINs** — fixed for `occupiedApartments` but no regression test.
3. **No migration framework** — adding a column requires re-running init scripts on empty volumes only.
4. **In-memory dashboard aggregation** — `ReportReadRepository` loads full tables, then counts in memory; not benchmarked.
5. **DBTools error swallowing** — `AsyncSqlClient.ExecuteReaderAsync` returns empty `DataTable` on SQL errors instead of throwing (lines 389–396).

---

*Data models summary: 2026-07-12 · Sources: GSD `CONCERNS.md` + `STACK.md` + `STRUCTURE.md` (2026-06-24). For exact DDL, see `docker/mysql/init/*.sql`.*
