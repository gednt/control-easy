# Requirements — Consent Policy & Gatehouse Workflow

> Spec folder: `.specs/consent-gatehouse/`
> Milestone: v2.0 — Gatehouse Photo & Consent Ledger
> Phase: 13 (consent policy & gatehouse workflow)
> Supersedes: `.specs/_retired/3 - photo-capture-hardware-integration/` (consent subset — original spec had no explicit consent model)

## Context

Dwellers, visitors, and service providers must consent to being photographed if the condominium's security policy requires it. If consent is required and the person refuses, they cannot enter the condominium. Service providers are an exception: they may not enter, but they may transact at the gatehouse (middle cell — drop a package, leave a delivery) without consent. The policy is per-condominium and per-category. ControlEasy does not enforce the policy — it *enables* the condominium to set it. The CCTV system (external, condominium-owned) is the backstop for verifying the porteiro's honesty. ControlEasy carries timestamps; the condominium carries footage.

## User Stories

### Consent Policy

- **UC-CG-01:** As a *tenant administrator*, I want to set a per-category consent policy (dwellers/visitors/service-providers/vehicles: photo required yes/no) for my condominium so the gatehouse workflow reflects our security policy.
- **UC-CG-02:** As a *tenant administrator*, I want the consent policy to be editable after initial provisioning so I can adjust it as the condominium's needs change.
- **UC-CG-03:** As a *security officer*, I want the consent policy stored per-tenant so each condominium operates independently — ControlEasy does not impose a universal policy.

### Gatehouse Entry Workflow

- **UC-CG-04:** As a *gatehouse attendant*, I want to register a visitor entry in under 3 seconds (select category → enter name → photo auto-opens if required → snap → done) so logging is faster than skipping.
- **UC-CG-05:** As a *gatehouse attendant*, I want to handle a visitor who refuses consent by clicking "entry denied" so the refusal is logged and the gate stays closed.
- **UC-CG-06:** As a *gatehouse attendant*, I want to log a service provider as "gatehouse only" (package dropped, no entry past the gate) so the transaction is recorded without requiring consent.
- **UC-CG-07:** As a *gatehouse attendant*, I want to override the photo requirement for a dweller or visitor in an emergency or when a resident vouches for them, so I can let someone in without a photo when justified — with a reason code and my ID attached.
- **UC-CG-08:** As a *gatehouse attendant*, I want the override reason dropdown to offer fixed options (emergency, vouched) so I don't have to type a custom justification in the moment.

### Audit & Verification

- **UC-CG-09:** As a *syndic (tenant administrator)*, I want to review the consent audit log filtered by date, category, entry state, and porteiro so I can verify the gatehouse is operating correctly.
- **UC-CG-10:** As a *syndic*, I want `entered_with_consent` entries to show the photo thumbnail and `entered_override` entries to be highlighted so I can focus my review on exceptions.
- **UC-CG-11:** As a *syndic*, I want to export the filtered audit log as CSV with millisecond-precision timestamps so I can cross-reference entries with the condominium's CCTV footage by time.
- **UC-CG-12:** As a *security officer*, I want the consent audit log to be append-only (no UPDATE or DELETE) so the record cannot be tampered with after the fact.
- **UC-CG-13:** As a *security officer*, I want every `entered_with_consent` entry to require a non-null photo (database-enforced) so a consent entry without a photo is impossible, not just unlikely.

## Requirements

### CONSENT-01: Consent Policy Config

- Per-tenant, per-category toggle: dwellers / visitors / service-providers / vehicles × `photo_required: yes/no`
- `tenant_consent_policy` table: `tenant_id`, `category`, `photo_required`
- Editable by tenant admin via `PUT /api/v1/tenants/{id}/consent-policy`
- Set at tenant provisioning; no runtime rules engine; no weekend logic; no custom policies

### CONSENT-02: Gatehouse Entry Workflow

- Four entry states: `entered_with_consent` (photo required), `entered_override` (dwellers/visitors only, no photo, reason code), `gatehouse_only` (service providers, no entry), `denied` (consent refused, entry refused)
- `consent_audit_log` table: `id`, `tenant_id`, `entry_state`, `entity_type`, `entity_id`, `photo_id` (nullable), `override_reason` (nullable, enum: `emergency`/`vouched`), `porteiro_id`, `recorded_at` (datetime3, millisecond precision), `created_at`
- Hard DB constraint: `entered_with_consent` → `photo_id` non-null
- Override reason codes: hardcoded enum (`emergency`, `vouched`) — no custom reasons, no admin config
- Append-only enforcement on `consent_audit_log` (no UPDATE/DELETE)
- `POST /api/v1/entry-log` — create entry (validates policy, enforces state transitions, creates audit log entry)
- `GET /api/v1/entry-log` — list with filters (date, category, state, porteiro)
- 3-second workflow target: category → name → photo (if required) → done

### CONSENT-03: Audit Review & Export

- Audit review UI: filter by date range, category, entry state, porteiro
- `entered_with_consent` → photo thumbnail (clickable to enlarge)
- `entered_override` → highlighted for review, reason + porteiro name shown
- `gatehouse_only` → "no entry" badge
- `denied` → "refused" badge
- CSV export with millisecond-precision `recorded_at`
- `GET /api/v1/entry-log/export` — CSV export

## Entry State Matrix

| State | Photo | Who can trigger | Reason code | Notes |
|---|---|---|---|---|
| `entered_with_consent` | Required (DB-enforced) | Porteiro | — | Hard constraint: no photo = rejected |
| `entered_override` | None | Porteiro (dwellers/visitors only) | `emergency` / `vouched` | Porteiro ID logged |
| `gatehouse_only` | None | Porteiro (service providers only) | — | Package dropped, no entry past gate |
| `denied` | None | Porteiro (all categories) | — | Consent refused, entry refused |

## Design Principles

1. **Logging is faster than skipping** — the 3-second workflow is the honesty enforcement. If logging takes longer than not logging, the porteiro will skip it.
2. **CCTV is the external backstop** — ControlEasy carries timestamps; the condominium carries footage. The syndic cross-references by `recorded_at`.
3. **ControlEasy enables, doesn't enforce** — the policy is the condominium's, not the software's. The software makes the policy *operational*.
4. **No approval workflow** — overrides are logged with reason + porteiro ID. No second-person approval. Visibility, not enforcement of honesty. Anomaly detection is v2.1+.
5. **No custom reason codes** — hardcoded two options. If a condominium needs more, that's a future spec.

## Out of Scope

- Anomaly detection on the audit log (patterns of dishonest logging) — v2.1+
- Second-person approval workflow for overrides — v2.1+ if data shows it's needed
- Custom reason codes per condominium — future spec
- Integration with CCTV/NVR footage retrieval — ControlEasy carries timestamps only
- Door/gate hardware integration — v2.1 (`.specs/door-integration/`)

## Traceability

| Requirement | Phase | Spec |
|---|---|---|
| CONSENT-01 | Phase 13 | `.specs/consent-gatehouse/` |
| CONSENT-02 | Phase 13 | `.specs/consent-gatehouse/` |
| CONSENT-03 | Phase 13 | `.specs/consent-gatehouse/` |

---
*Requirements defined: 2026-08-23*
*Derived from: party-mode v2.0 milestone design session*