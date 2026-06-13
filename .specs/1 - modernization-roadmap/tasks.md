# Tasks — Modernization Roadmap

> Companion to `requirements.md` and `design.md`. Each task is a single, verifiable unit of work. **Phases must be completed in order**; a phase gate (last task of the phase) must be green before starting the next phase.

## Task Dependency Graph

The roadmap is decomposed into execution **waves** for parallel work. Waves execute strictly in order: wave *N+1* starts only after **every** task in wave *N* is complete and its verification gate is green. Tasks inside a wave have **no** inter-dependencies and can be picked up by separate agents in parallel. A single agent (or human) may also execute them serially in any order.

Continuous tasks (C.1–C.7) are tracked separately and run alongside every phase — they are not in the wave graph below.

### Phase 1 — Foundation

```json
{
  "phase": 1,
  "waves": [
    { "wave": 1, "tasks": ["1.1", "1.0a"] },
    { "wave": 2, "tasks": ["1.2", "1.3", "1.0b", "1.0c"] },
    { "wave": 3, "tasks": ["1.4", "1.5", "1.6", "1.16"] },
    { "wave": 4, "tasks": ["1.7", "1.8", "1.12", "1.17"] },
    { "wave": 5, "tasks": ["1.9", "1.10", "1.11", "1.13"] },
    { "wave": 6, "tasks": ["1.14", "1.15"] }
  ]
}
```

**Wave rationale (Phase 1):**
- **Wave 1** — bootstrap: empty solution layout (1.1) and the Tenants root aggregate + multi-tenancy infrastructure (1.0a). 1.0a must precede every other Phase 1 task because the `tenant_id` global filter, `ITenantContext`, and `TenantAwareLinqFactory` are prerequisites for every module's repository.
- **Wave 2** — shared infrastructure: package management (1.2), building blocks (1.3), the default-tenant backfill SQL (1.0b), and the `PlatformAdmin` bootstrap (1.0c). 1.0b and 1.0c depend on the Tenants table being defined (1.0a) but not on each other; the NetArchTest rule referenced by 1.0b is created together with the 1.15 test project in Wave 6 (per F2 in `review.md`).
- **Wave 3** — host wiring + reference module: ASP.NET Core 8 host with Serilog / JWT / Swagger / DBTools_SQL (1.4), the Residents module template (1.5), the repository implementation (1.6), and the two ADRs (1.16). All four depend on BuildingBlocks (1.3) and Directory.Packages (1.2).
- **Wave 4** — endpoints + frontend skeleton: Residents Minimal API endpoints (1.7), the Angular 18+ SPA (1.8, depends on 1.7 so the OpenAPI schema is real), the `00-schema.sql` for the smoke test (1.12), and the Makefile (1.17). 1.8 generates the TypeScript client from 1.7's OpenAPI, so 1.7 must land first.
- **Wave 5** — containerization + unit tests: Dockerfiles (1.9), docker-compose (1.10), Traefik (1.11), and unit tests for the repository + TenantFilterInterceptor (1.13). 1.13 depends on 1.6 and 1.0a; container files depend on 1.7 and 1.8 being buildable.
- **Wave 6** — full integration + architecture gates: Testcontainers integration tests (1.14) and the NetArchTest architecture rules (1.15). These run last in Phase 1 because they validate every prior wave end-to-end.

### Phase 2 — Strangler Pilot

```json
{
  "phase": 2,
  "waves": [
    { "wave": 1, "tasks": ["2.1", "2.7a"] },
    { "wave": 2, "tasks": ["2.2", "2.3"] },
    { "wave": 3, "tasks": ["2.4", "2.5", "2.6"] }
  ]
}
```

**Wave rationale (Phase 2):**
- **Wave 1** — backend: the Security module with the full role set, attendant profile / shift / gatehouse / tenant-picker / tenant-switch API surface, and the attendant profile backend slice (2.7a). Both are backend-only and have no inter-dependency.
- **Wave 2** — frontend + strangler wiring: Angular Residents admin page + login (2.2) and the WPF-in-parallel feature flag plumbing (2.3). 2.2 depends on the API surface from 2.1; 2.3 depends on `Microsoft.FeatureManagement` (1.4) being in place.
- **Wave 3** — pilot cutover: WPF/web cross-verify smoke test (2.4), the 7-day pilot flip on one condominium (2.5), and the legacy-mapping doc update (2.6). 2.4 must pass before 2.5; 2.6 documents the result of 2.5.

### Phase 3 — Module Migrations

