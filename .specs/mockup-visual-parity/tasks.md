# Tasks: Mockup Visual Parity

> Companion to `requirements.md` and `design.md`. Phased, verifiable work. **Waves must run in order**; a wave's verification gate must be green before the next wave starts.

- [ ] 1. **Create the shared `ce-icon` component (Wave 1)**
  - [ ] 1.1 Create `src/app/design-system/components/icon/icon.component.ts` — Lucide-backed `ce-icon` with `name`, `size`, `strokeWidth` inputs
  - [ ] 1.2 Create `icon.spec.ts` — assert name/size passthrough and `aria-hidden` for decorative usage
  - [ ] 1.3 Create `icon.stories.ts` — add to `/design-system/showcase` gallery
  - [ ] 1.4 Export `CeIconComponent` from `src/app/design-system/index.ts`
  - [ ] 1.5 Verify `lucide-angular@0.454.0` resolves (or swap to `@lucide/angular` if tree-shaking fails)
  - [ ] 1.6 Verification gate: `ng build` succeeds; `ce-icon` renders `lucide-icon` in showcase

- [ ] 2. **Implement the mobile drawer service and shell wiring (Wave 2)**
  - [ ] 2.1 Create `src/app/core/services/drawer.service.ts` — signal-based `isOpen`, `open()`, `close()`, `toggle()`
  - [ ] 2.2 Update `app-shell.component.ts` — apply `is-drawer` class and `body.drawer-open` when service is open; add backdrop element
  - [ ] 2.3 Update `topbar.component.ts` — wire `toggleMobileMenu()` to `DrawerService.toggle()`; replace Unicode `&#9776;` with `ce-icon name="menu"`
  - [ ] 2.4 Add `(document:keydown.escape)` host listener to shell to close drawer
  - [ ] 2.5 Verification gate: at <640px, hamburger opens drawer; backdrop click and Escape close it

- [ ] 3. **Topbar parity (Wave 2)**
  - [ ] 3.1 Add a global search input (debounced 250ms) — `ce-icon name="search"` prefix, no-op on routes without table filtering
  - [ ] 3.2 Add notifications bell — `ce-icon name="bell"` wrapped in `ce-tooltip`; for now it can be a static button with a no-op
  - [ ] 3.3 Replace the theme toggle Unicode glyphs with `ce-icon name="sun"` / `ce-icon name="moon"`
  - [ ] 3.4 Replace the custom profile menu with `ce-dropdown` + `ce-avatar` (when `canSwitchTenant`, the dropdown body includes the tenant switcher)
  - [ ] 3.5 Wire `ce-breadcrumbs` to read `data.breadcrumb` from each route in `app.routes.ts`
  - [ ] 3.6 Verification gate: at 1440px, topbar shows breadcrumbs, search, bell, theme, avatar in mockup order; profile menu opens via `ce-dropdown`

- [ ] 4. **Sidebar parity (Wave 2)**
  - [ ] 4.1 Replace Unicode nav icons with `ce-icon` (dashboard, users, building, calendar, car, briefcase, settings, home)
  - [ ] 4.2 Replace the footer avatar div with `ce-avatar` keeping the existing gradient
  - [ ] 4.3 Add `id="primary-sidebar"` to the `<aside>` for drawer targeting
  - [ ] 4.4 Verification gate: sidebar at 1440px matches mockup; at <640px, drawer mode works

- [ ] 5. **Refactor login and change-password pages (Wave 3)**
  - [ ] 5.1 Replace `.ce-button` markup with `<ce-button variant="primary">` in both pages
  - [ ] 5.2 Replace `.ce-input-*` markup with `<ce-input>` using `[input-prefix]` and `[input-suffix]` content slots (user icon prefix, eye toggle suffix for password)
  - [ ] 5.3 Replace `.ce-checkbox-*` markup with `<ce-checkbox>`
  - [ ] 5.4 Replace inline `.ce-spinner` with `<ce-spinner size="sm" tone="primary">` on submit
  - [ ] 5.5 Add floating top-right theme toggle with `ce-icon` and click handler
  - [ ] 5.6 Delete all duplicated CSS from both pages' `styles: []` blocks
  - [ ] 5.7 Verification gate: at 1440px and 375px, login and change-password match `mockup/login.html`; password show/hide works

- [ ] 6. **Rebuild the residents page (Wave 4)**
  - [ ] 6.1 Replace the page-header buttons with `<ce-button>` (Refresh secondary, Add resident primary)
  - [ ] 6.2 Add a 4-tile stat grid using `<ce-stat-tile>` pulling from `DashboardApiService`
  - [ ] 6.3 Add `<ce-tabs>` (All, Active, Pending, Overdue) with counts from the residents list
  - [ ] 6.4 Add a filter toolbar with two `<ce-dropdown>` (Block, Sort) and a search input
  - [ ] 6.5 Replace the table body with `<ce-table>` + `<ce-badge>`; add row hover-reveal `<ce-dropdown>` action menu with Edit and Deactivate
  - [ ] 6.6 Add `<ce-pagination>` at the bottom
  - [ ] 6.7 Replace the inline Add/Edit modal markup with `<ce-modal>`
  - [ ] 6.8 Add a `<ce-card>` "Recent activity" section fed by `DashboardApiService.recentVisits`
  - [ ] 6.9 Wire the form to the residents API; client-side filter/sort/paginate as in `mockup/assets/app.js`
  - [ ] 6.10 Delete all duplicated `.ce-*` CSS from the page
  - [ ] 6.11 Verification gate: at 1440px and 375px, the page matches `mockup/app.html`; row action menu, modals, and pagination all work

