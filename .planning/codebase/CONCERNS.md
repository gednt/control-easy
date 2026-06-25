# Codebase Concerns

**Analysis Date:** 2026-06-24

## Tech Debt

**Vendored DBTools_SQL copy in-repo:**
- Issue: The full `DBTools_SQL` library lives at `src/lib/DBTools_SQL/DBTools/` as a vendored copy rather than a NuGet package reference. Upstream fixes and security patches require manual sync.
- Files: `src/lib/DBTools_SQL/DBTools/`, `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Data/ServiceCollectionExtensions.cs`
- Why: Greenfield bootstrap needed a working data-access layer before upstream packaging was wired.
- Impact: Drift from upstream `DBTools_SQL`; duplicate maintenance burden; deprecated APIs remain callable (e.g. `[Obsolete]` methods in `src/lib/DBTools_SQL/DBTools/Core/DBTools.cs`).
- Fix approach: Replace vendored tree with a pinned package or git submodule; run regression suite (`tests/ControlEasyReborn.IntegrationTests/`, architecture tests) after swap.

**Dual tenant discriminator columns (`TenantId` + `tenant_id`):**
- Issue: Every tenant-scoped table defines both PascalCase `TenantId` and snake_case `tenant_id`. Inserts must populate both; `TenantFilterInterceptor` only filters on `tenant_id`.
- Files: `docker/mysql/init/02a-residents-schema.sql`, `docker/mysql/init/07-vehicles-schema.sql`, `docker/mysql/init/09-administration-schema.sql`, `src/Modules/Tenants/ControlEasyReborn.Modules.Tenants.Infrastructure/Persistence/TenantAdminRepository.cs`
- Why: Strangler migration from legacy schema while adding interceptor-based filtering.
- Impact: Easy to write rows with mismatched values; redundant indexes; every repository insert lists both columns explicitly.
- Fix approach: Pick one canonical column, migrate data, update interceptor and repositories, drop the duplicate column in a dedicated schema phase.

**Split data-access registration (filtered vs unfiltered clients):**
- Issue: Tenant-aware repositories use `ITenantAwareLinqFactory` (with `TenantFilterInterceptor`); auth/platform repos inject raw `IAsyncSqlClient` without the interceptor.
- Files: `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Data/ServiceCollectionExtensions.cs`, `src/Modules/Security/ControlEasyReborn.Modules.Security.Infrastructure/Persistence/UserRepository.cs`, `src/Modules/Security/ControlEasyReborn.Modules.Security.Infrastructure/Persistence/RefreshTokenRepository.cs`, `src/Modules/Tenants/ControlEasyReborn.Modules.Tenants.Infrastructure/Persistence/TenantAdminRepository.cs`
- Why: Login, refresh-token lookup, and platform bootstrap must work before tenant context is resolved.
- Impact: New repositories can accidentally use the wrong client and bypass or over-apply tenant isolation. No architecture test enforces the split.
- Fix approach: Introduce named abstractions (`IPlatformSqlClient`, `ITenantSqlClient`) and a NetArchTest rule that Domain/Infrastructure repositories use the correct one.

**Hand-written Angular API clients (OpenAPI gen not wired):**
- Issue: `ng-openapi-gen.json` targets `src/app/api`, but that directory is absent. Each feature ships a manual `*-api.service.ts` with duplicated DTO shapes.
- Files: `src/Web/ControlEasyReborn.Web/ng-openapi-gen.json`, `src/Web/ControlEasyReborn.Web/src/app/features/residents/residents-api.service.ts`, `src/Web/ControlEasyReborn.Web/src/app/core/services/security-api.service.ts`, `docker/web.Dockerfile`
- Why: Early vertical slices prioritized shipping pages over build-pipeline integration.
- Impact: API contract drift between backend DTOs and frontend types; duplicate maintenance on every endpoint change.
- Fix approach: Add `openapi-gen` step to CI/Docker build (fetch Swagger from running API or checked-in spec), commit or generate at build time, migrate feature services to generated client.

