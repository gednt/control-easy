# Roadmap: ControlEasy Reborn

## Overview

Brownfield modernization of ControlEasy 5 into a modular web platform. Phases 1–6 shipped the foundation, core modules, tenant UX, and verification remediation. Remaining work focuses on design system polish, UI parity, dashboard live data, demo mode, and continuous engineering — all traced to `.specs/` folders.

**Spec inventory:** 19 folders under `.specs/` · ~191/308 checkbox tasks complete (2026-06-24)

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

- [ ] **Phase 7: Design System Hardening** — Token unification, component refactors, formal spec gap-fill *(IN PROGRESS)*
- [ ] **Phase 8: UI Parity & Functional Fixes** — Mockup visual parity + interaction fixes
- [ ] **Phase 9: Dashboard & Quick Wins** — Live stats, occupied-apartments bugfix, vehicle edit
- [ ] **Phase 10: Demo Mode** — One-command seeded evaluation stack
- [ ] **Phase 11: Photo Capture & Hardware** — Photos module + hardware framework *(v2)*
- [ ] **Phase 12: Continuous Engineering** — CI pipeline, cross-tenant tests, tokens contract
- [ ] **Phase 13: Multi-Arch Platform** — arm64 Docker/CI delta *(v2)*

## Phase Details

### Phase 1: Foundation ✅
**Goal**: Empty modular monolith + Docker stack + DBTools_SQL wiring with Residents smoke module
**Spec**: `.specs/1 - modernization-roadmap/` (tasks 1.0a–1.17)
**Status**: Complete — 20/20 tasks checked
**Success Criteria**:
  1. `docker compose up -d` brings all services healthy
  2. `GET /api/v1/residents` returns 200
  3. Angular Residents page creates and lists residents
  4. `dotnet test` green (unit + integration + architecture)

### Phase 2: Strangler Pilot ✅
**Goal**: Prove migration pattern with Security module and Residents web cutover
**Spec**: `.specs/1 - modernization-roadmap/` (tasks 2.1–2.7a) · `.specs/2-strangler-pilot/orchestration.md`
**Status**: Complete — WPF coexistence tasks (2.4, 2.5) cancelled
**Success Criteria**:
  1. JWT login with tenant_id, profile_id, roles claims
  2. Residents admin page with CRUD in Angular
  3. Attendant profile API surface with OpenAPI types

### Phase 3: Module Migrations ✅
**Goal**: Migrate remaining user-facing modules using Strangler pattern
**Spec**: `.specs/1 - modernization-roadmap/` (tasks 3.1–3.9a) · `.specs/apartments-and-residents-form/`
**Status**: Complete — 23/23 apartments spec tasks also complete
**Success Criteria**:
  1. All modules have Web — live in legacy-mapping doc
  2. Playwright UI test per primary page
  3. CQRS-lite reports read paths working

### Phase 4: Legacy Decommission ✅
**Goal**: Retire WPF and Strangler infrastructure
**Spec**: `.specs/1 - modernization-roadmap/` (tasks 4.1–4.6)
**Status**: Complete (cancelled) — WPF not in repo; all tasks marked complete with cancellation rationale

### Phase 5: Tenant & Session UX ✅
**Goal**: Multi-tenant operator experience and first-boot documentation
**Specs**:
- `.specs/post-login-role-routing/` — 3/3 ✅
- `.specs/tenant-administration-ui/` — 7/7 ✅
- `.specs/platform-admin-first-boot/` — 7/7 ✅ (bugfix)
- `.specs/getting-started/` — 24/24 ✅
**Success Criteria**:
  1. Post-login routes by role (PlatformAdmin vs porteiro)
  2. PlatformAdmin can manage condominiums
  3. First-boot credentials work on non-demo stack
  4. Operator guide documents compose up and troubleshooting

### Phase 6: Apartments & Verification ✅
**Goal**: Complete apartments feature slice and restore verification confidence
**Specs**:
- `.specs/apartment-edit-residents/` — 7/7 ✅
- `.specs/6 - verification-remediation/` — 10/10 ✅
**Success Criteria**:
  1. Manage residents inline from apartment edit modal
  2. Playwright auth fixes green
  3. Demo integration test repaired

---

### Phase 7: Design System Hardening 🚧
**Goal**: Unify tokens, refactor pages to `ce-*` components, close formal design system gaps
**Depends on**: Phase 6
**Specs**:
- `.specs/fix-design-system/` — **70/74** (4 verification sub-tasks open: 12.5–12.8)
- `.specs/2 - visual-design-system/` — **5/64** (formal spec; much work landed via 1.8 + fix-design-system)
**Requirements**: UI-01, UI-02, UI-05
**Success Criteria**:
  1. Zero `var(--spacing-*)` or `color: white` in component CSS
  2. Login, change-password, dashboard use design system components
  3. Modal, dropdown, tabs, breadcrumbs behave correctly in light/dark themes
**Plans**: 2 plans

Plans:
- [ ] 07-01: Complete fix-design-system verification gate (tasks 12.5–12.8)
- [ ] 07-02: Close remaining formal visual-design-system spec gaps (Phases A–G)

### Phase 8: UI Parity & Functional Fixes
**Goal**: Align Angular UI with mockup/Penpot reference; fix non-functional interactions
**Depends on**: Phase 7
**Specs**:
- `.specs/mockup-visual-parity/` — 0/71
- `.specs/2-mockup-functional-fixes/` — 0/15
**Requirements**: UI-03, UI-04
**Success Criteria**:
  1. Shell, login, residents page match mockup pixel parity
  2. Modals, dropdowns, filters, pagination work end-to-end
  3. Lucide icons, drawer, topbar aligned with Penpot handoff
