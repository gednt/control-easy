# Feature Specification: Dashboard Visits and Access Registration

**Feature Branch**: `feat/dashboard-visits-access-registration`

**Created**: 2026-09-13

**Status**: Draft

**Input**: User description: "The visits on the home dashboard do not have the same functions of the tab visits... the standard modal does not open, there is nowhere to attribute where (to which apartment) the visit will be. Also, the entry and exit of residents and vehicles need to be registered also"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Register a dashboard visit (Priority: P1)

A gatehouse operator registers a visitor directly from the home dashboard using the same complete visit flow available on the Visits page, including the apartment the visitor is visiting.

**Why this priority**: A visit without a destination apartment is not actionable for a gatehouse and leaves the dashboard route inconsistent with the primary Visits workflow.

**Independent Test**: From the dashboard, create a visit and confirm that it appears on the Visits page with its apartment and scheduled or arrival information.

**Acceptance Scenarios**:

1. **Given** an authorized gatehouse operator is viewing the dashboard, **When** they start a new visit, **Then** the standard visit form opens.
2. **Given** the operator completes the dashboard visit form with a visitor and apartment, **When** they save it, **Then** the visit is recorded with that apartment and is visible in the Visits page.
3. **Given** the operator omits the apartment, **When** they try to save, **Then** the system clearly prevents an incomplete registration.

---

### User Story 2 - Register resident access movements (Priority: P1)

A gatehouse operator records a resident's entry and exit so the current access status and operational history remain accurate.

**Why this priority**: Residents are regular occupants but their movements must be traceable just like visitor movements.

**Independent Test**: Select a resident, register entry, then register exit, and verify both movements in the access history and the resident's current status.

**Acceptance Scenarios**:

1. **Given** a known resident is outside the condominium, **When** the operator records an entry, **Then** the resident is shown as inside and the movement is recorded.
2. **Given** a resident is inside the condominium, **When** the operator records an exit, **Then** the resident is shown as outside and the exit is recorded.

---

### User Story 3 - Register vehicle access movements (Priority: P2)

A gatehouse operator records a vehicle's entry and exit, preserving an auditable movement history.

**Why this priority**: Vehicle access is a security-sensitive operational event and must not be limited to registration data alone.

**Independent Test**: Select a registered vehicle, register entry and exit, and verify the vehicle's access history records both events.

**Acceptance Scenarios**:

1. **Given** a registered vehicle is outside the condominium, **When** the operator records an entry, **Then** the vehicle is shown as inside and the movement is recorded.
2. **Given** a registered vehicle is inside the condominium, **When** the operator records an exit, **Then** the vehicle is shown as outside and the exit is recorded.

---

### User Story 4 - Find a known resident during access registration (Priority: P1)

A gatehouse operator finds an already registered resident by CPF or resident ID and uses the confirmed record to register the access movement without retyping personal data.

**Why this priority**: The gatehouse must identify residents accurately and quickly; re-entering data increases delays and risks recording the wrong person.

**Independent Test**: Search a known resident by a formatted CPF and by resident ID, select the result, and confirm that the access form uses the resident's registered name and CPF.

**Acceptance Scenarios**:

1. **Given** the operator has a resident CPF, **When** they search using digits with or without punctuation, **Then** the matching active resident is offered from the current condominium.
2. **Given** a future QR reader supplies a resident ID, **When** the operator searches using that ID, **Then** the matching active resident is selected and their known details populate the access form.
3. **Given** no active resident matches the entered identifier, **When** the operator searches, **Then** the system clearly reports that no resident was found and does not prefill the form.

### Edge Cases

- The dashboard must show the same validation feedback as the Visits page when the visit data is incomplete or invalid.
- A failed save must retain the entered information so the operator can correct it without starting over.
- The system must prevent exit records from being created for visitor and service-provider categories, which use their own visit workflows.
- Only users permitted to operate the gatehouse can register access movements.
- Search results must be limited to the current condominium and inactive residents cannot be selected for a new access movement.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The dashboard MUST provide the same visit registration workflow as the Visits page.
- **FR-002**: The dashboard visit workflow MUST require the operator to select the apartment receiving the visit before it can be saved.
- **FR-003**: A visit created from the dashboard MUST appear in the Visits page with the same details as a visit created there.
- **FR-004**: Authorized gatehouse users MUST be able to register entry and exit movements for residents.
- **FR-005**: Authorized gatehouse users MUST be able to register entry and exit movements for registered vehicles.
- **FR-006**: The system MUST retain the time, movement direction, subject, and gatehouse operator for each successful resident or vehicle access movement.
- **FR-007**: The system MUST distinguish exit records from entry and consent records in the operational history.
- **FR-008**: The system MUST reject exit records for subject categories other than residents and vehicles and provide clear feedback.
- **FR-009**: When registering access for a resident, the system MUST let an authorized gatehouse user find active residents by CPF or resident ID.
- **FR-010**: CPF lookup MUST accept digits entered with or without common punctuation.
- **FR-011**: Selecting a resident from lookup results MUST populate the registered name and CPF for the access record without requiring the operator to re-enter them.
- **FR-012**: Resident lookup MUST not expose or select records outside the active condominium.

### Key Entities

- **Visit**: A planned or active visitor stay, including the receiving apartment and visit details.
- **Access movement**: A time-stamped entry or exit event for a resident or vehicle, linked to the gatehouse operator who recorded it.
- **Current access status**: The latest known inside/outside state used to validate the next movement.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An authorized operator can register a visit with its destination apartment from the dashboard in under two minutes.
- **SC-002**: 100% of visits created from the dashboard display their destination apartment on the Visits page.
- **SC-003**: An authorized operator can record a resident or vehicle entry or exit in under one minute.
- **SC-004**: Every successful resident and vehicle movement is visible in the access history with its time and direction.
- **SC-005**: Exit records for visitor and service-provider categories are rejected without adding an access-history record.
- **SC-006**: An authorized operator can find and select a known resident by CPF or resident ID in under 20 seconds.
- **SC-007**: 100% of selected resident access records use the resident name and CPF stored by the system.

## Assumptions

- Existing visit permissions and apartment availability rules apply equally to dashboard-created visits.
- Existing gatehouse access controls determine which roles can register movements.
- Resident and vehicle movement history follows the same operational retention policy as other gatehouse records.
- This change records access movement; it does not infer a live inside/outside presence state or introduce physical gate or hardware integration.
- Resident ID is the stable machine-readable identifier for a future QR integration. Resident slugs are not yet part of the resident data model and are outside this change.
