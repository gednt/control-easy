# Feature Specification: Package Drop Tracking

**Feature Branch**: `feat/package-drop-tracking`

**Created**: 2026-09-13

**Status**: Draft

**Input**: User description: "Expand the package drop function so staff can record a short description of each received package, keep an organized view of packages at the entrance lodge, and record what was taken away and by whom."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Register a received package (Priority: P1)

As gatehouse staff, I want to record a package when a delivery is left at the entrance lodge, including a short description and its intended destination, so that every item held at the lodge can be identified.

**Why this priority**: An accurate receipt record is the foundation for knowing what is currently being stored and for making later collection traceable.

**Independent Test**: Staff can complete a package-drop record for a delivery, then find the new item in the held-package list with its identifying details.

**Acceptance Scenarios**:

1. **Given** a service provider has left a package at the lodge, **When** an authorized staff member records the package description and its intended resident or apartment, **Then** the system creates a held-package record with the delivery provider, receipt time, and recording staff member.
2. **Given** staff leave the package description blank or provide neither an intended resident nor an apartment, **When** they attempt to save the receipt, **Then** the system explains what information is required and does not create a package record.
3. **Given** two packages have similar descriptions, **When** staff register both deliveries, **Then** the system keeps them as separate held-package records.
4. **Given** the related package-drop activity cannot be recorded, **When** staff attempt to save a package receipt, **Then** the system does not show either an incomplete package receipt or an unlinked package-drop activity as completed.

---

### User Story 2 - See what remains at the lodge (Priority: P2)

As gatehouse staff, I want a clear list of packages still held at the entrance lodge, so that I can locate a package quickly and keep the physical storage area organized.

**Why this priority**: The inventory view turns individual delivery records into an operational list of items that still need attention.

**Independent Test**: With multiple held and collected packages, staff can open the package list and identify only the held packages, their descriptions, intended destinations, and receipt details.

**Acceptance Scenarios**:

1. **Given** the lodge holds packages for several apartments, **When** staff view the held-package inventory, **Then** it shows each held package's description, intended resident or apartment, delivery provider, receipt time, and recording staff member, with the oldest held packages first.
2. **Given** the held-package inventory contains many items, **When** staff search by description, intended resident, or apartment, **Then** the matching held packages are shown.
3. **Given** an item has been collected, **When** staff view the default held-package inventory, **Then** that item is not shown as still held.

---

### User Story 3 - Record a package collection (Priority: P3)

As gatehouse staff, I want to record when a package leaves the lodge and who collected it, so that the inventory stays accurate and the handover is accountable.

**Why this priority**: A receipt record alone does not establish whether an item is still in the lodge or who received it.

**Independent Test**: Staff can mark a held package as collected, enter the collector's name, and confirm that it leaves the held inventory while remaining visible in package history.

**Acceptance Scenarios**:

1. **Given** a package is currently held at the lodge, **When** an authorized staff member records its collection and the actual collector's name, **Then** the package is marked collected with the collection time, collector, and staff member who recorded the handover.
2. **Given** a package is already marked collected, **When** staff attempt to record another collection, **Then** the system prevents a duplicate handover record.
3. **Given** a package has been collected, **When** authorized staff view package history, **Then** they can see both the receipt details and the collection details without the original record being overwritten.
4. **Given** two staff members try to record collection of the same held package at the same time, **When** both submit the handover, **Then** exactly one collection is recorded and the other staff member is told that the package has already been collected.
5. **Given** the system cannot complete a collection record, **When** staff submit the handover, **Then** the package remains held at the lodge and no partial collection information is shown.

### Edge Cases

