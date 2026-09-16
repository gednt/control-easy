# ControlEasy Reborn

## What This Is

ControlEasy Reborn is the web-based modernization of ControlEasy 5 — a condominium access-control platform used by gatehouse staff (porteiro), tenant administrators, and platform operators to manage residents, visitors, vehicles, service providers, apartments, and gatehouse flow. The new stack is an ASP.NET Core 8 modular monolith with an Angular 18 SPA, MySQL 8, and Docker Compose, replacing the legacy WPF desktop app via a Strangler Fig migration.

## Core Value

Gatehouse staff can reliably register and control access for residents, visitors, and vehicles through a fast, tenant-isolated web UI backed by a secure multi-tenant API.

## Current State

**v1.0 Reborn MVP shipped 2026-07-13.** The active milestone delivered 32/32 requirements across 11 phases. The standard Docker Compose runtime is healthy and demo mode remains isolated to its opt-in overlay.

**Post-v1.0 work shipped 2026-09-12:**
- v1.1 Phase 10 (dashboard + vehicle edit) shipped without GSD artifacts
- v1.1 Phase 9 (residents page rebuild) partially shipped without GSD artifacts
- v2.0 Phase 11 (Photos & Consent Schema) shipped — full Photos module, storage abstraction, consent audit log, append-only triggers, 118 unit + 73 integration tests
- v2.0 Phase 13 backend (consent policy + entry log + CSV export) shipped in same commit
- Unplanned: tenant staff lifecycle management, bootstrap race fix, Traefik TLS, worktree script fixes

## Current Milestone

**v1.1 UI & Dashboard** (started 2026-08-23) — partially shipped (Phase 10 done, Phase 9 partial)

**v2.0 Gatehouse Photo & Consent Ledger** (defined 2026-08-23) — in progress (Phase 11 done, Phase 13 backend done, Phase 12 not started)

**v2.1 Door Integration** (defined 2026-08-23) — gated on real condominium hardware

Goal: Complete the remaining UI parity work (v1.1 Phase 9 — other feature pages), then ship browser-based photo capture UI (v2.0 Phase 12) and consent gatehouse workflow UI (v2.0 Phase 13), then optional door/card-reader integration for condominiums that opt in (v2.1).

Target features:
- UI Parity & Functional Fixes (Phase 9 — `UI-03`, `UI-04`): mockup visual parity + interaction fixes — **residents page done, other pages pending**
- Dashboard Live Stats & Vehicle Edit (Phase 10 — `DASH-01`, `DASH-02`, `DASH-04`): live tenant-scoped dashboard statistics; vehicle editing workflow — **shipped**
- Photo Capture & Storage (Phase 11–12 — `PHOTO-01`, `PHOTO-02`): browser camera + upload, client-side compression, S3/MinIO storage, photos on resident/visitor/vehicle/service-provider records — **Phase 11 shipped, Phase 12 pending**
- Consent Policy & Gatehouse Workflow (Phase 13 — `CONSENT-01`, `CONSENT-02`, `CONSENT-03`): per-tenant per-category consent policy, four entry states, 3-second gatehouse workflow, append-only audit log, CSV export for CCTV cross-reference — **backend shipped, UI pending**
- Door Relay & Unlock Commands (Phase 14 — `DOOR-01`): optional, gated on real hardware, HMAC-signed unlock, hardware fallback
- Reader Events & Device Health (Phase 15 — `DOOR-02`, `DOOR-03`): card reader event ingestion, enforced ledger, device health monitoring

Phase numbering continues from v1.1 (Phase 10). Old v1.0 placeholder phases (12, 14) are superseded by v2.0/v2.1 design. Multi-arch Docker/CI is a fast-cycle task, not a milestone.

## Requirements

### Validated

- ✓ Modular monolith foundation (Tenants, Security, Residents, Docker, JWT, DBTools_SQL) — Phase 1 / `.specs/1 - modernization-roadmap/` tasks 1.0a–1.17
- ✓ Strangler pilot (Security module, login, Residents admin, feature flags) — Phase 2
- ✓ Module migrations (Visits, Vehicles, ServiceProviders, Administration, Reports CQRS-lite, Playwright) — Phase 3
- ✓ Legacy decommission tasks (cancelled — WPF not in repo) — Phase 4
- ✓ Post-login tenant-scoped routing and session endpoint — `.specs/post-login-role-routing/`
- ✓ Tenant administration UI (PlatformAdmin condominiums page) — `.specs/tenant-administration-ui/`
- ✓ PlatformAdmin first-boot login bugfix — `.specs/platform-admin-first-boot/`
- ✓ Apartments module + frontend wiring — `.specs/apartments-and-residents-form/`
- ✓ Apartment edit: manage residents inline — `.specs/apartment-edit-residents/`
- ✓ First-boot operator documentation — `.specs/getting-started/`
- ✓ Verification remediation (Playwright auth, demo integration, spec baseline) — `.specs/6 - verification-remediation/`
- ✓ DBTools_SQL migrated to NuGet DBTools 1.4.3 — `.specs/dbtools-nuget-migration/`
- ✓ Design system hardening and formal design-system contract — `.specs/fix-design-system/`, `.specs/2 - visual-design-system/`
- ✓ Occupied-apartments dashboard count regression fixed with tenant-isolation coverage — `.specs/occupied-apartments-bug/`
- ✓ Demo mode one-command seeded evaluation stack — `.specs/4 - demo-mode/`
- ✓ Continuous engineering baseline (CI, generated OpenAPI client, architecture and token contracts) — `.specs/1 - modernization-roadmap/` C.1–C.7
- ✓ Photo capture & hardware integration spec retired and superseded — `.specs/_retired/3 - photo-capture-hardware-integration/` → `.specs/photo-capture/` + `.specs/consent-gatehouse/` + `.specs/door-integration/`
- ✓ Dashboard live stats endpoint and UI (DASH-01, DASH-02) — Phase 10, shipped 2026-09-12
- ✓ Vehicle edit UI (DASH-04) — Phase 10, shipped 2026-09-12
- ✓ Photo storage infrastructure & API (PHOTO-01) — Phase 11, commit `d895c01`
- ✓ Consent policy config (CONSENT-01) — Phase 13 backend, commit `d895c01`
- ✓ Gatehouse entry workflow backend (CONSENT-02) — Phase 13 backend, commit `d895c01`
- ✓ Tenant staff lifecycle management (unplanned) — commit `55377a0`

