# Source Tree Analysis — ControlEasy Reborn

**Analysis date:** 2026-07-12

## Annotated Directory Tree

```
ControlEasy/                          # Repository root
├── .agent/                           # Agent scratch / per-agent state (tool-specific)
├── .agents/                          # Cross-tool agent skill library
├── .amazonq/                         # Amazon Q agent scratch
├── .claude/                          # Claude Code skill library (Bmad, GSD, etc.)
├── .clinerules/                      # Cline agent scratch
├── .cursor/                          # Cursor rules + skills (contains gsd- hooks)
├── .devin/                           # Devin agent scratch
├── .github/                          # GitHub metadata — only copilot-instructions.md (no CI)
├── .kilo/                            # Kilo agent scratch
├── .kilocode/                        # Kilo Code agent scratch
├── .kiro/                            # Kiro agent scratch
├── .opencode/                        # OpenCode agent scratch
├── .planning/                        # GSD planning artifacts (codebase maps, phases)
│   ├── PROJECT.md                    # Top-level project brief (active vs validated)
│   ├── REQUIREMENTS.md               # v1/v2 requirements with phase traceability
│   ├── ROADMAP.md                    # 14 phases; current focus = Phase 8
│   ├── STATE.md                      # Live execution state
│   ├── codebase/                     # 7 codebase maps (STACK, STRUCTURE, ARCHITECTURE, ...)
│   └── phases/                       # Phase plans, summaries, verifications
│       └── 07-dbtools-nuget-migration-inserted/   # Last completed phase
├── .specs/                           # 20 spec-driven development folders (feature-level)
├── .specify/                         # Spec-Kit-style memory
├── .vscode/                          # Editor settings
├── _bmad/                            # BMad Method installation (skills, scripts, config)
├── _bmad-output/                     # BMad output artifacts
├── agents/                           # Per-domain agent instruction files
├── design-artifacts/                 # Design-related output
├── docker/                           # Container orchestration
│   ├── api.Dockerfile                # SDK 8.0 → aspnet 8.0 (multi-stage)
│   ├── web.Dockerfile                # node:20-alpine → nginx:alpine
│   ├── docker-compose.yml            # api + web + db + reverse-proxy + adminer + seq
│   ├── docker-compose.demo.yml       # Demo overlay (seeded tenants, fixed creds)
│   ├── nginx.conf                    # SPA fallback for web container
│   ├── reverse-proxy/                # Traefik static + dynamic config
│   ├── .env.example                  # Secrets template
│   └── mysql/init/                   # Ordered SQL init scripts (00-… 11-…)
├── docs/                             # Human-facing documentation
│   ├── architecture/decisions/       # ADRs 0001–0004 (modular monolith, DBTools, multi-tenant, multitarget)
│   ├── design-system/                # Tokens, theming, brand-customization
│   ├── migration/legacy-mapping.md   # WPF screen → web module mapping
│   ├── penpot/                       # Design system JSON + SVG screens
│   ├── demo-mode.md
│   ├── getting-started.md
│   └── project-scan-report.json      # BMad scan state (this file)
├── mockup/                           # Static HTML design prototypes (showcase)
├── openspec/                         # OpenSpec stub (config.yaml only; not yet populated)
├── scripts/                          # Utility scripts (SQL generation)
├── src/                              # Reborn solution (PRIMARY ACTIVE CODEBASE)
│   ├── ControlEasyReborn.sln         # Solution entry (includes tests)
│   ├── Directory.Build.props         # Shared MSBuild: net8.0, nullable, TreatWarningsAsErrors
│   ├── Directory.Packages.props      # Central Package Management versions
│   ├── global.json                   # .NET SDK 8.0.0 pin
│   ├── Host/
│   │   └── ControlEasyReborn.Api/    # ASP.NET Core 8 composition root
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── Hosting/              # FeatureEndpoints, PlatformAdminBootstrapService
│   ├── Web/
│   │   └── ControlEasyReborn.Web/    # Angular 18 SPA (Part: web)
│   │       ├── angular.json
│   │       ├── package.json
│   │       ├── playwright.config.ts
│   │       ├── proxy.conf.json
│   │       └── src/
│   │           ├── main.ts
│   │           ├── app/
│   │           │   ├── core/         # guards, interceptors, services, utils
│   │           │   ├── features/     # lazy-loaded feature pages + *-api.service.ts
│   │           │   ├── design-system/  # ce-* primitives, tokens, theme
│   │           │   ├── layout/       # app shell, demo banner
│   │           │   └── shared/       # cross-feature components (e.g. apartment picker)
│   │           ├── styles.css
│   │           └── api/              # OpenAPI-generated client (target; not yet produced)
│   │       └── e2e/                  # Playwright e2e specs
│   ├── BuildingBlocks/
│   │   ├── ControlEasyReborn.SharedKernel/      # ITenantContext, primitives, feature flags, demo
│   │   └── ControlEasyReborn.Infrastructure/   # DBTools DI, TenantResolutionMiddleware, demo, bootstrap
│   └── Modules/                      # 9 feature modules (vertical slices)
│       ├── Administration/           # Audit log + config
│       ├── Apartments/               # Apartments CRUD (no dedicated Angular route; used by Residents)
│       ├── Reports/                  # CQRS-lite read projections (dashboard, visit counts)
│       ├── Residents/                # Residents CRUD, search, soft-delete
│       ├── Security/                 # Auth, JWT, refresh tokens, roles, permissions
│       ├── ServiceProviders/         # Service providers CRUD
│       ├── Tenants/                  # Tenant admin, backup endpoints
│       ├── Vehicles/                 # Vehicles linked to apartments
│       └── Visits/                   # Check-in/out, open visits list
└── tests/                            # 4 test projects + Playwright tests
    ├── ControlEasyReborn.UnitTests/        # xUnit + NSubstitute + FluentAssertions
    ├── ControlEasyReborn.IntegrationTests/ # Testcontainers.MySql + WebApplicationFactory
    ├── ControlEasyReborn.ArchitectureTests/# NetArchTest layer rules
    ├── a11y/                              # Playwright a11y (axe-core)
    └── visual/                            # Playwright visual regression
```

