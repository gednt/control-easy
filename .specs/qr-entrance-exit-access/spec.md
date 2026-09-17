# Feature Specification: Gatehouse Access and Visit Destinations

**Feature Branch**: `feat/qr-entrance-exit-access`

**Created**: 2026-09-13

**Status**: Draft

**Input**: User description: "Support resident and vehicle entrance and exit through ID-based QR codes first; facial biometrics is planned for a later feature. Every visit must identify its destination apartment and block; resident-based flows automatically recover that destination."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Record resident access by QR code (Priority: P1)

As a gatehouse attendant, I scan a resident's QR code at an entrance or exit and immediately see whether it is valid, who it represents, and whether the access event was recorded. This replaces manual identification for the normal resident flow while preserving the existing gatehouse safeguards.

**Why this priority**: Residents are frequent users of condominium access points; a quick, reliable scan reduces queues and transcription errors.

**Independent Test**: Scan an active resident credential for both directions and confirm two correctly attributed, time-stamped access events are available for authorized review.

**Acceptance Scenarios**:

1. **Given** an active resident QR credential belonging to the current condominium and an active apartment assignment, **When** a gatehouse attendant scans it for entrance, **Then** the system confirms the resident identity, automatically shows the resident's apartment and block, and records an entrance access event with that destination.
2. **Given** the same active resident credential, **When** it is scanned for exit, **Then** the system records an exit access event with the recovered destination without requiring the attendant to re-enter identity details.
3. **Given** an inactive, expired, revoked, malformed, or unknown resident QR credential, **When** it is scanned, **Then** the system refuses the QR-based access action, records the attempted scan for security review, and gives the attendant an actionable reason without exposing another person's data.

---

### User Story 2 - Record vehicle access by QR code (Priority: P1)

As a gatehouse attendant, I scan a registered vehicle's QR code at an entrance or exit so the condominium has an accurate vehicle access history without manually searching a plate or vehicle record.

**Why this priority**: Vehicles are a separate access subject with their own registration lifecycle and need the same fast, auditable gatehouse flow as residents.

**Independent Test**: Scan an active vehicle credential at both directions and verify that the resulting events identify the registered vehicle and its associated resident where available.

**Acceptance Scenarios**:

1. **Given** an active vehicle QR credential registered in the current condominium, **When** it is scanned for entrance, **Then** the system confirms the vehicle identity and records an entrance access event.
2. **Given** an active vehicle QR credential, **When** it is scanned for exit, **Then** the system records an exit access event for that vehicle.
3. **Given** a vehicle credential that has been deactivated or belongs to another condominium, **When** it is scanned, **Then** the system rejects the QR-based access action and does not disclose the vehicle or resident identity to the attendant.

---

### User Story 3 - Find and record access without a QR code (Priority: P1)

As a gatehouse attendant, I can quickly find a resident by CPF or another registered identity document, name, apartment, or block when a QR code is unavailable, then record the resident's or associated vehicle's entrance or exit without abandoning the access workflow.

**Why this priority**: A QR code can be forgotten, damaged, or unavailable. The gatehouse needs a secure and fast fallback so a missing code does not turn into an unrecorded access event.

**Independent Test**: Without scanning a QR code, search the current condominium using each supported search criterion, select the intended eligible resident or vehicle, and record an attributed entrance or exit event.

**Acceptance Scenarios**:

1. **Given** a resident has no scannable QR code, **When** the attendant searches by their complete CPF or another registered identity document, **Then** the system returns only matching residents in the current condominium and lets the attendant select the intended access subject.
2. **Given** an attendant knows a resident's name, apartment, or block, **When** they enter a sufficiently specific search value, **Then** the system returns matching current-tenant residents and their registered vehicles as needed to distinguish the intended subject.
3. **Given** the attendant selects an eligible resident or registered vehicle from the manual search results, **When** they confirm entrance or exit, **Then** the system records the access event as a manual lookup and identifies the recording attendant.
4. **Given** the search returns no match or multiple plausible matches, **When** the attendant cannot confidently select a subject, **Then** the system prevents an un-attributed access event and directs the attendant to the condominium's existing manual-access procedure.