**Design system CSS duplication and token namespace split:**
- Issue: Design-system components use `--space-*` tokens; 15+ layout/feature files use `--spacing-*`. Feature pages copy-paste `.ce-button`, `.ce-table`, `.ce-input-group` styles instead of importing components.
- Files: `src/Web/ControlEasyReborn.Web/src/styles.css`, `src/Web/ControlEasyReborn.Web/src/app/features/login.page.ts`, `src/Web/ControlEasyReborn.Web/src/app/features/residents/residents.page.ts`, `.specs/fix-design-system/requirements.md`
- Why: Rapid page delivery before design-system adoption was enforced.
- Impact: Visual inconsistencies, dark-theme breakage from hardcoded `color: white` / `#ec4899`, large diffs when tokens change.
- Fix approach: Execute `.specs/fix-design-system/` — standardize on `--space-*`, refactor pages to use `src/Web/ControlEasyReborn.Web/src/app/design-system/components/*`.

**Strangler feature flags default off:**
- Issue: All `FeatureManagement.*.UseWeb` flags are `false` in `src/Host/ControlEasyReborn.Api/appsettings.json`. Legacy WPF/Blazor codebases are already removed from the repo.
- Files: `src/Host/ControlEasyReborn.Api/appsettings.json`, `src/BuildingBlocks/ControlEasyReborn.SharedKernel/FeatureFlags/FeatureFlags.cs`, `tests/ControlEasyReborn.IntegrationTests/FeatureFlagTests.cs`
- Why: Flags were scaffolded for gradual cutover that never started in-repo.
- Impact: Dead configuration surface; `AGENTS.md` still describes a WPF strangler that no longer exists in this repository.
- Fix approach: Either remove unused flags and update docs, or wire flags to route legacy operators (if WPF lives elsewhere) and document the cutover plan.

**HTML mockup coexists with Angular SPA:**
- Issue: Static prototype at `mockup/` (e.g. `mockup/app.html`, `mockup/assets/app.js`) runs parallel to the production Angular app.
- Files: `mockup/`, `src/Web/ControlEasyReborn.Web/`
- Why: Design exploration artifact retained for smoke checks (`mockup/SMOKE.md`).
- Impact: Two UI sources of truth; mockup can drift from implemented routes in `src/Web/ControlEasyReborn.Web/src/app/app.routes.ts`.
- Fix approach: Treat mockup as archived reference or sync via `.specs/mockup-visual-parity/`; avoid using mockup for new feature design.

**Spec task checkboxes out of sync with code:**
- Issue: `.specs/4 - demo-mode/tasks.md` marks demo tasks open (`[ ]`), but `DemoSeederService`, `DemoModeTests`, and demo Docker overlay already exist.
- Files: `.specs/4 - demo-mode/tasks.md`, `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Demo/DemoSeederService.cs`, `tests/ControlEasyReborn.IntegrationTests/DemoModeTests.cs`, `docker/docker-compose.demo.yml`
- Why: Implementation landed without spec housekeeping.
- Impact: Orchestration and planning agents mis-estimate remaining work; duplicate effort risk.
- Fix approach: Reconcile `.specs/*/tasks.md` against the codebase after each phase.

**Legacy ControlEasy5 / ControlEasyWeb absent from working tree:**
- Issue: `AGENTS.md` and several `.specs/` docs still reference `ControlEasy5/` (WPF) and `ControlEasyWeb/` (Blazor .NET 5), but those directories are not present in the repository.
- Files: `AGENTS.md`, `.specs/1 - modernization-roadmap/design.md`, `agents/agents/FrontendAgent/AGENTS.md`
- Why: Legacy code removed or never imported into this repo; docs not fully updated.
- Impact: Onboarding confusion; agents may search for non-existent paths.
- Fix approach: Update root docs to state legacy lives outside this repo (or link to archive); remove stale strangler references.

## Known Bugs

**Occupied-apartments dashboard stat (fixed, test gap remains):**
- Symptoms: `occupiedApartments` on dashboard returned `0` when JOIN queries hit ambiguous `tenant_id` under the interceptor.
- Trigger: `GET /api/v1/dashboard/stats` with apartments that have active residents.
- Files: `src/Modules/Reports/ControlEasyReborn.Modules.Reports.Infrastructure/Persistence/ReportReadRepository.cs` (lines 98–104), `.specs/occupied-apartments-bug/bugfix.md`
- Workaround: N/A — code now uses aliased `a.tenant_id` / `r.tenant_id` in raw SQL.
- Root cause: `TenantFilterInterceptor` injected unqualified `tenant_id` into multi-table JOINs; `AsyncSqlClient.ExecuteReaderAsync` swallowed the MySQL 1052 error and returned an empty `DataTable`.
- Blocked by: Missing regression test — no test references `dashboard/stats` or `occupiedApartments` under `tests/`.

