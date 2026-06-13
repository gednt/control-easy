# Orchestration Log — Multi-Tenant + Attendant Profiles (update to modernization roadmap)

> Spec folder: `.specs/1 - modernization-roadmap/`
> This log tracks the cross-agent work for folding **multi-tenant capacity** and **multiple door-attendant (porteiro) profiles** into the existing Modernization Roadmap spec. The work is an *update* to the existing spec, not a new spec.

## Goal

Extend the Modernization Roadmap so that the platform supports:

1. **Multi-tenant capacity** — N condominiums ("tenants") running on the same deployment, with strict data isolation.
2. **Multiple door-attendant profiles** — a condominium can have many attendants, each with shifts, gatehouse assignments, and a permission set, instead of a single global `Porteiro` role.

## Change map (where each addition lands in the existing spec)

### `requirements.md`
- Add `UC-21..UC-25` covering tenants, tenant isolation, tenant onboarding, attendant profiles, attendant shifts.
- Update `UC-9` to reflect the expanded role set (`PlatformAdmin`, `TenantAdmin`, `Morador`, plus `AttendantProfile` per tenant).
- Update the "Out of Scope" section: **remove** "Multi-tenant SaaS features" (now in scope); **add** "Billing / subscription management", "Cross-tenant analytics".

### `design.md`
- Add `Tenants` to the module list in the solution layout (must be the first module — root aggregate).
- Add a new top-level subsection **"Multi-Tenancy"** covering: data-isolation strategy (with rationale), tenant resolution, JWT claim shape, `ITenantContext`, per-tenant backup.
- Add a new subsection **"Attendant Profiles"** under Security: `User` (identity) vs `AttendantProfile` (per-tenant assignment with shifts, gatehouse, permissions).
- Update the **Security Module** JWT claims list (`tenant_id`, `profile_id`, `roles[]`).
- Update the **module template** so every aggregate carries `TenantId` and every repository takes `ITenantContext`.
- Add **"Tenant Data Migration"** subsection: how the existing single-tenant DB becomes a default tenant on first boot.
- Update glossary: `Tenant`, `AttendantProfile`, `ITenantContext`, `PlatformAdmin`, `TenantAdmin`.

### `tasks.md`
- **Phase 1** — add **1.0a Tenants module (must come first)**: `Tenants` aggregate, `ITenantContext`, tenant resolution middleware, `tenant_id` global filter in `Linq<T>`, `PlatformAdmin` seed, default-tenant backfill migration, tenant-aware integration test fixture.
- **Phase 1** — extend **1.13 / 1.14** to include a cross-tenant-access test.
- **Phase 2** — extend **2.1** to model the full role set + `AttendantProfile`; add `POST /api/v1/security/attendant-profiles` and shift endpoints.
- **Phase 2** — add **2.7a Attendant profile API surface (backend slice)**; **2.7 Attendant profile management UI (Angular)** is reserved for a future Frontend-Agent spec.
- **Phase 3** — add **3.9a Tenant administration API surface (backend slice)**; **3.9 Tenant administration UI (Angular)** is reserved for a future Frontend-Agent spec.
- **Continuous** — add **C.6** architecture rule: every module entity must include `TenantId` and have a cross-tenant-access test.

## Execution phases

| Phase | Agent | Status | Output |
|---|---|---|---|
| 1 | Backend Agent | pending | Drafts the technical deltas (data-isolation decision, `Tenants` module design, `AttendantProfile` model, request-pipeline changes, JWT claim shape, new tasks with acceptance criteria). |
| 2 | Documentation Agent | pending | Merges Backend Agent's deltas into the three existing files, preserving structure. |
| 3 | Code Review Agent | pending | Consistency review of the merged docs. |

## Open questions resolved by the Backend Agent

1. Final isolation strategy + rationale.
2. Tenant resolution approach (JWT claim vs. subdomain vs. path).
3. `AttendantProfile` shape: fields, shift model, permission model.
4. Default-tenant backfill SQL strategy.
5. Whether `PlatformAdmin` ships in v1 or is deferred.

## Coordination log

### Phase 1 — Backend Agent (done)

- **Delta document written:** `.specs/1 - modernization-roadmap/tenant-and-attendant-deltas.md`
- **Decisions made:**
  1. Isolation: shared schema + `tenant_id` discriminator, enforced by a `TenantFilterInterceptor : IQueryInterceptor` from DBTools_SQL.
  2. Tenant resolution: JWT `tenant_id` claim only.
  3. `AttendantProfile` shape: decoupled from `User`; one `User` can have many `AttendantProfile` rows across tenants.
  4. Default-tenant backfill: SQL seed + `tenant_id` backfill + WPF compatibility views (`X_legacy`); rejected EF6 interceptor.
  5. `PlatformAdmin` in v1: required, bootstrapped on first boot by a hosted service.
