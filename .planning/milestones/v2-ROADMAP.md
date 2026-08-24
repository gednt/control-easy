# Roadmap: ControlEasy Reborn — v2.0 & v2.1 Gatehouse Photo, Consent & Door Integration

**Milestone v2.0:** Gatehouse Photo & Consent Ledger (Phases 11–13)
**Milestone v2.1:** Door Integration — Optional (Phases 14–15)
**Fast-cycle:** Multi-Arch Docker/CI (no milestone)
**Defined:** 2026-08-23
**Phase numbering:** Continues from v1.1 (Phase 10). Old v1.0 placeholder phases (12, 14) are superseded.
**Origin:** Party-mode design session — 2026-08-23

## Design Principles (from the room)

1. **Logging is faster than skipping** — the 3-second gatehouse workflow is the honesty enforcement. If logging takes longer than not logging, the porteiro will skip it.
2. **CCTV is the external backstop** — ControlEasy carries timestamps; the condominium carries footage. The syndic cross-references by `recorded_at`.
3. **ControlEasy enables, doesn't enforce** — the consent policy is the condominium's, not the software's.
4. **Client-side only for image processing** — no server-side compression, thumbnailing, or EXIF stripping in v2.0.
5. **Door integration is opt-in** — condominiums without hardware use the voluntary ledger (v2.0). With hardware, the ledger becomes enforced.
6. **ControlEasy is a participant, not a gatekeeper** — the door opens independently; the API observes and can trigger, but doesn't block physical access.
7. **No speculative abstractions** — `IDeviceHandler` emerges from the second real integration, not before it.
8. **No approval workflow, no anomaly detection** — v2.0 ships visibility. Let humans catch dishonesty with the tools they already have.

## Milestone v2.0 — Gatehouse Photo & Consent Ledger

**Goal:** The porteiro can capture a photo of anyone entering the condominium, the condominium sets a per-category consent policy, and every entry is logged with millisecond-precision timestamps cross-referenceable with external CCTV.

### Phase 11: Photos & Consent Schema Infrastructure

**Goal:** Storage abstraction, database schema for photos and consent audit log, tenant consent policy config, upload/retrieve/delete endpoints with permission gates.

| | |
|---|---|
| **Depends on** | v1.1 complete (Phase 10) |
| **Requirements** | PHOTO-01 |
| **Spec** | `.specs/photo-capture/` |
| **UI hint** | no (backend + schema only) |

**Delivers:**
- `IStorageProvider` abstraction — `LocalFilesystemStorageProvider` (dev) + `S3StorageProvider` (production, MinIO-compatible)
- `photos` table — `id`, `tenant_id`, `entity_type` (resident/visitor/vehicle/service-provider), `entity_id`, `file_path`, `thumbnail_path`, `file_size_bytes`, `mime_type`, `captured_at`, `uploaded_by`, `created_at`, `deleted_at` (nullable)
- `consent_audit_log` table — `id`, `tenant_id`, `entry_state` (enum: `entered_with_consent` / `entered_override` / `gatehouse_only` / `denied`), `entity_type`, `entity_id`, `photo_id` (nullable), `override_reason` (nullable, enum: `emergency` / `vouched`), `porteiro_id`, `recorded_at` (datetime3, millisecond precision), `created_at`
- `tenant_consent_policy` table — `tenant_id`, `category` (dwellers/visitors/service-providers/vehicles), `photo_required` (boolean)
- Hard DB constraint: `entered_with_consent` entries → `photo_id` non-null
- Permissions: `photos.read`, `photos.write`, `photos.delete`
- Endpoints: `POST /api/v1/photos` (upload, `photos.write`), `GET /api/v1/photos/{id}` (retrieve, `photos.read`), `DELETE /api/v1/photos/{id}` (soft-delete, `photos.delete`)
- Storage config via env vars: `STORAGE__PROVIDER` (local/s3), `STORAGE__LOCAL__PATH`, `STORAGE__S3__ENDPOINT`, `STORAGE__S3__BUCKET`, `STORAGE__S3__ACCESSKEY`, `STORAGE__S3__SECRETKEY`
- Append-only enforcement on `consent_audit_log` (no UPDATE/DELETE — DB trigger or application constraint + integration test)

**Success Criteria:**
1. Upload a JPEG, retrieve it, soft-delete it; thumbnail served at 128×128
2. S3 provider works with MinIO in Docker; local provider works in dev
3. `consent_audit_log` rejects UPDATE and DELETE (append-only verified by integration test)
4. `entered_with_consent` entry without `photo_id` is rejected by DB constraint
5. `photos.read`/`photos.write`/`photos.delete` permissions enforced on endpoints
6. `dotnet test` green (unit + integration + architecture); Docker API healthy

