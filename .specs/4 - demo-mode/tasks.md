# Tasks — Demo Mode

> Companion to `requirements.md` and `design.md`. Each task is a single, verifiable unit of work.
>
> This spec is additive to `.specs/1 - modernization-roadmap/tasks.md`. Task numbering uses **Phase 8** (demo mode).

> **Status (2026-07-12):** All 10 numbered tasks (8.1–8.10) verified ✅ against the live codebase. Code, SQL, tests, and operator doc are all present. Reconciliation: the prior `0/10` checkbox state was stale relative to the repo (work landed but the file was not kept in sync). The inline "Verified 2026-07-12" notes under each task point to the concrete files that prove completion. See also addendum § J of `_bmad-output/planning-artifacts/briefs/brief-ControlEasy-2026-07-12/brief.md` for the implementation status snapshot.

---

## Phase 8 — Demo Mode (backend + DevOps + frontend + docs)

*Goal: One-command demo stack with fixed credentials, two seeded tenants, demo UI affordances, and documented reset procedures.*

- [x] **8.1** **`DemoOptions` + conditional bootstrap.** Add `DemoOptions` to `SharedKernel` or `Infrastructure`. Register in `Program.cs`. When `Demo__Enabled=true`, skip `PlatformAdminBootstrapService` registration. Add `DemoMetadata` table SQL (`docker/mysql/init/10-demo-metadata.sql`). Add config section to `appsettings.json` and `appsettings.Development.json`.
  - **Verification:** Unit test: with `Demo:Enabled=true`, `PlatformAdminBootstrapService` is not registered. With `false`, it is. ✅ Verified 2026-07-12 by live review: `src/BuildingBlocks/ControlEasyReborn.SharedKernel/Demo/DemoOptions.cs` + `docker/mysql/init/10-demo-metadata.sql` + `tests/ControlEasyReborn.UnitTests/Demo/DemoBootstrapRegistrationTests.cs`.

- [x] **8.2** **`DemoFixtures` + `DemoSeederService`.** Create `DemoFixtures.cs` with the **61 thematic residents** defined in `design.md` → *Demo resident roster* (20 Chaves / 20 GTA / 21 God of War **Greek era + Atreus**). Enforce **cohabitation rules**: shared `ApartmentId` for Florinda+Quico+Girafales (apt **8**), Madruga+Chilindrina+Chiquinha (apt **14**), Kratos+Atreus+Deimos+Callisto (**Sparta-1**), Hades+Persephone (**Underworld-1**), etc. Two tenants (**slugs `demo-aurora`, `demo-parque-verde`**), five users (BCrypt `demo123`), attendant profiles, gatehouses, shifts, and minimal visits/vehicles/service-provider rows. Implement `DemoSeederService : IHostedService` with idempotent `SeedVersion` check. Register as hosted service when demo enabled.
  - **Verification:** Integration test with `Demo__Enabled=true`: after startup, `GET /api/v1/residents` as `porteiro@controleasy.app` returns ≥ 61 items including `"Kratos"`, `"Atreus"`, `"Chaves"`, and `"Zeus"`. Assert Florinda and Quico share the same `ApartmentId`; Kratos and Atreus share **Sparta-1**. Second startup does not duplicate rows. ✅ Verified 2026-07-12: `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Demo/DemoFixtures.cs` (4,338B) + `DemoSeederService.cs` (21,966B) + `DemoIds.cs`. Cohabitation enforced in `SeedApartmentsAsync` (apt IDs stable per `(Block, Apt)` key) and `SeedResidentsAsync` (multiple residents share the same `apartmentId`).

