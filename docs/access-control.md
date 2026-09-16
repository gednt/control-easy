# AccessControl Module

The AccessControl module governs gatehouse access and visit destinations for
the ControlEasy Reborn condominium platform. It is the operational backbone
that records resident and visitor cycles (entrance/exit) while keeping the
multi-tenant, privacy-preserving invariants required by the project's
constitution.

## Scope

- Records immutable access events for residents, vehicles, and visitors.
- Manages QR credentials (issue, replace, revoke) with append-only lifecycle
  audit.
- Provides a guarded manual lookup fallback for tenants without a usable QR
  (protected CPF / identity document / name / apartment / block search).
- Resolves a destination apartment, block, and unit for every recorded event
  so visit destinations are never left in a `pending` state.
- Reserves `facial_biometric` as vocabulary but **does not ship biometric
  enrollment, template storage, matching, or liveness.**

## Architecture

The module follows Clean Architecture layering under
`src/Modules/AccessControl/`:

```
Modules/AccessControl/
├── ControlEasyReborn.Modules.AccessControl.Domain/      // entities, value objects, domain errors
├── ControlEasyReborn.Modules.AccessControl.Application/ // commands, validators, handlers, contracts
├── ControlEasyReborn.Modules.AccessControl.Infrastructure/ // DBTools repositories, crypto, DI
└── ControlEasyReborn.Modules.AccessControl.Api/         // minimal API endpoint groups
```

Tenancy is enforced through `ITenantAwareLinqFactory` and the `tenant_id`
column on every AccessControl-owned table. Cross-module reads happen only
through narrow directory ports (`IResidentDirectory`, `IVehicleDirectory`,
`IApartmentDirectory`, `IConsentPolicyEvaluator`). The module never executes
raw SQL against another module's tables (Constitution III).

## Endpoints

All endpoints are versioned under `/api/v1`. Authorization is governed by the
project's permission system:

| Permission | Use |
|---|---|
| `Access.Access.Operate` | QR scans, manual lookups, manual access recording |
| `Access.Read` | Read access events, refused attempts, credential lists |
| `Access.Control.Issue` | Issue new QR credentials |
| `Access.Control.Replace` | Replace an active credential |
| `Access.Control.Revoke` | Revoke a credential |

### QR scans

- `POST /api/v1/access-events/scans` — record an entrance or exit from a QR
  payload. Idempotent on `(TenantId, ScanAttemptId)`. Returns the resolved
  destination apartment / block / unit when accepted; otherwise RFC 7807 with
  `failureCode` (`invalid_credential`, `credential_inactive`,
  `subject_inactive`, `destination_required`, `destination_inactive`,
  `not_authorized`, `policy_action_required`, `vehicle_inactive`).
- `GET /api/v1/access-events` — list accepted events with filter chips for
  date range, direction, subject type / id, credential status, gatehouse,
  attendant. QR payload and document values are never returned.
- `GET /api/v1/access-events/refused-attempts` — refusal lens.

### Manual lookup

- `POST /api/v1/access-subjects/search` — protected lookup. Returns masked
  documents and result-count band only. Persists `AccessLookupAudits`.
- `POST /api/v1/access-events/manual` — record an access event bound to a
  prior lookup audit. Returns `manual_event_orphan_lookup_id` when the audit
  is missing or owned by another profile.

### Credentials

- `POST /api/v1/access-credentials` — issue a credential. Returns the QR
  payload **once** (`oneTimeDisplay: true`).
- `POST /api/v1/access-credentials/{id}/replace` — atomically replace a
  credential. Returns the new QR payload **once**.
- `POST /api/v1/access-credentials/{id}/revoke` — revoke a credential.
  Idempotent for already-revoked credentials. Subsequent scans of the revoked
  credential are refused within seconds (cold path lookup verifies status on
  every request).
- `GET /api/v1/access-credentials` and `GET /api/v1/access-credentials/{id}`
  — list and detail without ever exposing the raw QR.

## Data model

Tables live in `docker/mysql/init/12-access-control-schema.sql` and the
idempotent migration mirror `docker/mysql/migrations/0009-access-control.sql`:

- `AccessCredentials` — current and historical QR credentials, keyed by
  `tenant_id`.
- `CredentialLifecycleActions` — append-only history of `Issued`, `Replaced`,
  `Revoked`, `Expired`, `Deactivated` events.
- `AccessEvents` — append-only record of accepted entrance/exit cycles with
  immutable destination snapshot.
- `RefusedScanAttempts` — append-only record of refused scans with safe
  failure code.
- `AccessLookupAudits` — append-only record of lookup audits. Stores
  criterion type and result count band; never the raw document value.

The migration also adds `OwnerResidentId` to `Vehicles`,
`ResidentIdentityDocuments` table for the manual-lookup index, and
`DestinationBlock` / `DestinationUnit` snapshot columns to `Visits`.

## Biometric exclusion

`CredentialMethod` reserves `facial_biometric = 99` as a vocabulary value
that is never produced, persisted, or transmitted. The architectural tests
under `tests/ControlEasyReborn.ArchitectureTests/AccessControlBiometricExclusionTests.cs`
and `AccessControlTenantRulesTests.AccessControl_must_not_introduce_biometric_columns_or_strings`
fail the build if any biometric keyword (`facial_biometric`,
`biometric_template`, `embedding`, `liveness_score`, `face_match`) appears in
compiled code or DDL outside an explicit `reserved` comment. A future
release that ships facial biometrics will require a separate approved
specification covering privacy, storage, enrollment, matching, and liveness.

## Observability

- Serilog structured logs include `TenantId`, `ProfileId`, `ScanAttemptId`,
  and the decision kind for AccessControl flows.
- Raw QR payloads, raw document values, full CPF, and full names are never
  written to logs.

## Testing

- Unit tests: `tests/ControlEasyReborn.UnitTests/Modules/AccessControl/`.
- Integration tests: `tests/ControlEasyReborn.IntegrationTests/AccessControl/`.
- Architectural tests: `tests/ControlEasyReborn.ArchitectureTests/`.