**Plans:** TBD

---

### Phase 12: Photo Capture & Display

**Goal:** Browser camera capture + file upload with client-side compression, EXIF stripping, dual-artifact generation, upload retry. Photos displayed on resident/visitor/vehicle/service-provider records.

| | |
|---|---|
| **Depends on** | Phase 11 |
| **Requirements** | PHOTO-02 |
| **Spec** | `.specs/photo-capture/` |
| **UI hint** | yes |

**Delivers:**
- `ce-photo-capture` design system component:
  - Camera mode: `getUserMedia` → live preview → capture button
  - Upload mode: file picker + drag/drop fallback
  - Client-side compression: resize longest dimension to ≤1280px → `canvas.toBlob('image/jpeg', 0.8)` → size check → retry at 0.6 if >500KB → floor at quality 0.3
  - EXIF strip: canvas redraw (verify no GPS, no camera model, no timestamp survives)
  - 128×128 thumbnail generation client-side (separate canvas draw)
  - Upload with retry: 3 attempts, exponential backoff (1s, 2s, 4s)
  - File-type validation: accept `image/jpeg`, `image/png`, `image/heic`; reject others with toast
  - Error states: camera permission denied, upload failed after retry, file too large after compression floor
- `ce-photo` display component: thumbnail in table rows (lazy-loaded), source in detail view, click-to-enlarge lightbox
- Integration into existing record pages: residents, visitors, vehicles, service providers
- Multiple photos per entity (gallery — most recent as primary thumbnail)

**Success Criteria:**
1. Porteiro opens a resident record, clicks "take photo," browser camera opens, captures, compresses to ≤500KB, uploads — full flow under 5 seconds on gatehouse WiFi
2. File upload path: select an 8MB iPhone HEIC → resized to ≤1280px → compressed to ≤500KB → uploaded with thumbnail
3. EXIF verified stripped (no GPS coordinates in uploaded file metadata)
4. Upload retry: simulate network failure → 3 attempts with backoff → user sees retry indicator → succeeds on attempt 3
5. 128×128 thumbnail displays in table row; clicking opens source in lightbox
6. Photos display on resident, visitor, vehicle, service-provider record pages
7. Playwright E2E: camera capture flow (mocked `getUserMedia`), upload flow, photo display
8. `dotnet test` + `npm test` green; Docker stack healthy

**Plans:** TBD

---

### Phase 13: Consent Policy & Gatehouse Workflow

**Goal:** Per-tenant per-category consent policy, four entry states, 3-second gatehouse entry workflow, consent refusal and override flows, service-provider gatehouse-only flow, audit log review UI.

| | |
|---|---|
| **Depends on** | Phase 12 |
| **Requirements** | CONSENT-01, CONSENT-02, CONSENT-03 |
| **Spec** | `.specs/consent-gatehouse/` |
| **UI hint** | yes |

**Delivers:**
- **Consent policy config** (tenant admin UI):
  - Per-category toggle: dwellers / visitors / service-providers / vehicles × `photo_required: yes/no`
  - Set at tenant provisioning; editable by tenant admin
  - Stored in `tenant_consent_policy` table (from Phase 11)
- **Gatehouse entry workflow** (new Angular page — the 3-second flow):
  - Porteiro selects category → enters name → if policy requires photo: camera auto-opens → snap → entry logged as `entered_with_consent`
  - If policy does not require photo: name → enter → entry logged (photo optional)
  - Consent refusal flow: visitor refuses → "entry denied" button → logged as `denied`
  - Service-provider gatehouse-only flow: "gatehouse only" button → logged as `gatehouse_only`
  - Override flow (dwellers/visitors only): "override" button → reason dropdown (emergency/vouched) → porteiro ID → logged as `entered_override`
  - Workflow designed so logging is faster than skipping
- **Audit log review UI** (syndic/tenant admin):
  - Filter by date range, category, entry state, porteiro
  - `entered_with_consent` rows show photo thumbnail (clickable)
  - `entered_override` rows highlighted, reason + porteiro name shown
  - `gatehouse_only` rows show "no entry" badge
  - `denied` rows show "refused" badge
  - CSV export with millisecond-precision `recorded_at`
  - `recorded_at` displayed with millisecond precision
- **Backend:**
  - `POST /api/v1/entry-log` — create entry (validates consent policy, enforces state transitions, creates `consent_audit_log` entry)
  - `GET /api/v1/entry-log` — list with filters
  - `GET /api/v1/entry-log/export` — CSV export
  - `PUT /api/v1/tenants/{id}/consent-policy` — update policy (tenant admin only)
  - Hard constraint: `entered_with_consent` without photo → 400 ValidationException
  - Override reason codes: hardcoded enum (`emergency`, `vouched`)