**PlatformAdmin first-boot login without attendant profile (fixed):**
- Symptoms: Fresh non-demo stack seeded PlatformAdmin user but login returned `401` — `"No active attendant profile found for user."`
- Trigger: First boot with `Demo__Enabled=false`, login via `POST /api/v1/security/auth/login`.
- Files: `src/Host/ControlEasyReborn.Api/Hosting/PlatformAdminBootstrapService.cs`, `.specs/platform-admin-first-boot/bugfix.md`
- Workaround: Manually insert `AttendantProfiles` row.
- Root cause: Bootstrap inserted `Users` only; `LoginHandler` requires an active attendant profile.
- Fix: Bootstrap now inserts matching `AttendantProfiles` row (lines 73–83). Covered by `tests/ControlEasyReborn.UnitTests/Hosting/PlatformAdminBootstrapServiceTests.cs` and `src/Web/ControlEasyReborn.Web/e2e/first-boot-login.spec.ts`.

## Security Considerations

**Default JWT signing key in source and Compose:**
- Risk: Predictable signing key allows token forgery in dev/staging if env override is missed.
- Files: `src/Host/ControlEasyReborn.Api/appsettings.json`, `docker/docker-compose.yml` (line 30)
- Current mitigation: `docker/.env.example` documents `JWT_SIGNING_KEY`; production must override via environment.
- Recommendations: Fail fast at startup when `ASPNETCORE_ENVIRONMENT=Production` and key matches placeholder; never log signing key.

**PlatformAdmin bootstrap credentials logged in plaintext:**
- Risk: Container logs expose live admin email/password on first boot.
- Files: `src/Host/ControlEasyReborn.Api/Hosting/PlatformAdminBootstrapService.cs` (line 86)
- Current mitigation: `MustChangePassword=true` on seeded user; E2E expects change-password flow.
- Recommendations: Log email only; emit one-time credential via secure channel or `GET /api/v1/security/bootstrap` (already consumed by `e2e/first-boot-login.spec.ts`).

**Demo mode fixed password:**
- Risk: Well-known password `demo123` for all demo personas when demo stack is enabled.
- Files: `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Demo/DemoIds.cs`, `docker/docker-compose.demo.yml`
- Current mitigation: Demo disabled by default (`Demo:Enabled: false` in `appsettings.json`); demo overlay is dev-only.
- Recommendations: Block demo overlay in production Compose profiles; document never deploy demo to public networks.

**JWT and refresh tokens in browser `localStorage`:**
- Risk: XSS can exfiltrate tokens; no `httpOnly` cookie option.
- Files: `src/Web/ControlEasyReborn.Web/src/app/core/services/auth.service.ts`
- Current mitigation: Short access-token TTL (15 min in `appsettings.json`); refresh rotation in `RefreshHandler`.
- Recommendations: Evaluate `httpOnly` secure cookies for refresh tokens; tighten CSP on nginx (`docker/nginx.conf`).

**Adminer and Traefik dashboard exposed in dev Compose:**
- Risk: Database UI at `/db` and Traefik API on port `8082` with `--api.insecure=true` — no auth layer.
- Files: `docker/docker-compose.yml`, `docker/reverse-proxy/traefik.yml`
- Current mitigation: Localhost-only ports in default Compose.
- Recommendations: Remove Adminer from production overlay; protect Traefik dashboard or disable in prod.

**No rate limiting on authentication endpoints:**
- Risk: Credential stuffing and brute-force against `POST /api/v1/security/auth/login`.
- Files: `src/Modules/Security/ControlEasyReborn.Modules.Security.Api/Endpoints/SecurityEndpoints.cs`
- Current mitigation: Generic unauthorized responses; no lockout policy detected.
- Recommendations: Add ASP.NET Core rate limiting middleware or reverse-proxy throttling on `/api/v1/security/*`.

