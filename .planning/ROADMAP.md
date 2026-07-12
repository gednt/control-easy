# Roadmap: ControlEasy Reborn

## Overview

Brownfield modernization of ControlEasy 5 into a modular web platform. Phases 1–6 shipped the foundation, core modules, tenant UX, and verification remediation. **Phase 7** replaces vendored DBTools with the official NuGet package. Remaining work covers design system polish, UI parity, dashboard live data, demo mode, and continuous engineering — all traced to `.specs/` folders.

**Spec inventory:** 20 folders under `.specs/` · ~191/315 checkbox tasks complete (2026-06-24)

## Phases

<details>
<summary>✅ Phases 1–6 — SHIPPED (foundation through feature slices)</summary>

- [x] **Phase 1: Foundation** — Modular monolith, Docker, Tenants, Residents smoke module
- [x] **Phase 2: Strangler Pilot** — Security module, login, Residents admin, feature flags
- [x] **Phase 3: Module Migrations** — Visits, Vehicles, ServiceProviders, Administration, Reports
- [x] **Phase 4: Legacy Decommission** — Cancelled items marked complete (WPF not in repo)
- [x] **Phase 5: Tenant & Session UX** — Post-login routing, tenant admin UI, first-boot docs
- [x] **Phase 6: Apartments & Verification** — Apartments module, edit-residents, Playwright remediation

</details>