**Success Criteria:**
1. Tenant admin sets "visitors: photo required = yes" → porteiro registers a visitor → camera auto-opens → photo captured → entry logged as `entered_with_consent` → full workflow under 3 seconds
2. Visitor refuses consent → porteiro clicks "entry denied" → logged as `denied` → no photo → audit trail complete
3. Service provider drops package → porteiro clicks "gatehouse only" → logged as `gatehouse_only` → no photo, no entry
4. Porteiro overrides (dweller, emergency) → reason selected → entry logged as `entered_override` without photo → reason + porteiro ID in audit log
5. `entered_with_consent` without photo → rejected by API (400) and by DB constraint
6. Syndic opens audit review → filters by "override" → sees all overrides with reason, porteiro, timestamp → can cross-reference with CCTV via `recorded_at`
7. Audit log is append-only: PUT/DELETE on `consent_audit_log` returns 405
8. CSV export: filtered log → `recorded_at` column has millisecond timestamps
9. Playwright E2E: full gatehouse workflow (register visitor with photo, register refusal, register gatehouse-only, register override)
10. `dotnet test` + `npm test` green; Docker stack healthy

**Plans:** TBD

---

## Milestone v2.1 — Door Integration (Optional)

**Goal:** Condominiums that opt in can integrate ControlEasy with their door relay and card/biometric readers. The API triggers the relay with signed commands; reader events create entry log records automatically. The door opens independently of ControlEasy — the API is a participant, not a gatekeeper.

**Gated on:** A real condominium with hardware ready to integrate. No speculative builds.

### Phase 14: Door Relay & Unlock Commands

**Goal:** API-triggered door relay with HMAC-signed unlock commands, porteiro-role authorization, rate-limiting, command audit log. First vendor integration with one real condominium. Door opens independently of ControlEasy (hardware fallback).

| | |
|---|---|
| **Depends on** | v2.0 complete (Phase 13) + real condominium with door hardware |
| **Requirements** | DOOR-01 |
| **Spec** | `.specs/door-integration/` |
| **UI hint** | yes (porteiro "open gate" button) |

**Delivers:**
- `IDoorController` abstraction — `UnlockAsync(deviceId, command)` with vendor-specific implementations
- First implementation: one real condominium's door controller (vendor TBD)
- `POST /api/v1/devices/{deviceId}/unlock` — porteiro-role only, HMAC-SHA256 signed, rate-limited (max 1 per 3s per device), audit-logged
- Command audit: requesting user, device ID, timestamp, command hash, response status — append-only, tamper-evident
- `devices` table — `id`, `tenant_id`, `device_type`, `vendor`, `connection_params` (encrypted), `gatehouse_id`, `is_active`
- Tenant opt-in: `door_integration_enabled` flag — default false
- Fallback: door controller operates independently (card reader + manual release work without API)
- Event queue with replay: ControlEasy offline → reader queues events → reconnect → replay
- Security: HMAC-SHA256 per-tenant key, nonce + timestamp window, porteiro role required, rate limit
- Threat model document: remote unlock attack surface, key management, replay prevention

**Success Criteria:**
1. Porteiro clicks "open gate" → API sends signed unlock command → door relay fires → gate opens → event logged
2. Unauthorized user (non-porteiro role) → 403
3. Rate limit: 2nd unlock within 3 seconds → 429
4. Replay attack: captured command replayed with old timestamp → rejected
5. ControlEasy offline: card reader still opens door (hardware fallback verified manually)
6. ControlEasy back online: queued reader events replayed into entry log
7. Every unlock command in audit log with user, device, timestamp, command hash
8. `dotnet test` green; adversarial security review passes (zero CRITICAL/HIGH)

**Plans:** TBD

---

### Phase 15: Reader Events & Device Health

**Goal:** Card/biometric reader event ingestion via vendor-specific driver, door events create entry log records (enforced ledger), device health monitoring with alert thresholds. `IDeviceHandler` abstraction emerges from two real integrations.

| | |
|---|---|
| **Depends on** | Phase 14 + second real integration (card/biometric reader) |
| **Requirements** | DOOR-02, DOOR-03 |
| **Spec** | `.specs/door-integration/` (extended) |
| **UI hint** | yes (device health dashboard) |