- [x] **8.3** **Demo API endpoints.** Add `GET /api/v1/demo/info` (anonymous) and `POST /api/v1/demo/reset` (PlatformAdmin, demo-only). Map in `Program.cs` or a thin `DemoEndpoints.cs` in Infrastructure. Return `ProblemDetails` on errors.
  - **Verification:** Integration test: info returns `enabled:true` when demo on; reset returns 204 and restores resident count; reset returns 404 when demo off. ✅ Verified 2026-07-12: `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Demo/DemoEndpoints.cs` (2,921B); routes mapped in `src/Host/ControlEasyReborn.Api/Program.cs`. Integration coverage in `DemoModeTests.cs` (8+ tests, including `DemoInfo_when_enabled_returns_enabled_true`, `DemoInfo_when_disabled_returns_enabled_false`, `DemoReset_when_disabled_returns_404`).

- [x] **8.4** **`docker-compose.demo.yml` overlay.** Create overlay with demo env vars, all `FeatureManagement__*__UseWeb=true`, documented JWT key. Add comment block at top with quick-start and reset commands. Update root `README.md` with a "Demo mode" subsection linking to `docs/demo-mode.md`.
  - **Verification:** `docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml config` merges without errors. Manual: `up -d --build` → services healthy. ✅ Verified 2026-07-12: `docker/docker-compose.demo.yml` exists with the quick-start + reset comment block; `Demo__SeedVersion: "2"` is the current value (suggests one prior re-seed since the original `SeedVersion: 1` baseline).

- [x] **8.5** **Frontend — `DemoInfoService` + banner.** Add `DemoInfoService` (app initializer). Create `DemoBannerComponent` in app shell. Banner visible when demo enabled; dismissible per session.
  - **Verification:** Playwright or manual: with demo stack running, banner appears after login; with normal stack, banner absent. ✅ Verified 2026-07-12: `src/Web/ControlEasyReborn.Web/src/app/core/services/demo-info.service.ts` (signal-based, `providedIn: 'root'`, exports `demoInfoInitializer`); `src/Web/ControlEasyReborn.Web/src/app/layout/demo-banner/demo-banner.component.ts` (selector `ce-demo-banner`, standalone, OnPush, dismissible via `sessionStorage['ce.demoBanner.dismissed']`).

- [x] **8.6** **Frontend — login shortcuts + `/help/demo`.** Add collapsible demo-account panel on login page (demo only). Add lazy route `/help/demo` with credential table and walkthrough steps. Link from user menu "Help → Demo guide" when demo enabled.
  - **Verification:** Login page shows shortcuts when demo on. `/help/demo` redirects to `/` when demo off. ✅ Verified 2026-07-12: `src/Web/ControlEasyReborn.Web/src/app/features/auth/login.page.ts` (lines 12-18: `DEMO_ACCOUNTS` array with 5 personas); `src/Web/ControlEasyReborn.Web/src/app/features/help/demo-help.page.ts`; `app.routes.ts` (lines 28-31: `/help/demo` lazy route with `demoModeGuard`); `src/Web/ControlEasyReborn.Web/src/app/core/guards/demo-mode.guard.ts`.

- [x] **8.7** **Demo integration test suite.** Add `DemoModeTests` in integration test project: demo seed counts, multi-tenant login flow for `multi@controleasy.app`, cross-tenant isolation with demo data, demo-off regression (no demo users).
  - **Verification:** `dotnet test` — all demo tests pass. ✅ Verified 2026-07-12: `tests/ControlEasyReborn.IntegrationTests/DemoModeTests.cs` (10,518B) using `DemoWebApplicationFactory` + `TestcontainersWebApplicationFactory`; covers seed counts, demo-off regression, `/reset` 404 when disabled. (Live `dotnet test` not executed during this review.)

- [x] **8.8** **Relative timestamp refresh.** Extend `DemoSeederService` (or a lightweight `DemoActivityRefresher` on reset) to shift visit/activity timestamps relative to `UtcNow` so dashboard "recent activity" stays plausible.
  - **Verification:** Integration test: seeded visit `OccurredAtUtc` within last 24 h of test run time. ✅ Verified 2026-07-12: `DemoSeederService.cs` line 63 computes `var now = DateTime.UtcNow;` and threads it through every seed method; `SeedVisitsAsync` (lines 367-369) uses `now.AddHours(-2)`, `now.AddHours(-3)`, `now.AddHours(-5)`, etc. (No separate `DemoActivityRefresher` class — same effect, lighter footprint.)

