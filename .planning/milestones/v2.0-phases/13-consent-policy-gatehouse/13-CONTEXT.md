# Phase 13: Consent Policy & Gatehouse Workflow - Context

**Gathered:** 2026-09-13
**Status:** Ready for planning
**Mode:** Smart discuss (autonomous)

<domain>
## Phase Boundary

The porteiro can record every entry into the condominium using a fast 3-second workflow. The syndic/tenant admin can configure per-tenant per-category consent policy and review every entry in an append-only audit log with filter chips and CSV export.

Phase 11 (Photos & Consent Schema) and Phase 13 backend (entry-log endpoints, consent policy endpoints, append-only triggers) are already shipped. Phase 13 delivers only the client-side UI: gatehouse entry workflow (modal with 4 tiles), consent policy editor (settings page with 4 toggles), and audit review (filterable table with badges + thumbnails + CSV export).

Phase 12 (photo capture UI) is already shipped and provides `ce-photo-capture`, `ce-photo-gallery`, and `ce-photo` components reused here.

</domain>

<decisions>
## Implementation Decisions

### Gatehouse Entry Workflow — Modal Entry

- A modal that opens over the dashboard; porteiro can see recent activity while registering
- Single screen, no navigation; designed for one-handed gatehouse tablet use
- Modal can be triggered by a "New entry" floating action button (FAB) on the dashboard and from the topbar nav

### Workflow Layout — 4-tile single screen

- 4 large touch targets in a 2×2 grid:
  - "Register entry (with consent)" — primary tile, blue background
  - "Entry denied (consent refused)" — red tile
  - "Gatehouse only (package drop)" — gray tile
  - "Override (emergency / vouched)" — orange tile
- Each tile shows an icon + label + sub-text describing the action
- Selecting a tile advances the workflow within the same modal (no separate screens)

### Auto-Camera Trigger

- When the policy for the selected category has `photo_required: true`, the camera modal opens automatically with live preview
- Capture button is the only next step; no manual "Take photo" tap needed
- If policy is `photo_required: false` (default for non-visitor categories), the camera is skipped and the entry is logged without photo

### Override Flow

- Tap "Override" → reason modal with two large buttons: "Emergency" | "Vouched"
- Sub-second decision; tap → entry logged as `entered_override` with the reason code
- No free-text input; reason codes are hardcoded per ROADMAP

### Audit Review UI — Table with filter chips

- Single table on `/audit` page (or `/audit-log`)
- Filter chips at top: date range picker, category, entry state, porteiro
- 50 entries per page with pagination
- CSV export button in the page header (exports current filtered view)
- Override rows highlighted with red/orange left border

### Entry Row Display — Visual badges + thumbnails

- `entered_with_consent` — photo thumbnail (128×128) + name + category badge
- `entered_override` — orange "Override" badge + reason code + porteiro name + no photo (placeholder)
- `denied` — red "Refused" badge + visitor name (if provided)
- `gatehouse_only` — gray "No entry" badge + service-provider name
- All entries show `recorded_at` in millisecond precision

### Consent Policy Editor — 4-toggle settings page

- Tenant admin page at `/admin/consent-policy`
- 4 toggle switches: Dwellers / Visitors / Service-Providers / Vehicles — each on/off for `photo_required`
- Save button (single PUT request with all 4 categories)
- Show last-updated timestamp + who updated it

### CSV Export — Current filter

- Button in audit page header
- Downloads current filtered view (respects all filter chips)
- Filename: `entry-log-{tenant-name}-{ISO date}.csv`
- CSV columns: `id, recorded_at_ms, entry_state, subject_type, subject_name, subject_document, photo_id, override_reason, performed_by_profile_id, tenant_id`
- All timestamps in ISO-8601 with millisecond precision

### Component Architecture

- `ce-entry-workflow` — modal component with 4 tiles + step transitions
- `ce-entry-state-badge` — colored badge for each entry state
- `ce-audit-review` — page-level component with filters + table + pagination + CSV
- `ce-audit-filters` — filter chip components (date-range, category, state, porteiro)
- `ce-audit-row` — table row with state-aware rendering
- `ce-consent-policy-editor` — settings page with 4 toggles
- All components: standalone, OnPush, signals for state