- **Follow-ups flagged for downstream phases:**
  - **R1 (Documentation Agent — Phase 2):** In `design.md`, the Security subsection must restate the rule that **all per-tenant reads go through `TenantAwareLinqFactory` / `Linq<TModel>`**; raw `IAsyncSqlClient.ExecuteAsync` is not covered by the global filter and is the documented escape hatch only.
  - **R2 (Documentation Agent — Phase 2):** Add a rate-limit note to the `GET /api/v1/security/tenants?email=...` endpoint (the public tenant-picker lookup) — 10/min/IP, since it is an email-enumeration surface.
  - **R3 (Documentation Agent — Phase 2):** Add a NetArchTest rule to the existing 1.15 task: a `CREATE TABLE` in `docker/mysql/init/` that is not listed in the backfill script must fail the build (per task 1.0b).
  - **R4 (Frontend Agent — when its phase runs):** The Angular login screen must consume `GET /api/v1/security/tenants?email=...` to build the tenant picker, and must support `POST /api/v1/security/tenant-switch` to re-issue a JWT.
  - **R5 (DevOps Agent — when its phase runs):** Add `docker/cron/tenant-backup.Dockerfile` for nightly per-tenant `mysqldump`; document the PostgreSQL follow-up for when the platform moves providers.
  - **R6 (Code Review Agent — Phase 3):** Confirm all six follow-ups landed in the merged docs.
  - **Tooling note:** `agents/BackendAgent/AGENTS.md` does not currently exist in the repo. The Backend Agent operated from the repo-root `AGENTS.md` and the global agent rules. Create the file before the next phase that delegates to the Backend Agent runs again.

### Phase 2 — Documentation Agent (done)

- **Updated files in place:**
  - `.specs/1 - modernization-roadmap/requirements.md` (48 → 56 lines)
  - `.specs/1 - modernization-roadmap/design.md` (456 → 553 lines)
  - `.specs/1 - modernization-roadmap/tasks.md` (108 → 159 lines)
- **Follow-ups addressed by the Documentation Agent:**
  - **R1 (TenantAwareLinqFactory rule):** Addressed — added explicit bullet in the Multi-Tenancy subsection.
  - **R2 (rate-limit on tenant-picker lookup):** Addressed — "rate-limited to 10/min/IP" added to the `GET /api/v1/security/tenants?email=...` endpoint description.
  - **R3 (NetArchTest backfill rule):** Addressed — added as a sub-bullet under task 1.15.
  - **R4 (Frontend Agent):** Out of scope for this phase. The Angular tenant-picker and tenant-switch flows are owned by the Frontend Agent in a future spec.
  - **R5 (DevOps Agent):** Out of scope for this phase. The nightly per-tenant backup Dockerfile is owned by the DevOps Agent in a future spec.
  - **R6 (Code Review Agent):** Flagged for Phase 3 below.
- **Placement decisions** (Documentation Agent): Multi-Tenancy placed before API Design; Attendant Profiles placed as a sibling of Multi-Tenancy; Tenant Data Migration placed after Cross-Cutting Concerns.
- **Ambiguities flagged for Phase 3:**
  1. Task 1.0b depends on a NetArchTest rule added in 1.15; the Code Review Agent should confirm the rule ships in the same PR as 1.0b (or earlier).
  2. C.6 references "the existing C.1 GitHub Actions `test` job" — confirm C.1's job name matches.
  3. Glossary sort order: new terms added at the bottom of the table in alphabetical order, not re-sorted globally.
  4. Task numbering 2.7a vs 2.7: the delta explicitly chose 2.7a to leave room for a future Frontend-Agent-owned 2.7 UI task; preserved as 2.7a.
  5. Policy list line in design.md: the Documentation Agent updated `AdminOnly, PorteiroOnly, ResidentSelfOrAdmin` → `PlatformAdminOnly, TenantAdminOnly, ResidentSelfOrAdmin, plus fine-grained RequirePermission(...)` for consistency with the new role set. Code Review Agent should confirm this is acceptable (it goes beyond the literal delta instructions).
- **Typo note (separate edit, NOT for Phase 3):** `requirements.md` still has a duplicated `### Frontend` heading (lines 14 and 18). The Documentation Agent did not fix it; recorded here for a future cleanup pass.

### Phase 3 — Code Review Agent (done)

- **Review report:** `.specs/1 - modernization-roadmap/review.md` (verdict: **Request Changes** — 3 Major blockers + 2 Minor fixes).
- **Per-ambiguity verdicts:**
  1. **1.0b / 1.15 NetArchTest rule ordering:** **Needs fix** → became F2 (applied).
  2. **C.6 referencing C.1's `test` job:** **OK** — names match.
  3. **Glossary sort order:** **OK** — new terms appended in delta order, consistent with the existing grouped (not alphabetical) glossary style.
  4. **2.7a / 3.9a numbering:** **OK** — gap from 2.6 preserves a slot for future Frontend-Agent UI tasks; no cross-references break.
  5. **Policy list update in design.md:** **OK** — Documentation Agent's expansion was a necessary consistency fix.
