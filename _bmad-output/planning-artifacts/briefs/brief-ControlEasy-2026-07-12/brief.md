---
title: "Product Brief — ControlEasy Reborn Demo Mode"
status: draft
created: 2026-07-12
updated: 2026-07-12
brief_type: product-brief
project: ControlEasy
feature: demo-mode
spec_folder: .specs/4 - demo-mode/
author: bmad-product-brief (headless)
run: brief-ControlEasy-2026-07-12
related:
  - _bmad-output/planning-artifacts/briefs/brief-ControlEasy-2026-07-12/addendum.md
  - _bmad-output/planning-artifacts/briefs/brief-ControlEasy-2026-07-12/.memlog.md
  - .specs/4 - demo-mode/requirements.md
  - .specs/4 - demo-mode/design.md
  - .specs/4 - demo-mode/tasks.md
  - docs/demo-mode.md
  - docs/index.md
---

# Product Brief: ControlEasy Reborn Demo Mode

## Executive Summary

ControlEasy Reborn sells to condominium managers and operators who replace a legacy Windows desktop app. The hardest moment in that sale is the **first hands-on** — when a prospect needs to see real, populated screens, not a login prompt and an empty database. Today, that moment is broken: the only way to populate the stack is to manually run the `PlatformAdminBootstrapService`, log in, register a condominium, and hand-type every resident. Sales engineers lose half their demo to setup. Trainers ship laptops with stale snapshots. New developers boot the stack, see "0 residents," and assume the product is empty.

Demo Mode is a **one-command, pre-populated, safe-to-reset deployment profile** of the existing Reborn stack. A single `docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build` brings the full system to life with two themed condominiums (61 + 6 residents), five fixed personas (`demo123` for all), and a banner that always tells the operator they are in demo data. The same stack supports stakeholder evaluation, internal training, and local smoke testing with no code duplication.

## The Problem

Three roles hit the same friction on every first-run:

- **Sales engineers** demo to prospects who are five minutes from a decision. The prospect is not going to wait through manual data entry. Today, the sales engineer either (a) ships a frozen snapshot (drifts behind the codebase within weeks) or (b) types on stage (high failure rate, embarrassing).
- **Trainers** onboard new gatehouse staff and condo administrators. They need a reproducible environment with realistic-but-memorable data: a list of 61 residents where "Dona Florinda shares apt 8 with Quico and Prof. Girafales" is a teaching moment, not a clerical exercise.
- **Developers and QA** need to smoke-test the stack without polluting the canonical dev database. They want a `docker compose down -v && up` that returns to a known baseline.

The cost of the status quo is **slow first impressions, broken dogfooding, and an empty database masquerading as a working product**. The cost of a *bad* demo is worse: a sales prospect who logs into a half-seeded stack, sees "0 residents," and concludes the product is unfinished.

## The Solution

Demo Mode is a **runtime profile**, not a separate product. When `Demo__Enabled=true` is set (via the `docker-compose.demo.yml` overlay), the API on first boot:

1. Seeds two tenants — `[Demo] Residencial Aurora` (61 residents) and `[Demo] Condomínio Parque Verde` (6 residents) — using curated, themed fixtures (Chaves / GTA / God of War Greek era + Atreus).
2. Creates five demo personas (PlatformAdmin, TenantAdmin, Attendant, Morador, multi-tenant) with the documented password `demo123` and matching `AttendantProfiles`, `Shifts`, and `Gatehouses` rows.
3. Suppresses the random `PlatformAdminBootstrapService` so demo creds are predictable.
4. Writes a one-row `DemoMetadata` table that records the seed version, so restarts are idempotent.

The Angular SPA detects demo mode via `GET /api/v1/demo/info` and surfaces a **persistent banner** ("Modo demonstração — dados de exemplo; alterações podem ser restauradas."), a **collapsible "Try a demo account" panel** on the login page, and a **/help/demo route** with the credential table and a 5-step walkthrough. A `POST /api/v1/demo/reset` endpoint (PlatformAdmin only) wipes and re-seeds in place. `docker compose down -v && up` returns to a clean baseline in under two minutes.