```json
{
  "phase": 3,
  "waves": [
    { "wave": 1, "tasks": ["3.1", "3.2", "3.3", "3.4", "3.9a"] },
    { "wave": 2, "tasks": ["3.6"] },
    { "wave": 3, "tasks": ["3.5", "3.7", "3.8"] }
  ]
}
```

**Wave rationale (Phase 3):**
- **Wave 1** — module scaffolding: Visits (3.1), Vehicles (3.2), ServiceProviders (3.3), Administration (3.4), and the tenant-administration backend slice (3.9a). All five are independent feature modules with the same template and can be built in parallel by separate agents.
- **Wave 2** — cross-cutting: CQRS-lite for reports (3.6). 3.6 spans all modules built in Wave 1, so it starts after they land.
- **Wave 3** — per-module cutover: for-each-module pilot flips (3.5), Playwright UI tests per primary page (3.7), and the per-row legacy-mapping doc updates (3.8). All three can be performed in parallel per module.

### Phase 4 — Legacy Decommission

```json
{
  "phase": 4,
  "waves": [
    { "wave": 1, "tasks": ["4.1", "4.2"] },
    { "wave": 2, "tasks": ["4.3", "4.5", "4.6"] },
    { "wave": 3, "tasks": ["4.4"] }
  ]
}
```

**Wave rationale (Phase 4):**
- **Wave 1** — feature-flag retirement (4.1) and the WPF project being marked `Deprecated` in the solution and excluded from CI (4.2). Both must land before the read-only-WPF window.
- **Wave 2** — 30-day read-only-WPF observation (4.3) runs in parallel with the legacy-mapping doc archive (4.5) and the production cutover checklist (4.6).
- **Wave 3** — WPF removal (4.4) is the *last* commit; it runs only after the 30-day read-only window closes.

---



---

## Phase 1 — Foundation (greenfield skeleton)
*Goal: build the empty modular monolith + Docker stack + DBTools_SQL wiring, with a single smoke-test module (Residents) that proves the end-to-end flow works.*

- [x] **1.0a** **Tenants module (root aggregate).** Create `Modules/Tenants/{Domain,Application,Infrastructure,Api}` with the `Tenants` aggregate (`Id`, `Slug`, `DisplayName`, `Status`, `CreatedAtUtc`), the `ITenantContext` interface + `HttpTenantContext` implementation, the `TenantResolutionMiddleware`, the `TenantFilterInterceptor : IQueryInterceptor` registered in `Host/Program.cs`, the `TenantAwareLinqFactory` in `BuildingBlocks/Infrastructure/MultiTenancy/`, and the `Tenants` Minimal API endpoints (`POST /api/v1/tenants`, `GET /api/v1/tenants/{id}`, `POST /api/v1/tenants/{id}/suspend`, `POST /api/v1/tenants/{id}/resume` — `PlatformAdmin` only).
  - **Acceptance criteria:**
    - `ITenantContext` is registered as scoped; `NullTenantContext` exists for unit tests.
    - `TenantResolutionMiddleware` runs after `UseAuthentication()`; a request without a `tenant_id` claim and without `PlatformAdmin` role returns 403.
    - `TenantFilterInterceptor` is wired into every `Linq<TModel>` built by `TenantAwareLinqFactory`; verified by a unit test that runs two queries with different `ITenantContext` instances and asserts the generated SQL includes the right `WHERE tenant_id = @ctx_tenant` clause.
    - `Tenants` endpoints return ProblemDetails on error; OpenAPI surface is generated by Swashbuckle.

- [x] **1.0b** **Default-tenant backfill + WPF compatibility shim.** Add `docker/mysql/init/02-tenants-seed.sql`, `03-tenant-backfill.sql`, `04-tenant-views.sql` (per the "Tenant Data Migration" subsection). Add `scripts/generate-tenant-backfill.sql` to regenerate the backfill file from a table list, and add a NetArchTest rule (shipped in the same PR as 1.0b, added to the 1.15 test project as the 1.15 sub-bullet below) that fails the build if a new business table is missing from the backfill list.
  - **Acceptance criteria:**
    - `docker compose down -v && docker compose up -d` against a snapshot of the legacy DB leaves the WPF app able to start and read/write through the `_legacy` views with no source change beyond the EDMX regeneration.
    - NetArchTest rule fails the build if a `CREATE TABLE` is added to `docker/mysql/init/` that is not listed in the backfill script.
    - ADR `0003-multi-tenant-shared-schema.md` is committed.

