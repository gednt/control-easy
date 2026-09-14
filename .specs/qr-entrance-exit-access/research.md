# Research: Gatehouse Access and Visit Destinations

## Decisions

### 1. Use opaque, server-issued QR credentials

**Decision**: Generate a high-entropy opaque bearer reference for each QR credential. Persist only a keyed one-way verifier and key version; show the raw QR value only at issuance/replacement and never store it in event or application logs.

**Rationale**: Static QR codes are copyable. Their security comes from unpredictable values, server-side validation, lifecycle control, and tenant-scoped authorization—not from embedding an identifier in the image. This also prevents a photographed code from disclosing a CPF, resident name, apartment, plate, tenant, or status.

**Alternatives considered**:

- Signed QR claims: rejected because the payload exposes state or identity and revocation is more complex.
- Short-lived rotating QR codes: deferred because it requires a resident-facing delivery/device design outside this gatehouse-first scope.

### 2. Create a dedicated access-control ledger

**Decision**: Add an `AccessControl` module with dedicated access credential, successful-event, refused-attempt, lifecycle-action, and lookup-audit records. Do not repurpose `Photos` module's `ConsentAuditLog`.

**Rationale**: The current consent log has no direction, subject ID, access method, credential, reader/gatehouse, retry identity, or decision reason. It is append-only and also has consent-specific validation plus a legacy synthetic-visit coupling. A separate ledger lets the new feature preserve its own immutable security semantics while using a narrow policy-evaluation port to honor existing consent rules.

**Alternatives considered**:

- Add access fields to `ConsentAuditLog`: rejected because it mixes incompatible domain meanings, risks current consent constraints, and expands the legacy Visit coupling.
- Store access history on Resident or Vehicle records: rejected because it loses a neutral, immutable audit model and makes multi-subject reporting harder.

### 3. Add only the source-model extensions required for protected fallback lookup

**Decision**: Keep `Resident` as the CPF owner and add `ResidentIdentityDocument` for additional registered documents. Add nullable `OwnerResidentId` to `Vehicle`; do not infer vehicle ownership from the free-text `OwnerName`. Expose narrow directory ports from Residents, Vehicles, and Apartments to Access Control.

**Rationale**: Existing resident data has CPF but no generic document model. Existing vehicle data has an apartment and free-text owner name but no reliable resident relationship. A verified optional relationship lets a resident lookup present truly associated vehicles. Owner modules remain the source of truth and their tenant-aware repositories can serve the new directory without raw cross-table lookup SQL.

**Alternatives considered**:

- Treat `OwnerName` as a link: rejected because names are not stable identifiers.
- Query all master tables with an ad-hoc access SQL join: rejected by the LINQ-first constitutional rule and module ownership.
- Omit other documents and vehicle association: rejected because it would not satisfy the requested fallback search scope.

### 4. Make access decisions idempotent without hiding real repeated events

**Decision**: A scan includes a client-generated `scanAttemptId`. Retrying that identifier returns the original decision. A configurable short duplicate interval for the same eligible subject and direction returns a duplicate warning; it remains reviewable and an attendant may explicitly confirm a genuinely separate physical event.

**Rationale**: Network retries and scanner double reads must not inflate access history, but automatically refusing all rapid repeats could obstruct a real second event. This approach separates request idempotency from operational duplicate review.

**Alternatives considered**:

- Record every rapid repeat: rejected because it creates inaccurate history.
- Hard reject every repeat: rejected because a missed earlier record must not trap a resident or vehicle.

### 5. Protect manual fallback search and its audit trail

**Decision**: CPF and other document lookup accepts only complete normalized values. Name, apartment, and block lookup requires a server-defined sufficiently specific term, returns a capped/paginated tenant-local result set, and shows only minimal disambiguation details. Lookup audit keeps criterion type and result-count band, not raw search text, CPF, documents, or tokens.

**Rationale**: CPF and other identity documents are personal data. The fallback must be useful at a gatehouse without creating a tenant directory, document-enumeration, or sensitive-log surface.

**Alternatives considered**:

- Prefix document search: rejected because it enables enumeration.
- Unrestricted name search: rejected because it enables bulk browsing.
- Logging full query values: rejected because it duplicates sensitive personal data without audit value.

### 6. Keep facial biometrics out of this implementation

**Decision**: The method vocabulary reserves facial biometrics for a future approved feature, but adds no enrollment, photo-to-face conversion, template storage, comparison, match score, liveness result, endpoint, UI, or log field.

**Rationale**: Existing photos are not biometric templates. Facial biometrics is sensitive personal data and needs a dedicated privacy, security, accuracy, liveness, retention, human fallback, and incident-response design.

**Alternatives considered**:

- Reuse resident photos for recognition: rejected as outside scope and incompatible with privacy expectations.
- Add an empty biometric table now: rejected as speculative sensitive-data storage.

### 7. Make destination mandatory and derive it from trusted master data

**Decision**: Every new or updated visitor and service-provider visit requires an active destination apartment. Block and unit are derived from that apartment. Selecting a resident in either gatehouse flow automatically resolves the resident's active apartment; an associated vehicle uses the verified resident owner first and its registered apartment second. The completed visit/access record stores the resolved apartment, block, and unit as an immutable destination snapshot.

**Rationale**: The existing `Visits` model already has `ApartmentId`, while `Apartments` owns the canonical block/unit. Requiring the reference eliminates the "destination pending" outcome without duplicating unverified text during data entry. A snapshot preserves the factual destination when a resident later moves or an apartment label changes.

**Alternatives considered**:

- Allow a blank destination with a pending status: rejected by the expanded business requirement and because it weakens accountability.
- Require attendants to type block/unit for residents: rejected because master data should be recovered automatically and duplicate entry drifts.
- Use the free-text vehicle owner name to infer destination: rejected because it is not a reliable relationship.

## Sources

- [OWASP Session Management Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html) — opaque unpredictable server-side references and token protection.
- [OWASP Logging Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html) — do not log sensitive identifiers or authentication tokens.
- [ANPD FAQ — personal data](https://www.gov.br/anpd/pt-br/acesso-a-informacao/perguntas-frequentes/perguntas-frequentes) — CPF, names, and addresses are personal data; biometrics linked to a person is sensitive personal data.

## Existing-code findings that shape the design

- `Residents` currently has CPF, name, and apartment but no generic document model. `Vehicles` has no reliable resident link. `Apartments` owns block/unit. See `docker/mysql/init/02a-residents-schema.sql`, `02b-apartments-schema.sql`, and `07-vehicles-schema.sql`.
- The existing `ConsentAuditLog` is append-only but consent-oriented and unsuitable as an access event ledger. See `docker/mysql/init/09b-consent-schema.sql` and `src/Modules/Photos/ControlEasyReborn.Modules.Photos.Application/Handlers/EntryLogHandlers.cs`.
- The existing gatehouse surface is `src/Web/ControlEasyReborn.Web/src/app/features/entry-workflow/`; it is the integration point for scanning and fallback lookup, while credential administration is a separate admin surface.
- `Visits` already carries `ApartmentId`, but it is nullable and does not retain block/unit at recording time. The existing visit UI and gatehouse workflow must use the same active-apartment destination resolver and stop presenting destination pending for new or updated records.
