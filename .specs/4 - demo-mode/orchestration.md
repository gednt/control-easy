# Orchestration Log — Demo Mode

> Spec folder: `.specs/4 - demo-mode/`
> This log tracks the multi-agent work for designing a **Demo Mode** for ControlEasy Reborn — a one-command, pre-populated stack for sales demos, stakeholder walkthroughs, onboarding training, and local smoke testing without manual seeding.

## Goal

Design a technical implementation plan for **Demo Mode** so anyone can run:

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build
```

…and immediately sign in with documented credentials, explore two condominiums (multi-tenant picker), and see realistic residents, visits, vehicles, service providers, attendants, and gatehouse activity — without random bootstrap passwords or an empty database.

## Classification

- **Type:** Feature spec (cross-cutting: DevOps, Backend, Frontend, Documentation).
- **Agent:** Orchestrator (Backend + DevOps + Frontend + Documentation phases).
- **Companion specs:**
  - `.specs/1 - modernization-roadmap/` — Docker Compose baseline, multi-tenancy, Security module, feature flags.
  - `.specs/2 - visual-design-system/` — login page, app shell, demo banner styling.
  - `.specs/2-strangler-pilot/` — real JWT login flow and Residents module (demo must exercise the live stack, not the HTML mockup).
  - Thematic pop-culture fixtures — residents are characters from **Chaves**, **GTA**, and **God of War (Greek era)** (see `design.md` → Demo resident roster).

## Execution Plan

| Phase | Agent | Status | Output |
|---|---|---|---|
| 1 | Orchestrator (as Backend + DevOps) | done | `requirements.md` (UC-41..UC-54), `design.md` (DemoSeeder, compose overlay, seed SQL, cohabitation roster, API surface, security guardrails) |
| 2 | Orchestrator (as Frontend) | done | `design.md` frontend sections (demo banner, login shortcuts, `/demo` help panel) |
| 3 | Orchestrator (as Documentation) | done | `design.md` → `docs/demo-mode.md` outline; credential table in design |
| 4 | Orchestrator | done | `tasks.md` (Phase 8 with dependency graph, verification gates) |

## Dependencies on Existing Specs

- Requires Phase 1 foundation (Docker Compose, MySQL init scripts, JWT auth) and Phase 2 Security module (login, tenant lookup, attendant profiles) from `.specs/1 - modernization-roadmap/` and `.specs/2-strangler-pilot/`.
- Demo seed data must respect **C.6**: every business row carries `tenant_id`; cross-tenant isolation tests still pass when demo mode is **off**.
- Demo mode must **not** modify production behaviour when `Demo__Enabled=false` (default).
- Visual components follow `.specs/2 - visual-design-system/` (`ce-badge`, `ce-card`, login page layout).

## Key Decisions (resolved)

1. **Opt-in activation:** Demo mode is enabled only when `Demo__Enabled=true` (API env var) via `docker-compose.demo.yml` overlay. Default `docker compose up` remains a clean dev stack with random `PlatformAdminBootstrapService` credentials.
2. **Idempotent seeder:** `DemoSeederService : IHostedService` runs at API startup when demo mode is on. It checks a `DemoSeedVersion` marker row in a new `DemoMetadata` table (or config key) and skips if already seeded at the current version. Re-seed is triggered by bumping `Demo__SeedVersion` or calling `POST /api/v1/demo/reset` (PlatformAdmin-only).
3. **Fixed credentials:** All demo passwords are `demo123` (BCrypt-hashed in seed). Documented in `docs/demo-mode.md`. No random password generation when demo mode is active — `PlatformAdminBootstrapService` is suppressed.
4. **Two demo tenants** (all demo tenants use the **`[Demo]`** display-name prefix and **`demo-`** slug prefix):
   - **`[Demo] Residencial Aurora`** (`slug: demo-aurora`) — primary walkthrough tenant; **61 thematic residents** (20 Chaves + 20 GTA + 21 God of War Greek era incl. **Atreus**); Chaves/GoW cohabitation per `design.md`.
   - **`[Demo] Condomínio Parque Verde`** (`slug: demo-parque-verde`) — second tenant so `multi@controleasy.app` triggers the tenant picker on login.
5. **Demo personas (one User per persona, shared password):**

   | Email | Role | Tenant(s) | Purpose |
   |---|---|---|---|
   | `platform@controleasy.app` | PlatformAdmin | platform | Tenant administration |
   | `admin@controleasy.app` | TenantAdmin | `[Demo] Residencial Aurora` | Full tenant admin walkthrough |
   | `porteiro@controleasy.app` | AttendantProfile | `[Demo] Residencial Aurora` | Gatehouse / portaria flow |
   | `morador@controleasy.app` | Morador | `[Demo] Residencial Aurora` | Resident self-service (future screens) |
   | `multi@controleasy.app` | TenantAdmin + Attendant | `[Demo] Residencial Aurora` + `[Demo] Condomínio Parque Verde` | Multi-tenant picker demo |

6. **Feature flags in demo:** All `*.UseWeb` flags default to `true` in `docker-compose.demo.yml` so every migrated module is visible during demos.
7. **UI demo banner:** When `GET /api/v1/demo/info` returns `{ "enabled": true }`, the Angular app shell renders a persistent, dismissible banner: *"Demo mode — sample data; changes may be reset."* Styling uses `ce-badge tone-warning` per the design system.
8. **Login shortcuts:** On demo mode only, the login page shows a collapsible "Try a demo account" panel with one-click fill for each persona (no auto-submit; user still clicks Sign in).
9. **Reset strategy:** Local demos rely on `docker compose down -v` for a full reset. Hosted public demos (future) use a nightly `demo-reset` sidecar cron that calls the reset endpoint. v1 ships the endpoint + compose comment; cron container is optional in `docker-compose.demo.yml`.
10. **Security guardrails when demo is on:**
    - Outbound email disabled (`Demo__DisableOutboundEmail=true`).
    - `POST /api/v1/demo/reset` requires `PlatformAdmin` + demo mode enabled.
    - Demo credentials rejected when `Demo__Enabled=false` (seed rows absent; login fails normally).
    - JWT signing key in demo compose uses a well-known dev key (documented as **never for production**).
11. **Cohabitation + Atreus (2026-06-13):** Chaves households and GoW mythic households share apartments per `design.md` cohabitation rules. **Atreus** is the sole Norse-era exception, co-dwelling with Kratos at **Sparta-1**. Total Aurora residents = **61** (20 + 20 + 21).

## Open Items — NONE (2026-06-13)

All decisions resolved in this orchestration pass. Implementation proceeds per `tasks.md`.

## Coordination Log

### Phase 1 — Orchestrator (Backend + DevOps design)

- **Completed:** `requirements.md` with 14 user stories (UC-41..UC-54).
- **Completed:** `design.md` backend sections — `DemoOptions`, `DemoSeederService`, `DemoMetadata` table, seed SQL strategy (`10-demo-seed.sql` + C# seeder for dynamic dates), `GET /api/v1/demo/info`, `POST /api/v1/demo/reset`, suppression of `PlatformAdminBootstrapService`, `docker-compose.demo.yml` overlay.
- **Key design decisions:** Hybrid seed (SQL for static schema rows, C# seeder for relative timestamps like "2 hours ago" on activity feed). Demo module lives in `BuildingBlocks` or a thin `Modules/Demo/` slice — no new domain aggregates; seeder writes into existing module tables.

### Phase 2 — Orchestrator (Frontend design)

- **Completed:** `design.md` frontend sections — `DemoInfoService`, app-shell banner, login-page demo shortcuts panel, optional `/help/demo` route with credential table and suggested walkthrough script.
- **Key pattern:** Demo UI is gated on `DemoInfoService.enabled()` signal populated at app init from `GET /api/v1/demo/info`; zero demo-specific UI when endpoint returns `enabled: false`.

### Phase 3 — Orchestrator (Documentation)

- **Completed:** `docs/demo-mode.md` outline embedded in `design.md` (task 8.10 creates the file).
- **Walkthrough script:** Login as porteiro → dashboard → search "Kratos" or "Chaves" in residents → open visit (when Visits module ships) → switch tenant via multi@ account.

### Phase 4 — Orchestrator (Tasks)

- **Completed:** `tasks.md` with Phase 8 (8 tasks), dependency graph (4 waves), verification gate checklist.
- **Continuous task C.11:** Every implementation task in this repo must end with Docker image rebuild + compose restart (codified in root `AGENTS.md`).

## Verification Status (spec-only)

- Spec artifacts present: `requirements.md`, `design.md`, `tasks.md`, `orchestration.md`.
- No code changes in this phase — implementation is Wave 1 onward per `tasks.md`.