---

### User Story 4 - Always identify a visit destination (Priority: P1)

As a gatehouse attendant, I always record the apartment and block a visitor or service provider is going to, so every visit has an accountable destination and no workflow leaves a visit marked as destination pending.

**Why this priority**: A destination is essential gatehouse information. Allowing a visitor record without it undermines accountability and makes the visit history less useful.

**Independent Test**: Create or update each supported visit type and confirm an active destination apartment is required, its block and unit are shown before confirmation, and no new record displays a pending-destination status.

**Acceptance Scenarios**:

1. **Given** a gatehouse attendant creates or updates a visitor or service-provider visit, **When** they submit it, **Then** they must select an active destination apartment and the system displays its block and unit before the visit is recorded.
2. **Given** a resident is selected in either the existing visit interface or the new QR/manual-access interface, **When** the resident has an active apartment assignment, **Then** the system automatically recovers and displays that apartment's block and unit as the destination.
3. **Given** a resident, associated vehicle, visitor, or service provider has no valid destination apartment, **When** an attendant attempts to record the visit or access, **Then** the system prevents the record from being completed, explains that a destination must be assigned, and does not label the record as destination pending.
4. **Given** a resident's apartment changes after a visit is recorded, **When** an authorized user reviews the visit history, **Then** the recorded destination remains attributable to the apartment and block selected or recovered at the time of the visit.

---

### User Story 5 - Manage and audit QR credentials (Priority: P2)

As a tenant administrator, I can issue, replace, deactivate, and review QR credentials for residents and vehicles, so lost, copied, or no-longer-authorized credentials stop being usable and access history remains attributable.

**Why this priority**: QR access is secure only if credentials have a controlled lifecycle and administrators can investigate usage.

**Independent Test**: Issue a credential, use it successfully, deactivate it, and verify a later scan is refused while both the valid event and refused attempt remain reviewable.

**Acceptance Scenarios**:

1. **Given** an eligible resident or registered vehicle, **When** a tenant administrator issues or replaces its QR credential, **Then** the prior credential is no longer valid and the new credential is ready for gatehouse use.
2. **Given** a lost, compromised, or no-longer-authorized credential, **When** a tenant administrator deactivates it, **Then** later scans are refused and the deactivation is attributable to that administrator.
3. **Given** an authorized tenant administrator, **When** they review access history, **Then** they can filter QR-based events and refused attempts by date, direction, subject type, subject, and attendant.

---

### User Story 6 - Preserve a future facial-biometric path (Priority: P3)

As a product owner, I need QR access to be the first supported credential method while leaving a clearly bounded path for a later facial-biometric feature, without collecting or matching biometric data now.

**Why this priority**: The future direction is documented without delaying the usable QR release or creating a biometric privacy risk prematurely.

**Independent Test**: Review the QR feature's credential and audit records to confirm they identify the credential method and contain no facial biometric template, facial matching result, or biometric enrollment data.

**Acceptance Scenarios**:

1. **Given** a QR-based access event, **When** it is reviewed, **Then** its credential method is identifiable as QR code.
2. **Given** this release, **When** an administrator manages access credentials, **Then** no option exists to enroll, store, compare, or authenticate by facial biometrics.

### Edge Cases