### Active

- [~] Mockup visual parity (v1.1, Phase 9) — `UI-03` — residents page done; other pages pending — `.specs/mockup-visual-parity/`
- [~] Mockup functional fixes (v1.1, Phase 9) — `UI-04` — residents page interactions done; login form, toasts, other pages pending — `.specs/2-mockup-functional-fixes/`
- [ ] Photo browser capture & display (v2.0, Phase 12) — `PHOTO-02` — `.specs/photo-capture/`
- [~] Consent gatehouse workflow UI (v2.0, Phase 13) — `CONSENT-03` — backend done; audit review UI pending — `.specs/consent-gatehouse/`
- [ ] Package drop tracking — record package descriptions, held-at-lodge inventory, and collection accountability — `.specs/package-drop-tracking/`
- [ ] Door integration (v2.1, gated on hardware) — `.specs/door-integration/`
- [ ] Multi-arch Docker/CI (fast-cycle) — `.specs/1 - modernization-roadmap-arm64/`

### Out of Scope

- Legacy WPF coexistence smoke tests — WPF not in repo; macOS dev environment cannot run WPF
- WPF project removal from repo — WPF lives outside `ControlEasyReborn.sln`
- Production cutover checklist — owned by separate DevOps spec
- Legacy data migration from `controlEasyDB.db` — separate data-migration spec
- Mobile-native apps — responsive web / PWA sufficient for v1
- Billing / subscription management — external system

## Context

Brownfield modernization project. The repository contains ControlEasy Reborn only; legacy ControlEasy5 (WPF) and ControlEasyWeb (Blazor/.NET 5) are referenced in docs but not present in this repo.

Planning is derived from `.specs/` (20+ spec folders). Codebase intelligence lives in `.planning/codebase/`.

Known concerns from codebase map: dual tenant column patterns, JWT/localStorage defaults, generated OpenAPI client not consumed by handwritten Angular services (tech debt from v1.0), test coverage gaps.

## Constraints

- **Tech stack**: ASP.NET Core 8, Angular 18+, MySQL 8, DBTools NuGet package (LINQ-first), Docker Compose — per `AGENTS.md`
- **Architecture**: Modular monolith, Clean Architecture per module, multi-tenant shared schema
- **Data access**: No Entity Framework or MySql.Data in Reborn code; all queries via `IAsyncSqlClient` / `Linq<T>`
- **API style**: REST, JSON, kebab-case routes, `/api/v1/*`, ProblemDetails errors
- **Visual contract**: `mockup/` prototype + `docs/penpot/tokens.json` + `.specs/2 - visual-design-system/`

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Strangler Fig migration | Keep legacy running during cutover | ✓ Good — Phases 1–3 complete; WPF coexistence cancelled (not in repo) |
| Modular monolith over microservices | Single deployable today, extract later | ✓ Good — 10 feature modules in place (9 original + Photos) |
| DBTools over EF Core | LINQ-first, multi-provider, matches AGENTS.md | ✓ Good — vendored DBTools_SQL replaced with NuGet 1.4.3 (Phase 7) |
| Multi-tenant shared schema + JWT `tenant_id` | Host many condominiums on one deployment | ✓ Good — TenantFilterInterceptor wired |
| Angular 18 + Tailwind v4 design system | Modern SPA, mockup parity target | ✓ Good — design system hardened (Phase 8); residents page rebuilt with `ce-*` components |
| Spec-driven development via `.specs/` | AGENTS.md mandates spec-first workflow | ✓ Good — source of truth for roadmap |
| v2.0 photo capture: browser-only, client-side compression | No hardware framework; cameras belong to condominium | ✓ Good — party-mode design session 2026-08-23 |
| v2.0 consent: per-tenant per-category policy, no rules engine | ControlEasy enables, doesn't enforce; CCTV is backstop | ✓ Good — party-mode design session 2026-08-23 |
| v2.1 door: participant, not gatekeeper | Door opens independently; API observes + triggers, doesn't block | ✓ Good — hardware fallback required |
| `IDeviceHandler` emerges from 2nd integration, not speculative | Avoid premature abstraction | ✓ Good — party-mode design session 2026-08-23 |
| Phase 11 + Phase 13 backend shipped together | Implementation collapsed the 11/13 boundary; schema + consent backend are co-dependent | ✓ Good — commit `d895c01` |
| S3 storage: dual provider (MinIO-compatible + Amazon S3) | Support both self-hosted MinIO and native AWS S3 | ✓ Good — `S3StorageProvider` + `AmazonS3StorageProvider` |
| Tenant staff lifecycle shipped as unplanned work | Admin/porteiro CRUD was needed for real-world condominium management | ✓ Good — commit `55377a0` |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-09-12 — audited against codebase; v1.1 Phase 10 + v2.0 Phase 11 + Phase 13 backend marked validated; v1.1 Phase 9 marked partial; tenant staff lifecycle added as validated*