**Tenant backup dumps entire database:**
- Risk: `TenantBackupService` runs `mysqldump` on the full `controleasydb` schema, not tenant-scoped data; password passed on CLI (visible in process list).
- Files: `src/Modules/Tenants/ControlEasyReborn.Modules.Tenants.Infrastructure/Persistence/TenantBackupService.cs`, `src/Modules/Tenants/ControlEasyReborn.Modules.Tenants.Api/Endpoints/TenantEndpoints.cs`
- Current mitigation: Endpoint under `/api/v1/admin/backups` (PlatformAdmin policy).
- Recommendations: Tenant-scoped export (per-table `WHERE tenant_id = …`); use MySQL config file for credentials instead of `--password=` on argv.

**Silent SQL error swallowing in DBTools client:**
- Risk: SQL failures return empty results instead of exceptions — can mask injection/ambiguity bugs and hide data leaks.
- Files: `src/lib/DBTools_SQL/DBTools/Core/AsyncSqlClient.cs` (lines 389–396), `src/lib/DBTools_SQL/DBTools/Core/SqlClient.cs` (lines 230–233)
- Current mitigation: `_error` field set internally; callers rarely check it.
- Recommendations: Propagate exceptions in application layer or add opt-in strict mode; log errors at minimum.

## Performance Bottlenecks

**Dashboard stats full-table scans in memory:**
- Problem: `GetDashboardStatsAsync` loads all residents, vehicles, apartments, and visits into `DataTable`, then counts/filters in process.
- Files: `src/Modules/Reports/ControlEasyReborn.Modules.Reports.Infrastructure/Persistence/ReportReadRepository.cs` (lines 65–157)
- Measurement: Not benchmarked in-repo; complexity is O(n) per table per request.
- Cause: No SQL `COUNT(*)` / `GROUP BY` aggregation; multiple sequential queries.
- Improvement path: Replace with single aggregated SQL or DBTools grouped queries; add Redis caching for dashboard snapshot (planned in `AGENTS.md`, not implemented).

**Visit report client-side aggregation:**
- Problem: `GetVisitCountsByDayAsync` fetches all `CreatedAtUtc` rows then groups in LINQ-to-objects.
- Files: `src/Modules/Reports/ControlEasyReborn.Modules.Reports.Infrastructure/Persistence/ReportReadRepository.cs` (lines 20–41)
- Measurement: Not benchmarked; scales linearly with visit volume.
- Cause: Convenience over SQL `DATE()` grouping.
- Improvement path: Push grouping to SQL (`SELECT DATE(CreatedAtUtc), COUNT(*) … GROUP BY 1`).

## Fragile Areas

**TenantFilterInterceptor + raw SQL JOINs:**
- Why fragile: Interceptor skips injection when `tenant_id` appears anywhere in SQL (string contains check). Multi-table JOINs without aliases can still break; wrong alias pattern reintroduces silent empty results.
- Files: `src/BuildingBlocks/ControlEasyReborn.Infrastructure/MultiTenancy/TenantFilterInterceptor.cs`, `src/Modules/Reports/ControlEasyReborn.Modules.Reports.Infrastructure/Persistence/ReportReadRepository.cs`
- Common failures: MySQL "Column 'tenant_id' in where clause is ambiguous"; empty dashboard stats.
- Safe modification: Always alias tables in raw SQL; include explicit `a.tenant_id` and `r.tenant_id`; add integration test before new raw queries.
- Test coverage: Unit tests in `tests/ControlEasyReborn.UnitTests/MultiTenancy/TenantFilterInterceptorTests.cs`; no JOIN regression in integration suite for dashboard stats.

**MySQL init script + tenant backfill sync:**
- Why fragile: `SchemaBackfillSyncTests` requires every `CREATE TABLE` to appear in commented markers in `03-tenant-backfill.sql`. New tables without markers fail CI.
- Files: `docker/mysql/init/*.sql`, `tests/ControlEasyReborn.ArchitectureTests/SchemaBackfillSyncTests.cs`
- Common failures: Adding a module table without updating backfill markers.
- Safe modification: Add table to init script and commented `-- ALTER TABLE …` line in `03-tenant-backfill.sql`; exempt platform tables explicitly.
- Test coverage: Architecture test covers sync; no runtime migration framework (init scripts only).

