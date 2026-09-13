# Phase 13 UI Design Contract — Consent Policy & Gatehouse Workflow

**Phase:** 13 — Consent Policy & Gatehouse Workflow
**Status:** UI-SPEC Approved (smart discuss)
**Date:** 2026-09-13

---

## Visual Direction

This phase adds the 3-second gatehouse workflow, the audit review page, and the consent policy editor. The visual design follows the existing `mockup/` reference and uses the established `ce-*` design system. The gatehouse workflow is the moment of highest cognitive load and zero tolerance for friction; the audit review page prioritizes information density and quick filtering.

**Mood:** Functional, fast, neutral. Gatehouse modal is bold and minimal. Audit page is dense and filterable. Consent policy editor is simple and direct.

---

## Surfaces

### 1. `ce-entry-workflow` (Gatehouse Modal)

**Layout:**
- Full-screen overlay on mobile (< 640px), centered modal 720px on tablet/desktop
- Header: title "New entry" + close X button
- Body: 2×2 grid of large action tiles
  - Top-left (primary, blue): "Register entry" + sub-text "with consent"
  - Top-right (red): "Entry denied" + sub-text "consent refused"
  - Bottom-left (gray): "Gatehouse only" + sub-text "package drop"
  - Bottom-right (orange): "Override" + sub-text "emergency / vouched"
- Each tile: 200px × 200px on desktop, full half-width on mobile; 24px icon + label + sub-text

**States:**
- Idle (4 tiles visible)
- Tile pressed (subtle scale 0.95 + bg darken)
- Loading (after tile tap, full-screen spinner overlay)
- Auto-camera (when policy requires photo — reuses `ce-photo-capture` from Phase 12)
- Override-reason (modal with 2 buttons: Emergency | Vouched)
- Success (toast "Entry logged", modal closes, recent-activity refreshes)
- Error (toast; modal stays open for retry)

**Copy:**
- Modal title: "New entry"
- Tile 1: "Register entry" / "with consent"
- Tile 2: "Entry denied" / "consent refused"
- Tile 3: "Gatehouse only" / "package drop"
- Tile 4: "Override" / "emergency / vouched"
- Override modal: "Why?" / "Emergency" / "Vouched"
- Subject name input: "Name (optional)"
- Subject document input: "Document (optional)"

### 2. `ce-audit-review` (Audit Page)

**Layout:**
- Full-width page with sticky filter bar at top
- Filter bar (horizontal scroll on mobile):
  - Date range picker (default: last 7 days)
  - Category dropdown (All / Dwellers / Visitors / Service-Providers / Vehicles)
  - Entry state dropdown (All / Entered with consent / Override / Denied / Gatehouse only)
  - Porteiro dropdown (All / individual porteiro)
  - Reset filters button
- Page header (right side): "Export CSV" button (icon + text)
- Table:
  - Columns: `recorded_at`, `entry_state`, `subject_type`, `subject_name`, `photo`, `override_reason`, `porteiro`
  - 50 rows per page with pagination footer
  - Override rows: red left border 4px
  - Photo cells: 64×64 thumbnail (clickable → lightbox)
  - State cells: badge component with state-specific color

**States:**
- Loading (skeleton table)
- Loaded (rows visible)
- Empty (when no entries match filter): "No entries found for current filters"
- Error: "Failed to load audit log. Retry?"
- Filtered (chip counts update as filters change)
- CSV download in progress (button shows spinner)

**Copy:**
- Page title: "Audit Log"
- Filter chips: "Date range", "Category", "Entry state", "Porteiro"
- Reset: "Reset filters"
- Export: "Export CSV"
- Empty: "No entries match the current filters."

### 3. `ce-consent-policy-editor` (Settings Page)

**Layout:**
- Centered card on `/admin/consent-policy`, max-width 720px
- Header: "Consent Policy" + description "Configure photo requirement per category"
- Body: 4 toggle rows:
  - Dwellers (default: off)
  - Visitors (default: on)
  - Service-Providers (default: off)
  - Vehicles (default: off)
- Each row: category name + description + toggle switch (right-aligned)
- Footer: "Last updated: {timestamp} by {user name}"
- Action: "Save" button (sticky bottom or in header)
- Save button states: idle / saving / saved / error

