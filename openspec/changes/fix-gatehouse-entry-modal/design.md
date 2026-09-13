## Context

The gatehouse entry workflow (`CeEntryWorkflowComponent`) is the primary operational tool used by condominium porteiros (attendants) to record arrivals under the consent policy specified in `.specs/consent-gatehouse/requirements.md`. When the porteiro clicks "+ RECORD ENTRY" on the Shift Ledger dashboard or visits `/gatehouse`, a 4-tile selection modal opens:
1. `Register entry` (`with consent`)
2. `Entry denied` (`consent refused`)
3. `Gatehouse only` (`package drop`)
4. `Override` (`emergency / vouched`)

However, an audit of the current implementation reveals critical defects across interaction flow, event propagation, and backend validation:
- The modal cannot be closed by clicking the close button `&times;` or the backdrop because `CeModalComponent` lacks a `closed` output.
- "Entry denied" inappropriately forces a camera capture prompt onto an entrant who refused consent, and backend validation rejects the refusal entry if photos are required.
- "Register entry" does not evaluate the tenant's category consent policy before deciding whether to open camera capture.
- Attendants cannot select or change the subject category (resident, visitor, service provider, vehicle).

## Goals / Non-Goals

**Goals:**
- Enable reliable, keyboard-accessible, and click-accessible closing of the entry workflow modal and any sub-modals.
- Align "Entry denied" with spec requirements: refusal is recorded without forcing camera capture, and backend accepts `entered_without_consent` entries with `PhotoId: null`.
- Restore consent policy checking on "Register entry" so camera only triggers when policy mandates it.
- Allow attendants to choose or switch category among the four supported domain categories (`dweller`, `visitor`, `service_provider`, `vehicle`).
- Maintain the 3-second workflow goal for gatehouse operations.

**Non-Goals:**
- Redesigning the Shift Ledger dashboard layout itself.
- Changing the CSV export format or audit viewer beyond supporting valid records.
- Adding hardware integrations (IoT barriers/relays are deferred to v2.1).

## Decisions

### Decision 1: Add `closed = output<void>()` to `CeModalComponent`

**Choice:** Add `closed = output<void>()` to `CeModalComponent` and emit both `openChange(false)` and `closed.emit()` inside `close()`.

**Rationale:** Existing components throughout the codebase (`ce-entry-workflow`, `ce-photo-capture`, `ce-override-reason`) expect a `(closed)` event output. Adding this to `CeModalComponent` preserves two-way banana-in-a-box binding `[(open)]="isOpen"` via `openChange` while restoring compatibility with all `(closed)` handlers.

### Decision 2: Pure subject-info flow for "Entry denied"

**Choice:** Clicking "Entry denied" sets `pendingState = 'entered_without_consent'`, skips `CePhotoCaptureComponent`, and moves immediately to `step = 'subject-info'`.

**Rationale:** A refusal of consent means the individual refuses to have their picture taken. Attempting to activate the camera is contradictory to consent refusal. In `CreateEntryLogHandler.cs`, remove `or EntryStates.EnteredWithoutConsent` from line 55's photo requirement check so the backend permits recording this refusal without a photo.

### Decision 3: Policy-driven auto-camera on "Register entry"

**Choice:** Inject `ConsentPolicyService`. When "Register entry" is clicked:
1. Lookup the active policy for the selected category (`visitors` by default).
2. If `policy?.photoRequired == true`, set `showCapture.set(true)`.
3. If `policy?.photoRequired == false` or policy fetch fails gracefully, proceed directly to `step = 'subject-info'`.

**Rationale:** The gatehouse principle is "logging is faster than skipping". If a condominium does not require photos for a given category, forcing the camera slows down the attendant and breaks the 3-second operational budget.

### Decision 4: Add category selector to `subject-info`

**Choice:** In the `subject-info` form, include a segmented toggle or selector for `visitor` (Visitor), `dweller` (Resident), `service_provider` (Service Provider), and `vehicle` (Vehicle).

**Rationale:** The gatehouse attendant encounters all 4 types of arrivals. Hardcoding `'visitor'` prevents logging residents or vehicles at the gatehouse.

## Risks / Trade-offs

- **[Risk] Existing E2E test `gatehouse-workflow.spec.ts` expects camera on "entry denied".**
  - *Mitigation:* Update the E2E test to verify that "entry denied" bypasses camera and goes directly to subject info, consistent with the constitutional specification in `.specs/consent-gatehouse/requirements.md`.
- **[Trade-off] Additional network request to fetch consent policy on tile click.**
  - *Mitigation:* Cache policy in memory or pre-fetch on modal open so tile click responds within milliseconds.

## Migration Plan

No database migration required. The database schema already accommodates null `photo_id` for `entered_without_consent` entries.
1. Update `CeModalComponent` and `CeEntryWorkflowComponent`.
2. Update `CreateEntryLogHandler.cs` validation logic.
3. Update unit and E2E tests.
