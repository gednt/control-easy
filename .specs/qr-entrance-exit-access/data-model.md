# Data Model: Gatehouse Access and Visit Destinations

## Ownership and relationships

```text
Residents module                         Vehicles module
───────────────                          ───────────────
Resident 1 ── * ResidentIdentityDocument Vehicle ── 0..1 OwnerResidentId → Resident
     │                                           │
     └────────── optional apartment ─────────────┘
                         │
                    Apartments module
                         │
                    Visits module
                    Visit ── 1 destination apartment + destination snapshot

AccessControl module
────────────────────
AccessCredential ── 1 ── * CredentialLifecycleAction
       │
       ├── 0..* AccessEvent
       └── 0..* RefusedScanAttempt

AccessLookupAudit ── 0..1 selected Resident or Vehicle
```

The Access Control module stores stable subject references (`SubjectType`, `SubjectId`) rather than copying master data. It validates each reference through owner-module directory ports at the decision point. All new records include both `TenantId` and `tenant_id` and are read/written through the tenant-aware factory.

## Source-model additions

### ResidentIdentityDocument (Residents module)

| Field | Rules |
|-------|-------|
| `Id`, `TenantId` / `tenant_id`, `ResidentId` | Required; resident must belong to the same tenant. |
| `DocumentType` | Required controlled value; `cpf` remains resident's existing primary field and is not duplicated. |
| `NormalizedValue` | Required; whitespace/punctuation normalized using the applicable document rule. |
| `Active` | Only active documents participate in manual lookup. |
| timestamps | Creation and update timestamps; no raw document values are emitted in search responses or logs. |

Uniqueness is tenant-scoped by document type and normalized value. A duplicate is an explicit conflict, not a silently ambiguous identity.

### Vehicle.OwnerResidentId (Vehicles module)

| Field | Rules |
|-------|-------|
| `OwnerResidentId` | Nullable tenant-local resident reference. It is validated on create/update when supplied. |

Existing `OwnerName` remains display data and is never used as proof of ownership. Existing vehicles can remain unlinked; they are still accessed independently by their own QR credential or direct vehicle selection.

### Visit destination (Visits module)

| Field | Rules |
|-------|-------|
| `ApartmentId` | Required for every new or updated visit, including gatehouse-only service-provider interactions; references an active apartment in the same tenant. |
| `DestinationBlock`, `DestinationUnit` | Required immutable snapshot populated from the destination apartment at create/update/confirmation time; never entered independently by the attendant. |
| legacy records | Existing records without a destination remain historical data. They are not valid inputs to a new/updated destination-dependent flow and are never labeled as a new pending destination. |

The Visits module owns the destination invariant for visitor and service-provider workflows. Resident/vehicle access flows use the same resolver and store their destination snapshot in `AccessEvent`.

## Access Control module entities

### AccessCredential

| Field | Rules |
|-------|-------|
| `Id`, `TenantId` / `tenant_id` | Required tenant scope. |
| `SubjectType`, `SubjectId` | Required; only `resident` or `vehicle`; source subject is active at issuance and validation. |
| `Method` | `qr` is the only creatable value. `facial_biometric` is reserved but invalid for this release. |
| `SecretVerifier`, `KeyVersion` | Required keyed one-way verifier of the opaque QR value; plaintext token is never persisted. |
| `Status` | `active`, `replaced`, `revoked`, `expired`, or `inactive`. |
| `ValidFromUtc`, `ExpiresAtUtc` | Credential is usable only within its validity period. |
| `ReplacedByCredentialId` | Optional successor link, set when replaced. |
| `IssuedByProfileId`, timestamps | Required attribution and audit timing. |

Only one active QR credential is allowed for a subject at a time. Replacement creates the successor and marks its predecessor `replaced` in one atomic operation.

### CredentialLifecycleAction