- A scan that cannot be read, has an altered payload, or resolves to no credential is refused, recorded as an attempted scan, and does not reveal a matching subject.
- A QR credential for another tenant is treated as invalid to the current attendant and is never usable across condominium boundaries.
- If an entrance scan follows a previous unpaired entrance, or an exit scan follows a previous unpaired exit, the event is recorded and flagged for review rather than blocking a resident or vehicle because an earlier event was missed.
- Repeated scans of the same credential and direction within a short operational interval are identified as potential duplicates so the attendant can avoid unintentionally creating multiple access events.
- If the gatehouse cannot reach the access service, the attendant receives a clear failure state and follows the condominium's existing manual-access procedure; QR scanning does not itself operate a physical gate.
- If no QR code is available, the attendant can use the protected manual lookup flow; an absent or ambiguous search result cannot be used to create an un-attributed access event.
- A document, name, apartment, or block search returning more than one person shows only the minimum current-tenant information required for the attendant to distinguish the intended resident or vehicle.
- A resident or vehicle made inactive after a QR code was issued cannot use that QR code.
- A visit cannot be created, updated, or completed without a valid active destination apartment; legacy records that predate this requirement remain historical records but must never be presented as a newly pending destination.
- When a resident or associated vehicle is selected but its registered apartment is inactive or absent, the system requires the resident or vehicle registration to be corrected before a destination-dependent visit or access event can be recorded.
- Existing photo records are not facial biometric templates and must not be repurposed for identity matching in this feature.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST let authorized tenant administrators issue a unique QR credential to an eligible resident and to an eligible registered vehicle within their condominium.
- **FR-002**: The system MUST support a credential lifecycle of active, replaced, revoked, expired, and inactive, and MUST refuse QR-based access when the credential or its subject is not active.
- **FR-003**: The system MUST allow authorized gatehouse attendants to scan a QR credential or use a manual lookup when no QR code is available, and explicitly select or confirm whether the event is an entrance or exit.
- **FR-004**: For each successful scan, the system MUST validate the credential, subject eligibility, current condominium, and attendant authorization before recording an attributed access event with direction and time.
- **FR-005**: The system MUST record successful resident and vehicle QR access events in an immutable, tenant-scoped history that identifies the subject, credential method, direction, recording attendant, and event time.
- **FR-006**: The system MUST record refused QR scans with a non-sensitive failure category, time, direction when supplied, and recording attendant, while withholding any identity that does not belong to the current tenant.
- **FR-007**: The system MUST present the gatehouse attendant with a clear success, refusal, duplicate-warning, or service-unavailable result quickly enough to support a normal access-point workflow.
- **FR-008**: The system MUST allow authorized tenant administrators to replace or revoke a QR credential and retain the lifecycle action, reason when supplied, time, and acting administrator for audit.
- **FR-009**: The system MUST let authorized tenant administrators review and filter QR access events and refused attempts by date range, entrance/exit direction, resident or vehicle, QR credential status, and recording attendant.
- **FR-010**: QR-based access MUST honor existing tenant security and gatehouse policy requirements; it MUST NOT bypass consent, authorization, or audit obligations already applicable to the access subject.
- **FR-011**: The QR feature MUST record access only and MUST NOT directly unlock, lock, or otherwise control a physical door or gate.
- **FR-012**: The system MUST identify QR code as the credential method for this release and reserve facial biometrics as a future method without enrolling, storing, processing, comparing, or authenticating facial biometric data. See `docs/access-control.md#biometric-exclusion` for the architectural and schema-level enforcement (the AccessControlBiometricExclusionTests + AccessControlTenantRulesTests arch tests fail the build if a `biometric_*` keyword appears outside an explicit `reserved` comment).
- **FR-013**: The system MUST prevent QR credentials, access events, and refused attempts from being viewed, issued, or used across condominium boundaries.
- **FR-014**: When no QR code is available, the system MUST allow authorized gatehouse attendants to find current-tenant residents by complete CPF, another registered identity document, name, apartment, or block, and to identify their associated registered vehicles where relevant.
- **FR-015**: The system MUST require a sufficiently specific manual search and present only the minimum current-tenant information needed to distinguish an eligible resident or vehicle; it MUST not expose document values or identities from another condominium.
- **FR-016**: After an attendant selects an eligible resident or vehicle through manual lookup and confirms a direction, the system MUST record an attributed access event identified as manual lookup, with the subject, direction, event time, and recording attendant.
- **FR-017**: The system MUST apply the same eligibility, tenant security, consent, authorization, and audit requirements to manual-lookup access as to QR-based access.
- **FR-018**: The system MUST require every new or updated visitor and service-provider visit to have one active destination apartment and MUST display the corresponding block and unit before the visit is recorded or completed.
- **FR-019**: When an attendant selects a resident in the existing visit interface or in the QR/manual-access interface, the system MUST automatically recover the resident's active apartment and display its block and unit as the destination without requiring duplicate entry.
- **FR-020**: When an attendant selects a vehicle associated with a resident, the system MUST use that resident's active apartment as the destination; when no resident association exists, it MUST use the vehicle's active registered apartment. A destination-dependent event is refused if neither is valid.
- **FR-021**: The system MUST eliminate destination-pending as an option, status, or successful outcome for new and updated visit/access flows. It MUST instead require a valid destination or return a clear correction-required result.
- **FR-022**: The system MUST preserve the apartment, block, and unit attributable to a completed visit or access event so later changes to resident or vehicle registration do not rewrite the recorded destination.