## Critical Folders (with purpose)

| Path | Purpose | Part |
|------|---------|------|
| `src/Host/ControlEasyReborn.Api/` | Single deployable API entry: DI, middleware, endpoint mapping, exception mapping | api |
| `src/Host/ControlEasyReborn.Api/Hosting/` | Feature flags, PlatformAdmin bootstrap hosted service | api |
| `src/Modules/{Feature}/` | Vertical slice per feature — 4 projects (Domain, Application, Infrastructure, Api) | api |
| `src/Modules/.../Application/Handlers/` | Use-case handlers (no MediatR) | api |
| `src/Modules/.../Application/Abstractions/` | Repository interfaces (`I{Entity}Repository`) | api |
| `src/Modules/.../Application/Validators/` | FluentValidation `AbstractValidator<T>` per request | api |
| `src/Modules/.../Application/Contracts/` | `sealed record` DTOs for API surface | api |
| `src/Modules/.../Application/Errors/` | `NotFoundException`, `ValidationException`, `ConflictException` | api |
| `src/Modules/.../Infrastructure/Persistence/` | Repository implementations using `TenantAwareLinqFactory` | api |
| `src/Modules/.../Infrastructure/DI/` | `{Module}ModuleServiceCollectionExtensions` for module DI | api |
| `src/Modules/.../Api/Endpoints/` | Minimal API endpoint classes with `Map{Entity}Endpoints` | api |
| `src/BuildingBlocks/ControlEasyReborn.Infrastructure/` | DBTools registration, `TenantResolutionMiddleware`, `TenantFilterInterceptor`, `TenantAwareLinqFactory`, demo mode | api |
| `src/BuildingBlocks/ControlEasyReborn.SharedKernel/` | `ITenantContext`, primitives, feature flags, demo options | api |
| `src/Web/ControlEasyReborn.Web/src/app/core/` | Auth guard, role guards, interceptors, shared services | web |
| `src/Web/ControlEasyReborn.Web/src/app/features/` | Lazy-loaded pages + `*-api.service.ts` per feature | web |
| `src/Web/ControlEasyReborn.Web/src/app/design-system/` | `ce-*` primitives, tokens, theme, showcase | web |
| `src/Web/ControlEasyReborn.Web/src/app/layout/` | App shell, demo banner | web |
| `src/Web/ControlEasyReborn.Web/e2e/` | Playwright end-to-end specs | web |
| `docker/mysql/init/` | Ordered SQL init scripts (00–11); CI-enforced sync via `SchemaBackfillSyncTests` | infra |
| `tests/ControlEasyReborn.IntegrationTests/` | `MySqlContainerFixture`, `TenantAwareWebApplicationFactory`, endpoint tests | tests |

## Entry Points

- **API:** `src/Host/ControlEasyReborn.Api/Program.cs`
- **Web:** `src/Web/ControlEasyReborn.Web/src/main.ts`
- **Solution:** `src/ControlEasyReborn.sln` (includes all test projects)
- **Docker Compose:** `docker/docker-compose.yml` (demo overlay: `docker/docker-compose.demo.yml`)

## Cross-Part Boundaries

- **web → api:** REST over `/api/v1/*`, JWT bearer, Traefik routing. Angular dev proxy at `src/Web/ControlEasyReborn.Web/proxy.conf.json` forwards `/api` to `http://localhost:8080`.
- **api → db:** DBTools (`IAsyncSqlClient` / `Linq<T>`) on top of MySqlConnector. Multi-tenant queries are rewritten by `TenantFilterInterceptor`; platform-level queries bypass via `__bypassTenantFilter`.
- **api → logs:** Serilog → console JSON + Seq at `http://seq:5341` (docker compose) / `http://localhost:5341` (dev).
- **api → features:** Traefik at host port 8080 routes `/api` → `api:8080`, `/` → `web:8080`, `/db` → adminer.

## Special Directories (build/generated)

- `src/**/obj/`, `src/**/bin/` — .NET build artifacts (.gitignore)
- `src/Web/ControlEasyReborn.Web/dist/` — Angular build output (.gitignore)
- `src/lib/DBTools_SQL/` — **Removed** by Phase 7; DBTools now consumed from NuGet
- `mockup/` — Static HTML prototype; reference only, not production
- `.planning/`, `.specs/`, `docs/` — Tracked in git
- `openspec/` — Initialized but empty (`config.yaml` only; no `changes/`)

---

*Tree analysis: 2026-07-12 · Sources: GSD `STRUCTURE.md` (2026-06-24) cross-checked against live filesystem.*