- [ ] 7. **Refactor dashboard and audit showcase (Wave 5)**
  - [ ] 7.1 Replace duplicated `.ce-button` and `.ce-card` CSS in `dashboard.page.ts` with `<ce-card>` and `<ce-button>`; remove duplicated CSS
  - [ ] 7.2 Align the stat tile grid spacing with `mockup/index.html` preview
  - [ ] 7.3 Audit `showcase.page.ts` against `mockup/showcase.html`; fix any variant/size drift
  - [ ] 7.4 Replace the inline modal demo in showcase with `<ce-modal>` if still duplicated
  - [ ] 7.5 Verification gate: dashboard renders identically to the mockup preview; showcase matches `mockup/showcase.html` in both themes

- [ ] 8. **Cross-page component adoption (Wave 6)**
  - [ ] 8.1 `visits.page.ts` — replace duplicated `ce-button`/`ce-table`/`ce-input`/`ce-badge` markup with components; delete duplicated CSS
  - [ ] 8.2 `vehicles.page.ts` — same
  - [ ] 8.3 `apartments.page.ts` — same (also replace `.variant-primary` and `.variant-danger` rules)
  - [ ] 8.4 `service-providers.page.ts` — same
  - [ ] 8.5 `administration.page.ts` — same (including `.tab.active`)
  - [ ] 8.6 `condominiums.page.ts` — same
  - [ ] 8.7 Verification gate: `grep -rn '\.ce-button\|\.ce-input\|\.ce-table\|\.ce-card\|\.ce-badge\|\.ce-modal' src/Web/ControlEasyReborn.Web/src/app/features/` returns zero matches

- [ ] 9. **Visual regression coverage (Wave 7)**
  - [ ] 9.1 Create `tests/visual/login.spec.ts` — snapshots at 375×800 and 1440×900 in light and dark
  - [ ] 9.2 Create `tests/visual/residents.spec.ts` — authenticated fixture, same viewports/themes
  - [ ] 9.3 Create `tests/visual/dashboard.spec.ts` — 1440×900 light and dark, stat grid and recent activity card
  - [ ] 9.4 Update `tests/visual/showcase.spec.ts` to include dark variants if missing
  - [ ] 9.5 Run `mockup/SMOKE.md` checklist against Angular routes and document pass/fail in this file
  - [ ] 9.6 Verification gate: `npx playwright test` passes

- [ ] 10. **Complete prior verification and docs (Wave 7)**
  - [ ] 10.1 Tick `.specs/fix-design-system/tasks.md` tasks 12.5–12.8
  - [ ] 10.2 Update `agents/agents/FrontendAgent/AGENTS.md` — remove the stale "greenfield / no Angular app" line; document `ce-icon` and the mockup parity workflow
  - [ ] 10.3 Run final `ng build` and verify zero errors
  - [ ] 10.4 Verification gate: full Docker rebuild per [AGENTS.md](../../AGENTS.md):
    ```bash
    docker compose -f docker/docker-compose.yml build api web
    docker compose -f docker/docker-compose.yml up -d --force-recreate api web
    ```
    And confirm the rebuilt stack is healthy.

## Task Dependency Graph

```json
{
  "waves": [
    { "wave": 1, "tasks": ["1"] },
    { "wave": 2, "tasks": ["2", "3", "4"] },
    { "wave": 3, "tasks": ["5"] },
    { "wave": 4, "tasks": ["6"] },
    { "wave": 5, "tasks": ["7"] },
    { "wave": 6, "tasks": ["8"] },
    { "wave": 7, "tasks": ["9", "10"] }
  ]
}
```

**Notes on parallelism:**

- Tasks 2, 3, 4 share the topbar and shell but touch different files, so they can run in parallel within Wave 2.
- Task 6 (residents) is the largest single file edit; keep it as its own wave to make review manageable.
- Tasks 9 and 10 are independent and parallelizable in Wave 7.

## Out of Scope

- New Lucide icons beyond the catalog in this spec.
- pt-BR translation.
- Backend additions for status tags — pending/overdue tabs render with count `0` until demo mode adds them.
- Migrating `ce-modal`, `ce-dropdown`, or `ce-tooltip` to CDK Overlay (acceptable in `@if` form per `.specs/fix-design-system/design.md`).
- Re-skinning the design system itself — the design contract is locked.