- [x] **1.0c** **`PlatformAdmin` bootstrap + tenant-aware integration test fixture.** Add the `PlatformAdminBootstrapService : IHostedService` that seeds the first `PlatformAdmin` (random email, random password, force-rotate on first login), and extend `tests/ControlEasyReborn.IntegrationTests/TenantAwareWebApplicationFactory.cs` to seed two tenants and expose helpers `AsTenantA()`, `AsTenantB()`, `AsPlatformAdmin()` for tests.
  - **Acceptance criteria:**
    - On a fresh DB, the API logs a `// CHANGE IMMEDIATELY` warning with the temp password exactly once.
    - The first login as `PlatformAdmin` is rejected with 401 until the password is rotated.
    - `TenantAwareWebApplicationFactory.AsTenantA()` issues a JWT bound to tenant A; `AsTenantB()` issues one bound to tenant B; `AsPlatformAdmin()` issues a `PlatformAdmin` JWT.
    - A smoke test creates a `Resident` as tenant A and asserts tenant B and `PlatformAdmin` see it (only `PlatformAdmin` is allowed to; tenant B must get 404 / empty list).

- [x] **1.1** Create the new solution layout: `src/`, `tests/`, `docker/`, `docs/`. Add `src/ControlEasyReborn.sln`.
- [x] **1.2** Add `src/Directory.Packages.props` (CPM) and `src/Directory.Build.props` (`LangVersion=latest`, `Nullable=enable`, `TreatWarningsAsErrors=true`). Pin all package versions per `design.md`.
- [x] **1.3** Create `src/BuildingBlocks/ControlEasyReborn.SharedKernel` (Result, Guard, common abstractions) and `src/BuildingBlocks/ControlEasyReborn.Infrastructure` (extension methods for DBTools_SQL DI).
- [x] **1.4** Create `src/Host/ControlEasyReborn.Api` (ASP.NET Core 8 Web API) with `Program.cs` registering: Serilog, JWT bearer, Swagger, ProblemDetails, `AddDbTools(...)`, health checks, `Microsoft.FeatureManagement`.
- [x] **1.5** Wire the `Residents` module template (Domain/Application/Infrastructure/Api) as the reference module.
- [x] **1.6** Implement `IResidentRepository` with `Linq<Resident>` and `IAsyncSqlClient` per `design.md` (no raw SQL, no `MySql.Data`, no `EntityFramework`).
- [x] **1.7** Implement the `Residents` Minimal API endpoints: `GET /api/v1/residents?search=...&skip=...&take=...`, `GET /api/v1/residents/{id}`, `POST /api/v1/residents`, `PUT /api/v1/residents/{id}`.
- [x] **1.8** Create `src/Web/ControlEasyReborn.Web` (**Angular 18+** standalone-component SPA, TypeScript strict mode, **Tailwind CSS v4 + custom design tokens**, **Angular CDK** for headless overlays) with a single `Residents` page that lists and creates residents, talking to the API through `HttpClient` with a bearer token (added by an HTTP interceptor). Visual patterns must follow the spec in `.specs/2 - visual-design-system/`.
  - Configure `ng-openapi-gen` so DTOs/interfaces are generated from the API's `/swagger/v1/swagger.json` on every build.
  - Configure `proxy.conf.json` for `ng serve` so dev calls go to the API at `https://localhost/api`.
  - The Angular SPA's `@theme` block and component styles must **conform to `docs/penpot/tokens.json`** (light + dark); continuous task C.7 enforces this in CI.
  - During the Angular build, **`mockup/` is the visual reference** (open every Angular page side-by-side with the matching `mockup/*.html` page and confirm pixel parity before merging).
- [x] **1.9** Add multi-stage Dockerfiles: `docker/api.Dockerfile`, `docker/web.Dockerfile`.
- [x] **1.10** Add `docker/docker-compose.yml` with services: `reverse-proxy` (Traefik), `api`, `web`, `db` (MySQL 8, with healthcheck), `adminer`, `seq`. Add `docker/.env.example`.
- [x] **1.11** Add `docker/reverse-proxy/traefik.yml` and `docker/reverse-proxy/dynamic.yml` routing `/api/*` → `api`, `/` → `web`, `/db` → `adminer`. TLS via Let's Encrypt (staging cert in dev).
- [x] **1.12** Add `docker/mysql/init/00-schema.sql` with the minimal `Residents` table and a `Users` table for JWT issuance.
- [x] **1.13** Add `tests/ControlEasyReborn.UnitTests` (xUnit + FluentAssertions) covering `ResidentRepository` against an in-memory fake of `IAsyncSqlClient` (only for unit-level logic; real SQL is verified by integration tests).
  - **1.13 (updated):** Add unit tests covering the `TenantFilterInterceptor`: a fake `IAsyncSqlClient` captures the generated SQL, and the test asserts that two `Linq<Resident>` queries built by `TenantAwareLinqFactory` with different `ITenantContext` instances produce SQL containing the right `tenant_id` parameter. The original `ResidentRepository` in-memory tests stay; this is *additive*.

