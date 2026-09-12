# Roadmap: ControlEasy Reborn — v1.1 UI & Dashboard

**Milestone:** v1.1 — UI & Dashboard
**Started:** 2026-08-23
**Phase numbering:** Continues from v1.0 (phases 9 and 10 retain their original numbers, reserved in the v1.0 archive roadmap)
**Granularity:** standard (2 phases — two natural delivery boundaries from 5 requirements)

## Phases

- [~] **Phase 9: UI Parity & Functional Fixes** - Align the Angular SPA with the `mockup/` + Penpot visual reference and make every interactive component in `mockup/` behave correctly — **partially shipped** (residents page rebuilt with `ce-*` components, commit `0976037`; other pages pending)
- [x] **Phase 10: Dashboard Live Stats & Vehicle Edit** - Replace placeholder dashboard data with live tenant-scoped statistics and add the vehicle editing workflow — **shipped** (dashboard endpoint + UI + vehicle edit, no GSD artifacts)

## Phase Details

### Phase 9: UI Parity & Functional Fixes
**Goal**: Align the Angular SPA with the `mockup/` + Penpot visual reference and make every interactive component in `mockup/` behave correctly at 375px, 768px, and 1440px in light and dark themes
**Depends on**: Phase 8 (Design System Hardening — shipped in v1.0)
**Requirements**: UI-03 (UI-03a–h), UI-04 (UI-04a–j)
**Specs**: `.specs/mockup-visual-parity/`, `.specs/2-mockup-functional-fixes/`
**Status**: Partially shipped — residents page rebuilt with `ce-*` components (stat tiles, tabs, dropdowns, table, badge, pagination, modals, card, inputs, client-side filter/sort/pagination, status tabs, row action menus, recent-activity card). Login, dashboard, showcase, visits, vehicles, apartments, service-providers, administration, condominiums pages not yet rebuilt with `ce-*` components. Playwright visual snapshots not confirmed.
**Success Criteria** (what must be TRUE):
  1. The app shell, login, residents, dashboard, and showcase pages match the `mockup/` reference at 375px, 768px, and 1440px in both light and dark themes
  2. The mobile sidebar slides in as a drawer with a dark backdrop below 640px and opens via the topbar hamburger
  3. Every modal and dropdown in `mockup/` opens, closes (backdrop/Escape/outside click), and focuses the first field correctly
  4. The mockup login form demonstrates spinner, demo-credential success, validation, inline error, rate-limit lockout, and tenant-picker flows
  5. Playwright visual snapshots for `/login`, `/residents`, `/` (dashboard) at light/dark × 375/1440 pass on every change
**Plans**: TBD (shipped without GSD artifacts)
**UI hint**: yes

### Phase 10: Dashboard Live Stats & Vehicle Edit
**Goal**: Replace placeholder dashboard data with live tenant-scoped statistics and add the vehicle editing workflow
**Depends on**: Phase 8 (Design System Hardening — shipped in v1.0); DASH-02 UI benefits from Phase 9's `ce-*` component adoption but the backend endpoint (DASH-01) does not hard-depend on Phase 9 — phases may run in parallel
**Requirements**: DASH-01, DASH-02, DASH-04
**Specs**: `.specs/dashboard/`, `.specs/vehicle-edit/`
**Status**: ✅ Shipped — `GET /api/v1/dashboard/stats` endpoint in Reports module; dashboard page with live stat tiles (Active Residents, Active Vehicles, Occupied Apartments, Open Visits), recent-visits table, loading/error states; vehicle edit modal + `update()` in VehiclesApiService.
**Success Criteria** (what must be TRUE):
  1. ✅ An authenticated tenant-scoped `GET /api/v1/dashboard/stats` returns correct aggregate counts and recent visits
  2. ✅ The dashboard page shows live stat tiles (Active Residents, Active Vehicles, Occupied Apartments, Open Visits) that link to their feature pages, plus a recent-visits table, with loading and error states
  3. ✅ A user with Vehicles.Write permission can open an edit modal from a vehicle table row, change fields, and save successfully
  4. ⚠️ Unverified — Vehicles.Write permission check on edit button (needs confirmation)
  5. ⚠️ Unverified — Vehicle deactivation with confirmation prompt (needs confirmation)
**Plans**: TBD (shipped without GSD artifacts)
**UI hint**: yes

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 9. UI Parity & Functional Fixes | — | Partially shipped (residents page only) | — |
| 10. Dashboard Live Stats & Vehicle Edit | — | Shipped (no GSD artifacts) | 2026-09-12 |

---
*Milestone v1.1 roadmap defined 2026-08-23. Audited and updated 2026-09-12 — Phase 9 partially shipped, Phase 10 shipped. Phase numbering continues from v1.0 (phases 9 and 10 reserved in the v1.0 archive).*