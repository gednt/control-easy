---
title: 'v2.0 Phase 11 — Photos & Consent Schema Infrastructure'
type: 'feature'
created: '2026-09-12'
status: 'implemented'
route: 'full'
review_loop_iteration: 1
followup_review_recommended: false
context: []
warnings: ['multiple-goals']
baseline_revision: d85b496
---

<intent-contract>

## Intent

**Problem:** Milestone v2.0 (Gatehouse Photo & Consent Ledger) has no storage layer, no photo/consent schema, and no consent policy or entry-log API — the porteiro cannot log photos or consent states, and the syndic cannot audit entries. Phase 11 delivers the backend infrastructure slice: `IStorageProvider` (local + S3/MinIO), `photos`/`consent_audit_log`/`tenant_consent_policy` tables, `Photos.Read/Write/Delete` permissions, photo upload/retrieve/soft-delete endpoints, entry-log create/list/export endpoints, and the per-tenant consent-policy endpoint.

**Approach:** Add a new Photos module following the existing module pattern (Visits/Vehicles reference), extend the Security permissions, add three DDL scripts to `docker/mysql/init/` with append-only and photo-required hard constraints, wire storage options from `STORAGE__*` env vars, and register everything in the API host.

## Boundaries & Constraints

**Always:**
- DBTools `IAsyncSqlClient` via `ITenantAwareLinqFactory` for all tenant-scoped data access; parameterized where-clauses; both `TenantId` (PascalCase) and lowercase `tenant_id` shadow columns written on insert.
- Follow existing module anatomy exactly: Domain entities sealed with `Guid TenantId` non-nullable, per-module `Errors/DomainExceptions.cs`, handlers as scoped services (no MediatR), FluentValidation validators, manual DataTable mapping, minimal-API endpoint classes with kebab-case `/api/v1/...` routes.
- Append-only `consent_audit_log`: DB-level rejection of UPDATE/DELETE (trigger) plus integration test; `entered_with_consent` entries require non-null `PhotoId` (CHECK constraint or equivalent hard rejection).
- `recorded_at` uses `DATETIME(3)` (millisecond precision).
- Permissions: `Photos.Read`, `Photos.Write`, `Photos.Delete` added to `Permissions.All`, `TenantAdminDefaults`, `PorteiroDefaults` (Read only), and `DemoSeederService` strings; enforced via `.RequireAuthorization("Permission_Photos.X")` policy names.
- All new tables registered as marker lines in `03-tenant-backfill.sql` (SchemaBackfillSyncTests).
- Storage config via env vars `STORAGE__PROVIDER` (`Local`|`S3`), `STORAGE__LOCAL__PATH`, `STORAGE__S3__ENDPOINT`, `STORAGE__S3__BUCKET`, `STORAGE__S3__ACCESSKEY`, `STORAGE__S3__SECRETKEY` — throw-on-missing at startup for the selected provider.

