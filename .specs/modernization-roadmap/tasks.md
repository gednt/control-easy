# Tasks — Modernization Roadmap

> Companion to `requirements.md` and `design.md`. Each task is a single, verifiable unit of work. **Phases must be completed in order**; a phase gate (last task of the phase) must be green before starting the next phase.

---

## Phase 0 — Inventory & Cleanup (preparation)
*Goal: get a clean baseline of the existing code, kill the dead Blazor-on-.NET 5 prototype, and document the legacy screen → module mapping.*

- [ ] **0.1** Inventory every WPF View in `ControlEasy5/ControlEasy5/View/` and Controller in `ControlEasy5/ControlEasy5/Controller/`, list them in `docs/migration/legacy-inventory.md`.
- [ ] **0.2** Inventory the Model layer (`ControlEasy5/ControlEasy5/Model/*.cs`) and the MySQL schema (reverse-engineer from `controlEasyDB.db` or `privilegios_backup.sql`) into `docs/migration/legacy-schema.md`.
- [ ] **0.3** Delete the dead Blazor Server prototype (`ControlEasy5/ControlEasyWeb/`) and remove its project reference from `ControlEasy5.sln`. Commit: `chore: remove dead ControlEasyWeb .NET 5 prototype`.
- [ ] **0.4** Remove hard-coded MySQL credentials from `App.config`; replace with a stub reading from environment variables. Commit: `chore(security): externalize db credentials`.
- [ ] **0.5** Produce `docs/migration/legacy-mapping.md` with a table: `WPF screen → new web module → target Angular page → migration phase`.
- **Verification gate (Phase 0):** `dotnet build ControlEasy5/ControlEasy5.sln` succeeds; the four docs exist; no plaintext passwords appear in `git grep -i "passwd" ControlEasy5/`.

---

## Phase 1 — Foundation (greenfield skeleton)
*Goal: build the empty modular monolith + Docker stack + DBTools_SQL wiring, with a single smoke-test module (Residents) that proves the end-to-end flow works.*

- [ ] **1.1** Create the new solution layout: `src/`, `tests/`, `docker/`, `docs/`. Add `src/ControlEasyReborn.sln`.
- [ ] **1.2** Add `src/Directory.Packages.props` (CPM) and `src/Directory.Build.props` (`LangVersion=latest`, `Nullable=enable`, `TreatWarningsAsErrors=true`). Pin all package versions per `design.md`.
- [ ] **1.3** Create `src/BuildingBlocks/ControlEasyReborn.SharedKernel` (Result, Guard, common abstractions) and `src/BuildingBlocks/ControlEasyReborn.Infrastructure` (extension methods for DBTools_SQL DI).
- [ ] **1.4** Create `src/Host/ControlEasyReborn.Api` (ASP.NET Core 8 Web API) with `Program.cs` registering: Serilog, JWT bearer, Swagger, ProblemDetails, `AddDbTools(...)`, health checks, `Microsoft.FeatureManagement`.
- [ ] **1.5** Wire the `Residents` module template (Domain/Application/Infrastructure/Api) as the reference module.
- [ ] **1.6** Implement `IResidentRepository` with `Linq<Resident>` and `IAsyncSqlClient` per `design.md` (no raw SQL, no `MySql.Data`, no `EntityFramework`).
- [ ] **1.7** Implement the `Residents` Minimal API endpoints: `GET /api/v1/residents?search=...&skip=...&take=...`, `GET /api/v1/residents/{id}`, `POST /api/v1/residents`, `PUT /api/v1/residents/{id}`.
- [ ] **1.8** Create `src/Web/ControlEasyReborn.Web` (**Angular 18+** standalone-component SPA, TypeScript strict mode, Angular Material) with a single `Residents` page that lists and creates residents, talking to the API through `HttpClient` with a bearer token (added by an HTTP interceptor).
  - Configure `ng-openapi-gen` so DTOs/interfaces are generated from the API's `/swagger/v1/swagger.json` on every build.
  - Configure `proxy.conf.json` for `ng serve` so dev calls go to the API at `https://localhost/api`.