**Plans**: 2 plans

Plans:
- [ ] 08-01: Mockup visual parity (shell, login, feature pages)
- [ ] 08-02: Mockup functional fixes (interactions, error states)

### Phase 9: Dashboard & Quick Wins
**Goal**: Replace static dashboard with live stats; fix data bugs; add vehicle edit
**Depends on**: Phase 7 (dashboard uses design tokens)
**Specs**:
- `.specs/dashboard/` — 0/3 (T1–T3, no checkboxes)
- `.specs/occupied-apartments-bug/` — 0/3 (bugfix)
- `.specs/vehicle-edit/` — 0/2 (T1–T2)
**Requirements**: DASH-01, DASH-02, DASH-03, DASH-04
**Success Criteria**:
  1. `GET /api/v1/dashboard/stats` returns live tenant-scoped counts
  2. Dashboard tiles and recent visits render correctly
  3. Occupied-apartments count correct across tenants
  4. Vehicle edit modal mirrors residents pattern
**Plans**: 3 plans

Plans:
- [ ] 09-01: Dashboard live stats (backend + frontend)
- [ ] 09-02: Occupied-apartments bugfix + test
- [ ] 09-03: Vehicle edit UI

### Phase 10: Demo Mode
**Goal**: One-command pre-populated demo stack for evaluation
**Depends on**: Phase 9
**Spec**: `.specs/4 - demo-mode/` — 0/11
**Requirements**: DEMO-01, DEMO-02
**Success Criteria**:
  1. Demo overlay compose starts with two seeded tenants
  2. Fixed credentials documented; demo banner visible in UI
  3. Reset procedure documented and tested
**Plans**: 1 plan

Plans:
- [ ] 10-01: Demo mode stack, seed scripts, UI banner, docs

### Phase 11: Photo Capture & Hardware (v2)
**Goal**: Photos module and pluggable hardware framework
**Depends on**: Phase 10
**Spec**: `.specs/3 - photo-capture-hardware-integration/` — 0/35
**Requirements**: PHOTO-01, PHOTO-02
**Success Criteria**:
  1. Photo capture, storage, thumbnails working
  2. Hardware adapter framework for biometrics/cameras/intercoms
**Plans**: TBD

### Phase 12: Continuous Engineering
**Goal**: CI pipeline, architecture guardrails, tokens contract enforcement
**Depends on**: Phase 7 (tokens contract needs design system stable)
**Spec**: `.specs/1 - modernization-roadmap/` (tasks C.1–C.7) — 0/7
**Requirements**: CI-01, CI-02, CI-03, CI-04
**Success Criteria**:
  1. GitHub Actions: lint, build matrix, test, docker, smoke
  2. Cross-tenant test rule fails build on new modules without coverage
  3. Tokens-contract check diffs penpot vs styles.css on every PR
**Plans**: 2 plans

Plans:
- [ ] 12-01: GitHub Actions CI pipeline (C.1)
- [ ] 12-02: Architecture guardrails (C.6, C.7) + doc sync (C.2–C.5)

### Phase 13: Multi-Arch Platform (v2)
**Goal**: Multi-arch Docker builds for linux/arm64 and windows/arm64
**Depends on**: Phase 12
**Spec**: `.specs/1 - modernization-roadmap-arm64/` — 0/8
**Requirements**: ARCH-01, ARCH-02
**Success Criteria**:
  1. `docker-bake.hcl` multi-arch matrix
  2. GHCR manifest with smoke and signing
**Plans**: 1 plan

Plans:
- [ ] 13-01: Multi-arch Docker/CI delta

## Progress

**Execution Order:** 7 → 8 → 9 → 10 → (11 v2) → 12 → (13 v2)

| Phase | Spec(s) | Tasks | Status | Completed |
|-------|---------|-------|--------|-----------|
| 1. Foundation | `1 - modernization-roadmap` | 20/20 | ✅ Complete | — |
| 2. Strangler Pilot | `1 - modernization-roadmap` | 7/7 | ✅ Complete | — |
| 3. Module Migrations | `1 - modernization-roadmap`, `apartments-and-residents-form` | 9/9 + 23/23 | ✅ Complete | — |
| 4. Legacy Decommission | `1 - modernization-roadmap` | 6/6 cancelled | ✅ Complete | — |
| 5. Tenant & Session UX | `post-login-role-routing`, `tenant-administration-ui`, `platform-admin-first-boot`, `getting-started` | 41/41 | ✅ Complete | — |
| 6. Apartments & Verification | `apartment-edit-residents`, `6 - verification-remediation` | 17/17 | ✅ Complete | — |
| 7. Design System Hardening | `fix-design-system`, `2 - visual-design-system` | 70/74 + 5/64 | 🚧 In Progress | — |
| 8. UI Parity & Fixes | `mockup-visual-parity`, `2-mockup-functional-fixes` | 0/86 | Not started | — |
| 9. Dashboard & Quick Wins | `dashboard`, `occupied-apartments-bug`, `vehicle-edit` | 0/8 | Not started | — |
| 10. Demo Mode | `4 - demo-mode` | 0/11 | Not started | — |
| 11. Photo & Hardware | `3 - photo-capture-hardware-integration` | 0/35 | Deferred (v2) | — |
| 12. Continuous Engineering | `1 - modernization-roadmap` C.1–C.7 | 0/7 | Not started | — |
| 13. Multi-Arch | `1 - modernization-roadmap-arm64` | 0/8 | Deferred (v2) | — |

**Overall spec checkbox progress:** ~191/308 (62%) across all `.specs/` folders

---
*Roadmap created: 2026-06-24 from `.specs/` inventory*
*Current focus: Phase 7 — Design System Hardening*
