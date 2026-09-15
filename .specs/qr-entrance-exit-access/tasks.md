# Tasks: Gatehouse Access and Visit Destinations

**Input**: Design documents from `.specs/qr-entrance-exit-access/`
**Branch**: `feat/qr-entrance-exit-access` | **Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)
**Data Model**: [data-model.md](data-model.md) | **Contracts**: [contracts/access-api.md](contracts/access-api.md)

**Prerequisites**: plan.md (✔), spec.md (✔), research.md (✔), data-model.md (✔), contracts/access-api.md (✔), checklists/requirements.md (✔ — 16/16 PASS)

**Constitution**: `.specify/memory/constitution.md` v1.3.0
- I. Spec-Driven Development: every task references a feature requirement (FR-001 .. FR-022).
- II. Multi-Tenant Isolation: every new entity carries `TenantId`/`tenant_id`; queries use `ITenantAwareLinqFactory`.
- III. LINQ-First Data Access: only the manual-lookup composition uses joined reads via narrow directory ports (no raw SQL introduced).
- IV. Test-First: every implementation task has a `[VERIFY]` line stating Docker rebuild + `dotnet test`/Angular test slice.
- V. Observability/Security/Container Parity: RFC 7807 + Serilog; no facial data, no plaintext QR; physical gate control is NOT implemented.
- VI. Workflow: spec-kit owns tasks.md; GSD owns STATE.md/PROJECT.md updates (handled out of band).

**Tests**: This spec explicitly requires tests (FR-002..022 obligate enforcement; agents mandate test-first per Constitution IV). Test tasks are interleaved before implementation tasks inside each user story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: parallelizable (different files, no transitive deps).
- **[Story]**: which user story the task belongs to (US1..US6); Setup and Foundational phases have NO story label.
- File paths are absolute under the worktree root.
- Every implementation task carries a `[VERIFY]` step.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project-init scaffolding for a new `AccessControl` module without disturbing existing ones.

