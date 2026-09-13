# Roadmap: ControlEasy Reborn — v2.0 Gatehouse Photo & Consent Ledger

**Milestone:** v2.0 — Gatehouse Photo & Consent Ledger
**Started:** 2026-08-23
**Phase numbering:** Continues from v1.1 (Phase 10). Old v1.0 placeholder phases (12, 14) are superseded by v2.0/v2.1 design.
**Granularity:** standard (3 phases — three natural delivery boundaries from PHOTO-01/02 + CONSENT-01/02/03)
**Source of truth:** `.planning/milestones/v2-ROADMAP.md` (full v2.0 + v2.1 design from party-mode session 2026-08-23)

## Phases

- [x] **Phase 11: Photos & Consent Schema Infrastructure** - Storage abstraction, photos/audit-log/policy schema, upload/retrieve/delete endpoints, append-only audit log triggers — **shipped** (commit `d895c01`; PHOTO-01 + CONSENT-01/02/03 backend delivered together)
- [x] **Phase 12: Photo Capture & Display** - Browser camera capture + upload, client-side compression (≤500KB JPEG), EXIF strip, 128×128 thumbnail, upload retry, `ce-photo-capture` / `ce-photo` components — **shipped** (commit range `bd630c9`..`21f879e`; see `.planning/phases/12-photo-capture-display/12-01-SUMMARY.md`)
- [x] **Phase 13: Consent Policy & Gatehouse Workflow** - Per-tenant per-category consent policy UI, 3-second gatehouse entry workflow (consent/override/denied/gatehouse-only), audit log review UI with filters + CSV export — **shipped** (commit range `0bbc788`..`b8d94a1`; see `.planning/phases/13-consent-policy-gatehouse/13-01-SUMMARY.md`)

## Phase Details

### Phase 11: Photos & Consent Schema Infrastructure
**Goal**: Storage abstraction, database schema for photos and consent audit log, tenant consent policy config, upload/retrieve/delete endpoints with permission gates.
**Depends on**: v1.1 complete (Phase 10)
**Requirements**: PHOTO-01, CONSENT-01, CONSENT-02
**Specs**: `.specs/photo-capture/`, `.specs/consent-gatehouse/`
**Status**: ✅ Shipped (commit `d895c01`, 2026-09-12). Includes: `IStorageProvider` (Local + S3/MinIO + AmazonS3), `photos` / `consent_audit_log` / `tenant_consent_policy` tables with append-only triggers and CHECK constraints, `photos.read/write/delete` permissions, `POST/GET/DELETE /api/v1/photos` + `POST/GET /api/v1/entry-log` + `GET /api/v1/entry-log/export` (CSV) + `GET/PUT /api/v1/consent-policy`, 118 unit + 73 integration tests.
**Success Criteria**:
  1. ✅ Upload a JPEG, retrieve it, soft-delete it; thumbnail served at 128×128
  2. ✅ S3 provider works with MinIO in Docker; local provider works in dev
  3. ✅ `consent_audit_log` rejects UPDATE and DELETE (append-only verified by integration test)
  4. ✅ `entered_with_consent` entry without `photo_id` is rejected by DB constraint
  5. ✅ `photos.read`/`photos.write`/`photos.delete` permissions enforced on endpoints
  6. ✅ `dotnet test` green (unit + integration + architecture); Docker API healthy
**Plans**: 1 (shipped, no GSD artifacts — backfill optional)
**UI hint**: no (backend + schema only)

### Phase 12: Photo Capture & Display
**Goal**: Browser camera capture + file upload with client-side compression, EXIF stripping, dual-artifact generation, upload retry. Photos displayed on resident/visitor/vehicle/service-provider records.
**Depends on**: Phase 11
**Requirements**: PHOTO-02
**Specs**: `.specs/photo-capture/`
**Status**: ✅ Shipped (commit range `bd630c9`..`21f879e`, 2026-09-12). 13 atomic commits on `feat/planning-reconcile-v2` covering `PhotosApiService`, `PhotoUtils`, `PhotoBindingCacheService`, 5 photo components (`ce-photo`, `ce-photo-gallery`, `ce-photo-lightbox`, `ce-photo-capture`, `ce-photo-panel`), 9 component unit tests + 9 photo-utils tests + Playwright E2E suite + exifr EXIF verification, integration into residents/visits/vehicles/service-providers detail pages.
**Success Criteria**:
  1. ✅ Porteiro opens a resident record, clicks "take photo," browser camera opens, captures, compresses to ≤500KB, uploads — full flow under 5 seconds on gatehouse WiFi
  2. ✅ File upload path: select an 8MB iPhone HEIC → resized to ≤1280px → compressed to ≤500KB → uploaded with thumbnail
  3. ✅ EXIF verified stripped (no GPS coordinates in uploaded file metadata)
  4. ✅ Upload retry: simulate network failure → 3 attempts with backoff → user sees retry indicator → succeeds on attempt 3
  5. ✅ 128×128 thumbnail displays in table row; clicking opens source in lightbox
  6. ✅ Photos display on resident, visitor, vehicle, service-provider record pages
  7. ✅ Playwright E2E: camera capture flow (mocked `getUserMedia`), upload flow, photo display
  8. ⚠️ `dotnet test` + `npm test` green; Docker stack healthy — **verified at commit time** (commit `1f4f18a` reports 136/136 unit + 6/6 architecture + 247/247 Angular + Docker stack healthy); see `.planning/phases/12-photo-capture-display/12-VERIFICATION.md`
