<!--
DEPRECATED — redirect to canonical home.
This file is a `bmad-document-project --mode deep` output from
2026-07-12. It is on a milestone-boundary retirement schedule
(see `.specify/memory/constitution.md` v1.1.0 § "Documentation
Systems and Source of Truth — Retirement schedule").

Canonical home: `AGENTS.md` § 7 "Documentation" (this file's
content is summarized in the source-of-truth table there).
-->

# Project Documentation Index — ControlEasy Reborn

**Generated:** 2026-07-12 by `bmad-document-project` (initial_scan · deep)
**Project type:** multi-part (`api` + `web`)
**Primary entry point for AI-assisted development**

---

## Project Overview

- **Type:** multi-part with 2 parts
- **Primary language:** C# 12 (.NET 8) · TypeScript 5.5 (Angular 18)
- **Architecture:** Modular Monolith + Clean Architecture per module; CQRS-lite (Reports); Strangler Fig (scaffolding only — legacy not in repo)
- **Database:** MySQL 8 via DBTools NuGet 1.4.3 (LINQ-first, multi-provider-ready)
- **Containerization:** Docker Compose (api, web, db, reverse-proxy, adminer, seq)

## Quick Reference

#### `api` — ControlEasy Reborn API

- **Type:** backend
- **Tech stack:** C# 12 / .NET 8, ASP.NET Core, DBTools NuGet 1.4.3, MySQL 8, JWT bearer
- **Root:** `src/Host`, `src/Modules`, `src/BuildingBlocks`
- **Entry point:** `src/Host/ControlEasyReborn.Api/Program.cs`

#### `web` — ControlEasy Reborn Web

- **Type:** web
- **Tech stack:** TypeScript 5.5, Angular 18.2 (standalone, signals, OnPush), Tailwind v4, RxJS 7.8
- **Root:** `src/Web/ControlEasyReborn.Web`
- **Entry point:** `src/Web/ControlEasyReborn.Web/src/main.ts`

## Generated Documentation

- [Project Overview](./project-overview.md) — executive summary, parts, tech stack table, repository structure
- [Architecture](./architecture.md) — system diagram, layer responsibilities, multi-tenancy, error/auth, anti-patterns, abstractions
- [Source Tree Analysis](./source-tree-analysis.md) — annotated directory tree, critical folders, entry points, cross-part boundaries
- [Component Inventory](./component-inventory.md) — 9 feature modules × 4 layers + 2 building blocks; `ce-*` design-system components; test layout
- [Development Guide](./development-guide.md) — prerequisites, setup, dev modes, build/test/lint commands, how to add new code
- [Deployment Guide](./deployment-guide.md) — topology, env vars, build/deploy, health checks, operational concerns, production hardening checklist
- [API Contracts](./api-contracts.md) — route group catalog, key endpoints, conventions, OpenAPI / TypeScript client notes
- [Data Models](./data-models.md) — schema layout, dual-column tenancy pattern, repositories, migration strategy
- [Integration Architecture](./integration-architecture.md) — cross-part communication, Docker service inventory, auth flow, multi-tenancy data path

## Existing Documentation (consumed as source of truth)

### GSD codebase maps (`.planning/codebase/`)
- [STACK.md](../.planning/codebase/STACK.md) — language/framework/runtime/dependency catalog
- [STRUCTURE.md](../.planning/codebase/STRUCTURE.md) — directory layout, naming, where to add new code
- [ARCHITECTURE.md](../.planning/codebase/ARCHITECTURE.md) — full system diagram, data flow, key abstractions
- [CONVENTIONS.md](../.planning/codebase/CONVENTIONS.md) — coding style (C# + Angular + DBTools)
- [CONCERNS.md](../.planning/codebase/CONCERNS.md) — tech debt, bugs, security, performance, fragile areas, scaling limits
- [INTEGRATIONS.md](../.planning/codebase/INTEGRATIONS.md) — external services, auth, monitoring, CI/CD, env vars
- [TESTING.md](../.planning/codebase/TESTING.md) — xUnit + Testcontainers + Karma + Playwright patterns

### ADRs (`docs/architecture/decisions/`)
- [0001 — Modular Monolith](../architecture/decisions/0001-modular-monolith.md)
- [0002 — DBTools SQL as Only Data Access](../architecture/decisions/0002-dbtools-sql-as-only-data-access.md)
- [0003 — Multi-Tenant Shared Schema](../architecture/decisions/0003-multi-tenant-shared-schema.md)
- [0004 — Target Framework Multitarget](../architecture/decisions/0004-target-framework-multitarget.md)

### Project briefs
- [Project Overview](../project-overview.md) — repeat of the generated one above (for navigation)
- [Getting Started](./getting-started.md) — first-boot operator walkthrough
- [Demo Mode](./demo-mode.md) — `docker-compose.demo.yml` reference
- [Migration: Legacy Mapping](./migration/legacy-mapping.md) — WPF screen → web module mapping
- [Design System README](./design-system/README.md)
- [Design System: Tokens](./design-system/tokens.json) (where applicable)
- [Design System: Theming](./design-system/theming.md)
- [Design System: Brand Customization](./design-system/brand-customization.md)
- [Penpot manifest](./penpot/manifest.json) — design system JSON + SVG screens

### Project context
- [AGENTS.md](../AGENTS.md) — authoritative project overview, target stack, conventions, post-task verification
- [README.md](../README.md)
- [Orchestrator.md](../Orchestrator.md)
- [CHANGELOG.md](../CHANGELOG.md)

### GSD planning
- [PROJECT.md](../.planning/PROJECT.md) — top-level project brief (validated vs active)
- [REQUIREMENTS.md](../.planning/REQUIREMENTS.md) — v1/v2 requirements + traceability
- [ROADMAP.md](../.planning/ROADMAP.md) — 14 phases
- [STATE.md](../.planning/STATE.md) — live execution state

## Getting Started

1. Read [AGENTS.md](../AGENTS.md) for the authoritative project overview.
2. Read the [Project Overview](./project-overview.md) for the current state snapshot.
3. For code work, read the [Architecture](./architecture.md) and the relevant per-topic GSD map in `.planning/codebase/`.
4. For new features, read [Component Inventory](./component-inventory.md) + [Where to Add New Code](../.planning/codebase/STRUCTURE.md#where-to-add-new-code).
5. For local dev, follow the [Development Guide](./development-guide.md) end-to-end.

## Brownfield PRD Workflow

When ready to plan a new feature, run the BMad PRD workflow and provide this `index.md` as the project knowledge input.

## State

- **State file:** `docs/project-scan-report.json`
- **Mode:** initial_scan
- **Scan level:** deep
- **Last updated:** 2026-07-12