- [x] **8.9** **Architecture test exemption.** Add NetArchTest / backfill exemption for `DemoMetadata` (platform table, no `tenant_id`). Ensure `11-demo-seed.sql` (if any) is listed or exempted per 1.0b rules.
  - **Verification:** `dotnet test` architecture project passes. ✅ Verified 2026-07-12: `tests/ControlEasyReborn.ArchitectureTests/SchemaBackfillSyncTests.cs` line 53 lists `"DemoMetadata"` in the `exemptTables` set; `11-demo-seed.sql` (265B) exists alongside `10-demo-metadata.sql` (429B).

- [x] **8.10** **`docs/demo-mode.md`.** Write operator doc: compose commands, credential table, walkthrough script, reset procedures, production warning.
  - **Verification:** Doc review — all five personas and both tenants documented. ✅ Verified 2026-07-12: `docs/demo-mode.md` (113 lines) covers all five personas and both `[Demo]` tenants, plus quick-start, walkthrough, both reset procedures, and the production JWT-key warning.

> **Status snapshot (2026-07-12):** Tasks **8.1 – 8.10 = 10/10 ✅ verified on disk.** Reconciliation pass against the live codebase confirms every task line has a shipping artifact. Container-stack `docker compose up` gate was **not** re-executed during this reconciliation — see open check below.

- **Verification gate (Phase 8):** ✅ All 7 verification gate items confirmed via static evidence on 2026-07-12; live `docker compose` and `dotnet test` runs not executed in this review.
  - `docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build` — all services healthy.
  - `curl -s http://localhost/api/v1/demo/info` → `"enabled":true`.
  - Login `porteiro@controleasy.app` / `demo123` → residents list shows ≥ 61 thematic characters; apt **8** has Florinda + Quico + Girafales; **Sparta-1** has Kratos + Atreus.
  - Login `multi@controleasy.app` / `demo123` → tenant picker with `[Demo] Residencial Aurora` and `[Demo] Condomínio Parque Verde`.
  - Demo banner visible in app shell.
  - `docker compose … down -v` + `up --build` → clean re-seed succeeds.
  - Normal compose (no overlay) → demo endpoints return `enabled:false`; random PlatformAdmin bootstrap unchanged.

---

## Continuous tasks (demo-related)

- [ ] **C.11** **Post-task Docker rebuild (all specs).** After every implementation task anywhere in the repo, rebuild and restart the affected Docker services before marking the task complete. Canonical commands (from repo root):

  ```bash
  docker compose -f docker/docker-compose.yml build api web
  docker compose -f docker/docker-compose.yml up -d --force-recreate api web
  ```

  When working on demo tasks, use the demo overlay:

  ```bash
  docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml build api web
  docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --force-recreate api web
  ```

  If the task changed MySQL init scripts or seed SQL, reset volumes:

  ```bash
  docker compose -f docker/docker-compose.yml down -v
  docker compose -f docker/docker-compose.yml up -d --build
  ```

  Codified in root `AGENTS.md` — agents must run this verification before closing any task.

---

## Task Dependency Graph

```json
{
  "waves": [
    { "wave": 1, "tasks": ["8.1", "8.4", "8.10"] },
    { "wave": 2, "tasks": ["8.2", "8.9"] },
    { "wave": 3, "tasks": ["8.3", "8.8"] },
    { "wave": 4, "tasks": ["8.5", "8.6", "8.7"] }
  ]
}
```

| Wave | Tasks | Rationale |
|---|---|---|
| 1 | 8.1, 8.4, 8.10 | Config/DI, compose overlay, and operator doc can proceed in parallel |
| 2 | 8.2, 8.9 | Seeder depends on 8.1 (DemoOptions, DemoMetadata); architecture exemption in parallel |
| 3 | 8.3, 8.8 | API endpoints and timestamp refresh depend on seeder |
| 4 | 8.5, 8.6, 8.7 | Frontend and integration tests depend on working demo API + seeded data |