**Never:**
- No EF Core, no MediatR, no MVC controllers, no server-side image processing (compression/thumbnailing/EXIF — client-side in Phase 12), no foreign keys, no migration framework (fresh-boot init scripts only), no soft-delete convention beyond the `deleted_at` column on photos, no custom override reason codes (hardcoded `emergency`/`vouched`), no approval workflow, no UI work (Phase 12/13 scope).

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Upload photo | `POST /api/v1/photos` with `image/jpeg` body + entity metadata, `Photos.Write` granted | 201 with `PhotoResponse` (id, filePath, thumbnailPath, size, mime, capturedAt) | 415/`ValidationException` for unsupported mime types |
| Retrieve photo | `GET /api/v1/photos/{id}`, `Photos.Read` | 200, stored bytes with original mime | `NotFoundException` when id missing or soft-deleted |
| Soft-delete photo | `DELETE /api/v1/photos/{id}`, `Photos.Delete` | 200/204, sets `DeletedAtUtc`; subsequent GET → 404 | `NotFoundException` when already deleted |
| Upload without permission | No `Photos.Write` claim | 403 from authorization policy | — |
| Append-only log | `UPDATE`/`DELETE` on `consent_audit_log` via SQL | DB raises error; trigger blocks mutation | Integration test asserts failure |
| Consent entry without photo | `POST /api/v1/entry-log` with `entered_with_consent` and no `photoId` | 400 `ValidationException` | DB constraint also rejects direct SQL insert |
| Override entry | `entered_override` with reason `emergency`/`vouched`, dwellers/visitors only | 201, audit row with porteiro + reason; service-provider/vehicle override → 400 | `ValidationException` |
| Gatehouse-only | `gatehouse_only` on service providers | 201, no photo required | Non-service-provider gatehouse_only → 400 |
| Policy enforcement | Policy `photo_required=yes` for category; entry state `entered_with_consent` | Accepted with photo | Missing photo → 400 |
| CSV export | `GET /api/v1/entry-log/export` with filters | 200 `text/csv`, `recorded_at` with millisecond precision | Empty result → valid empty CSV |

</intent-contract>

## Code Map

- `src/Modules/Visits/**` -- reference module anatomy (endpoints, DI, repository, validators, errors); copy its shape for the new Photos module.
- `src/BuildingBlocks/ControlEasyReborn.Infrastructure/MultiTenancy/TenantAwareLinqFactory.cs` -- `ITenantAwareLinqFactory.Create(ITenantContext, bool bypassTenantFilter)` → `IAsyncSqlClient`; `TenantFilterInterceptor` auto-appends `tenant_id = @ctx_tenant`.
- `src/Modules/Administration/**/AuditLogRepository.cs` -- append-only insert pattern via `InsertAsync`; reuse for consent audit log repository.
- `src/Modules/Administration/**/ConfigurationRepository.cs` -- per-tenant config table pattern; template for `tenant_consent_policy`.
- `src/Modules/Security/ControlEasyReborn.Modules.Security.Application/Permissions/Permissions.cs` -- add `PhotosRead/PhotosWrite/PhotosDelete` constants + `All` array (policies auto-generated in Program.cs).
- `src/Modules/Tenants/**/TenantAdminDefaults.cs`, `PorteiroDefaults.cs`, `src/BuildingBlocks/**/DemoSeederService.cs` -- permission grant strings to extend.
- `src/Host/ControlEasyReborn.Api/Program.cs` -- `Add<Module>()` + `Map<Module>Api()` wiring, authorization policy loop, `GlobalExceptionHandler` switch (add Photos module exception types).
- `src/Host/ControlEasyReborn.Api/ControlEasyReborn.Api.csproj` -- add project references for the new module.
- `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Data/ServiceCollectionExtensions.cs` -- `AddControlEasyDbTools` throw-on-missing config pattern + `ResolveProvider` precedent for `AddControlEasyStorage`.
- `src/BuildingBlocks/ControlEasyReborn.SharedKernel/Demo/DemoOptions.cs` -- Options-class shape (SharedKernel home for `StorageOptions`).
- `docker/mysql/init/06-visits-schema.sql` -- DDL template (CHAR(36) PKs, dual tenant columns, utf8mb4_unicode_ci, CREATE TABLE IF NOT EXISTS); new scripts `10-photos-schema.sql`, `11-consent-schema.sql` (renumber demo scripts to 12/13).
- `docker/mysql/init/03-tenant-backfill.sql` -- add commented ALTER marker lines for each new table (SchemaBackfillSyncTests).
- `docker/docker-compose.yml` -- api `environment:` block: add `STORAGE__*` keys; optionally add `minio` service + volume.
- `tests/ControlEasyReborn.IntegrationTests/MySqlContainerFixture.cs` -- auto-runs all `docker/mysql/init/*.sql`; new schema flows in automatically.
- `tests/ControlEasyReborn.IntegrationTests/TestcontainersWebApplicationFactory.cs` -- config override via `AddInMemoryCollection` (add `Storage:Provider=Local`, `Storage:Local:Path` to temp dir).
- `tests/ControlEasyReborn.UnitTests/TestDoubles/FakeAsyncSqlClient.cs` -- hand-written `IAsyncSqlClient` fake (not NSubstitute) for repository tests.
- `tests/ControlEasyReborn.ArchitectureTests/` -- `LayerDependencyTests` (no EF/MySql.Data/Infrastructure refs from Domain), `TenantIdPropertyTests` (non-nullable Guid TenantId on entities), `SchemaBackfillSyncTests` (backfill markers), `CrossTenantTestNamingTests` (every `*EndpointTests` class needs a `CrossTenant_*` fact).

