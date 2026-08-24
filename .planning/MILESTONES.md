# Milestones

## v1.0 Reborn MVP (Shipped: 2026-07-13)

**Phases completed:** 11 phases, 11 plans, 0 tasks

**Delivered:** A tenant-isolated ASP.NET Core and Angular access-control platform with core condominium modules, canonical Docker runtime, hardened design system, demo environment, and continuous engineering gates.

**Key accomplishments:**

- Shipped the modular monolith and tenant-aware Residents, Visits, Vehicles, ServiceProviders, Apartments, Security, Administration, and Reports slices.
- Migrated persistence to the official DBTools 1.4.3 NuGet package and added a DBTools-backed runtime health check.
- Hardened the Angular design system with canonical tokens, reusable `ce-*` components, accessibility coverage, and visual regression snapshots.
- Added deterministic demo mode, first-boot administration, and tenant-switching flows.
- Added GitHub Actions, generated OpenAPI artifacts with drift enforcement, tenant-isolation guards, and token contract tests.

**Verification:** 79 unit, 55 integration, 6 architecture, and 203 Angular tests passed; Docker API/web rebuilt and `/health` returned `Healthy`.

**Known debt:** Generated OpenAPI artifacts are drift-checked but handwritten Angular services do not consume them yet.

**What's next:** v1.1 UI parity, dashboard live data, and vehicle editing.

---

## v1.1 UI & Dashboard (Defined: 2026-08-23)

**Phases:** 9, 10
**Status:** Planning (not started)

**Scope:** Complete the deferred UI parity, functional fixes, dashboard live data, and vehicle editing work from the v1.0-era user experience surface.

**Phases:**
- Phase 9: UI Parity & Functional Fixes (`UI-03`, `UI-04`)
- Phase 10: Dashboard Live Stats & Vehicle Edit (`DASH-01`, `DASH-02`, `DASH-04`)

**Roadmap:** `.planning/ROADMAP.md`

---

## v2.0 Gatehouse Photo & Consent Ledger (Defined: 2026-08-23)

**Phases:** 11, 12, 13
**Status:** Defined (not started — follows v1.1)

**Scope:** Browser-based photo capture for residents, visitors, vehicles, and service providers with per-tenant per-category consent policy. The porteiro captures a photo in under 3 seconds. The condominium sets whether photo consent is required per category. Every entry is logged with millisecond-precision timestamps cross-referenceable with the condominium's external CCTV. No hardware framework — the camera is the browser's. No server-side image processing — all compression, EXIF stripping, and thumbnailing is client-side.

**Phases:**
- Phase 11: Photos & Consent Schema Infrastructure (`PHOTO-01`) — `IStorageProvider`, `photos` table, `consent_audit_log` table, `tenant_consent_policy` table, permissions, endpoints
- Phase 12: Photo Capture & Display (`PHOTO-02`) — `ce-photo-capture` component, `getUserMedia`, client-side compression, EXIF strip, thumbnails, upload retry, `ce-photo` display
- Phase 13: Consent Policy & Gatehouse Workflow (`CONSENT-01`, `CONSENT-02`, `CONSENT-03`) — per-category policy toggle, four entry states, 3-second workflow, audit review, CSV export

**Design principles:**
1. Logging is faster than skipping — 3-second workflow is the honesty enforcement
2. CCTV is the external backstop — ControlEasy carries timestamps, not footage
3. ControlEasy enables, doesn't enforce — policy is the condominium's
4. Client-side only for image processing — no server-side compression/thumbnailing/EXIF
5. No approval workflow, no anomaly detection — ship visibility

**Specs:** `.specs/photo-capture/`, `.specs/consent-gatehouse/`
**Roadmap:** `.planning/milestones/v2-ROADMAP.md`
**Origin:** Party-mode design session (2026-08-23)

---

## v2.1 Door Integration — Optional (Defined: 2026-08-23)

**Phases:** 14, 15
**Status:** Defined (gated on real condominium with hardware)

**Scope:** Condominiums that opt in can integrate ControlEasy with their door relay and card/biometric readers. The API triggers the relay with HMAC-signed commands; reader events create entry log records automatically. The door opens independently of ControlEasy — the API is a participant, not a gatekeeper. `IDeviceHandler` abstraction emerges from the second real integration, not speculatively.

**Phases:**
- Phase 14: Door Relay & Unlock Commands (`DOOR-01`) — `IDoorController`, HMAC-SHA256 signed unlock, rate-limited, audit-logged, hardware fallback, threat model
- Phase 15: Reader Events & Device Health (`DOOR-02`, `DOOR-03`) — `IDeviceHandler` (from 2 real integrations), `DeviceEvent` normalization, enforced ledger, device health monitoring

**Design principles:**
1. ControlEasy is a participant, not a gatekeeper — door opens independently; API observes + triggers
2. No speculative abstractions — `IDeviceHandler` from 2nd integration, not before
3. Security-sensitive — remote unlock is a new attack surface; own threat model, own adversarial review
4. Cameras are external — ControlEasy does not deploy/manage/proxy cameras

**Specs:** `.specs/door-integration/`
**Roadmap:** `.planning/milestones/v2-ROADMAP.md`
**Origin:** Party-mode design session (2026-08-23)

---

## Fast-Cycle: Multi-Arch Docker/CI (No Milestone)

**Status:** Defined (~1 week, no ceremony)

**Scope:** Multi-arch Docker builds (linux/amd64 + linux/arm64), GitHub Actions native-runner matrix, GHCR multi-arch manifest with cosign signing, `DOCKER_PLATFORM` opt-in env var. Pulled out of v2 milestones — low-risk DevOps hygiene.

**Specs:** `.specs/1 - modernization-roadmap-arm64/`
**Requirements:** `ARCH-01`, `ARCH-02`

---