- [x] **1.14** Add `tests/ControlEasyReborn.IntegrationTests` (xUnit + Testcontainers.MySql) with a test that boots a real MySQL container, runs the schema, calls `GET /api/v1/residents` through `WebApplicationFactory`, and asserts the round-trip.
  - **1.14 (updated):** Add a cross-tenant-access integration test to the Testcontainers fixture: seed tenant A and tenant B; `POST /api/v1/residents` as tenant A; assert that `GET /api/v1/residents` as tenant B returns 0 rows and that `GET /api/v1/residents/{aId}` as tenant B returns 404. The original round-trip smoke test stays; this is *additive*.

- [x] **1.15** Add a `ArchitectureTests` project using `NetArchTest` asserting: `Domain` projects don't reference `Infrastructure`; `Infrastructure` projects don't reference `Microsoft.EntityFrameworkCore` or `MySql.Data`.
  - Sub-bullet: Fail the build if a `CREATE TABLE` is added to `docker/mysql/init/` that is not listed in the backfill script (per task 1.0b).
- [x] **1.16** Add `docs/architecture/decisions/0001-modular-monolith.md` and `0002-dblools-sql-as-only-data-access.md`.
- [x] **1.17** Add a `Makefile` (or `package.json` of scripts) with `make up`, `make down`, `make logs`, `make test`, `make migrate`.
- **Verification gate (Phase 1):**
  - `docker compose up -d` brings all services healthy in < 2 min.
  - `curl -k https://localhost/health` → 200.
  - `curl -k https://localhost/api/v1/residents` → 200 with `[]` (empty list).
  - Open `https://localhost` in Chrome via Playwright, see the `Residents` page, create one resident, refresh, see it in the list. Screenshot for the user.
  - Open `mockup/index.html` and `mockup/app.html` in Chrome via Playwright and confirm every `ce-*` selector used in the Angular app renders identically to its mockup counterpart (token parity + component parity).
  - `dotnet test` is green (unit + integration + architecture).
  - `git grep -ri "EntityFramework\|MySql.Data" src/` returns nothing.

---

## Phase 2 — Strangler Pilot (prove the migration pattern)
*Goal: cut over the *least critical* legacy screen to the new web app, with a feature flag, to prove the Strangler pattern end to end.*

- [x] **2.1** Implement the `Security` module with the full role set (`PlatformAdmin`, `TenantAdmin`, `Morador`, plus `AttendantProfile` per tenant) and the JWT claim shape from the "Multi-Tenancy" subsection (`sub`, `tenant_id`, `profile_id`, `roles[]`, `permissions[]`). Existing endpoints carried over: `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`. Endpoints added by the multi-tenant + attendant-profiles work:
    - `POST /api/v1/security/attendant-profiles`
    - `GET  /api/v1/security/attendant-profiles?activeOnly=&skip=&take=`
    - `GET  /api/v1/security/attendant-profiles/{id}`
    - `PUT  /api/v1/security/attendant-profiles/{id}`
    - `POST /api/v1/security/attendant-profiles/{id}/deactivate`
    - `GET  /api/v1/security/attendant-profiles/me`
    - `POST /api/v1/security/shifts`, `GET /api/v1/security/shifts`
    - `POST /api/v1/security/gatehouses`, `GET /api/v1/security/gatehouses`
    - `GET  /api/v1/security/tenants?email=...` (public, used by the login screen)
    - `POST /api/v1/security/tenant-switch` (re-issues a JWT for a different tenant the user belongs to)
    - Login response payload now includes `tenantId`, `profileId`, `roles`, `permissions`.