- [x] T001 Create AccessControl module skeleton under `src/Modules/AccessControl/` with four projects (`ControlEasyReborn.Modules.AccessControl.{Domain,Application,Infrastructure,Api}.csproj`) wired into `src/ControlEasyReborn.sln`, `src/Directory.Build.props`, and `src/Directory.Packages.props`. [VERIFY] `dotnet sln src/ControlEasyReborn.sln list` enumerates the four new projects; `dotnet build src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Domain/ControlEasyReborn.Modules.AccessControl.Domain.csproj` succeeds in the `api` container.
- [x] T002 [P] Add the four AccessControl projects to `src/Host/ControlEasyReborn.Api/ControlEasyReborn.Api.csproj` as `ProjectReference` items and to the central package version list (`DBTools 1.4.3`, `FluentValidation`, `Mapster`, `Serilog.Extensions.Hosting`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Swashbuckle.AspNetCore.Annotations`). [VERIFY] `dotnet build src/ControlEasyReborn.sln` inside `api` container.
- [x] T003 [P] Add `docker/mysql/init/12-access-control-schema.sql` (fresh-schema DDL for `access_credentials`, `credential_lifecycle_actions`, `access_events`, `refused_scan_attempts`, `access_lookup_audits`) and `docker/mysql/migrations/0009-access-control.sql` (idempotent live-migration mirror). [VERIFY] `docker exec ce-feat-qr-entrance-exit-access-db-1 mysql -uroot -p$MYSQL_ROOT_PASSWORD ControlEasyReborn < docker/mysql/migrations/0009-access-control.sql` runs cleanly against the worktree db.
- [x] T004 [P] Wire `AccessControl.Api.EndpointRegistration` in `src/Host/ControlEasyReborn.Api/Program.cs` (placeholder `MapGroup("/api/v1/access-credentials")` returns 404 until real endpoints land). [VERIFY] `docker compose up -d --force-recreate api` logs the new `AccessControl` module; `curl -i https://ce-feat-qr-entrance-exit-access.localhost/api/v1/access-credentials` returns 404 from authenticated profile.
- [x] T005 [P] Add `Access.Control.Issue`, `Access.Control.Replace`, `Access.Control.Revoke`, `Access.Access.Operate`, `Access.Read` permission constants to `src/BuildingBlocks/ControlEasyReborn.BuildingBlocks.Authorization/Permissions.cs` and seed into role grant table via existing `docker/mysql/init/05-security-schema.sql` merge snippet. [VERIFY] Integration test enumerates effective permissions per role.
- [x] T006 [P] Generate Angular workspace placeholders `src/Web/ControlEasyReborn.Web/src/app/features/access-control/{access-credentials.page.ts,access-events-review.page.ts,gateway-control.service.ts}` and `src/Web/ControlEasyReborn.Web/src/app/api/access-control.types.ts` (regenerated OpenAPI facade). [VERIFY] `cd src/Web/ControlEasyReborn.Web && ng build` succeeds.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain primitives, source-model extensions, and tenant-aware data-access wiring that every user story depends on. **No user story can begin until this phase completes.**

- [ ] T007 [P] Create entities in `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Domain/Entities/AccessCredential.cs` (with `Status`, `SubjectType`, `SubjectId`, `SecretVerifier`, `KeyVersion`, `ValidFromUtc`, `ExpiresAtUtc`, `ReplacedByCredentialId`, `IssuedByProfileId`, dual `TenantId`/`tenant_id`). [VERIFY] Domain unit tests in `tests/ControlEasyReborn.UnitTests/Modules/AccessControl/Entities/AccessCredentialTests.cs` assert lifecycle state-machine guard.
- [ ] T008 [P] Create `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Domain/Entities/CredentialLifecycleAction.cs`, `AccessEvent.cs`, `RefusedScanAttempt.cs`, `AccessLookupAudit.cs` matching [data-model.md](data-model.md) field rules. [VERIFY] Unit tests assert append-only invariants (`AppendOnlyBaseEntity.Check_No_Mutation`).
- [ ] T009 [P] Define value objects in `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Domain/ValueObjects/{CredentialMethod,CycleDirection,AccessMethod,SubjectType,LookupCriterionType,ResultCountBand}.cs` with parsing/validation helpers (`TryParse`, safe error codes). [VERIFY] Unit tests in `tests/ControlEasyReborn.UnitTests/Modules/AccessControl/ValueObjects/*Tests.cs`.
- [ ] T010 [P] Define domain exceptions `CredentialAlreadyActiveException`, `CredentialLifecycleConflictException`, `ManualLookupNotFoundException`, `AccessDestinationRequiredException`, `AccessDuplicateConfirmationRequiredException` in `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Domain/Errors/`. [VERIFY] xUnit tests assert each exception maps to the RFC 7807 code in `Application` ProblemDetails builder.
- [ ] T011 [P] Add `ResidentIdentityDocument` entity to `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Domain/Entities/ResidentIdentityDocument.cs` and migration to `docker/mysql/migrations/0008-resident-identity-documents.sql`. [VERIFY] Migration applies cleanly; arch test confirms `tenant_id` presence.
- [ ] T012 [P] Add nullable `OwnerResidentId` column to `src/Modules/Vehicles/ControlEasyReborn.Modules.Vehicles.Domain/Entities/Vehicle.cs` and migration `docker/mysql/migrations/0008-vehicle-owner-resident.sql` with FK to residents within tenant. [VERIFY] Migration applies; existing vehicle seed remains valid.
- [ ] T013 [P] Make `Visit.ApartmentId` non-null at create/update through `VisitValidator` and add immutable `DestinationBlock`/`DestinationUnit` snapshot fields; add migration `docker/mysql/migrations/0008-visit-destination-snapshot.sql` to backfill snapshot from current apartment. [VERIFY] Backfill is idempotent; legacy rows are unchanged.
- [ ] T014 [P] Implement directory ports in `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Application/Abstractions/IResidentDirectory.cs` (methods: `FindActiveByCpfAsync`, `SearchByNameAsync`, `SearchByApartmentAsync`, `SearchByDocumentAsync`, `GetActiveIdentityDocumentsAsync`). [VERIFY] Unit tests with NSubstitute verify contract.
- [ ] T015 [P] Implement directory ports in `src/Modules/Vehicles/ControlEasyReborn.Modules.Vehicles.Application/Abstractions/IVehicleDirectory.cs` and `src/Modules/Apartments/ControlEasyReborn.Modules.Apartments.Application/Abstractions/IApartmentDirectory.cs`. [VERIFY] Unit tests with NSubstitute verify contract.
- [ ] T016 [P] Implement `IConsentPolicyEvaluator` port in `src/Modules/Photos/ControlEasyReborn.Modules.Photos.Application/Abstractions/IConsentPolicyEvaluator.cs` (`EvaluateAsync(subjectType, subjectId, action)` returns `Permitted | RequiresAction | Refused`). [VERIFY] Unit tests cover all three outcomes.
- [ ] T017 Implement opaque-token issuer in `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Infrastructure/Services/OpaqueTokenIssuer.cs` using `RandomNumberGenerator` (≥128 bits) and keyed one-way verifier via built-in `System.Security.Cryptography.HMACSHA256`. Persist verifier only; never raw token. [VERIFY] Unit tests verify deterministic verifier output for same input + key; raw token never appears in logs.
- [ ] T018 Implement `ITenantAwareLinqFactory`-backed repositories in `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Infrastructure/Persistence/{AccessCredentialRepository,AccessEventRepository,RefusedScanAttemptRepository,AccessLookupAuditRepository}.cs`. [VERIFY] All four include `netarchtest` rule for `Modules.AccessControl.Infrastructure` → `ITenantAwareLinqFactory` use.
- [ ] T019 Implement DBTools model classes in `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Infrastructure/Persistence/Models/{AccessCredentialModel,CredentialLifecycleActionModel,AccessEventModel,RefusedScanAttemptModel,AccessLookupAuditModel}.cs` matching `tenant_id`/`TenantId` dual column and DBTools 1.4.3 model conventions. [VERIFY] `dotnet build src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Infrastructure` succeeds.
- [ ] T020 Implement `AccessControlAccessModule` registration in `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Api/DI/ModuleRegistration.cs`. Register repositories, validators, handlers, and endpoint mappers as scoped services. [VERIFY] `dotnet build src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Api` succeeds.
- [x] T021 Add architecture test in `tests/ControlEasyReborn.ArchitectureTests/AccessControlTenantRulesTests.cs` enforcing (a) `tenant_id` on every new entity, (b) no cross-module raw SQL, (c) no `MySql.Data` references in AccessControl, (d) no `facial_biometric`/`biometric_template` columns/strings in compiled model or schema. [VERIFY] `dotnet test tests/ControlEasyReborn.ArchitectureTests` passes.
- [x] T022 Add infrastructure-test container bootstrap in `tests/ControlEasyReborn.IntegrationTests/AccessControlFixture.cs` using Testcontainers.MySql 8 with the new migration file preloaded; expose seed helpers (`SeedActiveResident`, `SeedActiveVehicle`, `SeedActiveApartment`). [VERIFY] `dotnet test tests/ControlEasyReborn.IntegrationTests --filter AccessControlFixture` passes fixture-only smoke.

**Checkpoint**: Foundation ready — user story implementation can begin in parallel.

---

## Phase 3: User Story 1 — Record resident access by QR code (Priority: P1) 🎯 MVP

**Goal**: Gatehouse attendants can scan an active resident QR code for entrance or exit and receive an attributed access event with auto-recovered destination, or a refused-attempt record with a safe reason.

**Independent Test**: Inside `api` container, seed resident + active apartment + active credential. POST `/api/v1/access-events/scans` twice (entrance, exit) with distinct `scanAttemptId` values. Assert two correctly attributed immutable events, distinct credential reuse, destination block/unit snapshot matches apartment. Repeat with revoked credential and assert `RefusedScanAttempt` row + safe failure code; no cross-tenant disclosure.

### Tests for User Story 1 (write FIRST, observe RED)

- [ ] T023 [P] [US1] Contract/integration test `tests/ControlEasyReborn.IntegrationTests/AccessControl/QrScanEndpointsTests.cs` for `POST /api/v1/access-events/scans`: resident QR success path, refusal path, duplicate-`scanAttemptId` idempotency, cross-tenant refusal. [VERIFY] `dotnet test --filter QrScanEndpoints` returns 4 failing tests before implementation.
- [ ] T024 [P] [US1] Unit tests `tests/ControlEasyReborn.UnitTests/Modules/AccessControl/Application/Handlers/RecordAccessScanHandlerTests.cs` for handler invariants: subject eligibility, credential status, destination resolution, idempotency, duplicate-warning, refusal codes. [VERIFY] `dotnet test --filter RecordAccessScanHandler` returns failing tests.
- [ ] T025 [P] [US1] Angular unit tests `src/Web/ControlEasyReborn.Web/src/app/features/entry-workflow/entry-workflow.page.spec.ts` for QR scan tab: requests scan, displays outcome (recorded/duplicate/refused/unavailable), never shows raw QR. [VERIFY] `npm test -- --no-watch --browsers=ChromeHeadless --include=**/entry-workflow*.spec.ts` fails pre-implementation.

### Implementation for User Story 1

- [ ] T026 [P] [US1] Implement command/handler `RecordAccessScanHandler` in `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Application/Handlers/RecordAccessScanHandler.cs` with idempotent `(TenantId, ScanAttemptId)` lookup, credential resolve, subject eligibility, destination snapshot, policy outcome, append-only AccessEvent + RefusedScanAttempt writes. [VERIFY] T024 tests pass; handler returns discriminated `ScanDecision` (`Recorded | DuplicateConfirmationRequired | PolicyActionRequired | Refused | Unavailable`).
- [ ] T027 [P] [US1] Implement FluentValidation validators for `RecordAccessScanCommand`, `LookupSubjectCommand`, `RecordManualAccessCommand`, `IssueCredentialCommand`, `ReplaceCredentialCommand`, `RevokeCredentialCommand` under `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Application/Validators/`. [VERIFY] `dotnet test --filter Validators` passes.
- [ ] T028 [P] [US1] Implement Mapster mappings `AccessCredentialMappings` for create/read DTOs under `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Application/Mappings/`. [VERIFY] Unit mapping tests snapshot equality.
- [ ] T029 [US1] Implement `AccessCredentialsEndpoints` + `AccessEventsEndpoints` in `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Api/Endpoints/{AccessCredentialsEndpoints,AccessEventsEndpoints}.cs` with `POST /access-events/scans`, `GET /access-credentials/{id}`, full RFC 7807 mapping, `[Authorize(Policy=...)]` checks, and tenant claim guard. [VERIFY] `dotnet build src/Modules/AccessControl.ControlEasyReborn.Modules.AccessControl.Api` succeeds; T023 tests pass.
- [ ] T030 [P] [US1] Implement Angular QR-scan tab UI in `src/Web/ControlEasyReborn.Web/src/app/features/entry-workflow/entry-workflow.page.ts` (resident/vehicle/visitor tabs; QR scanner input via `getUserMedia` + `jsQR`); wire to regenerated API facade `access-control.service.ts`. Never persist raw QR. Display result kinds per [contracts/access-api.md](contracts/access-api.md). [VERIFY] `npm run build` succeeds; T025 tests pass.
- [ ] T031 [P] [US1] Implement Angular `AccessScanResultComponent` (presentational) showing recorded/duplicate/refused/unavailable outcomes with safe refusal code, dest block+unit when available. OnPush, signals-based. [VERIFY] Angular unit tests pass; a11y checks (aria-live polite).
- [ ] T032 [US1] Rebuild + smoke: `docker compose -p ce-feat-qr-entrance-exit-access -f docker/docker-compose.yml build api web && up -d --force-recreate api web`; curl the scan endpoint with a real seeded token, observe healthy services. [VERIFY] `docker compose ps` shows api/web healthy; container logs include no raw QR.

**Checkpoint**: At this point, User Story 1 is fully functional and independently testable in the `api` container + Angular dev server.

---

## Phase 4: User Story 2 — Record vehicle access by QR code (Priority: P1)

**Goal**: Vehicle QR scans record an attributed event with resolved destination (vehicle's verified resident owner first, vehicle's registered apartment second), refuse deactivated/cross-tenant vehicles.

**Independent Test**: Seed active vehicle + verified `OwnerResidentId`. Scan for entrance and exit. Assert two events tagged `subjectType=vehicle`; if owner exists, destination matches resident's apartment, not the vehicle's, when they differ; deactivated vehicle is refused with safe code; cross-tenant vehicle never reveals identity.

### Tests for User Story 2

- [ ] T033 [P] [US2] Integration test `tests/ControlEasyReborn.IntegrationTests/AccessControl/VehicleScanEndpointsTests.cs` for `POST /access-events/scans` when subject is vehicle (active, deactivated, cross-tenant, owner-resident-apartment precedence). [VERIFY] failing-then-green after T034/T035.
- [ ] T034 [P] [US2] Unit tests `tests/ControlEasyReborn.UnitTests/Modules/AccessControl/Application/AccessEventDestinationResolverTests.cs` validating precedence (owner-resident > vehicle-apartment), refusal on missing both, snapshot immutability across changes. [VERIFY] failing-then-green after implementation.

### Implementation for User Story 2

- [ ] T035 [US2] Extend `RecordAccessScanHandler` (T026) with vehicle-specific destination resolution via `IVehicleDirectory`. Refuse `vehicle_inactive`, `destination_required`, `destination_inactive`, `not_authorized`. [VERIFY] T033/T034 tests pass.
- [ ] T036 [P] [US2] Add `OwnerResidentId` validation in `VehicleValidator` to reject cross-tenant resident owner. [VERIFY] Unit + integration assertions hold.
- [ ] T037 [P] [US2] Extend Angular `EntryWorkflowPage` so a scanned vehicle never shows owner identity beyond what tenant policy displays; vehicle record includes plate (masked if the policy evaluator demands it). [VERIFY] Angular unit tests + a11y snapshot.

**Checkpoint**: US1 and US2 both work independently for QR scans.

---

## Phase 5: User Story 3 — Find and record access without a QR code (Priority: P1)

**Goal**: Protected manual lookup (CPF, identity document, name, apartment, block). Searches mask documents, enforce minimum specificity on name/apartment/block, return capped tenant-local results, and persist `AccessLookupAudit` (criterion type + result band only). Recording a manual event is bound to a tenant-local authorized lookup.

**Independent Test**: For each criterion type, attempt valid searches → expected results, attempt underspecific queries → 400, attempt cross-tenant searches → no identity disclosure, bind a recorded event to a valid lookup, refuse `manual_event_orphan_lookup_id`.

### Tests for User Story 3

- [ ] T038 [P] [US3] Integration test `tests/ControlEasyReborn.IntegrationTests/AccessControl/ManualLookupEndpointsTests.cs` — covers all 5 criterion types + cross-tenant rejection + lookup-audit record shape. [VERIFY] failing then green.
- [ ] T039 [P] [US3] Unit tests `tests/ControlEasyReborn.UnitTests/Modules/AccessControl/Application/Handlers/LookupSubjectHandlerTests.cs` + `RecordManualAccessHandlerTests.cs`. [VERIFY] failing then green.

### Implementation for User Story 3

- [ ] T040 [US3] Implement `LookupSubjectHandler` in `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Application/Handlers/LookupSubjectHandler.cs`. Enforces: CPF/document must be complete normalized values; name/apartment/block rejected with `search_too_broad` if below min-specificity; result count capped (default 25); document values masked in response; writes `AccessLookupAudit` (criterion type + result band). [VERIFY] T038/T039 tests pass.
- [ ] T041 [US3] Implement `RecordManualAccessHandler` in `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Application/Handlers/RecordManualAccessHandler.cs`. Verifies lookup belongs to tenant + actor, subject is present in lookup result, runs the same eligibility + destination + policy checks as QR flow, sets `AccessMethod=manual_lookup`. [VERIFY] T038/T039 tests pass.
- [ ] T042 [P] [US3] Add `POST /access-subjects/search` and `POST /access-events/manual` endpoints to `AccessEventsEndpoints` with RFC 7807 + safe refusal codes. [VERIFY] endpoints return correct codes.
- [ ] T043 [P] [US3] Add Angular `ManualLookupPanelComponent` under `features/entry-workflow/manual-lookup/` (CPF/doc/name/apartment/block tabs, validation, masking, scoped results, selection binding). [VERIFY] Angular unit tests + a11y; `ng-openapi-gen` regenerates the API types.
- [ ] T044 [US3] Rebuild + smoke per [docker/mysql migrations](#) and integration-test slice. [VERIFY] US3 end-to-end works.

**Checkpoint**: US1, US2, US3 each work independently; lookup never discloses across tenants and never reveals full documents.

---

## Phase 6: User Story 4 — Always identify a visit destination (Priority: P1)

**Goal**: Every new or updated visit and resident/vehicle access event has a recorded destination apartment, block, and unit. Resident/vehicle selections auto-recover it. `destination_pending` is removed everywhere.

**Independent Test**: Existing visit create/update API requires `apartmentId`; block+unit are server-populated snapshots. Existing visit UI auto-recovers and displays the resident's destination. New QR + manual flows resolve the same destination. Submitting with no active destination returns `destination_required` or `destination_inactive` ProblemDetails and never creates a `destination_pending` record.

### Tests for User Story 4

- [ ] T045 [P] [US4] Integration test `tests/ControlEasyReborn.IntegrationTests/AccessControl/VisitDestinationValidationTests.cs` — visit create/update missing apartment, inactive apartment, cross-tenant apartment all rejected; legacy non-destination rows treated as historical; resident/vehicle selections in the access flow auto-resolve. [VERIFY] failing then green.
- [ ] T046 [P] [US4] Angular tests for visit create/update form: apartment dropdown required, block+unit shown read-only after selection, error message when apartment missing/inactive, resident auto-recovery. [VERIFY] Jasmine + Karma pass.

### Implementation for User Story 4

- [ ] T047 [P] [US4] In `src/Modules/Visits/ControlEasyReborn.Modules.Visits.Application/Validators/VisitValidator.cs`, make `ApartmentId` required and reject `Inactive`/`OtherTenant` apartments; populate `DestinationBlock`/`DestinationUnit` snapshot from the resolved apartment. Remove `DestinationPending` enum/status. [VERIFY] Unit + integration assertions.
- [ ] T048 [US4] Introduce `AccessEventDestinationResolver` consumed by `RecordAccessScanHandler`, `RecordManualAccessHandler`, and `VisitService` so resident/vehicle destination logic is single-source-of-truth. Implements owner-resident-first precedence and snapshot capture. [VERIFY] T034 tests still pass; new resolver tests pass.
- [ ] T049 [US4] Search the Visits module domain/application/api for `DestinationPending` strings and remove any consumer path; sweep existing web visit pages for `destination_pending` rendering. [VERIFY] `grep` returns no matches; integration tests assert no `Pending` is reachable.
- [ ] T050 [P] [US4] Update Angular visit form `features/visits/visit-create/visit-create.page.ts` and `entry-workflow` to surface auto-resolved destination block+unit; never let user edit them directly. [VERIFY] T046 tests pass; a11y snapshot.
- [ ] T051 [US4] Rebuild + smoke + tests for both US1 and Visits. [VERIFY] QrScan + ManualLookup + VisitDestination test slices all green.

**Checkpoint**: US1..US4 together demonstrate QR scan, manual lookup, and required destination recovery end-to-end.

---

## Phase 7: User Story 5 — Manage and audit QR credentials (Priority: P2)

**Goal**: Tenant administrators issue, replace, revoke, deactivate, and review QR credentials. Revocations take effect on the next scan within 30s. Audit reviews by date range, direction, subject, credential status, attendant.

**Independent Test**: Issue a credential → use it (success) → replace it → attempt predecessor scan (refused, record the new one). Then revoke → scan attempt refused. Audit UI filters by date/direction/subject/credential status/attendant; includes a refused-attempt lens. Lifecycle actions retain actor/time/reason.

### Tests for User Story 5

- [ ] T052 [P] [US5] Integration test `tests/ControlEasyReborn.IntegrationTests/AccessControl/CredentialLifecycleEndpointsTests.cs`: issue, replace (predecessor unusable, successor active, both events recorded), revoke (subsequent scan refused within 30s), deactivate flow, list + filter by all axes. [VERIFY] failing then green.
- [ ] T053 [P] [US5] Unit tests for `IssueCredentialHandler`, `ReplaceCredentialHandler`, `RevokeCredentialHandler` and the `CredentialStatusPolicy` enforcing valid transitions. [VERIFY] Unit tests pass.
- [ ] T054 [P] [US5] Angular tests for `access-control/admin/access-credentials.page.spec.ts` (issue, replace, revoke dialogs, lists, audit lens). [VERIFY] Angular tests pass.

### Implementation for User Story 5

- [ ] T055 [US5] Implement `IssueCredentialHandler`, `ReplaceCredentialHandler`, `RevokeCredentialHandler` in `src/Modules/AccessControl/ControlEasyReborn.Modules.AccessControl.Application/Handlers/CredentialLifecycleHandlers.cs`. Enforce one-active-credential-per-subject invariant, append `CredentialLifecycleAction`, atomic replace, rejection of duplicate active credential, revocation idempotency. [VERIFY] T052/T053 tests pass.
- [ ] T056 [P] [US5] Add `POST /access-credentials`, `POST /access-credentials/{id}/replace`, `POST /access-credentials/{id}/revoke`, `GET /access-credentials` endpoints to `AccessCredentialsEndpoints`. Never expose raw QR in any list/read response. [VERIFY] Integration test slice.
- [ ] T057 [P] [US5] Add `GET /access-events` and `GET /access-events/refused-attempts` endpoints supporting filters by `fromUtc`, `toUtc`, `direction`, `subjectType`, `subjectId`, `credentialStatus`, `gatehouseId`, `attendantProfileId`. Always mask QR and document values. [VERIFY] endpoint tests pass.
- [ ] T058 [P] [US5] Angular pages `access-credentials.page.ts` and `access-events-review.page.ts` (filter chips, paginated tables, dialogs for issue/replace/revoke; detail dialog); OnPush + signals + a11y. [VERIFY] T054 tests pass; ng-openapi-gen regenerates DTOs.
- [ ] T059 [US5] Update `RolePermissionSeeder` with `Access.Control.*` and `Access.Read` grants for `TenantAdmin` and operator role per plan.md. [VERIFY] integration assertion that role resolves to expected permissions.
- [ ] T060 [US5] Rebuild + smoke + run `quickstart.md` step 5 manually-equivalent. [VERIFY] all credential lifecycle slices green.

**Checkpoint**: Credential lifecycle management is administered; predecessors unrevocable within testable SLA.

---

## Phase 8: User Story 6 — Preserve a future facial-biometric path (Priority: P3)

**Goal**: Reserve `facial_biometric` as a credential-method vocabulary value, but ship no enrollment, template storage, matching, liveness, endpoint, or log field for it.

**Independent Test**: Scan a QR code and verify the event's `credentialMethod` is `qr`. Inspect credential admin UI and OpenAPI spec for any biometric field — none should exist. Static analysis: `grep -r 'biometric' src/Modules/AccessControl/` returns the reserved-enum constant only; no `template`/`embedding`/`liveness` symbols. Architecture test fails the build if a `facial_biometric` column or field is added.

### Tests for User Story 6

- [ ] T061 [P] [US6] Arch test in `tests/ControlEasyReborn.ArchitectureTests/AccessControlBiometricExclusionTests.cs`: forbids `facial_biometric` DB columns, biometric template fields in entities, biometric endpoints, biometric service registrations. [VERIFY] build fails if intentional or accidental biometrics are introduced.
- [ ] T062 [P] [US6] Schema test asserting no `biometric_*` table exists in `docker/mysql/init/12-access-control-schema.sql` nor `docker/mysql/migrations/0009-access-control.sql`. [VERIFY] grep/find returns zero matches.

### Implementation for User Story 6

- [ ] T063 [US6] Add reserved value `facial_biometric = 99` to `CredentialMethod` enum with `IsReservedForFuture` flag; ensure no API surface creates credentials with that method. [VERIFY] unit tests reject any attempt to create such a credential.
- [ ] T064 [P] [US6] Add Swagger filter to exclude biometric endpoints/schemas from the OpenAPI document. [VERIFY] OpenAPI document contains no biometric route or schema.
- [ ] T065 [US6] Document the reservation in `docs/access-control.md` (new) and link from `spec.md` & `plan.md`; update `quickstart.md` step 7 to mention the exclusion scan. [VERIFY] docs build cleanly; quickstart validation passes.

**Checkpoint**: No biometric path shipped; only the reserved vocabulary remains.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Hardening, performance, observability, and end-to-end validation.

- [ ] T066 [P] Add Serilog enrichers + structured log fields for AccessControl (`TenantId`, `ProfileId`, `ScanAttemptId`, decision kind) while **never** logging raw QR, raw documents, raw names, or full CPFs. [VERIFY] grep over log calls; integration snapshot.
- [ ] T067 [P] Add `docker compose` worktree override `docker/docker-compose.worktree.feat-qr-entrance-exit-access.yml` (port shift +80, DB volume `ce-feat-qr-entrance-exit-access-mysql-data`, project name already matches). [VERIFY] `docker compose -p ce-feat-qr-entrance-exit-access -f docker/docker-compose.yml config` resolves cleanly.
- [ ] T068 [P] Performance sweep: ensure `RecordAccessScanHandler` cold-path latency p95 ≤ 3s, manual lookup p95 ≤ 15s in a Testcontainer run. Add `Stopwatch`+Serilog timing on each handler boundary. [VERIFY] integration perf test slices pass.
- [ ] T069 [P] Angular `entry-workflow` a11y + i18n audit: keyboard navigability, ARIA roles, focus management on result cards, screen-reader announcements for live region. [VERIFY] `npm run lint && npm run e2e` passes.
- [ ] T070 [P] Playwright E2E `tests/e2e/access-control.spec.ts`: issue → scan entrance → scan exit → revoke → refused scan; manual lookup by CPF/document/name/apartment/block; visit destination recovery; credential administration dialogs. [VERIFY] `npm run e2e` passes against `ce-feat-qr-entrance-exit-access.localhost`.
- [ ] T071 [P] Architecture-wide test ensuring AccessControl module respects all seven constitution principles (architecture rules: no cross-module raw SQL, module layering, tenant_id everywhere, no biometric, etc.). [VERIFY] `dotnet test tests/ControlEasyReborn.ArchitectureTests` passes.
- [ ] T072 [P] Update `.planning/PROJECT.md` and `.planning/STATE.md` per spec-kit → GSD ownership rule (spec-kit writes `STATE.md` decision rows; GSD owns roadmap pointer). [VERIFY] diff is minimal, additive only.
- [ ] T073 Run `quickstart.md` end-to-end in devcontainer — issue → scan → revoke → re-scan → manual lookup → cross-tenant → policy handoff → exclusions. [VERIFY] quickstart validation checklist all PASS.
- [ ] T074 Commit and tag: `chore(qr-entrance): post-implementation docs and release notes`. [VERIFY] `git log --oneline feat/qr-entrance-exit-access..HEAD` shows clean atomic commits.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: no dependencies.
- **Phase 2 (Foundational)**: depends on Phase 1; **BLOCKS all user stories**.
- **Phase 3..8**: depend on Phase 2 completion.
  - All P1 phases (US1, US2, US3, US4) are mutually independent after Foundational.
  - US5 depends on US1 (audit reads events).
  - US6 is documentation/architecture-only but depends on the entity model from US1.
- **Phase 9 (Polish)**: depends on US1..US6 completion.

### User Story Dependencies

- US2 (P1) — can start after Phase 2. Builds on US1's `RecordAccessScanHandler`.
- US3 (P1) — can start after Phase 2. Builds on the lookup ports from Phase 2.
- US4 (P1) — can start after Phase 2. Cross-cuts US1 and US3 via `AccessEventDestinationResolver`.
- US5 (P2) — depends on US1 (reuse repository + audit scope). Can run parallel to US2/US3/US4 with isolation.
- US6 (P3) — architecture-level constraint; can run from Phase 2 onward.

### Within Each User Story

- Tests (T023–T025 etc.) MUST be written and observed **failing** before the corresponding implementation task.
- Models before services, services before endpoints, endpoints before UI integration.
- Each story's checkpoint must pass before advancing to the next priority.

### Parallel Opportunities

- Within Phase 1, T001..T006 marked [P] can run in parallel after the module skeleton is created.
- Within Phase 2, T007..T010 (entities/VO/exceptions) and T011..T013 (source-model additions) are parallel.
- T014..T017 (ports and token issuer) parallel.
- T018..T019 (DBTools repos + models) parallel.
- T022 fixture bootstrap can run after T011..T013.
- US2 depends on US1's `RecordAccessScanHandler` for the QR scan endpoint contract; US2's destination tests can be authored in parallel but the implementation merges into T035.
- US3 lookup handler and validator are parallelizable as separate writers.
- US5 admin endpoints and Angular admin pages can be built in parallel with US2/US3/US4 once entities exist.

---

## Implementation Strategy

### MVP First (User Story 1 only)

1. Complete Phase 1 (Setup).
2. Complete Phase 2 (Foundational) — blocks every story.
3. Complete Phase 3 (US1) only.
4. **STOP and VALIDATE**: US1 fully testable in `api` container; manual QR scan end-to-end works.
5. Demo if ready.

### Incremental Delivery

1. Setup + Foundational → foundation ready.
2. US1 → test → demo (MVP).
3. US2 → test → demo.
4. US3 → test → demo.
5. US4 → test → demo (visits + access unified).
6. US5 → test → demo (admin/audit).
7. US6 → arch test, no functional change.
8. Polish → quickstart → e2e → release tag.

Each story adds value without breaking previous ones.

### Parallel Team Strategy

With multiple developers inside one Compose stack:

1. Dev A: Setup + Foundational + US1.
2. After Phase 2 completes:
   - Dev A → US1.
   - Dev B → US2 (depends on US1 handler signature).
   - Dev C → US3 lookup; US4 destination resolver.
3. Dev D → US5 admin + audit.
4. Dev E → US6 architectural exclusions + Polish.
5. Stories integrate through the same `AccessControl.*` namespace — coordinate merges via `[verify]`-tagged task slices.

---

## Notes

- `[P]` tasks = different files, no transitive dependencies on incomplete tasks.
- `[Story]` label maps each task to a specific user story for traceability back to `spec.md` FR list.
- Every implementation task carries a `[VERIFY]` step covering (1) build, (2) the affected Docker Compose rebuild, (3) the affected test slice, (4) any necessary arch-test assertions. Constitution IV requires all four before a task is "done".
- Commit messages follow `feat(qr-access): …` / `fix(qr-access): …` / `test(qr-access): …` / `chore(qr-access): …`; reference the spec-kit task ID (e.g., `T026`).
- Stop at any checkpoint to validate the story independently.
- Anti-patterns to avoid: vague tasks, single-file conflicts, cross-module references that break independence, raw SQL inside the access-control module.