**States:**
- Loading (4 skeleton rows)
- Loaded (toggles reflect current policy)
- Saving (button shows spinner)
- Saved (toast "Policy updated"; last-updated refreshes)
- Error (toast "Failed to save policy")

**Copy:**
- Page title: "Consent Policy"
- Description: "When photo is required, the gatehouse camera opens automatically for that category."
- Row labels: "Dwellers", "Visitors", "Service-Providers", "Vehicles"
- Row descriptions: "Residents living in the condominium", "Visitors and guests", "Service providers and contractors", "Vehicles entering the gate"
- Toggle on: "Photo required"
- Toggle off: "Photo not required"
- Save: "Save policy"
- Last updated: "Last updated {time} by {user}"

### 4. `ce-entry-state-badge` (Reusable Badge)

**Layout:**
- Inline-flex pill: 6px vertical padding, 12px horizontal padding, 14px font
- 4px border-radius
- Icon + state label

**States (one per entry state):**
- `entered_with_consent` — green background (success token), check icon, "With consent"
- `entered_override` — orange background (warning token), alert icon, "Override"
- `gatehouse_only` — gray background (muted token), package icon, "No entry"
- `denied` — red background (error token), x-circle icon, "Refused"
- `entered_without_consent` (defensive state) — yellow background, warning icon, "No consent"

---

## Typography

Uses existing design-system tokens:

- Modal title: `font-size: 18px`, `font-weight: 600`
- Tile label: `font-size: 18px`, `font-weight: 600`
- Tile sub-text: `font-size: 13px`, `font-weight: 400`, `color: var(--ce-text-muted)`
- Page title: `font-size: 24px`, `font-weight: 700`
- Table cell: `font-size: 14px`, `font-weight: 400`
- Badge text: `font-size: 13px`, `font-weight: 500`
- Description text: `font-size: 14px`, `font-weight: 400`, `color: var(--ce-text-muted)`
- Last-updated footer: `font-size: 12px`, `color: var(--ce-text-muted)`

No new fonts introduced. All text uses existing `font-sans` family.

---

## Color

Uses existing design-system tokens:

- Tile 1 (with consent): `background: var(--ce-primary)`, `color: white`
- Tile 2 (denied): `background: var(--ce-error)`, `color: white`
- Tile 3 (gatehouse only): `background: var(--ce-surface-alt)`, `color: var(--ce-text)`
- Tile 4 (override): `background: var(--ce-warning)`, `color: white`
- Override row border: `var(--ce-error)` 4px left
- Badge backgrounds: state-specific tokens (above)
- Sticky filter bar: `background: var(--ce-surface)`, `border-bottom: 1px solid var(--ce-border)`

No new colors introduced. Dark theme auto-inherits.

---

## Spacing

- Modal padding: `var(--ce-space-4)` (16px)
- Tile gap: `var(--ce-space-3)` (12px)
- Filter chip gap: `var(--ce-space-2)` (8px)
- Page max-width: 1200px with `var(--ce-space-4)` gutter
- Audit row padding: `var(--ce-space-3)` vertical, `var(--ce-space-2)` horizontal
- Consent policy toggle row gap: `var(--ce-space-4)` (16px)

All spacing uses existing `ce-space-*` tokens.

---

## Motion

- Tile tap: scale 0.95 + bg darken over 100ms (ease-out)
- Modal enter: fade + scale from 0.95 → 1 over 200ms (ease-out)
- Override modal: slide up from bottom over 200ms (ease-out)
- Filter chip selection: bg color fade over 150ms
- Table row hover: bg lighten over 100ms
- Pagination: instant (no animation)
- Toast: slide up from bottom + fade over 200ms

Reduced-motion: respects `prefers-reduced-motion`, removes scale and slide.

---

## Design System Components Used

