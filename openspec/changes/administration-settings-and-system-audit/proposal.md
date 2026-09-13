## Why

The current Administration page (`/administration`, `CeAdministrationPageComponent`) and underlying `Administration` module suffer from critical architectural and UX limitations:

1. **Useless "Configuration register"**: It is implemented as an untyped key-value table (`Key`, `Value`, `Description`) requiring administrators to manually invent and type string keys. No other service consumes these keys, and essential condominium settings (e.g. gatehouse operating hours, attendant shift durations, visitor access windows, auto-checkout rules) cannot be managed or validated through the UI.
2. **Dead "Audit ledger"**: The audit log table in `Administration` is completely disconnected from platform activity. When visits are logged, residents updated, gates operated, overrides approved, or attendant shifts started, nothing writes to the administration audit log. As a result, the audit table is perpetually empty, providing zero security or operational visibility.
3. **No true System-Wide Audit view**: Condominium administrators and security supervisors currently have no centralized timeline to investigate security incidents, audit attendant actions (such as consent overrides or gate overrides), or track configuration changes.

This change transforms the Administration module and screen into two high-value operational capabilities:
- **Structured Condominium & Gatehouse Settings**: A strongly-typed configuration system replacing arbitrary string keys with structured settings for gatehouse operations, visitor policies, photo consent defaults, attendant shifts, and alert rules.
- **System-Wide Audit Trail**: A centralized, cross-module audit ledger capturing security, access-control, and configuration events across the platform with rich filtering (by date range, category, actor, action, and severity) and expandable event detail inspection.

---

## What Changes

### 1. Structured Condominium & Gatehouse Settings
- **Domain & Application**:
  - Introduce `CondominiumSettings` domain aggregate in `ControlEasyReborn.Modules.Administration.Domain` with strongly-typed properties:
    - **Gatehouse Operations**: default visit duration limit (minutes), require shift handover notes (boolean), default attendant shift length (hours), emergency contact number.
    - **Visitor & Access Rules**: allowed visitor entry time window (e.g., 06:00 to 22:00), auto-checkout overdue visits at midnight (boolean), maximum active visitors per apartment unit.
    - **Photo & Consent Enforcement**: photo capture requirement policy per subject category (`dweller`, `visitor`, `service_provider`), behavior on consent refusal (strict refusal vs. supervisor override permitted).
    - **Notification & Alerts**: trigger alert on overdue visit (boolean), alert threshold (minutes).
  - Seed default settings per tenant on initialization and provide `GetCondominiumSettingsHandler` and `UpdateCondominiumSettingsHandler` with FluentValidation constraints.
- **API Endpoints**:
  - `GET /api/v1/administration/settings` — retrieve current tenant settings.
  - `PUT /api/v1/administration/settings` — update tenant settings with validation and automatic audit trail emission.
  - Retain backwards compatibility for legacy key-value endpoints while deprecating them.

### 2. System-Wide Security Audit Trail
- **Cross-Module Event Emission**:
  - Introduce an `IAuditLogWriter` interface in shared building blocks or administration application layer so modules (`Visits`, `Photos`, `Security`, `Administration`) can record audit events cleanly.
  - Capture key system events:
    - `Gatehouse.EntryCreated`, `Gatehouse.EntryDenied`, `Gatehouse.OverrideReasonRecorded`
    - `Visits.VisitStarted`, `Visits.VisitCompleted`, `Visits.OverdueVisitAutoClosed`
    - `Residents.ResidentCreated`, `Residents.ResidentDeactivated`, `Apartments.UnitAssigned`
    - `Security.UserLoggedIn`, `Security.ShiftStarted`, `Security.ShiftEnded`
    - `Administration.SettingsUpdated`
  - Enhance `AuditLogEntry` to include `Category` (Gatehouse, Visits, Residents, Security, Settings), `Severity` (Info, Warning, SecurityAlert), and structured `MetadataJson` for contextual payload inspection.
- **API Endpoints**:
  - Enhanced `GET /api/v1/administration/audit-logs` supporting query parameters: `category`, `action`, `severity`, `performedByUserId`, `fromUtc`, `toUtc`, `searchTerm`, `skip`, `take`.
  - `GET /api/v1/administration/audit-logs/{id}` — returns full audit entry details including metadata payload.

### 3. Frontend Redesign (`CeAdministrationPageComponent`)
- **Tab 1 — Condominium & Gatehouse Settings**:
  - Replace the generic "+ Add configuration" modal with a dedicated, structured settings form grouped into clear operational cards:
    - *Gatehouse Operations*: shift parameters, handover requirements.
    - *Visitor Access Rules*: entry time windows, unit visitor quotas.
    - *Photo & Consent Policies*: category requirements and refusal handling.
    - *Alerts & Notifications*: overdue thresholds.
  - Interactive form controls: toggle switches, numeric duration steppers, time range pickers, dropdowns.
  - Action bar with "Save changes", dirty state detection, and success toast/notification.
- **Tab 2 — System-Wide Audit Trail**:
  - Filter bar: Date range picker (Today, Last 7 Days, Custom), Category selector, Severity filter (All, Info, Warning, Alert), and Search filter.
  - Rich data table showing: Timestamp, Severity badge, Category, Action, Actor / Performed By, Entity, and "Inspect" button.
  - Detail drawer / modal: Displays the complete audit event trail, IP/client info, and formatted JSON/key-value change summary.
  - Export to CSV / JSON convenience button for compliance and security archiving.

---

## Capabilities

### New Capabilities
- `condominium-settings`: Strongly-typed configuration for condominium gatehouse rules, visitor access policies, photo consent enforcement, and alert thresholds.
- `system-wide-audit-trail`: Centralized cross-module security and operational event logging with multi-criteria filtering and detailed event inspection.

### Modified Capabilities
- `administration-page`: Replaces generic key-value editor and dummy audit list with integrated settings management and interactive audit trail.

---

## Impact

- **Backend**:
  - `src/Modules/Administration/ControlEasyReborn.Modules.Administration.Domain/`: New `CondominiumSettings` entity, enhanced `AuditLogEntry` entity and enum types.
  - `src/Modules/Administration/ControlEasyReborn.Modules.Administration.Application/`: Settings handlers, enhanced audit log querying, `IAuditLogWriter` implementation.
  - `src/Modules/Administration/ControlEasyReborn.Modules.Administration.Api/`: Settings endpoints, enhanced audit endpoints.
  - Shared / other modules: Wire audit event writing on key operations (`Visits`, `Photos`, `Security`).
- **Frontend**:
  - `src/app/features/administration/administration.page.ts`: Redesigned with two dedicated feature sub-views (Settings and Audit Trail).
  - `src/app/features/administration/administration-api.service.ts`: Updated API methods for typed settings and filtered audit logs.
  - New subcomponents or templates for Settings sections and Audit Filter Bar / Detail modal.
- **Tests**:
  - Unit tests for settings validation, handlers, and audit log filtering in `ControlEasyReborn.UnitTests`.
  - Architecture tests confirming no circular dependencies between Administration and other modules.
  - Angular unit tests for `AdministrationPage` signals, settings form validation, and audit trail rendering.