- [x] **2.2** Add the `Residents` admin page in the Angular SPA (table, edit, soft-delete) and a login page (reactive forms + `AuthService` with signals).
- [x] **2.3** Run the legacy WPF app and the new web app in parallel, both pointed at the same MySQL (different app settings). Add `Microsoft.FeatureManagement` flag `Residents.UseWeb` (default `false`).
- [x] ~~**2.4** Smoke test: start WPF, perform a CRUD on Residents, verify the change is visible in the web app. Reverse: change in the web app is visible in WPF.~~ **Cancelled** — WPF app cannot run on macOS; legacy WPF is no longer in production use. Strangler Fig coexistence test is unnecessary.
- [x] ~~**2.5** Flip `Residents.UseWeb=true` for one test condominium; monitor for 7 days; collect error rates via Serilog/Seq.~~ **Cancelled** — same reason as 2.4.
- [x] **2.6** Update `docs/migration/legacy-mapping.md` marking `Residents` (WPF) as "**Web — pilot live**".
- [x] **2.7a** **Attendant profile API surface (backend slice).** Backend-only — the Angular UI for attendant profile management is owned by the Frontend Agent in a separate spec. Deliverables: implement every endpoint listed in the updated 2.1 above, the `IAttendantProfileRepository` and `IShiftRepository` / `IGatehouseRepository` (all going through `TenantAwareLinqFactory`), the `Permissions` static class, the `RequirePermissionAttribute` + handler, the `AttendantProfile` / `Shift` / `Gatehouse` entities with their value objects, the FluentValidation validators, and the OpenAPI surface (so `ng-openapi-gen` produces the TypeScript DTOs for the Frontend Agent).
  - **Acceptance criteria:**
    - Integration tests: create profile as `TenantAdmin`, list as `TenantAdmin`, attempt to create a profile in tenant B while authenticated as tenant A → 403/404 (cross-tenant access denied).
    - Permission check: an attendant JWT with `visits.checkin` is allowed to `POST /api/v1/visits/{id}/checkin` (smoke test against a stub endpoint); the same JWT without that permission is denied 403.
    - OpenAPI spec regenerated; `ng-openapi-gen` produces the `AttendantProfile`, `Shift`, `Gatehouse` TypeScript types.
- **Verification gate (Phase 2):**
  - Web app handles 100% of Residents CRUD for the pilot condominium.
  - Zero data inconsistencies between WPF and web for the pilot.
  - p95 latency for `GET /api/v1/residents?search=...` < 200 ms against the seeded DB.
  - Architecture tests still green; no new `MySql.Data` or `EntityFramework` references.

---

## Phase 3 — Module Migrations (parallel feature work)
*Goal: migrate the remaining user-facing modules one by one, in priority order, using the same Strangler pattern.*

- [x] **3.1** **Visits** module: `Visitante`, `Fluxo` (gatehouse log). Endpoints: `GET/POST /api/v1/visits`, `POST /api/v1/visits/{id}/checkin`, `POST /api/v1/visits/{id}/checkout`, `GET /api/v1/visits?status=open`.
- [x] **3.2** **Vehicles** module: `Veiculo`, link to `Apartamento` via `Linq<Veiculo>.InnerJoin<Apartamento>()`.
- [x] **3.3** **ServiceProviders** module: `PrestadorServico` (the old `PrestadoresServico.razor` prototype page is already gone from Phase 0; build a real Angular CRUD page for service providers in this module).
- [x] **3.4** **Administration** module: audit log, configuration tables.
- [ ] **3.5** For each module: domain entity → application handlers → repository (Linq) → Minimal API endpoints → Angular page (lazy-loaded feature module + generated types) → integration test → feature flag → pilot cutover.
- [ ] **3.6** Add CQRS-lite for the **reports** read paths (visit counts per day, residents per apartment) using `Linq<T>.AsQueryable()` projections.
- [ ] **3.7** Add a **Playwright** UI test for each module's primary page (open in Chrome via the Playwright tool, click "Create", assert the new row appears).
- [ ] **3.8** Update `docs/migration/legacy-mapping.md` marking each row "Web — live" as it cuts over.
- [x] **3.9a** **Tenant administration API surface (backend slice).** Backend-only — the Angular UI for tenant administration is owned by the Frontend Agent. Deliverables: the `Tenants` admin endpoints (some already in 1.0a) plus `POST /api/v1/tenants/{id}/admins` (create `TenantAdmin` for that tenant), `GET /api/v1/tenants/{id}/admins`, `POST /api/v1/tenants/{id}/admins/{userId}/revoke`, `POST /api/v1/admin/backups/{tenantId}` (trigger an on-demand per-tenant backup), and the `TenantAdmin` user management in the `Security` module.
  - **Acceptance criteria:**
    - Only `PlatformAdmin` can create / suspend / resume tenants and assign `TenantAdmin`s; verified by integration test.
    - On-demand backup endpoint produces a gzipped SQL dump under a configurable `Backup:Path` (default `./backups/tenants/<slug>/<timestamp>.sql.gz`); integration test asserts the file exists and contains the right `WHERE tenant_id` clause.
    - OpenAPI regenerated; `ng-openapi-gen` produces the `Tenant`, `TenantAdmin`, `BackupResult` TypeScript types.