- `ce-modal` — host for entry workflow + override reason modal
- `ce-button`, `ce-icon`, `ce-spinner`, `ce-toast` — standard primitives
- `ce-badge` — extended via `ce-entry-state-badge` for state-specific colors
- `ce-photo` — Phase 12 thumbnail component (audit row photo cell)
- `ce-photo-lightbox` — Phase 12 lightbox (audit photo click)
- `ce-photo-capture` — Phase 12 capture component (auto-camera in entry workflow)
- `ce-table` — existing table primitive (audit page)
- `ce-pagination` — existing pagination primitive (audit page)
- `ce-input` — existing input primitive (subject name/document)
- `ce-checkbox` or new `ce-toggle` — toggle switch for consent policy editor

---

## Component Architecture

| Component | Selector | Inputs | Outputs |
|-----------|----------|--------|---------|
| EntryWorkflowComponent | `ce-entry-workflow` | `{ open: boolean }` | `(entryLogged)`, `(closed)` |
| OverrideReasonComponent | `ce-override-reason` | `{ open: boolean }` | `(reasonSelected: 'emergency' \| 'vouched')`, `(closed)` |
| AuditReviewComponent | `ce-audit-review` | (none — page-level) | (none) |
| AuditFiltersComponent | `ce-audit-filters` | `{ filters: AuditFilters }` | `(filtersChanged)` |
| AuditRowComponent | `ce-audit-row` | `{ entry: EntryLogResponse }` | `(photoClicked)` |
| ConsentPolicyEditorComponent | `ce-consent-policy-editor` | (none — page-level) | (none) |
| EntryStateBadgeComponent | `ce-entry-state-badge` | `{ state: EntryState }` | (none) |

All components: standalone, OnPush, signals for state, `ce-` selector prefix.

---

## Integration Points

1. **New routes:**
   - `/gatehouse` — entry workflow page (porteiro role)
   - `/audit` — audit review page (syndic/tenant admin)
   - `/admin/consent-policy` — settings page (tenant admin)

2. **Dashboard update:** Floating action button (FAB) at bottom-right with "+" icon, opens entry workflow modal.

3. **Topbar nav update:** Add "Gatehouse" link for porteiros, "Audit" link for syndic, "Consent Policy" link for tenant admins. Use existing role-gated rendering pattern.

4. **EntryLogService** — handwritten service in `features/entry-log/entry-log.service.ts`:
   - `create(request: CreateEntryLogRequest): Promise<EntryLogResponse>`
   - `list(filters: AuditFilters): Promise<EntryLogResponse[]>`
   - `export(filters: AuditFilters): Promise<Blob>` — triggers download

5. **ConsentPolicyService** — handwritten service in `features/consent-policy/consent-policy.service.ts`:
   - `getAll(): Promise<ConsentPolicyResponse[]>` — calls GET /api/v1/consent-policy/{category} 4× in parallel
   - `update(policies: ConsentPolicyResponse[]): Promise<void>` — calls PUT /api/v1/consent-policy 4× OR (future) PUT /api/v1/consent-policy/bulk

6. **API endpoint deviations:**
   - Backend GET /api/v1/consent-policy takes `{subjectCategory}` (single). Frontend will call 4× in parallel.
   - Future endpoint `GET /api/v1/consent-policy` (list) and `PUT /api/v1/consent-policy/bulk` would simplify; tracked as open.

---

## Copywriting Contract

| Context | Text | Notes |
|---------|------|-------|
| Gatehouse modal title | "New entry" | |
| Tile 1 | "Register entry" / "with consent" | Primary, blue |
| Tile 2 | "Entry denied" / "consent refused" | Red |
| Tile 3 | "Gatehouse only" / "package drop" | Gray |
| Tile 4 | "Override" / "emergency / vouched" | Orange |
| Override modal title | "Why?" | |
| Override reason buttons | "Emergency" / "Vouched" | |
| Subject name input | "Name (optional)" | |
| Subject document input | "Document (optional)" | |
| Audit page title | "Audit Log" | |
| Filter labels | "Date range" / "Category" / "Entry state" / "Porteiro" | |
| Reset filters | "Reset filters" | |
| Export CSV | "Export CSV" | |
| Audit empty | "No entries match the current filters." | |
| Audit error | "Failed to load audit log. Retry?" | |
| Consent policy title | "Consent Policy" | |
| Consent policy description | "When photo is required, the gatehouse camera opens automatically for that category." | |
| Consent policy rows | "Dwellers" / "Visitors" / "Service-Providers" / "Vehicles" | |
| Consent policy toggle on | "Photo required" | |
| Consent policy toggle off | "Photo not required" | |
| Consent policy save | "Save policy" | |
| Consent policy last updated | "Last updated {time} by {user}" | |
| Badge: entered_with_consent | "With consent" | Green |
| Badge: entered_override | "Override" | Orange |
| Badge: gatehouse_only | "No entry" | Gray |
| Badge: denied | "Refused" | Red |
| Badge: entered_without_consent | "No consent" | Yellow |

