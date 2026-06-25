# Requirements: ControlEasy Reborn

**Defined:** 2026-06-24
**Core Value:** Gatehouse staff can reliably register and control access for residents, visitors, and vehicles through a fast, tenant-isolated web UI backed by a secure multi-tenant API.

## v1 Requirements

Requirements for the modernization milestone. Status reflects `.specs/` task completion as of 2026-06-24.

### Foundation & Platform

- [x] **FOUND-01**: Modular monolith solution with Clean Architecture per module
- [x] **FOUND-02**: Multi-tenant data isolation via JWT `tenant_id` + query interceptor
- [x] **FOUND-03**: Docker Compose stack (api, web, db, reverse-proxy, adminer)
- [x] **FOUND-04**: DBTools LINQ-first data access via vendored library (no EF Core in Reborn)
- [ ] **PLAT-01**: DBTools consumed from NuGet ([DBTools 1.4.3](https://www.nuget.org/packages/DBTools)) — vendored `src/lib/DBTools_SQL/` removed
- [x] **FOUND-05**: JWT authentication with PlatformAdmin, TenantAdmin, Attendant roles
- [x] **FOUND-06**: OpenAPI + ng-openapi-gen TypeScript client generation
- [x] **FOUND-07**: Integration tests with Testcontainers.MySql
- [x] **FOUND-08**: Architecture tests (NetArchTest layer rules)

### Core Modules

- [x] **MOD-01**: Residents module (CRUD, search, soft-delete)
- [x] **MOD-02**: Visits module (check-in/out, open visits list)
- [x] **MOD-03**: Vehicles module (linked to apartments)
- [x] **MOD-04**: ServiceProviders module (CRUD)
- [x] **MOD-05**: Administration module (audit log, config)
- [x] **MOD-06**: Apartments module (CRUD, picker, visits integration)
- [x] **MOD-07**: Reports read paths (CQRS-lite projections)
- [x] **MOD-08**: Security module (attendant profiles, shifts, gatehouses, tenant switch)

### Tenant & Admin UX

- [x] **TENANT-01**: Post-login tenant-scoped session and role-based routing
- [x] **TENANT-02**: PlatformAdmin tenant administration UI (list/register/suspend/resume)
- [x] **TENANT-03**: PlatformAdmin first-boot login works on non-demo stack
- [x] **TENANT-04**: Apartment edit modal: view/add/deactivate residents inline
- [x] **TENANT-05**: First-boot operator documentation (`docs/getting-started.md`)

### Design System & UI

- [ ] **UI-01**: Unified design tokens (`--space-*`, `--color-*`, dark theme)
- [ ] **UI-02**: All auth pages use `ce-*` design system components
- [ ] **UI-03**: Mockup visual parity (shell, login, residents, feature pages)
- [ ] **UI-04**: Mockup functional fixes (modals, dropdowns, filters, pagination)
- [ ] **UI-05**: Formal design system showcase and component library complete

### Dashboard & Data Quality

- [ ] **DASH-01**: Live dashboard stats endpoint (`GET /api/v1/dashboard/stats`)
- [ ] **DASH-02**: Dashboard stat tiles and recent visits UI
- [ ] **DASH-03**: Occupied-apartments count correct across tenants (bugfix)
- [ ] **DASH-04**: Vehicle edit UI (update modal mirroring residents pattern)

### Demo & Evaluation

- [ ] **DEMO-01**: One-command demo stack with seeded tenants and fixed credentials
- [ ] **DEMO-02**: Demo UI banner and reset procedures

### Continuous Engineering

- [ ] **CI-01**: GitHub Actions (lint, build matrix, test, docker, smoke)
- [ ] **CI-02**: Cross-tenant integration test rule on every module
- [ ] **CI-03**: Tokens-contract CI check (penpot vs styles.css)
- [ ] **CI-04**: ng-openapi-gen regeneration on every API change

## v2 Requirements

Deferred to future milestone. Tracked in `.specs/` but not in current execution path.

### Photo & Hardware

- **PHOTO-01**: Photos module (capture, storage, thumbnails)
- **PHOTO-02**: Pluggable hardware framework (biometrics, cameras, intercoms)

### Platform

- **ARCH-01**: Multi-arch Docker builds (linux/arm64, windows/arm64)
- **ARCH-02**: GHCR multi-arch manifest and signing pipeline

## Out of Scope

| Feature | Reason |
|---------|--------|
| WPF/web coexistence smoke tests | WPF not in repo; cannot run on macOS dev |
| WPF project removal | Lives outside ControlEasyReborn.sln |
| Legacy data migration | Separate data-migration spec |
| Production cutover checklist | Separate DevOps spec |
| Mobile-native apps | Responsive web sufficient for v1 |
| Billing / subscriptions | External system |
| Cross-tenant analytics BI | Separate product |

## Traceability

| Requirement | Phase | Spec | Status |
|-------------|-------|------|--------|
| FOUND-01–08 | Phase 1 | `1 - modernization-roadmap` | Complete |
| MOD-01–08 | Phases 2–3 | `1 - modernization-roadmap`, `apartments-and-residents-form` | Complete |
| TENANT-01 | Phase 5 | `post-login-role-routing` | Complete |
| TENANT-02 | Phase 5 | `tenant-administration-ui` | Complete |
| TENANT-03 | Phase 5 | `platform-admin-first-boot` | Complete |
| TENANT-04 | Phase 6 | `apartment-edit-residents` | Complete |
| TENANT-05 | Phase 5 | `getting-started` | Complete |
| PLAT-01 | Phase 7 | `dbtools-nuget-migration` | Pending |
| UI-01–02 | Phase 8 | `fix-design-system` | In Progress (70/74) |
| UI-03 | Phase 9 | `mockup-visual-parity` | Pending |
| UI-04 | Phase 9 | `2-mockup-functional-fixes` | Pending |
| UI-05 | Phase 8 | `2 - visual-design-system` | Pending |
| DASH-01–02 | Phase 10 | `dashboard` | Pending |
| DASH-03 | Phase 10 | `occupied-apartments-bug` | Pending |
| DASH-04 | Phase 10 | `vehicle-edit` | Pending |
| DEMO-01–02 | Phase 11 | `4 - demo-mode` | Pending |
| PHOTO-01–02 | Phase 12 | `3 - photo-capture-hardware-integration` | Deferred (v2) |
| CI-01–04 | Phase 13 | `1 - modernization-roadmap` C.1–C.7 | Pending |
| ARCH-01–02 | Phase 14 | `1 - modernization-roadmap-arm64` | Deferred (v2) |

**Coverage:**
- v1 requirements: 36 total
- Complete: 21
- In progress: 2
- Pending: 13
- Unmapped: 0 ✓

---
*Requirements defined: 2026-06-24*
*Last updated: 2026-06-24 after adding PLAT-01 (DBTools NuGet migration)*