### Key Entities *(include if feature involves data)*

- **Access Credential**: A tenant-scoped authorization assigned to one resident or registered vehicle, with a credential method, lifecycle status, validity period, and auditable issuance/replacement/revocation history. QR code is the only supported method in this feature; facial biometrics is a planned future method.
- **Access Event**: An immutable record of a successful resident or vehicle entrance or exit, including its subject, access method (QR code or manual lookup), direction, event time, and recording attendant.
- **Manual Lookup**: A protected, tenant-scoped way for a gatehouse attendant to identify an eligible resident or registered vehicle by registered document, name, apartment, or block when no QR code can be scanned.
- **Visit Destination**: The active apartment a visitor, service provider, resident, or vehicle access event is associated with, including the block and unit shown at recording time.
- **Refused Scan Attempt**: A tenant-scoped security record for a failed QR scan, retaining a safe failure category and operational context without exposing another tenant's identity.
- **Credential Lifecycle Action**: An attributable action that issues, replaces, revokes, expires, or deactivates an access credential.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Gatehouse attendants can complete a successful resident or vehicle QR scan and receive a final outcome in 3 seconds or less in at least 95% of normal-operation scans.
- **SC-002**: At least 99% of successful QR scans create exactly one correctly attributed, tenant-scoped entrance or exit event.
- **SC-003**: 100% of refused scans and credential lifecycle actions are available to authorized tenant administrators for audit with the required time, actor, and failure or action category.
- **SC-004**: A credential revoked or replaced by an administrator is refused on every subsequent scan within 30 seconds of the lifecycle action being confirmed.
- **SC-005**: In usability validation, at least 90% of trained gatehouse attendants complete both resident and vehicle entrance and exit flows on their first attempt without manual record lookup.
- **SC-006**: No facial biometric data is collected, retained, matched, or used to make an access decision in this release.
- **SC-007**: In at least 95% of normal-operation fallback cases, a trained gatehouse attendant can find an eligible resident or vehicle and record an attributed entrance or exit through manual lookup in 15 seconds or less.
- **SC-008**: 100% of new or updated visits and destination-dependent resident/vehicle access events have a recorded destination apartment, block, and unit; none are shown as destination pending.
- **SC-009**: In usability validation, at least 95% of resident selections in both the existing visit interface and the new QR/manual-access interface display the registered apartment, block, and unit without attendant re-entry.

## Assumptions

- QR codes contain an opaque credential reference rather than personal information, vehicle plate details, or a reusable resident identifier.
- Gatehouse attendants use an approved camera-equipped device or QR scanner connected to the ControlEasy gatehouse interface.
- CPF and other identity documents are already registered only when permitted by the condominium's existing policies; document searches require the complete document value, while name, apartment, and block searches require a sufficiently specific value.
- Tenant administrators determine which residents and registered vehicles are eligible for QR credentials according to their existing condominium policy.
- Every active resident used for a destination-dependent gatehouse flow has one active apartment assignment. Visitors and service providers select an active apartment as their destination; this requirement applies even when their interaction is recorded as gatehouse-only.
- The existing gatehouse audit and consent workflow remains the source of truth for policies that apply to an access attempt; this feature integrates with it rather than replacing it.
- A missed or duplicate physical event must be reviewable but must not automatically trap a resident or vehicle at an access point.
- QR access is a software validation and logging workflow only. Physical gate or door control remains a separate, hardware-gated future integration.
- Facial biometrics is a planned follow-on capability. It requires a separate approved specification covering consent, legal/privacy obligations, enrollment, liveness, accuracy, retention, human fallback, and security before any biometric processing begins.
