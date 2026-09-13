# Tasks — Fix Gatehouse Entry Modal & Workflow

> **Scope.** Fixes to `CeModalComponent`, `CeEntryWorkflowComponent`, `CePhotoCaptureComponent`, `CeOverrideReasonComponent`, backend `CreateEntryLogHandler.cs`, and corresponding unit/E2E test suites.

## 1. Modal Dismissal & Event Lifecycle

- [x] 1.1 Add `closed = output<void>()` to `CeModalComponent` in `src/Web/ControlEasyReborn.Web/src/app/design-system/components/modal/modal.component.ts`. Ensure `close()` emits both `this.openChange.emit(false)` and `this.closed.emit()`.
- [x] 1.2 Verify `CeModalComponent` unit tests in `modal.component.spec.ts` assert `closed` emission on backdrop click, close button click, and Escape keydown.
- [x] 1.3 Verify `CeEntryWorkflowComponent` properly propagates modal dismissal to parent components (`DashboardPage` and `EntryWorkflowPage`).

## 2. Backend Validation Fix for Consent Refusal

- [x] 2.1 Update `CreateEntryLogHandler.cs` in `src/Modules/Photos/ControlEasyReborn.Modules.Photos.Application/Handlers/EntryLogHandlers.cs` line 55: remove `or EntryStates.EnteredWithoutConsent` from the `PhotoRequired` check so that only `EnteredWithConsent` requires a `PhotoId`.
- [x] 2.2 Add unit tests in `ControlEasyReborn.UnitTests` covering `CreateEntryLogHandler` for `entered_without_consent` with a null `PhotoId` when `PhotoRequired == true`, asserting success.

## 3. Entry Workflow Interaction & Category Selection

- [x] 3.1 Update `CeEntryWorkflowComponent` "Entry denied" flow to transition directly to `step = 'subject-info'` without invoking `CePhotoCaptureComponent`.
- [x] 3.2 Restore tenant consent policy evaluation on "Register entry": if category policy requires a photo, trigger `CePhotoCaptureComponent`; otherwise advance straight to `step = 'subject-info'`.
- [x] 3.3 Add Category selector in `CeEntryWorkflowComponent` subject-info form (`dweller`, `visitor`, `service_provider`, `vehicle`).
- [x] 3.4 Ensure cancel/close actions within sub-components (`CePhotoCaptureComponent` and `CeOverrideReasonComponent`) return cleanly to the tile selection step without freezing or resetting invalid state.

## 4. Test Suite Verification & Verification Gate

- [x] 4.1 Update Angular unit tests in `entry-workflow.component.spec.ts` to reflect the corrected flows:
  - "denied" tile transitions to `subject-info` without opening camera.
  - Modal close button emits `closed`.
  - Category selection updates payload.
- [x] 4.2 Update E2E test `gatehouse-workflow.spec.ts` to verify "entry denied" bypasses camera and logs refusal directly.
- [x] 4.3 Run Angular unit tests and .NET tests (136/136 .NET unit tests passing).
- [ ] 4.4 Verify live interaction in the browser (user conducting manual verification).

## Task Dependency Graph

```json
{
  "waves": [
    {
      "wave": 1,
      "tasks": ["1.1", "1.2", "1.3", "2.1", "2.2"]
    },
    {
      "wave": 2,
      "tasks": ["3.1", "3.2", "3.3", "3.4"]
    },
    {
      "wave": 3,
      "tasks": ["4.1", "4.2", "4.3", "4.4"]
    }
  ]
}
```

## Verification Gate

1. `CeModalComponent` emits `closed` on click of `&times;`, backdrop, or Escape.
2. Clicking "Entry denied" never launches the camera, collects optional details, and successfully logs `entered_without_consent`.
3. Backend succeeds on `entered_without_consent` even with tenant `PhotoRequired = true`.
4. Attendants can select subject category (Dweller, Visitor, Service Provider, Vehicle).
5. All .NET unit tests, Angular unit tests, and Playwright E2E tests pass.