**Delivers:**
- `IDeviceHandler` abstraction — `HandleEventAsync(deviceEvent)` — strategy per device type. Built from two real integrations.
- First reader integration: one real condominium's card reader or biometric scanner
- Event ingestion pipeline: reader sends event → `IDeviceHandler` normalizes to `DeviceEvent` → entry log created
- Card matches registered dweller → `entered_with_consent` (dweller pre-consented at registration)
- Unknown card → `denied` (or porteiro override per policy)
- `device_events` table — `id`, `tenant_id`, `device_id`, `event_type`, `payload_json`, `occurred_at`, `processed_at`, `entry_log_id` (nullable)
- Device health monitoring:
  - `device_heartbeats` table — `device_id`, `last_heartbeat_at`, `status`, `error_count`
  - Alert thresholds per device: `offline_after_seconds` (default 60), `error_rate_threshold` (default 10%)
  - Health dashboard UI: per-device status, last heartbeat, error rate, alert badges
  - Alert toast in porteiro UI when device goes offline
- Event replay: queued events on reader/gateway replayed when ControlEasy reconnects; `processed_at` tracks ingestion vs event time

**Success Criteria:**
1. Dweller taps card → reader fires → door opens → event ingested → entry log created as `entered_with_consent` → photo not required (pre-consented)
2. Unknown card → reader fires → door stays closed → event ingested → entry log as `denied`
3. Reader goes offline → health dashboard shows "offline" → porteiro UI alert toast
4. Reader back online → queued events replayed → entry logs with correct `occurred_at`
5. `IDeviceHandler` verified: two device types use same interface with different implementations
6. Device health thresholds configurable per device
7. `dotnet test` green; architecture tests pass (`IDeviceHandler` in correct layer)

**Plans:** TBD

---

## Fast-Cycle (Not a Milestone)

### Multi-Arch Docker/CI

Pulled out of v2 per design session recommendation. Low-risk DevOps hygiene, ~1 week.

| | |
|---|---|
| **Requirements** | ARCH-01, ARCH-02 |
| **Spec** | `.specs/1 - modernization-roadmap-arm64/` (existing, 0/8 tasks) |
| **Effort** | ~1 week, no milestone ceremony |

- `docker buildx` multi-arch build for `api` and `web` images (linux/amd64 + linux/arm64)
- GitHub Actions matrix: `ubuntu-latest` (amd64) + `ubuntu-24.04-arm` (arm64) — native runners, no QEMU
- Multi-arch manifest published to GHCR with cosign signing
- `DOCKER_PLATFORM` opt-in env var for cross-arch builds (default: host-native)
- Docs: `docs/operations/arm64.md`
- Verify: `docker compose up` on Apple Silicon pulls arm64 natively

---

## Entry State Matrix (v2.0, Phase 13)

| State | Photo | Who can trigger | Reason code | Notes |
|---|---|---|---|---|
| `entered_with_consent` | Required (DB-enforced) | Porteiro | — | Hard constraint: no photo = rejected |
| `entered_override` | None | Porteiro (dwellers/visitors only) | `emergency` / `vouched` | Porteiro ID logged |
| `gatehouse_only` | None | Porteiro (service providers only) | — | Package dropped, no entry past gate |
| `denied` | None | Porteiro (all categories) | — | Consent refused, entry refused |

---

## Progress

### v2.0 — Gatehouse Photo & Consent Ledger

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 11. Photos & Consent Schema Infrastructure | 0/0 | Not started | - |
| 12. Photo Capture & Display | 0/0 | Not started | - |
| 13. Consent Policy & Gatehouse Workflow | 0/0 | Not started | - |

### v2.1 — Door Integration (Optional)

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 14. Door Relay & Unlock Commands | 0/0 | Not started (gated on real hardware) | - |
| 15. Reader Events & Device Health | 0/0 | Not started (gated on Phase 14) | - |

### Fast-Cycle

| Task | Status | Completed |
|------|--------|-----------|
| Multi-Arch Docker/CI | Not started | - |

---

## Execution Order

```
v1.1 complete (Phase 10)
  │
  ├──▶ Phase 11 (schema) ──▶ Phase 12 (capture) ──▶ Phase 13 (consent)   [v2.0 — linear pipeline]
  │
  ├──▶ Multi-Arch Docker/CI (anytime, independent)
  │
  └──▶ Phase 14 (door relay) ──▶ Phase 15 (reader events)               [v2.1 — gated on real hardware]
```

---
*v2.0 + v2.1 roadmap defined: 2026-08-23*
*Origin: party-mode design session (Vex, Grumbal, Boundary, Yui, Dana, Wildcard, Level, Killjoy, Splinter)*
*Supersedes: old v1.0 placeholder Phase 12 (photo/hardware) and Phase 14 (multi-arch)*