| Field | Rules |
|-------|-------|
| `Id`, `TenantId` / `tenant_id`, `CredentialId` | Required. |
| `Action` | `issued`, `replaced`, `revoked`, `expired`, or `deactivated`. |
| `PreviousStatus`, `ResultingStatus` | Required to reconstruct lifecycle history. |
| `ActorProfileId`, `ReasonCode`, `OccurredAtUtc`, `CorrelationId` | Actor/time required; free-text reason is bounded and sanitized when policy permits it. |

Lifecycle actions are append-only. No credential deletion is permitted.

### AccessEvent

| Field | Rules |
|-------|-------|
| `Id`, `TenantId` / `tenant_id` | Required. |
| `SubjectType`, `SubjectId` | Required `resident` or `vehicle`; validated eligible in current tenant. |
| `Direction` | Required `entrance` or `exit`. |
| `AccessMethod` | Required `qr` or `manual_lookup`. |
| `CredentialId` | Required for QR, null for manual lookup. |
| `LookupAuditId` | Required for manual lookup, null for QR. |
| `ScanAttemptId` | Required and unique per tenant for QR; retained for idempotent retries. |
| `PerformedByProfileId`, `GatehouseId`, `OccurredAtUtc`, `CorrelationId` | Attendant and time required; gatehouse is optional until a device/gatehouse selection exists. |
| `DuplicateOfAccessEventId`, `DuplicateConfirmed` | Optional relationship and explicit attendant outcome for duplicate-window review. |
| `PolicyOutcome` | Required non-sensitive outcome showing direct permit or required handoff to the existing consent workflow. |
| `DestinationApartmentId`, `DestinationBlock`, `DestinationUnit` | Required immutable snapshot from the active resolved apartment. Resident flow uses `Resident.ApartmentId`; vehicle flow uses verified `OwnerResidentId` first, then `Vehicle.ApartmentId`. |

Access events are append-only. A successful event is created only after source-subject eligibility, tenant scope, attendant permission, duplicate policy, and consent-policy evaluation pass.

### RefusedScanAttempt

| Field | Rules |
|-------|-------|
| `Id`, `TenantId` / `tenant_id`, `ScanAttemptId` | Required; `(TenantId, ScanAttemptId)` supports idempotent retries. |
| `CredentialFingerprint` | Optional keyed non-reversible fingerprint; never the raw QR value. |
| `Direction`, `FailureCode`, `PerformedByProfileId`, `GatehouseId`, `OccurredAtUtc`, `CorrelationId` | Required except optional gatehouse. Failure code is generic and never exposes a foreign identity. |
| `RelatedCredentialId`, `RelatedSubjectId` | Optional and populated only when it is safe and authorized within the tenant. |

This record covers malformed, unknown, invalid, inactive, expired, revoked, unauthorized, duplicate-pending, and policy-action-required scans.

### AccessLookupAudit

| Field | Rules |
|-------|-------|
| `Id`, `TenantId` / `tenant_id` | Required. |
| `CriterionType` | `cpf`, `identity_document`, `name`, `apartment`, or `block`. |
| `ResultCountBand` | Required coarse band (`0`, `1`, `2_to_10`, `over_10`), not raw query content. |
| `SelectedSubjectType`, `SelectedSubjectId` | Null until a selection; then tenant-local resident or vehicle only. |
| `PerformedByProfileId`, `OccurredAtUtc`, `CorrelationId` | Required. |

The raw query, full CPF, additional document value, name, and QR value are never stored in this audit record or in application logs.

## State transitions

```text
issue → active ──────────→ revoked
          │  └───────────→ inactive
          │  └───────────→ expired
          └──────────────→ replaced → successor active

QR scan → resolve active destination → idempotent prior result | refused attempt | duplicate review | policy handoff | access event
manual lookup → lookup audit → select eligible subject → resolve active destination → policy handoff | access event
```

An exit after an unmatched entrance, or the converse, is reviewable rather than automatically denied. Repeated scans are not silently discarded: the UI receives a duplicate warning and the explicit outcome remains auditable.
