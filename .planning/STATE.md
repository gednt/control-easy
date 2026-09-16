---
gsd_state_version: 1.0
milestone: v2.0
milestone_name: Gatehouse Photo & Consent Ledger
status: completed
last_updated: "2026-09-16T12:00:00.000Z"
last_activity: 2026-09-16
progress:
  total_phases: 3
  completed_phases: 3
  total_plans: 3
  completed_plans: 3
  percent: 100
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-09-13)

**Core value:** Gatehouse staff can reliably register and control access through a fast, tenant-isolated web UI.
**Current focus:** v2.0 milestone COMPLETE (2026-09-13). All 3 phases (11, 12, 13) shipped. No active milestone.

## Current Position

Phase: All v2.0 phases complete
Plan: —
Status: Milestone archived
Last activity: 2026-09-16 — Phases 3..8 of the Gatehouse Access & Visit Destinations feature shipped on `feat/qr-entrance-exit-access`. T033-T074 implemented: vehicle destination tests, manual lookup handler + endpoints, visit destination validation tests, credential lifecycle handlers (issue/replace/revoke) + admin endpoints + audit endpoints, biometric exclusion arch tests, access-control documentation. Solution build green; Docker rebuild pending.

## Performance Metrics

**Velocity:**

- Total plans completed (cumulative): 14 (11 v1.0 + 1 v1.1 + 1 v2.0 Phase 11 + 1 v2.0 Phase 12 + 1 v2.0 Phase 13)
- v2.0 milestone: 3 plans, 3 summaries, 3 verifications, 1 audit, 26 atomic commits
- Average duration per plan: ~5 min (autonomous execution)

## Accumulated Context

### Decisions

- v2.0 photo capture: browser-only, client-side compression (0.8→0.6→0.3 ladder), EXIF strip via canvas redraw, 3× retry with 1s/2s/4s backoff
- v2.0 consent model: per-tenant per-category policy, five entry states (entered_with_consent / entered_override / gatehouse_only / denied / entered_without_consent), append-only audit log, hardcoded override reason codes
- v2.0 design principle: 3-second gatehouse workflow as honesty enforcement; CCTV is external backstop
- Phase 12/13 used forward-compatible shims for backend endpoints not yet shipped:
  - Photos entity binding via localStorage cache
  - Thumbnails via CSS object-fit cover (no /thumbnail route)
  - Consent policy list via 4× parallel category calls (no list endpoint)
  - Pagination total via entries.length (no X-Total-Count header)
- v1.1 Phase 9 (UI parity) explicitly deferred to a future milestone (out of v2.0 scope)
- v2.1 Door Integration (Phases 14-15) gated on real condominium hardware
- Gatehouse access is a software-only validation and audit workflow: QR is the initial credential method, with a protected manual lookup fallback by document, name, apartment, or block. Every visit now requires an apartment/block destination, automatically recovered for residents and associated vehicles; facial biometrics requires a separate future privacy, security, and enrollment specification.
- QR feature implementation pragmatic deviation (2026-09-14): spec-kit generated 74 atomic tasks across 9 phases (Setup, Foundational, US1–US6, Polish). Constitution IV requires per-task Docker rebuild + tests. To stay within realistic session scope, the workflow groups Docker rebuilds at phase boundaries while still producing one atomic commit per task. This deviation was approved explicitly by the operator and is recorded here per Constitution VII (concurrency-budget deviation rationale lives next to the work it justifies). Reviewer actions: per-task rebuild pass can be re-run during `/gsd-verify-work`.
- Multi-arch Docker/CI is fast-cycle (no milestone)

### Blockers/Concerns

- No blocking concerns. v2.0 milestone is complete.
- v1.1 Phase 9 partial work (residents page only) deferred to future milestone.
- v2.1 (Phases 14-15) gated on hardware.
- OpenAPI client not yet consumed by handwritten Angular services (carried over from v1.0 tech debt).
- Photo entity binding is now persisted by the Photos API; existing MySQL databases need the versioned migration once.

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| v1.1 | UI Parity & Functional Fixes (Phase 9 partial) | Residents page only; login/dashboard/showcase/visits/vehicles/etc. pending | 2026-09-13 |
| v2.0 (tech debt) | Backend /api/v1/photos/{id}/thumbnail route | Forward-compatible shim in place (CSS object-fit cover) | 2026-09-13 |
| v2.0 (tech debt) | Backend /api/v1/consent-policy list endpoint | Forward-compatible shim (4× parallel calls) | 2026-09-13 |
| v2.0 (tech debt) | Backend X-Total-Count header on /api/v1/entry-log | Forward-compatible shim (entries.length approximation) | 2026-09-13 |
| v2.1 | Door relay & unlock commands (DOOR-01) | Phase 14, gated on hardware | 2026-08-23 |
| v2.1 | Reader events & device health (DOOR-02, DOOR-03) | Phase 15, gated on Phase 14 | 2026-08-23 |
| Future access method | Facial biometrics | Explicitly deferred; requires separate approved specification before biometric enrollment, matching, or storage | 2026-09-13 |
| Fast-cycle | Multi-arch Docker/CI (ARCH-01, ARCH-02) | No milestone, ~1 week | 2026-06-24 |
| v1.0 | OpenAPI client integration with handwritten services | Carried over | 2026-09-12 |

## Session Continuity

Last session: 2026-09-14
Stopped at: Spec-kit /speckit-tasks + Phase 1 (Setup) of /speckit-implement complete on `feat/qr-entrance-exit-access` (7 atomic commits). 68 tasks remain across Phase 2 (Foundational) → Phase 9 (Polish). Pragmatic deviation: phase-boundary Docker rebuilds instead of per-task rebuilds (recorded under Decisions per Constitution VII).
Resume file: None

## Operator Next Steps

- Run `/gsd-new-milestone` to define next milestone (likely v1.1 Phase 9 completion OR v2.1 Door Integration prep OR fast-cycle multi-arch)
- v2.0 ship commit ready for human review on `feat/planning-reconcile-v2` (26 atomic commits, clean working tree)
- Forward-compatible shims can be removed by future backend work (4 backend endpoints/migrations)
- v2.1 roadmap at `.planning/milestones/v2.0-ROADMAP.md` (Phases 14-15, gated on hardware)
- Multi-arch Docker/CI: fast-cycle, ~1 week, `.specs/1 - modernization-roadmap-arm64/`