**Demo mode does not introduce a new business module.** It reuses every existing endpoint, design-system component, and architecture constraint. The only new surface is a configuration switch, a startup hosted service, two demo endpoints, and a handful of front-end affordances.

## What Makes This Different

- **Themed, canonical, fan-pleasing data** — the 61-resident roster is a contract, not a placeholder. Florinda, Quico, and Prof. Girafales share **apt 8** because that's their home in *Chaves*. Kratos and Atreus share **Sparta-1** because that's their home in *God of War* (the spec explicitly excludes Norse-era characters; Atreus is the documented exception). The roster is what makes a demo memorable.
- **Hard guardrails, not soft warnings** — when `Demo__Enabled=false`, no demo users exist, the demo endpoints return 404, and the random `PlatformAdminBootstrapService` is restored. There is no path to accidentally running demo data in production.
- **Idempotent re-seeding** — the `DemoMetadata.SeedVersion` row means restarts are no-ops; bumping the version forces a re-seed. Sales engineers don't have to think about state.
- **Operator-facing docs already ship with the code** — `docs/demo-mode.md` is part of the deliverable (task 8.10), not a separate wiki page that drifts. The brief, the spec, the code, and the operator doc are co-located.
- **Spec-driven, not ad-hoc** — `.specs/4 - demo-mode/` carries 11 numbered tasks (8.1–8.10 plus a verification gate) with a documented four-wave dependency graph. The build is reproducible by anyone reading the spec.

## Who This Serves

**Primary:**

- **Sales engineer (UC-41, UC-47).** Wants one command, a credible-looking screen, and no surprises. Success = 2 minutes from `docker compose up` to "look at these 61 residents and the visit flow." Failure = an empty list, a crashed container, or a "where's the data?" moment on stage.
- **Demo prospect / new user (UC-46).** Wants to click "Try a demo account" on the login page and explore without reading a README. Success = in-app, in under 10 seconds. Failure = a credential form, a sign-up screen, a "contact sales" gate.
- **Trainer (UC-53).** Wants a repeatable lab environment for 5–10 trainees with a known walkthrough. Success = same data on every laptop, every reset. Failure = state drift between sessions ("the Kratos row got deleted, can you re-seed?").

**Secondary:**

- **New developer** — wants a populated stack to exercise UI flows during onboarding without manual setup.
- **Platform operator** — wants `POST /api/v1/demo/reset` to restore a known baseline after exploratory sales-engineer changes (UC-54).
- **CI / integration tests** — Testcontainers MySQL can boot a pre-seeded demo database (the optional `11-demo-seed.sql` is the path here).

## Success Criteria

From the spec (`design.md` § Success Criteria + tasks 8.1–8.10 verification gates):

1. **One-command bootstrap.** `docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build` brings all six services (reverse-proxy, api, web, db, adminer, seq) to healthy in under 2 minutes on a developer laptop.
2. **Demo info endpoint.** `curl http://localhost:8080/api/v1/demo/info` returns `{"enabled":true,"seedVersion":1,"tenants":[...]}`. When demo is off, returns `{"enabled":false}`.
3. **Resident count and canonical households.** Login as `porteiro@controleasy.app` / `demo123`; residents list shows ≥ 61 rows. `Florinda`, `Quico`, and `Prof. Girafales` share one `ApartmentId` (apt 8). `Kratos` and `Atreus` share `Sparta-1`.
4. **Multi-tenant persona.** Login as `multi@controleasy.app`; tenant picker shows `[Demo] Residencial Aurora` and `[Demo] Condomínio Parque Verde`.
5. **Demo UI surface.** App shell shows the `ce-demo-banner`; login page shows the demo shortcut panel; `/help/demo` renders the credential table.
6. **Negative case.** With the demo overlay **off** (default compose), no demo users exist in MySQL, `/api/v1/demo/info` returns `enabled:false`, the `PlatformAdminBootstrapService` runs as normal, and the random platform-admin credential appears in logs.
7. **Cross-tenant isolation unchanged.** A `demo-aurora` resident is invisible to a `demo-parque-verde` session (C.6 invariant).
8. **Reset is reproducible.** `docker compose down -v && up --build` produces the same 61 + 6 resident counts with the same canonical households, byte-identical for the seeded data; `POST /api/v1/demo/reset` does the same without dropping the volume.

