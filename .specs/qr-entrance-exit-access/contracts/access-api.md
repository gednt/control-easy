# Access Control API Contract

## Conventions

- Base path: `/api/v1`.
- All routes require JWT authentication and tenant resolution.
- Gatehouse scanning/manual lookup requires `Access.Operate`; credential lifecycle and history review require explicit Access administration/read permissions. The permissions are granted only to roles authorized by tenant policy.
- All validation, authorization, conflicts, rate limits, and unavailable-service responses use RFC 7807 `ProblemDetails` with safe public error codes.
- The service never returns raw QR values after issuance, raw search documents, biometric data, or a different tenant's identity.
- Every successful destination-dependent visit/access response includes the resolved apartment, block, and unit. A missing or inactive destination returns a safe `destination_required` or `destination_inactive` ProblemDetails response; `destination_pending` is not a response state.

## Credential administration

### `POST /access-credentials`

Issues the first QR credential for an active resident or vehicle.

```json
{
  "subjectType": "resident",
  "subjectId": "guid",
  "validFromUtc": "2026-09-13T00:00:00Z",
  "expiresAtUtc": null
}
```

Returns `201 Created` with credential metadata and the one-time printable QR payload. The raw payload is excluded from later reads and logs.

### `POST /access-credentials/{credentialId}/replace`

Issues a successor QR credential and atomically marks the predecessor replaced. Returns the successor's one-time printable payload.

### `POST /access-credentials/{credentialId}/revoke`

Revokes an active credential. Request accepts a controlled reason code and optional bounded justification. Subsequent scans are refused immediately after the action is confirmed.

### `GET /access-credentials`

Lists tenant-local credentials with subject, type, status, validity, and lifecycle filters. It never returns the QR payload or verifier.

## Gatehouse operations

### `POST /access-events/scans`

Resolves and records a QR-based decision.

```json
{
  "qrPayload": "opaque-printed-value",
  "direction": "entrance",
  "scanAttemptId": "guid",
  "gatehouseId": "guid-or-null",
  "confirmDuplicate": false
}
```

Possible result kinds are `recorded`, `duplicate_confirmation_required`, `policy_action_required`, `refused`, and `unavailable`. A successful resident or vehicle result includes its resolved destination apartment, block, and unit. Reusing the same `scanAttemptId` returns the original decision. Refusal responses contain only a safe code such as `invalid_credential`, `credential_inactive`, `subject_inactive`, `destination_required`, `destination_inactive`, `not_authorized`, or `policy_action_required`.

### `POST /access-subjects/search`

Performs the protected no-QR fallback lookup.

```json
{
  "criterion": {
    "type": "cpf",
    "value": "normalized-complete-value",
    "unit": null
  }
}
```

Allowed criterion types are `cpf`, `identity_document`, `name`, `apartment`, and `block`. CPF and identity-document values must be complete. Name, apartment, and block values must meet server validation for minimum specificity. The response is capped and contains only tenant-local active residents/vehicles plus minimal disambiguation fields; document values are masked or omitted. It returns a `lookupId` used to bind a later manual event to the audited lookup.

### `POST /access-events/manual`

Records a manually selected tenant-local resident or vehicle.

```json
{
  "lookupId": "guid",
  "subjectType": "vehicle",
  "subjectId": "guid",
  "direction": "exit",
  "gatehouseId": "guid-or-null"
}
```

The selected subject must be present and eligible in the referenced lookup. The same policy, authorization, tenant, destination, and duplicate checks used by QR flow apply. A successful response identifies `accessMethod` as `manual_lookup` and includes the resolved destination apartment, block, and unit.

## Existing visit destination contract

### Visitor and service-provider visit create/update

The existing visit create/update contract requires `apartmentId` for every new or updated visit. The server resolves the apartment's block and unit; callers do not submit a separate block or unit. The operation rejects a missing, inactive, or cross-tenant apartment and never creates a `destination_pending` record. Completed visit responses include the immutable destination snapshot.

## Review

### `GET /access-events`

Lists immutable successful events using tenant-local filters: time range, direction, subject type/id, access method, credential status, gatehouse, and attendant. Tenant administrators can request the matching lifecycle/audit context through authorized detail endpoints.

### `GET /access-events/refused-attempts`

Lists non-sensitive refused scan attempts with time, direction, failure category, attendant, and gatehouse filters. It never reveals the raw QR payload, full document value, or cross-tenant subject identity.

## Contract test obligations

- QR issuance returns an opaque payload once, while GET/list/log responses never expose it.
- Revoke/replace immediately makes the predecessor unusable and leaves lifecycle evidence.
- A repeated `scanAttemptId` is idempotent; a distinct rapid repeated scan enters duplicate review.
- Cross-tenant QR and manual searches reveal no identifying response data.
- CPF/document searches require complete values; broad queries are rejected; responses mask/omit documents.
- Manual event creation must use a tenant-local authorized lookup result.
- Every new/updated visitor or service-provider visit requires an active destination apartment, and resident selections in both existing and new gatehouse flows recover the same apartment/block/unit automatically.
- Facial-biometric values/endpoints/models are absent.