---

## Accessibility

- All buttons have `aria-label` (icon-only) or visible text
- Modal traps focus; returns focus to triggering button on close
- Filter chips announce state changes via `aria-live="polite"`
- Table rows have descriptive `aria-label` (combines state, subject, time)
- Keyboard support:
  - Tab navigates through tiles and filters
  - Enter activates focused tile
  - Escape closes modal
  - Arrow keys navigate table rows
- Reduced-motion preference respected
- Color contrast: all text meets WCAG AA
- Toggle switches have explicit `role="switch"` + `aria-checked`

---

## Responsive

- **Gatehouse modal:** Full-screen on mobile (< 640px), 720px centered on tablet/desktop
- **Audit page:** Filter chips stack vertically on mobile; table horizontally scrolls
- **Consent policy editor:** Card full-width on mobile, 720px max on tablet/desktop

---

## Performance

- Audit list paginated (50 per page) — never load all entries
- Filter changes debounced 300ms
- CSV export streams response to file (no buffer in memory)
- Toggle switches optimistic-update; revert on save failure
- Recent-activity refresh on dashboard polls every 30s (or uses HTTP cache)

---

## Open Questions for Implementer (agent's discretion)

- Exact tile sizes, colors, icons
- Modal dimensions (mobile vs tablet vs desktop)
- Pagination style (numbered vs infinite scroll)
- Filter chip layout (horizontal vs vertical)
- Empty state for audit page
- Default sort order in audit table (newest first vs oldest first)
- Whether to use existing `ce-checkbox` or build a `ce-toggle` switch

---

## Verification (per ROADMAP success criteria)

1. ✅ Tenant admin sets "visitors: photo required = yes" → porteiro registers a visitor → camera auto-opens → photo captured → entry logged → workflow under 3s (Playwright E2E)
2. ✅ Visitor refuses consent → "Entry denied" tile → logged as `denied` → no photo (Playwright E2E)
3. ✅ Service provider drops package → "Gatehouse only" tile → logged as `gatehouse_only` (Playwright E2E)
4. ✅ Porteiro overrides (dweller, emergency) → reason selected → logged as `entered_override` without photo (Playwright E2E)
5. ✅ `entered_with_consent` without photo → rejected by API (integration test in Phase 11 + Playwright assertion)
6. ✅ Syndic opens audit → filters by "override" → sees overrides with reason, porteiro, timestamp (Playwright E2E)
7. ✅ Audit log append-only: PUT/DELETE returns 405 (integration test in Phase 11)
8. ✅ CSV export: filtered log → `recorded_at` column has millisecond timestamps (Playwright E2E)
9. ✅ Playwright E2E: full gatehouse workflow
10. ✅ `dotnet test` + `npm test` green; Docker stack healthy

---

## UI Considerations

**UI element states to cover:**

- **Entry workflow modal:** idle, tile-pressed, loading, auto-camera, override-reason, success, error (7 states)
- **Override reason modal:** idle, button-pressed, success (3 states)
- **Audit page:** loading, loaded, empty, error, filtered, csv-downloading (6 states)
- **Audit row:** 5 state-specific renderings (entered_with_consent, entered_override, gatehouse_only, denied, entered_without_consent)
- **Consent policy editor:** loading, loaded, idle, saving, saved, error (6 states)
- **Toggle switch:** on, off, disabled, loading (4 states)
- **Photo cell (audit row):** loading, loaded, error, no-photo-placeholder (4 states)
- **Filter chip:** default, selected, hovered, focus (4 states)

All states have explicit copy in the copywriting contract. Implementation must render all states (not just happy path).