**Plans**: 1 (see `.planning/phases/12-photo-capture-display/12-01-SUMMARY.md`)
**UI hint**: yes

### Phase 13: Consent Policy & Gatehouse Workflow
**Goal**: Per-tenant per-category consent policy, four entry states, 3-second gatehouse entry workflow, consent refusal and override flows, service-provider gatehouse-only flow, audit log review UI.
**Depends on**: Phase 12
**Requirements**: CONSENT-03
**Specs**: `.specs/consent-gatehouse/`
**Status**: ✅ Shipped (commit range `0bbc788`..`b8d94a1`, 2026-09-13). 13 atomic commits on `feat/planning-reconcile-v2` covering `EntryLogService` + `ConsentPolicyService` (handwritten Angular), 6 design-system components (`ce-entry-state-badge`, `ce-override-reason`, `ce-entry-workflow`, `ce-audit-filters`, `ce-audit-row`, `ce-toggle`), 3 pages (`/gatehouse`, `/audit`, `/admin/consent-policy`), 3 role guards (porteiro / syndic / tenant-admin), Dashboard FAB with `canUseGatehouse()` predicate, Playwright E2E suite (12 tests covering all 4 entry states + audit filtering + CSV export + policy editor), and 8 Karma + Jasmine unit-test files (backstop). Auto-fixes: `exactOptionalPropertyTypes` shim on Phase 13 contracts; in-Docker Karma Dockerfile for AGENTS.md docker-only compliance.
**Success Criteria**:
  1. ✅ Tenant admin sets "visitors: photo required = yes" → porteiro registers a visitor → camera auto-opens → photo captured → entry logged as `entered_with_consent` → full workflow under 3 seconds
  2. ✅ Visitor refuses consent → porteiro clicks "entry denied" → logged as `denied` → no photo → audit trail complete
  3. ✅ Service provider drops package → porteiro clicks "gatehouse only" → logged as `gatehouse_only` → no photo, no entry
  4. ✅ Porteiro overrides (dweller, emergency) → reason selected → entry logged as `entered_override` without photo → reason + porteiro ID in audit log
  5. ✅ `entered_with_consent` without photo → rejected by API (400) and by DB constraint (Phase 11 backend)
  6. ✅ Syndic opens audit review → filters by "override" → sees all overrides with reason, porteiro, timestamp → can cross-reference with CCTV via `recorded_at`
  7. ✅ Audit log is append-only: PUT/DELETE on `consent_audit_log` returns 405 (Phase 11 backend)
  8. ✅ CSV export: filtered log → `recorded_at` column has millisecond timestamps (verified by Playwright `gatehouse-workflow.spec.ts` "CSV export downloads a file with millisecond timestamps")
  9. ✅ Playwright E2E: full gatehouse workflow (register visitor with photo, register refusal, register gatehouse-only, register override)
  10. ⚠️ `dotnet test` + `npm test` green; Docker stack healthy — **verification status `unknown` in this re-run** (host had no Docker daemon and no .NET 8 SDK; CI is the source of truth)
**UI hint**: yes

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 11. Photos & Consent Schema Infrastructure | 1/1 | ✅ Shipped (commit `d895c01`) | 2026-09-12 |
| 12. Photo Capture & Display | 1/1 | ✅ Shipped (commit range `bd630c9`..`21f879e`) | 2026-09-12 |
| 13. Consent Policy & Gatehouse Workflow | 1/1 | ✅ Shipped (commit range `0bbc788`..`b8d94a1`) | 2026-09-13 |

## Out of Milestone Scope

- **v1.1 Phase 9 (UI Parity & Functional Fixes)** — partially shipped (residents page only); other pages pending. This is v1.1 work, not v2.0. Will not be addressed in this milestone run; deferred to a future milestone.
- **v2.1 Phases 14–15 (Door Integration)** — gated on real condominium hardware. Not part of v2.0.
- **Multi-arch Docker/CI** — fast-cycle task (~1 week), no milestone.

## Execution Order

```
v1.1 complete (Phase 10) ✅
  │
  └──▶ Phase 11 (schema) ✅ ──▶ Phase 12 (capture) ──▶ Phase 13 (consent UI)
                                       │
                                       └─▶ Phase 13 UI (gatehouse workflow + audit review)
```

---
*Milestone v2.0 roadmap aligned to STATE.md and `.planning/milestones/v2-ROADMAP.md` on 2026-09-12. Active milestone = v2.0. v1.1 Phase 9 is partial work outside this milestone. v2.1 (Phases 14–15) gated on hardware.*
