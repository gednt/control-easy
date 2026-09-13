## Why

The Gatehouse "New entry" modal (`CeEntryWorkflowComponent`) on the dashboard and `/gatehouse` route is broken across multiple fundamental user journeys and contracts:

1. **Modal close and backdrop dismissal are inert**: `CeModalComponent` declares `openChange = output<boolean>()` but does not expose a `closed = output<void>()` event. However, `CeEntryWorkflowComponent`, `CePhotoCaptureComponent`, and `CeOverrideReasonComponent` all bind `(closed)="close()"`. As a result, clicking the modal's `&times;` close button, clicking the backdrop, or pressing Escape does nothing. Attendants are locked into the modal unless they complete a submission or refresh the page.
2. **"Entry denied (consent refused)" inappropriately forces photo capture**: When an entrant refuses consent to be photographed, clicking the "Entry denied / consent refused" tile triggers camera capture (`showCapture.set(true)`). A person refusing photo consent cannot and should not be forced into a camera prompt. If the attendant cancels the camera, the workflow resets to the initial 4 tiles, preventing the attendant from ever logging the refused entry.
3. **Backend rejects `entered_without_consent` when photo policy is required**: In `CreateEntryLogHandler.cs` (line 55), the validation checks:
   ```csharp
   if (policy is { PhotoRequired: true }
       && request.EntryState is EntryStates.EnteredWithConsent or EntryStates.EnteredWithoutConsent
       && request.PhotoId is null)
   ```
   This erroneously enforces that `entered_without_consent` must have a non-null `PhotoId` whenever the tenant policy requires photos. Because consent was refused, no photo exists, causing the backend to throw a 400 ValidationException and blocking all refusal logging.
4. **"Register entry" ignores tenant consent policy**: The requirement (UC-CG-04, CONSENT-02) dictates that photo capture auto-opens *only if required by policy*. Currently, `ConsentPolicyService` checking was removed, and photo capture is blindly launched regardless of policy.
5. **Category selection is inaccessible**: The domain and API contract support 4 subject categories (`dweller`, `visitor`, `service_provider`, `vehicle`). The UI hardcodes `selectedCategory` to `'visitor'` (or `'service_provider'` for gatehouse-only) with no option to select or switch subject category.
6. **Nested modal overlay issues**: When opening sub-modals (`CePhotoCaptureComponent`, `CeOverrideReasonComponent`) within the workflow modal, double backdrops and identical z-indexes cause visual glitches and focus traps.

This change fixes both frontend and backend to restore the gatehouse entry modal to its documented design and operational specifications.

## What Changes

- **`CeModalComponent`**:
  - Add `closed = output<void>()` alongside `openChange` so that `(closed)` listeners receive close notifications when the close button, backdrop, or Escape key is triggered.
- **`CeEntryWorkflowComponent`**:
  - Fix modal close bindings so clicking `&times;`, backdrop, or Escape properly closes the modal and emits `closed` to parent pages (`DashboardPage`, `EntryWorkflowPage`).
  - Correct "Entry denied" flow: bypass photo capture completely, transition immediately to `subject-info` (visitor name/document optional), and submit `entered_without_consent` without a photo ID.
  - Re-integrate `ConsentPolicyService` into "Register entry": query policy for the selected category; if photo required, open photo capture; otherwise proceed directly to `subject-info`.
  - Add Category selection (Dweller/Resident, Visitor, Service Provider, Vehicle) in the subject-info step or initial flow so attendants can record entries for all supported categories.
  - Cleanly handle cancellation/dismissal in child flows (cancelling photo capture or override reason returns to the tile view without getting stuck).
- **`CreateEntryLogHandler.cs` (Backend)**:
  - Fix policy enforcement on line 55: only require `PhotoId != null` when `request.EntryState == EntryStates.EnteredWithConsent`. Do not require a photo for `EnteredWithoutConsent`.
- **Tests**:
  - Add unit tests in `CeEntryWorkflowComponent.spec.ts` for close button dismissal, consent refusal without photo, and category selection.
  - Add unit tests in `CeModalComponent.spec.ts` verifying `closed` output emission.
  - Update `EntryLogHandlersTests.cs` to verify `entered_without_consent` succeeds without a photo even when policy has `PhotoRequired = true`.
  - Update E2E test `gatehouse-workflow.spec.ts` to reflect the corrected entry denied flow (no camera capture).

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `gatehouse-entry-workflow`: Modifies gatehouse entry workflow modal interactions, consent refusal handling, consent policy auto-camera triggers, category selection, and modal close lifecycle.

## Impact

- **Frontend**:
  - `src/app/design-system/components/modal/modal.component.ts`: Expose `closed` output.
  - `src/app/design-system/components/entry-workflow/entry-workflow.component.ts`: Fix tile taps, policy check, category selector, close propagation.
- **Backend**:
  - `src/Modules/Photos/.../Handlers/EntryLogHandlers.cs`: Correct validation condition for photo requirement.
- **Tests**:
  - `entry-workflow.component.spec.ts`, `modal.component.spec.ts`, `EntryLogHandlersTests.cs`, `gatehouse-workflow.spec.ts`.
