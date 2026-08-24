---
gsd_state_version: 1.0
milestone: v1.1
milestone_name: UI & Dashboard
status: planning
last_updated: "2026-08-23T23:59:00.000Z"
last_activity: 2026-08-23
progress:
  total_phases: 2
  completed_phases: 0
  total_plans: 0
  completed_plans: 0
  percent: 0
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-06-24)

**Core value:** Gatehouse staff can reliably register and control access through a fast, tenant-isolated web UI.
**Current focus:** v1.1 UI & Dashboard in planning; v2.0 Gatehouse Photo & Consent Ledger defined (next milestone); v2.1 Door Integration defined (gated on hardware)

## Current Position

Phase: Not started (roadmap defined)
Plan: —
Status: Planning
Last activity: 2026-08-23 — v1.1 roadmap created (Phase 9 + Phase 10)

## Performance Metrics

**Velocity:**

- Total plans completed: 11 (v1.0)
- Average duration: —
- Total execution time: —

## Accumulated Context

### Decisions

- DBTools migration targets [NuGet DBTools 1.4.3](https://www.nuget.org/packages/DBTools) — replaces vendored `src/lib/DBTools_SQL/`
- Phase 7 inserted before design system work; phases 8–14 renumbered
- MySqlConnector stays pinned as optional MySQL provider per NuGet package docs
- v1.1 phase numbering continues from v1.0 — Phase 9 (UI parity) and Phase 10 (dashboard + vehicle edit) retain their original numbers from the v1.0 archive
- Phase 10 backend (DASH-01) has no hard dependency on Phase 9; DASH-02 UI benefits from Phase 9 `ce-*` adoption — phases may run in parallel
- v2.0 photo capture is browser-only (getUserMedia + file upload) — no hardware framework, no MQTT, no WebRTC (party-mode design 2026-08-23)
- v2.0 image processing is entirely client-side — resize ≤1280px, compress ≤500KB JPEG, EXIF strip, 128×128 thumbnail — no server-side processing
- v2.0 consent model: per-tenant per-category toggle, four entry states, append-only audit log, hardcoded reason codes (emergency/vouched) — no rules engine, no approval workflow
- v2.0 design principle: logging must be faster than skipping (3-second gatehouse workflow); CCTV is external backstop; ControlEasy carries timestamps, not footage
- v2.1 door integration: API is participant, not gatekeeper — door opens independently (hardware fallback); `IDeviceHandler` emerges from second real integration, not speculative
- Old `.specs/3 - photo-capture-hardware-integration/` retired and superseded by `photo-capture`, `consent-gatehouse`, `door-integration` specs
- Multi-arch Docker/CI pulled out of v2 milestones — fast-cycle task, ~1 week, no ceremony

### Blockers/Concerns

- No blocking concerns. v1.1 in planning; v2.0/v2.1 defined and ready for future planning. v2.1 gated on real condominium hardware.

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| v2.0 | Photo capture & storage (PHOTO-01, PHOTO-02) | Phase 11–12, defined | 2026-08-23 |
| v2.0 | Consent policy & gatehouse workflow (CONSENT-01, CONSENT-02, CONSENT-03) | Phase 13, defined | 2026-08-23 |
| v2.1 | Door relay & unlock commands (DOOR-01) | Phase 14, gated on hardware | 2026-08-23 |
| v2.1 | Reader events & device health (DOOR-02, DOOR-03) | Phase 15, gated on Phase 14 | 2026-08-23 |
| Fast-cycle | Multi-arch Docker/CI (ARCH-01, ARCH-02) | No milestone, ~1 week | 2026-06-24 |
| Retired | Old photo-capture-hardware spec (MQTT/WebRTC/biometrics) | Retired, superseded | 2026-08-23 |

## Session Continuity

Last session: 2026-08-23
Stopped at: v2.0 + v2.1 milestones defined via party-mode design session; old photo-capture-hardware spec retired
Resume file: None

## Operator Next Steps

- Plan the first v1.1 phase with `/gsd-plan-phase 9`
- v2.0 roadmap at `.planning/milestones/v2-ROADMAP.md` (Phases 11–13)
- v2.1 roadmap at `.planning/milestones/v2-ROADMAP.md` (Phases 14–15, gated on hardware)
- Multi-arch Docker/CI: fast-cycle, ~1 week, `.specs/1 - modernization-roadmap-arm64/`