- **Follow-up edits applied by the orchestrator (post-review):**
  - **F1 (Major — duplicate task 2.1):** Replaced the conflicting two-line definition in `tasks.md:79` with a single unified task that uses the new role set. Endpoints sub-bullets preserved.
  - **F2 (Major — 1.0b/1.15 wording):** Reworded `tasks.md:29` so the NetArchTest rule is explicit about shipping in the same PR as 1.0b.
  - **F3 (Major — missing `tenant-switch` endpoint + wording):** Fixed `design.md:310` ("POST /api/v1/security/..." → "/api/v1/security/...") and appended the `POST /api/v1/security/tenant-switch` endpoint bullet after the `tenants?email=...` line.
  - **F4 (Minor — change map stale):** Updated `orchestration.md:33-34` to reference `2.7a`/`3.9a` and note that `2.7`/`3.9` are reserved for future Frontend-Agent UI specs.
   - **F5 (Minor — missing `Attendant` glossary row):** Inserted the `Attendant` role row in `design.md:34`, between `TenantAdmin` and `TenantResolutionStrategy`.
- **Final consistency sweep:**
  - Single task 2.1 (no duplicate).
  - `tenant-switch` endpoint present in both `design.md` and `tasks.md`.
  - No leftover `condominium_id` references.
  - No leftover single-`Porteiro` role references (the only remaining `Porteiro` mention is in UC-24, where it is an intentional contrast with the old model).
  - No emojis in any of the three updated files.

### Phase 4 — Documentation Agent (design-system cross-reference)

- **Date:** 2026-06-12
- **Files touched:**
  - `.specs/1 - modernization-roadmap/docchange.md` (new)
  - `.specs/1 - modernization-roadmap/requirements.md` (one-line cross-references added under UC-4, UC-5, UC-6; no structural change)
  - `.specs/1 - modernization-roadmap/design.md` (new "Design system & visual prototype" top-level subsection added; four rows in the "Components & Files" table; one row in the "Cross-Cutting Concerns" table; the "Verification Approach" Visual bullet updated)
  - `.specs/1 - modernization-roadmap/tasks.md` (task 1.8 extended; Phase 1 verification gate extended; C.2 and C.4 updated; new C.7 added; Task Dependency Graph unchanged)
  - `.specs/1 - modernization-roadmap/orchestration.md` (this entry)
- **Summary of changes:** added a single canonical "Design system & visual prototype" section in `design.md` that lists `.specs/2 - visual-design-system/`, `docs/penpot/design-system.penpot.json`, `docs/penpot/tokens.json`, `docs/penpot/manifest.json`, and `mockup/` (including `mockup/SMOKE.md`); turned every other touchpoint in the roadmap into a one-line link back to that section (no duplicate lists); added new continuous task C.7 to enforce token parity between `docs/penpot/tokens.json` and the Angular `@theme` block in CI.
- **Placement decision (per orchestrator):** the new "Design system & visual prototype" section was placed between the existing "Frontend Design" and "Containerization" subsections in `design.md` so that the visual contract sits with the other frontend material but is not nested under it.
- **No re-sort of the glossary** in `design.md` (per the existing convention noted in `review.md`).
- **No change to task numbers** (continuous task C.7 is the next free number; the existing C.1..C.6 sequence is preserved).
- **No emojis** in any of the updated files.
- **No code changes** under `src/`, `mockup/`, or `docs/penpot/`.

## Outcome

The Modernization Roadmap spec is now internally consistent and includes:
- 5 new user stories (UC-21..UC-25) covering tenants, tenant isolation, tenant onboarding, attendant profiles, and shifts.
- An updated UC-9 reflecting the new role set (`PlatformAdmin`, `TenantAdmin`, `Morador`, `AttendantProfile`).
- A new "Multi-Tenancy" subsection (isolation strategy, tenant resolution, JWT claims, `ITenantContext`, per-tenant backup, `PlatformAdmin` bootstrap, default-tenant seeding).
- A new "Attendant Profiles" subsection under Security (User vs AttendantProfile, shift model, permission model, gatehouse assignment, multi-tenant attendant support, repository, API endpoints).
- A new "Tenant Data Migration" subsection (backfill SQL, default-tenant row, WPF compatibility shim).
- 5 new tasks (1.0a, 1.0b, 1.0c, 2.7a, 3.9a) and one new continuous task (C.6).
- Updated tasks 1.13, 1.14, 2.1 to reflect the new model.
- Updated "Out of Scope" list (removed "Multi-tenant SaaS features"; added "Billing / subscription management" and "Cross-tenant analytics").
- Updated glossary with 6 new terms + the `Attendant` role.

The implementation work for these additions is owned by:
- **Backend Agent** — all backend tasks (1.0a, 1.0b, 1.0c, updated 1.13/1.14, updated 2.1, 2.7a, 3.9a, C.6).
- **Frontend Agent** — Angular UI for the login tenant-picker, attendant profile management (`2.7`), and tenant administration (`3.9`). Not started in this round.
- **DevOps Agent** — `docker/cron/tenant-backup.Dockerfile` (R5). Not started in this round.

The spec is ready for backend implementation.
