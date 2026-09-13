## ADDED Requirements

### Requirement: Gatehouse entry modal must be closable via close button, backdrop, and Escape
The system SHALL ensure that `CeModalComponent` emits a `closed` event when the user clicks the close button (`&times;`), clicks the backdrop, or presses the Escape key, allowing `CeEntryWorkflowComponent` and any other modal consumer to dismiss the dialog cleanly.

#### Scenario: Attendant closes entry workflow modal with close button
- **WHEN** the attendant opens the "New entry" modal on the dashboard or gatehouse page
- **AND** clicks the `&times;` close button in the modal header
- **THEN** the modal closes and returns focus to the trigger button.

#### Scenario: Attendant dismisses entry workflow modal with backdrop click
- **WHEN** the attendant opens the "New entry" modal
- **AND** clicks outside the modal dialog onto the backdrop
- **THEN** the modal closes cleanly.

### Requirement: Entry denied flow records refusal without camera capture
The system SHALL record an entry refusal (`entered_without_consent`) when an entrant refuses consent, without invoking photo capture or opening the camera stream, and without requiring a photo ID at the backend.

#### Scenario: Attendant logs entry denied for entrant refusing photo consent
- **WHEN** an entrant refuses consent to be photographed
- **AND** the attendant taps the "Entry denied / consent refused" tile
- **THEN** the camera capture interface is NOT opened
- **AND** the workflow advances directly to the subject-info form
- **AND** when the attendant submits, the backend creates a `ConsentAuditLogEntry` with `entryState: "entered_without_consent"` and `photoId: null` even if the category policy mandates photos for consented entries.

### Requirement: Register entry evaluates category consent policy
The system SHALL inspect the tenant's consent policy for the chosen subject category when the attendant taps "Register entry":
- If the policy specifies `photo_required: true`, the camera interface SHALL open automatically.
- If the policy specifies `photo_required: false`, the camera interface SHALL be bypassed and the workflow SHALL advance directly to the subject-info form.

#### Scenario: Policy requires photo for visitor
- **GIVEN** the tenant consent policy has `photo_required: true` for visitors
- **WHEN** the attendant taps "Register entry"
- **THEN** the camera capture interface opens automatically.

#### Scenario: Policy does not require photo for category
- **GIVEN** the tenant consent policy has `photo_required: false` for visitors
- **WHEN** the attendant taps "Register entry"
- **THEN** the camera capture interface is skipped and the subject-info form is displayed.

### Requirement: Attendant can select entry subject category
The system SHALL allow the attendant to select any of the supported subject categories (`dweller`, `visitor`, `service_provider`, `vehicle`) during the entry workflow.

#### Scenario: Attendant switches subject category to resident/dweller
- **WHEN** recording an entry
- **THEN** the attendant can select "Resident" (`dweller`) as the subject category
- **AND** the submitted entry log record specifies `subjectType: "dweller"`.
