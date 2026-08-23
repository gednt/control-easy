# Requirements: ControlEasy Reborn

**Defined:** 2026-08-23
**Core Value:** Gatehouse staff can reliably register and control access for residents, visitors, and vehicles through a fast, tenant-isolated web UI backed by a secure multi-tenant API.

## v1.1 Requirements

Requirements for the v1.1 "UI & Dashboard" milestone. These continue the REQ-IDs reserved as deferred-to-v1.1 in the v1.0 archive (`UI-03`, `UI-04`, `DASH-01`, `DASH-02`, `DASH-04`) and decompose them into atomic, testable sub-requirements traced to the existing spec folders. Each maps to a roadmap phase.

### UI — Visual Parity & Functional Fixes

- [ ] **UI-03**: Mockup visual parity — app shell, login, residents, showcase, dashboard, and feature pages match `mockup/` + `docs/penpot/` at 375/768/1440px in light/dark themes
  - [ ] **UI-03a**: Shared `ce-icon` component backed by Lucide replaces Unicode glyphs across sidebar, topbar, login, and residents pages
  - [ ] **UI-03b**: Mobile drawer slides in with dark backdrop below 640px; topbar hamburger toggles it
  - [ ] **UI-03c**: Topbar parity — breadcrumbs, debounced global search, notifications bell, theme toggle, avatar-driven profile menu
  - [ ] **UI-03d**: Login and change-password pages use `ce-input`, `ce-checkbox`, `ce-button` (no duplicated component CSS)
  - [ ] **UI-03e**: Residents page parity — stat tiles, status tabs, block/sort filters, sortable table, pagination, row action menu, add/edit/deactivate modals, recent-activity card
  - [ ] **UI-03f**: All feature pages (visits, vehicles, apartments, service-providers, administration, condominiums) import `ce-*` components; duplicated CSS removed
  - [ ] **UI-03g**: Dashboard stat grid and showcase page align with mockup (no variant/size drift)
  - [ ] **UI-03h**: Playwright visual regression snapshots for `/login`, `/residents`, `/` (dashboard) at light/dark × 375/1440
- [ ] **UI-04**: Mockup functional fixes — every interactive component in `mockup/` behaves correctly
  - [ ] **UI-04a**: Modals and dropdowns hidden by default; open/close with backdrop, Escape, outside click; first-field focus on open
  - [ ] **UI-04b**: Topbar search and residents search filter table client-side (case-insensitive substring); pagination footer updates to "Showing X–Y of Z"
  - [ ] **UI-04c**: "All / Active / Pending / Overdue" status tabs filter the table and update count badges
  - [ ] **UI-04d**: "Sort" dropdown re-sorts the table (Apartment, Last visit, etc.)
  - [ ] **UI-04e**: Pagination navigation — page 2/3 shows rows 11–20 / 21–30, active highlight moves, Prev/Next enable/disable
  - [ ] **UI-04f**: Login form — submit spinner with `aria-busy`, demo creds `test@test.com`/`test123` land on `app.html`, <6-char password validation, `fail@x.com` inline 401 error, `ratelimit@x.com` "Too many attempts" with 15s lockout, "multi" emails show tenant picker
  - [ ] **UI-04g**: Password eye icon toggles input type and swaps icon
  - [ ] **UI-04h**: "Use a different account" returns to empty login form (email cleared if "Remember me" unchecked)
  - [ ] **UI-04i**: "Forgotten password" link shows toast; success/warning/error toast buttons auto-dismiss after 5s
  - [ ] **UI-04j**: `index.html` dashboard preview regains `mb-8` margin; `app.html` "Início" breadcrumb links to `index.html`

### DASH — Dashboard & Vehicle Edit

- [ ] **DASH-01**: `GET /api/v1/dashboard/stats` endpoint returns aggregate counts (total/active residents, total/active vehicles, total/occupied apartments, open/today visits) and a recent-visits list; tenant-scoped; authorized
- [ ] **DASH-02**: Dashboard page UI — stat tiles (Active Residents, Active Vehicles, Occupied Apartments, Open Visits) linking to feature pages, recent-visits table, loading and error states, using `ce-*` design-system components
- [ ] **DASH-04**: Vehicle edit UI — `update()` and `get()` methods in `VehiclesApiService`, edit modal mirroring residents pattern, edit/deactivate action buttons in table rows, vehicle-type dropdown on create and edit, `Vehicles.Write` permission checks

## v2 Requirements

Deferred to a future milestone. Tracked in `.specs/` but not in the v1.1 execution path.

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
| Mobile-native apps | Responsive web / PWA sufficient for v1 |
| Billing / subscriptions | External system |
| Cross-tenant analytics BI | Separate product |
| Photo capture & hardware (v2) | Deferred to Phase 12 — separate milestone |
| Multi-arch Docker/CI (v2) | Deferred to Phase 14 — separate milestone |

## Traceability

Populated during roadmap creation. Each v1.1 requirement maps to exactly one phase.

| Requirement | Phase | Spec | Status |
|-------------|-------|------|--------|
| UI-03 (and UI-03a–h) | TBD | `mockup-visual-parity` | Pending |
| UI-04 (and UI-04a–j) | TBD | `2-mockup-functional-fixes` | Pending |
| DASH-01 | TBD | `dashboard` | Pending |
| DASH-02 | TBD | `dashboard` | Pending |
| DASH-04 | TBD | `vehicle-edit` | Pending |

**Coverage:**
- v1.1 requirements: 5 top-level (22 atomic sub-requirements)
- Mapped to phases: 0
- Unmapped: 5 ⚠️ (resolved by roadmap)

---
*Requirements defined: 2026-08-23*
*Last updated: 2026-08-23 after v1.1 milestone definition*