- **Verification gate (Phase 3):**
  - All modules listed in the inventory have a `Web — live` row in the mapping doc.
  - All integration tests green; UI smoke tests green for every primary page.
  - p95 latency budgets met for every endpoint.

---

## Phase 4 — Legacy Decommission
*Goal: turn off the WPF app and the Strangler infrastructure.*

- [ ] **4.1** Remove the `Microsoft.FeatureManagement` flags (or default them to `true` permanently).
- [ ] **4.2** Mark `ControlEasy5` (WPF) project as `Deprecated` in the solution; do not build it in CI.
- [ ] **4.3** Run WPF and web in read-only-WPF mode for 30 days; collect any data drift.
- [ ] **4.4** Delete the WPF project, the WPF-specific CI job, and the `App.config` credential stubs from the repo. Commit: `chore: remove legacy ControlEasy5 WPF`.
- [ ] **4.5** Move `docs/migration/legacy-mapping.md` → `docs/migration/legacy-mapping-archive.md`.
- [ ] **4.6** Production cutover checklist: TLS certs live, `Jwt__SigningKey` rotated, DB user locked down to the new API's IP, monitoring dashboards (Seq/Grafana) live.
- **Verification gate (Phase 4):**
  - CI builds only `src/ControlEasyReborn.sln`; WPF project is gone.
  - `docker compose -f docker-compose.prod.yml up -d` runs the full stack with no WPF container.
  - One month post-cutover: zero WPF-only incidents, all legacy screens retired.

---

## Continuous (every phase)

- [ ] **C.1** GitHub Actions: `lint` (dotnet format), `build` (matrix: linux-x64, win-x64), `test` (unit + integration with Testcontainers), `docker` (build images, push to GHCR), `smoke` (docker compose up + curl + Playwright).
- [ ] **C.2** Keep `AGENTS.md` updated whenever a new module, library, or convention is added. Keep `docs/penpot/manifest.json` and `docs/penpot/tokens.json` in sync with `.specs/2 - visual-design-system/`.
- [ ] **C.3** Every ADRs recorded in `docs/architecture/decisions/` with the date and the decision made.
- [ ] **C.4** Every UI change is verified in a real Chrome browser via the Playwright tool before being marked done. The visual review surface is `mockup/` (smoke-tested via `mockup/SMOKE.md`).
- [ ] **C.5** After every API change, run `ng-openapi-gen` to regenerate the Angular TypeScript client and fix any breaking call sites.
- [ ] **C.6** **Architecture rule — `TenantId` everywhere + cross-tenant test.** Two NetArchTest rules in `tests/ControlEasyReborn.ArchitectureTests/`:
  - All classes in `Modules/*/Domain/Entities/` whose name does not start with `Platform` (i.e. the per-tenant entities) must have a non-nullable `TenantId` property of type `Guid` (or a `TenantId` value object wrapping `Guid`).
  - Every `tests/ControlEasyReborn.IntegrationTests/*` test class that touches a module's repository must contain at least one `[Fact]` whose name matches the regex `CrossTenant_.*` and that asserts the module's `Linq<T>`-backed read returns no rows for a foreign tenant and that writes to a foreign tenant are rejected. CI fails the build if a new test file ships without such a fact.
  - These rules run on every PR (in the existing C.1 GitHub Actions `test` job).
- [ ] **C.7** **Tokens-contract CI check.** A test or script (in `tests/`, e.g. `tests/ControlEasyReborn.ArchitectureTests/TokensContractTests.cs`, or a new GitHub Actions step in `.github/workflows/ci.yml` named `tokens-contract`) diffs the `light` and `dark` blocks of `docs/penpot/tokens.json` against the `@theme` block in `src/Web/ControlEasyReborn.Web/src/styles.css` and fails the build if a token is added to one side but not the other. The check runs on every PR in the existing C.1 `build` job (not `test`). Verification command: `dotnet test tests/ControlEasyReborn.ArchitectureTests/ --filter TokensContract` (and/or the `tokens-contract` GitHub Actions step name in the C.1 `build` job's `steps:` block).