- [ ] **Phase 7: DBTools NuGet Migration (INSERTED)** — Replace `src/lib/DBTools_SQL/` with [DBTools 1.4.3](https://www.nuget.org/packages/DBTools) ✅
- [ ] **Phase 8: Design System Hardening** — Token unification, component refactors, formal spec gap-fill *(was Phase 7)*
- [ ] **Phase 9: UI Parity & Functional Fixes** — Mockup visual parity + interaction fixes
- [ ] **Phase 10: Dashboard & Quick Wins** — Live stats, occupied-apartments bugfix, vehicle edit
- [ ] **Phase 11: Demo Mode** — One-command seeded evaluation stack
- [ ] **Phase 12: Photo Capture & Hardware** — Photos module + hardware framework *(v2)*
- [ ] **Phase 13: Continuous Engineering** — CI pipeline, cross-tenant tests, tokens contract
- [ ] **Phase 14: Multi-Arch Platform** — arm64 Docker/CI delta *(v2)*

## Phase Details

### Phase 1: Foundation ✅
**Goal**: Empty modular monolith + Docker stack + DBTools wiring with Residents smoke module
**Spec**: `.specs/1 - modernization-roadmap/` (tasks 1.0a–1.17)
**Status**: Complete — 20/20 tasks checked
**Success Criteria**:
  1. `docker compose up -d` brings all services healthy
  2. `GET /api/v1/residents` returns 200
  3. Angular Residents page creates and lists residents
  4. `dotnet test` green (unit + integration + architecture)

### Phase 2: Strangler Pilot ✅
**Goal**: Prove migration pattern with Security module and Residents web cutover
**Spec**: `.specs/1 - modernization-roadmap/` (tasks 2.1–2.7a)
**Status**: Complete — WPF coexistence tasks cancelled

### Phase 3: Module Migrations ✅
**Goal**: Migrate remaining user-facing modules using Strangler pattern
**Spec**: `.specs/1 - modernization-roadmap/` · `.specs/apartments-and-residents-form/`
**Status**: Complete

### Phase 4: Legacy Decommission ✅
**Goal**: Retire WPF and Strangler infrastructure
**Status**: Complete (cancelled)

### Phase 5: Tenant & Session UX ✅
**Specs**: `post-login-role-routing`, `tenant-administration-ui`, `platform-admin-first-boot`, `getting-started`
**Status**: Complete — 41/41 tasks

### Phase 6: Apartments & Verification ✅
**Specs**: `apartment-edit-residents`, `6 - verification-remediation`
**Status**: Complete — 17/17 tasks

---

### Phase 7: DBTools NuGet Migration (INSERTED) ✅
**Goal**: Replace vendored `src/lib/DBTools_SQL/` with official NuGet package; simplify Docker build and dependency management
**Depends on**: Phase 6
**Spec**: `.specs/dbtools-nuget-migration/` — **7/7**
**Requirements**: PLAT-01
**Package**: [DBTools 1.4.3](https://www.nuget.org/packages/DBTools) (.NET 8, multi-provider LINQ, `IAsyncSqlClient`, `AddDbTools` DI)
**Current state**:
- Vendored at `src/lib/DBTools_SQL/DBTools/` with `<ProjectReference>` from `ControlEasyReborn.Infrastructure`
- Listed in `src/ControlEasyReborn.sln` and copied in `docker/api.Dockerfile`
**Success Criteria**:
  1. `Directory.Packages.props` pins `DBTools` 1.4.3; Infrastructure uses `<PackageReference>`
  2. `src/lib/DBTools_SQL/` removed; solution folder gone
  3. `docker/api.Dockerfile` restores from NuGet (no vendored COPY)
  4. `dotnet test` green including cross-tenant interceptor tests
  5. Docker API container healthy after rebuild
**Plans**: 1 plan

Plans:
- [x] 07-01: DBTools NuGet swap (CPM pin, project ref → package ref, remove vendor, Docker, docs)

### Phase 8: Design System Hardening 🚧
**Goal**: Unify tokens, refactor pages to `ce-*` components, close formal design system gaps
**Depends on**: Phase 7
**Specs**:
- `.specs/fix-design-system/` — **70/74** (4 verification sub-tasks open)
- `.specs/2 - visual-design-system/` — **5/64**
**Requirements**: UI-01, UI-02, UI-05
**Plans**: 2 plans

Plans:
- [ ] 08-01: Complete fix-design-system verification gate (tasks 12.5–12.8)
- [ ] 08-02: Close remaining formal visual-design-system spec gaps

### Phase 9: UI Parity & Functional Fixes
**Goal**: Align Angular UI with mockup/Penpot reference; fix non-functional interactions
**Depends on**: Phase 8
**Specs**: `mockup-visual-parity` (0/71), `2-mockup-functional-fixes` (0/15)
**Requirements**: UI-03, UI-04
**Plans**: 2 plans

Plans:
- [ ] 09-01: Mockup visual parity
- [ ] 09-02: Mockup functional fixes

### Phase 10: Dashboard & Quick Wins
**Goal**: Live dashboard stats; fix data bugs; vehicle edit
**Depends on**: Phase 8
**Specs**: `dashboard`, `occupied-apartments-bug`, `vehicle-edit`
**Requirements**: DASH-01–04
**Plans**: 3 plans

Plans:
- [ ] 10-01: Dashboard live stats
- [ ] 10-02: Occupied-apartments bugfix
- [ ] 10-03: Vehicle edit UI

### Phase 11: Demo Mode
**Spec**: `.specs/4 - demo-mode/` — 10/10 (spec numbered 8.1–8.10; the 11th "task" was a continuous-engineering item in `1 - modernization-roadmap/tasks.md`, not a demo task)
**Requirements**: DEMO-01, DEMO-02
**Status**: ✅ Complete on disk (verified 2026-07-12) — flag set in `STATE.md` to move "Phase 11" to "Complete"; the spec checkbox reconciliation is in commit history.

Plans:
- [x] 11-01: Demo mode stack, seed scripts, UI banner, docs

### Phase 12: Photo Capture & Hardware (v2)
**Spec**: `.specs/3 - photo-capture-hardware-integration/` — 0/35
**Plans**: TBD

### Phase 13: Continuous Engineering
**Spec**: `.specs/1 - modernization-roadmap/` C.1–C.7 — 0/7
**Requirements**: CI-01–04
**Plans**: 2 plans

Plans:
- [ ] 13-01: GitHub Actions CI pipeline
- [ ] 13-02: Architecture guardrails + doc sync

### Phase 14: Multi-Arch Platform (v2)
**Spec**: `.specs/1 - modernization-roadmap-arm64/` — 0/8
**Plans**: 1 plan

Plans:
- [ ] 14-01: Multi-arch Docker/CI delta

## Progress

**Execution Order:** 7 → 8 → 9 → 10 → 11 → (12 v2) → 13 → (14 v2)

| Phase | Spec(s) | Tasks | Status | Completed |
|-------|---------|-------|--------|-----------|
| 1. Foundation | `1 - modernization-roadmap` | 20/20 | ✅ Complete | — |
| 2. Strangler Pilot | `1 - modernization-roadmap` | 7/7 | ✅ Complete | — |
| 3. Module Migrations | `1 - modernization-roadmap`, `apartments-and-residents-form` | 32/32 | ✅ Complete | — |
| 4. Legacy Decommission | `1 - modernization-roadmap` | 6/6 cancelled | ✅ Complete | — |
| 5. Tenant & Session UX | 4 specs | 41/41 | ✅ Complete | — |
| 6. Apartments & Verification | 2 specs | 17/17 | ✅ Complete | — |
| 7. DBTools NuGet Migration | `dbtools-nuget-migration` | 7/7 | ✅ Complete | 2026-06-24 |
| 8. Design System | `fix-design-system`, `2 - visual-design-system` | 75/138 | 🎯 Next | — |
| 9. UI Parity & Fixes | 2 specs | 0/86 | Not started | — |
| 10. Dashboard & Quick Wins | 3 specs | 0/8 | Not started | — |
| 11. Demo Mode | `4 - demo-mode` | 10/10 | ✅ Complete | 2026-07-12 |
| 12. Photo & Hardware | `3 - photo-capture-hardware-integration` | 0/35 | Deferred (v2) | — |
| 13. Continuous Engineering | C.1–C.7 | 0/7 | Not started | — |
| 14. Multi-Arch | `1 - modernization-roadmap-arm64` | 0/8 | Deferred (v2) | — |

---
*Roadmap updated: 2026-06-24 — Phase 7 inserted (DBTools NuGet migration)*
*Current focus: Phase 7 — DBTools NuGet Migration*