- [ ] **1.9** Add multi-stage Dockerfiles: `docker/api.Dockerfile`, `docker/web.Dockerfile`.
- [ ] **1.10** Add `docker/docker-compose.yml` with services: `reverse-proxy` (Traefik), `api`, `web`, `db` (MySQL 8, with healthcheck), `adminer`, `seq`. Add `docker/.env.example`.
- [ ] **1.11** Add `docker/reverse-proxy/traefik.yml` and `docker/reverse-proxy/dynamic.yml` routing `/api/*` → `api`, `/` → `web`, `/db` → `adminer`. TLS via Let's Encrypt (staging cert in dev).
- [ ] **1.12** Add `docker/mysql/init/00-schema.sql` with the minimal `Residents` table and a `Users` table for JWT issuance.
- [ ] **1.13** Add `tests/ControlEasyReborn.UnitTests` (xUnit + FluentAssertions) covering `ResidentRepository` against an in-memory fake of `IAsyncSqlClient` (only for unit-level logic; real SQL is verified by integration tests).
- [ ] **1.14** Add `tests/ControlEasyReborn.IntegrationTests` (xUnit + Testcontainers.MySql) with a test that boots a real MySQL container, runs the schema, calls `GET /api/v1/residents` through `WebApplicationFactory`, and asserts the round-trip.
- [ ] **1.15** Add a `ArchitectureTests` project using `NetArchTest` asserting: `Domain` projects don't reference `Infrastructure`; `Infrastructure` projects don't reference `Microsoft.EntityFrameworkCore` or `MySql.Data`.
- [ ] **1.16** Add `docs/architecture/decisions/0001-modular-monolith.md` and `0002-dblools-sql-as-only-data-access.md`.
- [ ] **1.17** Add a `Makefile` (or `package.json` of scripts) with `make up`, `make down`, `make logs`, `make test`, `make migrate`.
- **Verification gate (Phase 1):**
  - `docker compose up -d` brings all services healthy in < 2 min.
  - `curl -k https://localhost/health` → 200.
  - `curl -k https://localhost/api/v1/residents` → 200 with `[]` (empty list).
  - Open `https://localhost` in Chrome via Playwright, see the `Residents` page, create one resident, refresh, see it in the list. Screenshot for the user.
  - `dotnet test` is green (unit + integration + architecture).
  - `git grep -ri "EntityFramework\|MySql.Data" src/` returns nothing.

---

## Phase 2 — Strangler Pilot (prove the migration pattern)
*Goal: cut over the *least critical* legacy screen to the new web app, with a feature flag, to prove the Strangler pattern end to end.*

- [ ] **2.1** Implement the `Security` module: `Users` entity, `POST /api/v1/auth/login` → JWT, `POST /api/v1/auth/refresh`, role claims (`Admin`, `Porteiro`, `Morador`).
- [ ] **2.2** Add the `Residents` admin page in the Angular SPA (table, edit, soft-delete) and a login page (reactive forms + `AuthService` with signals).
- [ ] **2.3** Run the legacy WPF app and the new web app in parallel, both pointed at the same MySQL (different app settings). Add `Microsoft.FeatureManagement` flag `Residents.UseWeb` (default `false`).
- [ ] **2.4** Smoke test: start WPF, perform a CRUD on Residents, verify the change is visible in the web app. Reverse: change in the web app is visible in WPF.
- [ ] **2.5** Flip `Residents.UseWeb=true` for one test condominium; monitor for 7 days; collect error rates via Serilog/Seq.
- [ ] **2.6** Update `docs/migration/legacy-mapping.md` marking `Residents` (WPF) as "**Web — pilot live**".
- **Verification gate (Phase 2):**
  - Web app handles 100% of Residents CRUD for the pilot condominium.
  - Zero data inconsistencies between WPF and web for the pilot.
  - p95 latency for `GET /api/v1/residents?search=...` < 200 ms against the seeded DB.
  - Architecture tests still green; no new `MySql.Data` or `EntityFramework` references.

---

## Phase 3 — Module Migrations (parallel feature work)
*Goal: migrate the remaining user-facing modules one by one, in priority order, using the same Strangler pattern.*

- [ ] **3.1** **Visits** module: `Visitante`, `Fluxo` (gatehouse log). Endpoints: `GET/POST /api/v1/visits`, `POST /api/v1/visits/{id}/checkin`, `POST /api/v1/visits/{id}/checkout`, `GET /api/v1/visits?status=open`.
- [ ] **3.2** **Vehicles** module: `Veiculo`, link to `Apartamento` via `Linq<Veiculo>.InnerJoin<Apartamento>()`.
- [ ] **3.3** **ServiceProviders** module: `PrestadorServico` (the old `PrestadoresServico.razor` prototype page is already gone from Phase 0; build a real Angular CRUD page for service providers in this module).
- [ ] **3.4** **Administration** module: audit log, configuration tables.
- [ ] **3.5** For each module: domain entity → application handlers → repository (Linq) → Minimal API endpoints → Angular page (lazy-loaded feature module + generated types) → integration test → feature flag → pilot cutover.
- [ ] **3.6** Add CQRS-lite for the **reports** read paths (visit counts per day, residents per apartment) using `Linq<T>.AsQueryable()` projections.
- [ ] **3.7** Add a **Playwright** UI test for each module's primary page (open in Chrome via the Playwright tool, click "Create", assert the new row appears).
- [ ] **3.8** Update `docs/migration/legacy-mapping.md` marking each row "Web — live" as it cuts over.
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
- [ ] **C.2** Keep `AGENTS.md` updated whenever a new module, library, or convention is added.
- [ ] **C.3** Every ADRs recorded in `docs/architecture/decisions/` with the date and the decision made.
- [ ] **C.4** Every UI change is verified in a real Chrome browser via the Playwright tool before being marked done.
- [ ] **C.5** After every API change, run `ng-openapi-gen` to regenerate the Angular TypeScript client and fix any breaking call sites.