### API Integration (using Phase 11/13 backend)

- `EntryLogService` — POST/GET entry-log + GET entry-log/export (CSV)
- `ConsentPolicyService` — GET/PUT consent-policy
- Note: backend only has GET `/consent-policy/{subjectCategory}` (single category). Phase 13 will need to call this 4× on page load OR add a `/consent-policy` GET endpoint. Implementation choice: call 4× in parallel (forward-compatible with a future list endpoint).

### the agent's Discretion

- Exact tile sizes, colors, icons
- Modal dimensions (mobile vs tablet vs desktop)
- Pagination style (numbered vs infinite scroll)
- Filter chip layout (horizontal vs vertical)
- Empty state for audit page
- Default sort order in audit table (newest first vs oldest first)

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets

- `ce-modal` — host for entry workflow modal
- `ce-photo-capture` — Phase 12 component, reused for auto-camera in entry workflow
- `ce-photo` — Phase 12 thumbnail component, reused in audit row display
- `ce-photo-gallery` — Phase 12 gallery, may be used in audit row for multi-photo entries
- `ce-badge` — existing badge component, may be extended for state-specific colors
- `ce-button`, `ce-icon`, `ce-spinner`, `ce-toast` — standard primitives
- `ce-table` — used by residents page, will host audit table
- `ce-pagination` — existing pagination component

### Established Patterns

- Standalone components, OnPush, signals (per AGENTS.md)
- Handwritten services (e.g., `ResidentsApiService`) wrapping HttpClient
- OpenAPI client at `src/app/api/` (per `npm run openapi-check`) — handwritten services are the v2 pattern
- Reactive forms for the consent policy editor (4 toggles + save button)
- `firstValueFrom` for converting observables to promises
- Toast for transient feedback; modal for blocking workflow

### Integration Points

- New route: `/gatehouse` or `/entry` — entry workflow page
- New route: `/audit` — audit review page (syndic/tenant admin only)
- New route: `/admin/consent-policy` — settings page (tenant admin only)
- Floating action button on `/dashboard` page for quick "New entry"
- Topbar nav update: add "Gatehouse" link for porteiro, "Audit" link for syndic, "Consent Policy" link for tenant admin
- `src/Web/.../src/app/features/entry-log/entry-log.service.ts` (new)
- `src/Web/.../src/app/features/consent-policy/consent-policy.service.ts` (new)
- `src/Web/.../src/app/features/audit/audit.page.ts` (new)
- `src/Web/.../src/app/features/entry-workflow/entry-workflow.page.ts` (new)

</code_context>

<specifics>
## Specific Ideas

- 3-second gatehouse workflow is the honesty enforcement (per party-mode design 2026-08-23) — every tap should be sub-second
- CCTV cross-reference is the external backstop — `recorded_at` timestamp must be precise to milliseconds
- Override reasons are hardcoded (emergency/vouched) — no rules engine, no free-text
- Append-only audit log is enforced via DB triggers — UI must not offer edit/delete on entries
- Photos display in audit only for `entered_with_consent` rows; overrides and denials show no photo
- Per-tenant per-category consent policy — toggling visitors to photo_required: true immediately affects the gatehouse workflow for all porteiros in that tenant
- 4 entry states: `entered_with_consent`, `entered_override`, `gatehouse_only`, `denied` (code also includes `entered_without_consent` defensive state per Phase 11 summary)

No additional specific requirements beyond ROADMAP. Decisions capture the design intent.

</specifics>

<deferred>
## Deferred Ideas

- Bulk entry import (CSV upload of historical entries) — out of scope
- Real-time audit dashboard (WebSocket updates) — out of scope
- SMS/email alerts on overrides — out of scope (CCTV + audit review is the workflow)
- Photo OCR for visitor document capture — out of scope
- Anomaly detection / pattern analysis on overrides — out of scope (humans review)
- Approval workflow for overrides — out of scope (porteiro makes the call)
- Multi-tenant cross-audit (platform admin view) — out of scope
- Mobile-native gatehouse app (PWA / Capacitor) — out of scope (responsive web only)

</deferred>