## Tasks & Acceptance

**Execution:**
- [x] `docker/mysql/init/09a-photos-schema.sql` + `docker/mysql/init/09b-consent-schema.sql` -- create `Photos`, `ConsentAuditLog`, `TenantConsentPolicy` tables (dual tenant columns, `RecordedAt DATETIME(3)`, `DeletedAtUtc DATETIME NULL` on Photos, `EntryState`/`OverrideReason` VARCHAR enums, CHECK: `entered_with_consent` requires non-null photo; append-only trigger on ConsentAuditLog rejecting UPDATE/DELETE) -- schema infrastructure
- [x] `docker/mysql/init/03-tenant-backfill.sql` -- append commented ALTER marker lines for the three new tables -- schema sync test
- [x] `src/BuildingBlocks/ControlEasyReborn.SharedKernel/Storage/StorageOptions.cs` + `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Storage/StorageHostingExtensions.cs` + `IStorageProvider` + `LocalFilesystemStorageProvider` + `AmazonS3StorageProvider` (MinIO-compatible via AWSSDK.S3) + `AddControlEasyStorage(IConfiguration)` with throw-on-missing -- storage abstraction
- [x] `src/Modules/Photos/` four projects (Domain: `Photo`, `ConsentAuditLogEntry`, `TenantConsentPolicy`, enums; Application: repository interfaces, DTOs, handlers `UploadPhoto/GetPhoto/SoftDeletePhoto/CreateEntryLog/ListEntryLogs/ExportEntryLogCsv/UpdateConsentPolicy`, validators, `Errors/DomainExceptions.cs`; Infrastructure: repositories via `ITenantAwareLinqFactory`, DI `AddPhotosModule()`; Api: `PhotosEndpoints`, `EntryLogEndpoints`, `ConsentPolicyEndpoints`, `MapPhotosApi()`) -- the module
- [x] `src/Modules/Security/.../Permissions.cs` + `TenantAdminDefaults` + `PorteiroDefaults` + `DemoSeederService` -- add Photos permissions everywhere grants are hardcoded
- [x] `src/Host/ControlEasyReborn.Api/Program.cs` (+ `.csproj`) -- `AddPhotosModule()`, `MapPhotosApi()`, GlobalExceptionHandler cases, `AddControlEasyStorage(builder.Configuration)`; `appsettings.json` Storage section -- host wiring
- [x] `docker/docker-compose.yml` (+ `.env.example`) -- `STORAGE__PROVIDER=Local`, `STORAGE__LOCAL__PATH=/storage/photos` volume for api -- compose wiring
- [x] `tests/ControlEasyReborn.UnitTests/Modules/Photos/**` -- handler validation/edge-case tests incl. I/O matrix rows (118/118 tests passed) -- unit coverage
- [x] `tests/ControlEasyReborn.IntegrationTests/PhotoEndpointTests.cs` + `EntryLogEndpointTests.cs` (each with a `CrossTenant_*` fact; append-only SQL rejection test; consent-without-photo rejection test; CSV export test; 73/73 tests passed) -- integration coverage