- A package may be addressed to an apartment without a known individual recipient; staff must be able to record the apartment alone.
- A package may be collected by someone other than the intended recipient; the actual collector's name is recorded as entered by staff.
- A delivery provider can leave more than one package in the same visit; each package requires its own record.
- A selected resident or apartment that does not belong to the current condominium, or a resident and apartment that do not match each other, must be rejected before receipt is recorded.
- If two collection attempts occur at the same time, only the first completed handover is retained; no second collection history is created.
- A failed save must leave no partial held-package record visible in the inventory.
- Records from one condominium must never be visible to staff from another condominium.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow authorized gatehouse staff to create one package record for each package left at the entrance lodge during the existing package-drop flow.
- **FR-002**: Each package record MUST require a short, non-blank package description of no more than 160 characters.
- **FR-003**: Each package record MUST identify at least one intended destination: a resident, an apartment, or both. Any selected resident or apartment MUST belong to the current condominium; when both are selected, the resident MUST belong to that apartment.
- **FR-004**: Each package record MUST retain the associated delivery provider, receipt date and time, and the authenticated staff member who recorded its receipt.
- **FR-005**: A newly received package MUST have the status **Held at lodge** and appear in the held-package inventory until it is collected.
- **FR-006**: The held-package inventory MUST show the package description, intended destination, delivery provider, receipt date and time, and recording staff member; it MUST place the oldest held packages first by default.
- **FR-007**: Authorized staff MUST be able to search the held-package inventory by package description, intended resident, or apartment.
- **FR-008**: The system MUST allow authorized gatehouse staff to record collection only for a package currently held at the lodge.
- **FR-009**: Recording a collection MUST require the actual collector's name and retain the collection date and time plus the authenticated staff member who recorded the handover.
- **FR-010**: Once collected, a package MUST leave the held-package inventory and remain available in package history with both its receipt and collection information.
- **FR-011**: The system MUST prevent a package from being collected more than once. When concurrent collection attempts are made for the same held package, exactly one attempt MUST create the collection record; every other attempt MUST be rejected without changing the successful record.
- **FR-012**: The system MUST preserve package receipt and collection records as an auditable history; users must not be able to delete a record or overwrite receipt details through this feature.
- **FR-013**: The system MUST restrict package records and package history to the current condominium and to users authorized for gatehouse operations.
- **FR-014**: Every package receipt MUST be linked to its originating package-drop activity. The receipt and its originating package-drop activity MUST either both be recorded as completed or neither be shown as completed, so the inventory and gatehouse history cannot diverge.
- **FR-015**: A package collection MUST update the package's held status and create its immutable collection history as one all-or-nothing outcome. If the collection cannot be fully recorded, the package MUST remain held at the lodge and no collection history may be shown.

### Key Entities *(include if feature involves data)*

- **Package record**: One physical package received at the entrance lodge, including its short description, intended resident and/or apartment, delivery provider, receipt details, and current status.
- **Package collection**: The recorded handover of a held package, including the actual collector, collection time, and staff member who recorded it.
- **Held-package inventory**: The operational view of package records that have been received but not yet collected.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Gatehouse staff can register a received package with its description and intended destination in 60 seconds or less in a usability test.
- **SC-002**: In a test set of 100 held packages, staff can locate a requested package by its description, intended resident, or apartment within 30 seconds in at least 95% of attempts.
- **SC-003**: Immediately after a recorded collection, 100% of the collected test packages are absent from the held-package inventory and visible in package history with a collector and collection time.
- **SC-004**: In acceptance testing, 100% of attempted duplicate collections are rejected without changing the original collection record.
- **SC-005**: In tenant-isolation testing, staff can view package information only for their own condominium in 100% of tested requests.
- **SC-006**: In 20 simultaneous-collection test runs, each package has exactly one recorded handover and no duplicate collection history.

## Assumptions

- The existing package-drop action remains the point at which a staff member records a package's arrival; this feature adds package inventory and handover tracking to that operation.
- Gatehouse staff and tenant administrators who already have gatehouse-operation access are the initial users of this feature; resident self-service package views and notifications are outside this scope.
- The intended recipient is selected when known; recording an apartment alone is sufficient when the package does not identify a resident.
- The collector's name is captured by the staff member handling the physical handover. Signatures, identity-document scans, and photographic proof are outside this scope.
- Each physical package is recorded separately, including packages delivered together by the same provider.
- Existing historical package-drop entries remain unchanged; detailed package tracking begins for packages recorded after this feature is introduced.