**Serilog bootstrap before host build:**
- Why fragile: Pre-host `appsettings.json` load can crash test hosts if working directory differs.
- Files: `src/Host/ControlEasyReborn.Api/Program.cs` (lines 35–42), `.specs/6 - verification-remediation/demo-host-root-cause.md`
- Common failures: `WebApplicationFactory` tests fail with "entry point exited without building IHost".
- Safe modification: Keep bootstrap config `optional: true` (current state); ensure test factory sets content root.
- Test coverage: `tests/ControlEasyReborn.IntegrationTests/DemoModeTests.cs` exercises demo host boot.

**PlatformAdmin bootstrap idempotency:**
- Why fragile: Skip path when PlatformAdmin exists does not verify attendant profile presence (partial state from older builds).
- Files: `src/Host/ControlEasyReborn.Api/Hosting/PlatformAdminBootstrapService.cs` (lines 47–51)
- Common failures: Upgraded DB with user but no profile → login still fails.
- Safe modification: On skip, verify profile exists or run repair insert.
- Test coverage: `tests/ControlEasyReborn.UnitTests/Hosting/PlatformAdminBootstrapServiceTests.cs` covers happy path only.

## Scaling Limits

**Single MySQL instance (Compose default):**
- Current capacity: One `mysql:8.0` container, all tenants in shared schema.
- Limit: Write throughput and connection pool (~default 151 connections) become bottleneck before app tier.
- Symptoms at limit: Slow reports, connection timeouts, dashboard timeouts.
- Scaling path: Read replicas for reports module; connection pooling tuning; eventual per-tenant export/import for large customers.

**No caching layer:**
- Current capacity: Every request hits MySQL; no Redis service in `docker/docker-compose.yml`.
- Limit: Repeated dashboard/report queries amplify load linearly with concurrent portaria users.
- Symptoms at limit: Elevated API latency on `/api/v1/dashboard/stats` and report endpoints.
- Scaling path: Add Redis + cache invalidation on writes per module (as specified in target architecture).

**In-memory dashboard aggregation:**
- Current capacity: Suitable for demo-scale data (tens–low hundreds of rows per tenant).
- Limit: Thousands of visits/residents per tenant will increase latency and memory per request.
- Symptoms at limit: Multi-second dashboard loads, GC pressure on API container.
- Scaling path: SQL-side aggregation and pagination for recent visits (currently hard-coded `.Take(10)` in `ReportReadRepository.cs`).

## Dependencies at Risk

**Unpinned `datalust/seq:latest` image:**
- Risk: Breaking image updates on rebuild.
- Impact: Logging pipeline fails; API may still run but observability breaks.
- Migration plan: Pin to specific Seq version tag in `docker/docker-compose.yml`.

**Angular 18 / .NET 8 LTS pairing:**
- Risk: Low near-term; both are LTS-aligned today.
- Impact: Future major upgrades (Angular 19+, .NET 10) require coordinated frontend/backend CI matrix.
- Migration plan: Track LTS schedules; keep `global.json` and `src/Directory.Build.props` SDK pins current.

**Legacy Entity Framework 6 / .NET Framework 4.8 (external):**
- Risk: If operators still run legacy WPF outside this repo, parallel schema evolution can diverge from `docker/mysql/init/*.sql`.
- Impact: Data migration surprises during cutover.
- Migration plan: Document single schema source (`docker/mysql/init/`) as Reborn authority; freeze legacy schema changes.

## Missing Critical Features

**Photo capture and hardware integration:**
- Problem: No `IStorageProvider`, camera capture, MQTT device framework, or WebSocket alerts — entire spec unimplemented.
- Files: `.specs/3 - photo-capture-hardware-integration/requirements.md` (35 open tasks)
- Current workaround: None in web UI.
- Blocks: Gatehouse identity verification with photos; IoT device integration.
- Implementation complexity: High (storage abstraction, permissions, MQTT, media proxy).

**CI/CD pipeline:**
- Problem: No GitHub Actions workflow under `.github/` (only `copilot-instructions.md`).
- Current workaround: Manual `dotnet test` and Docker rebuild per `AGENTS.md` post-task gate.
- Blocks: Automated regression on PRs; reproducible deployments.
- Implementation complexity: Medium (matrix for API, web, integration tests with Testcontainers).

