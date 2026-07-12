# Project Overview — ControlEasy Reborn

**Project:** ControlEasy Reborn
**Repository type:** multi-part (api + web)
**Analysis date:** 2026-07-12
**Workflow:** bmad-document-project · initial_scan · deep

## Executive Summary

ControlEasy Reborn is the **web-based modernization** of ControlEasy 5, a condominium access-control platform used by gatehouse staff (porteiro), tenant administrators, and platform operators to manage residents, visitors, vehicles, service providers, apartments, and gatehouse flow.

The repository contains **only the Reborn stack**. Legacy ControlEasy 5 (WPF / .NET Framework 4.8) and the half-started ControlEasyWeb (Blazor Server / .NET 5) are referenced in `AGENTS.md`, `.specs/`, and `docs/migration/legacy-mapping.md` but are **not in the working tree** (they live outside this repo).

The Strangler Fig migration story is therefore partially in-repo: the new stack fully replaces the legacy flow, while the dead `FeatureManagement.*.UseWeb` flags remain as scaffolding for a cutover that was never started because the legacy code is no longer in scope of this repository.

## Parts

### `api` — ASP.NET Core 8 Modular Monolith

- **Type:** backend
- **Root:** `src/Host`, `src/Modules`, `src/BuildingBlocks`
- **Stack:** C# 12 / .NET 8 (`global.json` pins 8.0.0), ASP.NET Core 8 Web API, DBTools 1.4.3 (NuGet), MySQL 8, JWT bearer, Serilog + Seq
- **Entry point:** `src/Host/ControlEasyReborn.Api/Program.cs`
- **Architecture:** 9 feature modules × 4 Clean-Architecture layers + 2 building blocks. Single deployable, modular monolith, designed to extract microservices later.
- **Multi-tenancy:** JWT `tenant_id` claim → `TenantFilterInterceptor` injects `WHERE tenant_id = @ctx_tenant` into all tenant-scoped queries. Platform tables bypass via `__bypassTenantFilter`.

### `web` — Angular 18 SPA

- **Type:** web
- **Root:** `src/Web/ControlEasyReborn.Web`
- **Stack:** TypeScript 5.5, Angular 18.2 (standalone, signals, OnPush), Tailwind CSS 4.1, RxJS 7.8, Lucide icons
- **Entry point:** `src/Web/ControlEasyReborn.Web/src/main.ts`
- **Architecture:** Lazy-loaded feature pages, design-system primitives (`ce-*`), signals-based services, `OpenAPI` client (`ng-openapi-gen` configured but not yet wired into Docker build).
- **Auth:** JWT in `localStorage` via `AuthService`; `authInterceptor` attaches to all `/api/v1/*` requests.

## Tech Stack Snapshot

| Category | Technology | Version | Source |
|----------|------------|---------|--------|
| Backend language | C# | 12 (LangVersion=latest) | `src/Directory.Build.props` |
| Backend runtime | .NET | 8 LTS | `global.json` |
| Web framework | ASP.NET Core | 8.0 | `src/Host/.../Program.cs` |
| Data access | DBTools (NuGet) | 1.4.3 | `src/Directory.Packages.props` |
| Database | MySQL | 8.0 | `docker/docker-compose.yml` |
| Auth | JWT (HMAC-SHA256) + BCrypt | JwtBearer 8.0.10 | `appsettings.json` |
| Frontend framework | Angular | 18.2 | `src/Web/.../package.json` |
| Frontend lang | TypeScript | 5.5 | `package.json` |
| Frontend styling | Tailwind CSS | 4.1 | `postcss.config.mjs` |
| Frontend testing | Karma + Jasmine | 6.4 + 5.2 | `package.json` |
| E2E / a11y | Playwright | 1.49 | `package.json` |
| Container orchestration | Docker Compose | — | `docker/docker-compose.yml` |
| Reverse proxy | Traefik | v3.1 | `docker/reverse-proxy/` |
| Logging | Serilog + Seq | 8.0.3 + 6.0.0 | `appsettings.json` |
| CI | **None in repo** | — | `.github/` contains only `copilot-instructions.md` |
| API docs | Swashbuckle | 6.6.2 | `Program.cs` |
| Mapping | Mapster | 7.4.0 (pinned, unused) | `Directory.Packages.props` |
| Validation | FluentValidation | 11.9.0 | module Application/Validators |
| Unit testing | xUnit + FluentAssertions + NSubstitute | 2.9 + 8.2 + 5.1 | `Directory.Packages.props` |
| Integration testing | Testcontainers.MySql | 4.0.0 | integration tests |
| Architecture testing | NetArchTest.Rules | 1.3.2 | architecture tests |

## Repository Structure

```
ControlEasy/                          # Repository root
├── .planning/                        # GSD planning artifacts (codebase maps, phases, STATE)
├── .specs/                           # 20 spec-driven-development folders
├── agents/                           # Agent instruction files (FrontendAgent, etc.)
├── design-artifacts/                 # Design-related artifacts
├── docker/                           # Docker Compose, Dockerfiles, MySQL init, Traefik
├── docs/                             # ADRs, migration mapping, design-system docs, Penpot assets
│   └── project-scan-report.json      # BMad workflow state (this scan)
├── mockup/                           # Static HTML design prototypes
├── openspec/                         # OpenSpec stub (config.yaml only; no changes yet)
├── scripts/                          # Utility scripts (SQL generation)
├── src/                              # Reborn solution source (primary active codebase)
│   ├── ControlEasyReborn.sln
│   ├── Directory.Build.props         # Shared MSBuild: net8.0, nullable, warnings as errors
│   ├── Directory.Packages.props      # Central Package Management versions (DBTools 1.4.3 here)
│   ├── global.json                   # .NET SDK pin
│   ├── Host/ControlEasyReborn.Api/   # ASP.NET Core 8 composition root
│   ├── Web/ControlEasyReborn.Web/    # Angular 18 SPA
│   ├── BuildingBlocks/               # SharedKernel + Infrastructure
│   └── Modules/                      # 9 feature modules
└── tests/                            # xUnit unit + integration + architecture; Playwright e2e
```

**Not in repository:** `ControlEasy5/` (WPF) and `ControlEasyWeb/` (Blazor) referenced in `AGENTS.md` and `docs/migration/legacy-mapping.md` but not checked in.

## Architecture Type

- **Primary:** Modular Monolith with Clean Architecture per module
- **Secondary:** CQRS-lite for Reports module (read projections separated from writes)
- **Migration pattern:** Strangler Fig (scaffolded but legacy code not in repo)
- **Multi-tenancy:** Shared schema, JWT `tenant_id` claim, query interceptor

## Links to Detailed Docs

- [Architecture](./architecture.md)
- [Source Tree Analysis](./source-tree-analysis.md)
- [Component Inventory](./component-inventory.md)
- [Development Guide](./development-guide.md)
- [Deployment Guide](./deployment-guide.md)
- [API Contracts](./api-contracts.md)
- [Data Models](./data-models.md)
- [Integration Architecture](./integration-architecture.md)

---

*Project overview generated 2026-07-12 by bmad-document-project (initial_scan, deep).*
