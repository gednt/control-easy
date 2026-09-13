## ADDED Requirements

### Requirement: Condominium and Gatehouse Settings Management
The system SHALL provide strongly-typed settings per tenant to govern gatehouse operations, visitor policies, photo consent enforcement, and alert thresholds.

#### Scenario: Administrator views current condominium settings
- **GIVEN** an authenticated condominium administrator (`TenantAdmin`)
- **WHEN** the administrator navigates to the Administration page under the "Settings" tab
- **THEN** the system loads and displays the structured settings for:
  - Gatehouse operations (visit timeout, handover notes requirement, shift duration)
  - Visitor rules (allowed visit hours, auto-checkout at midnight, max visitors per unit)
  - Photo consent policies (photo required per entrant category, refusal override rule)
  - Alert parameters (overdue visit notification, warning threshold in minutes).

#### Scenario: Administrator updates condominium settings
- **GIVEN** an administrator changes the visitor allowed hours to "07:00 - 22:00" and enables "auto-checkout at midnight"
- **WHEN** the administrator submits the updated settings
- **THEN** the backend validates the inputs against domain constraints
- **AND** persists the updated settings for the tenant
- **AND** automatically emits an audit log entry in the `Settings` category recording the modifications
- **AND** the UI indicates successful persistence without requiring a page reload.

#### Scenario: Validation fails for invalid settings
- **WHEN** an administrator attempts to save invalid settings (e.g. negative shift duration, end time earlier than start time)
- **THEN** the system rejects the update with a 400 Validation ProblemDetails response
- **AND** highlights the invalid form fields with descriptive validation errors.

---

### Requirement: Cross-Module System-Wide Audit Logging
The system SHALL provide a centralized audit logging mechanism that records operational, access-control, and security events across modules with categorization, severity levels, and structured event metadata.

#### Scenario: Gatehouse entry refusal is captured in system audit
- **WHEN** a visitor or entrant refuses photo consent at the gatehouse
- **THEN** the system records an audit entry with:
  - `Category`: `Gatehouse`
  - `Action`: `EntryRefused`
  - `Severity`: `Warning`
  - `PerformedBy`: The logged-in attendant
  - `Metadata`: Entrant category, gatehouse ID, and reason.

#### Scenario: Gatehouse supervisor override is captured in system audit
- **WHEN** an attendant executes a supervisor override to authorize entry without required identification
- **THEN** the system records an audit entry with:
  - `Category`: `Gatehouse`
  - `Action`: `OverrideAuthorized`
  - `Severity`: `SecurityAlert`
  - `Metadata`: Justification reason, supervisor identity, and subject ID.

#### Scenario: Resident changes and tenant settings updates are captured
- **WHEN** a resident record is created or deactivated, or condominium settings are modified
- **THEN** corresponding audit events are automatically written to `Residents` or `Settings` categories with timestamp, actor, and summary of changes.

---

### Requirement: System-Wide Audit Ledger Filtering and Inspection
The system SHALL provide condominium administrators with an interactive, filterable view of the system-wide audit trail with full event detail inspection.

#### Scenario: Filtering audit entries by category and date range
- **GIVEN** an administrator viewing the System-Wide Audit Ledger
- **WHEN** the administrator selects `Category: Gatehouse`, `Severity: Warning & SecurityAlert`, and `Date: Last 7 Days`
- **THEN** the table displays only the audit events matching the criteria, ordered reverse-chronologically by timestamp.

#### Scenario: Inspecting audit event details
- **GIVEN** a table of audit entries
- **WHEN** the administrator clicks the "Inspect" action on a specific audit event
- **THEN** a detail drawer or modal opens displaying:
  - Event ID and exact UTC & localized timestamp
  - Category and Action name
  - Actor name, role, and ID
  - Target entity type and ID
  - Formatted JSON or key-value display of the contextual metadata payload.

---

### Requirement: Removal of Generic Untyped Configuration Register
The system SHALL replace the legacy arbitrary key-value configuration table in the web UI with the new structured Condominium Settings interface.

#### Scenario: Navigating to Administration page shows structured sections
- **WHEN** an administrator navigates to `/administration`
- **THEN** the page displays the structured "Condominium & Gatehouse Settings" tab and "System-Wide Audit" tab
- **AND** the generic untyped key/value text input form is no longer exposed.