**Email / outbound notifications:**
- Problem: `Demo:DisableOutboundEmail` flag exists in config but no email provider integration detected.
- Files: `src/Host/ControlEasyReborn.Api/appsettings.json`
- Current workaround: None.
- Blocks: Password reset emails, visit notifications, admin alerts.
- Implementation complexity: Medium.

**OpenAPI-driven frontend contract:**
- Problem: `npm run openapi-gen` not part of `docker/web.Dockerfile` build.
- Files: `src/Web/ControlEasyReborn.Web/package.json`, `docker/web.Dockerfile`
- Current workaround: Manual DTO duplication in `*-api.service.ts` files.
- Blocks: Contract-safe API evolution at scale.
- Implementation complexity: Low–medium (CI step + refactor imports).

## Test Coverage Gaps

**Dashboard stats / occupied apartments:**
- What's not tested: `GET /api/v1/dashboard/stats` response correctness, especially `occupiedApartments`.
- Files: `tests/ControlEasyReborn.IntegrationTests/ReportEndpointTests.cs` (covers other report routes only)
- Risk: JOIN/interceptor regressions return silently wrong counts again.
- Priority: High
- Difficulty to test: Low — extend existing Testcontainers factory pattern.

**Reports module handlers:**
- What's not tested: `GetDashboardStatsHandler`, `GetVisitCountsByDayHandler` unit tests.
- Files: `src/Modules/Reports/ControlEasyReborn.Modules.Reports.Application/Handlers/`
- Risk: Handler logic changes untested.
- Priority: Medium
- Difficulty to test: Low with `FakeAsyncSqlClient` (`tests/ControlEasyReborn.UnitTests/TestDoubles/FakeAsyncSqlClient.cs`).

**Vehicles, ServiceProviders, Administration application layer:**
- What's not tested: No unit tests under `tests/ControlEasyReborn.UnitTests/Modules/` for these modules (integration tests exist for endpoints).
- Files: `tests/ControlEasyReborn.IntegrationTests/VehicleEndpointTests.cs`, `tests/ControlEasyReborn.IntegrationTests/ServiceProviderEndpointTests.cs`, `tests/ControlEasyReborn.IntegrationTests/AdministrationEndpointTests.cs`
- Risk: Repository/handler regressions caught only by slow integration tests.
- Priority: Medium
- Difficulty to test: Low — follow `ResidentRepositoryTests.cs` pattern.

**Tenant backup service:**
- What's not tested: `TenantBackupService.CreateBackupAsync` — no unit or integration tests; depends on host `mysqldump`/`gzip` binaries.
- Files: `src/Modules/Tenants/ControlEasyReborn.Modules.Tenants.Infrastructure/Persistence/TenantBackupService.cs`
- Risk: Backup endpoint fails silently in API container (tools missing) or produces full-DB dumps unexpectedly.
- Priority: Medium
- Difficulty to test: Medium — requires process mocking or container test with CLI tools.

**Frontend feature pages (beyond design system):**
- What's not tested: Feature pages (`residents.page.ts`, `visits.page.ts`, etc.) have no component specs; only design-system components and Playwright e2e cover UI.
- Files: `src/Web/ControlEasyReborn.Web/src/app/features/`, `src/Web/ControlEasyReborn.Web/e2e/*.spec.ts`
- Risk: Regressions in forms/tables caught only by e2e (requires running stack).
- Priority: Medium
- Difficulty to test: Medium — Angular TestBed + HTTP mocking.

**Demo integration suite runtime fragility:**
- What's not tested reliably: Full `DemoModeTests` suite under all local runtimes (historical net10 TestHost `PipeWriter.UnflushedBytes` issue documented).
- Files: `tests/ControlEasyReborn.IntegrationTests/DemoModeTests.cs`, `.specs/6 - verification-remediation/final-report.md`
- Risk: Demo regressions slip through on certain SDK/runtime combinations.
- Priority: Medium
- Difficulty to test: Environment-dependent — needs CI matrix on net8.0 (current `src/Directory.Build.props` target).

---

*Concerns audit: 2026-06-24*
*Update as issues are fixed or new ones discovered*
