# Tasks: Dashboard Visits and Access Registration

## Phase 1: Shared visit workflow

- [x] T001 [US1] Create the shared visit modal in `src/Web/ControlEasyReborn.Web/src/app/features/visits/visit-create-modal.component.ts`.
- [x] T002 [US1] Use the shared modal from `src/Web/ControlEasyReborn.Web/src/app/features/visits/visits.page.ts`.
- [x] T003 [US1] Add the shared modal to `src/Web/ControlEasyReborn.Web/src/app/features/dashboard/dashboard.page.ts` and refresh dashboard data after creation.

## Phase 2: Resident and vehicle access movements

- [x] T004 [US2] Add the `exited` event state and validation in `src/Modules/Photos/ControlEasyReborn.Modules.Photos.Domain/Entities/ConsentAuditLog.cs`, `Application/Handlers/EntryLogHandlers.cs`, and `Application/Validators/EntryLogRequestValidator.cs`.
- [x] T005 [US2] Prevent access audit records from being copied into `Visits` in `src/Modules/Photos/ControlEasyReborn.Modules.Photos.Infrastructure/Persistence/ConsentAuditLogRepository.cs`.
- [x] T006 [US2] Add the exit choice and resident/vehicle-only selection to `src/Web/ControlEasyReborn.Web/src/app/design-system/components/entry-workflow/entry-workflow.component.ts`.
- [x] T007 [US2] Test exit validation in `tests/ControlEasyReborn.UnitTests/Modules/Photos/EntryLogHandlersTests.cs`.

## Phase 3: Audit visibility and verification

- [x] T008 [US3] Render and filter exit records in `src/Web/ControlEasyReborn.Web/src/app/design-system/components/entry-state-badge/entry-state-badge.component.ts` and `audit-filters.component.ts`.
- [x] T009 [US3] Extend the entry workflow and entry-state badge tests in their corresponding `.spec.ts` files.
- [x] T010 Run the Docker Compose build, health check, and all required test suites.

## Phase 4: Resident lookup during access registration

- [x] T011 [US4] Add CPF and resident-ID lookup, active-result selection, and record prefill to `src/Web/ControlEasyReborn.Web/src/app/design-system/components/entry-workflow/entry-workflow.component.ts` using `features/residents/residents-api.service.ts`.
- [x] T012 [US4] Add Angular tests for CPF lookup, resident-ID lookup, selection, and no-result feedback in `src/Web/ControlEasyReborn.Web/src/app/design-system/components/entry-workflow/entry-workflow.component.spec.ts`.
- [x] T013 Run the Docker Compose rebuild, health check, and affected test suites.
