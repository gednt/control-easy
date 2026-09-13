# Feature Specification: Photo Upload Reliability and Resident Actions

**Feature Branch**: `fix/photo-upload-resident-menu`

**Created**: 2026-09-13

## User Scenarios & Testing

### User Story 1 - Save a registration photo (Priority: P1)

An authenticated operator uploads an image or captures one with a device camera while managing a resident, vehicle, or service provider. The photo is accepted through the same validated path, appears immediately, and remains associated with that record after a refresh.

**Acceptance Scenarios**:

1. A supported image file can be selected and saved for the active record.
2. A camera capture becomes a valid image upload with the active record's identity.
3. A rejected upload explains the actionable server or validation reason in context.
4. Refreshing the page reloads the saved photos for that record without relying on browser-local associations.

### User Story 2 - Use resident actions confidently (Priority: P1)

An operator can open the resident three-dot menu on desktop or mobile, identify each action, and select it without the menu being obscured or clipped.

**Acceptance Scenarios**:

1. The trigger has a clear touch target, focus state, and sufficient contrast.
2. The menu renders above table and nearby content, with readable spacing and action labels.
3. The menu opens toward available viewport space and remains usable near an edge.

## Functional Requirements

- **FR-001**: Authenticated users with photo permission can upload supported images through multipart requests that retain the target entity type and identity.
- **FR-002**: Camera frames are encoded as JPEG files and use the same validation and upload path as selected files.
- **FR-003**: The system returns and displays useful validation, authorization, size, type, and storage errors without hiding them behind a generic message.
- **FR-004**: Photo records are tenant-scoped, persisted with their entity association, and listed for the correct entity after refresh.
- **FR-005**: Image previews retrieve protected photo content using authenticated application requests.
- **FR-006**: The Resident action menu is visible, un-clipped, keyboard accessible, and works on desktop and touch devices.

## Assumptions

- JPEG, PNG, and WebP remain the server-supported stored formats. Browser capture is always encoded as JPEG.
- Existing photo records without an entity binding remain readable by direct identifier but cannot be inferred safely into a record-specific gallery.
- Existing permissions and server-side validation remain enforced.

## Success Criteria

- A permitted user completes file or camera photo upload and sees it in the active gallery without navigating away.
- Reloading the page shows photos previously saved for each supported entity.
- Rejected uploads show an actionable message for every expected error category.
- Resident menu actions remain visible and selectable at normal desktop and mobile viewport sizes.