Measurable business outcome (not in the spec, but the brief's success hinge): **time-to-first-meaningful-screen drops from ~15 min (manual bootstrap + data entry) to ~2 min (single compose command) for sales engineers, trainers, and new developers.**

## Scope

**In (v1 of demo mode):**

- Configuration: `DemoOptions` (`Enabled`, `SeedVersion`, `DisableOutboundEmail`).
- Database: `DemoMetadata` table (platform, no `tenant_id`); architecture-test exemption added.
- Backend: `DemoSeederService` (idempotent hosted service), `DemoFixtures` static roster, two endpoints (`/api/v1/demo/info`, `/api/v1/demo/reset`).
- DevOps: `docker-compose.demo.yml` overlay with demo env vars, all `FeatureManagement__*__UseWeb=true`, documented JWT signing key, comment block with quick-start and reset commands.
- Frontend: `DemoInfoService` (app-initializer, signal-based), `DemoBannerComponent` (`ce-demo-banner`), collapsible login-shortcut panel, lazy `/help/demo` route, link from user menu when demo enabled.
- Documentation: `docs/demo-mode.md` (compose commands, credential table, walkthrough script, reset procedures, security warning).
- Tests: `DemoModeTests` (seed counts, multi-tenant login, cross-tenant isolation, demo-off regression); cohabitation assertions for canonical households.

**Out (explicit, per spec):**

- Public internet-facing demo SaaS (TLS at edge, rate limiting, WAF) — future DevOps spec.
- Anonymized import of real legacy `controlEasyDB.db` data — separate data-migration spec.
- Demo mode for the legacy WPF app — web stack only (and the WPF is no longer in this repo anyway).
- Synthetic MQTT / hardware simulation — deferred to `.specs/3 - photo-capture-hardware-integration/`.
- Billing, subscription, or trial-expiry flows.
- LLM-generated demo data — static curated fixtures only.
- Re-skinning the demo for non-pt-BR locales (i18n follows the rest of the product, not a demo-specific concern).

## Vision

If Demo Mode succeeds, **"show me your software" stops being a 30-minute pre-meeting ritual and becomes a 5-minute live walkthrough.** A prospect clicks a link, sees real condominiums with real data, plays with the visit flow, and decides. A new developer runs one command and is in the codebase, not a setup script. A trainer's classroom is reproducible across every laptop on every cohort.

In 2–3 years, **every ControlEasy Reborn deployment profile — production, staging, training, demo, eval — is a Compose overlay on the same image, distinguished only by env vars and seed data**. Demo Mode is the first overlay; the pattern (Docker Compose overlay + idempotent seeder + platform table version marker) is the template for the next ones (smoke-test, load-test, partner-eval). The brief is for the first instance. The pattern is the dividend.

---

*Authored 2026-07-12 by `bmad-product-brief` (headless, intent: create). Sources consumed: `.specs/4 - demo-mode/{requirements,design,tasks}.md`, `docs/demo-mode.md`, live `docker-compose.demo.yml` (exists), `.planning/ROADMAP.md`, `.planning/codebase/CONCERNS.md`, `.planning/REQUIREMENTS.md` (DEMO-01, DEMO-02 traceability). Editorial review: structure + prose passes recommended before promoting from `status: draft` to `status: approved`.*