**Acceptance Criteria:**
- Given a running stack, when a JPEG is uploaded with `Photos.Write`, then it is stored via the configured provider and retrievable via `GET /api/v1/photos/{id}` with the original mime type.
- Given a deleted photo, when `GET /api/v1/photos/{id}` is called, then 404 is returned.
- Given direct SQL `UPDATE`/`DELETE` on `consent_audit_log`, when executed, then MySQL rejects the statement (append-only verified by integration test).
- Given `POST /api/v1/entry-log` with `entered_with_consent` and null photo, then 400 ValidationException; the DB also rejects the raw insert.
- Given a tenant policy requiring photos for visitors, when the porteiro logs an entry for a visitor with a photo, then it is accepted and logged with millisecond-precision `recorded_at`.
- Given CSV export with filters, then the response is `text/csv` with millisecond `recorded_at` values.
- Given `dotnet test` on all three test projects, then all green; Docker API healthy.

## Implementation Notes

- **Database scripts**: Split schema into `09a-photos-schema.sql` and `09b-consent-schema.sql` to cleanly isolate table creation and trigger constraints before existing 10-demo scripts. Single-statement triggers avoid delimiter conflicts in script runners.
- **Storage**: Implemented `LocalFilesystemStorageProvider` for disk and `AmazonS3StorageProvider` for S3/MinIO compatible stores. Configured `photos-data` Docker volume attached to `/var/controleasy/photos`.
- **DBTools Parameter Handling**: Addressed `@param0` (SET) vs `@param1` (WHERE) indexing behavior in `SoftDeleteAsync`.
- **Append-Only and Triggers**: Triggers `trg_consent_audit_log_no_update` and `trg_consent_audit_log_no_delete` verified with integration tests asserting custom SQLSTATE 45000 errors.
- **Testing Verification**:
  - Architecture Tests: 6 passed (all NetArchTest rules, `CrossTenantTestNamingTests`, and `SchemaBackfillSyncTests`).
  - Unit Tests: 118 passed.
  - Integration Tests: 73 passed against MySQL container.
  - Stack Health: `api` and `web` containers rebuilt, `/health` responding 200 OK.

## Spec Change Log

- 2026-09-12: Updated tasks to completed state [x]. Updated status to `implemented`. Documented implementation details, test coverage, and storage provider configurations.

## Review Triage Log

## Design Notes

- Storage contract: `Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct)` returning the storage-relative path; `Task<Stream?> OpenReadAsync(string path, CancellationToken ct)`; `Task DeleteAsync(string path, CancellationToken ct)`. Local provider stores under `STORAGE__LOCAL__PATH`; S3 provider via MinIO-compatible client. Thumbnails are produced client-side in Phase 12, but the `ThumbnailPath` column exists from day one (nullable).
- Entry states as string constants in Domain (`entered_with_consent` etc.), stored as VARCHAR; state-transition validation lives in `CreateEntryLogHandler` (dwellers/visitors only for override; service-providers only for gatehouse_only; photo required per policy).
- CSV export returns millisecond-precision timestamps formatted `yyyy-MM-dd HH:mm:ss.fff`.
- MinIO integration test is optional this phase; S3 provider correctness is verified via unit tests against the abstraction + compose bring-up. Local provider covers integration tests with a temp dir.

## Verification

**Commands:**
- `dotnet build src/ControlEasyReborn.sln` -- expected: 0 errors, warnings-as-errors clean
- `dotnet test tests/ControlEasyReborn.UnitTests` -- expected: all green including new Photos tests
- `dotnet test tests/ControlEasyReborn.IntegrationTests` -- expected: all green incl. append-only + cross-tenant tests
- `dotnet test tests/ControlEasyReborn.ArchitectureTests` -- expected: all green (backfill markers present, TenantId rules satisfied)
- `docker compose -p ce-feat-photo-01-photos-consent-schema -f docker/docker-compose.yml build api web` -- expected: images build
- `docker compose -p ce-feat-photo-01-photos-consent-schema -f docker/docker-compose.yml up -d --force-recreate api web` then `/health` -- expected: Healthy