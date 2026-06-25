# Codebase Structure

**Analysis Date:** 2026-06-24

## Directory Layout

```
ControlEasy/                          # Repository root
├── .planning/                        # GSD planning artifacts (codebase maps, phases)
├── .specs/                           # Spec-driven development (requirements, design, tasks)
├── agents/                           # Agent instruction files (FrontendAgent, etc.)
├── docker/                           # Docker Compose, Dockerfiles, MySQL init, Traefik
├── docs/                             # ADRs, migration mapping, design-system docs, Penpot assets
├── mockup/                           # Static HTML mockups (showcase)
├── scripts/                          # Utility scripts (SQL generation)
├── src/                              # Reborn solution source (primary active codebase)
│   ├── ControlEasyReborn.sln
│   ├── Directory.Build.props         # Shared MSBuild: net8.0, nullable, warnings as errors
│   ├── Directory.Packages.props      # Central Package Management versions
│   ├── global.json                   # .NET SDK pin
│   ├── Host/
│   │   └── ControlEasyReborn.Api/    # ASP.NET Core 8 composition root
│   ├── Web/
│   │   └── ControlEasyReborn.Web/    # Angular 18 SPA
│   ├── BuildingBlocks/
│   │   ├── ControlEasyReborn.SharedKernel/
│   │   └── ControlEasyReborn.Infrastructure/
│   ├── lib/
│   │   └── DBTools_SQL/DBTools/      # Vendored multi-provider data access library
│   └── Modules/
│       ├── Administration/
│       ├── Apartments/
│       ├── Reports/
│       ├── Residents/
│       ├── Security/
│       ├── ServiceProviders/
│       ├── Tenants/
│       ├── Vehicles/
│       └── Visits/
└── tests/
    ├── ControlEasyReborn.UnitTests/
    ├── ControlEasyReborn.IntegrationTests/
    ├── ControlEasyReborn.ArchitectureTests/
    ├── a11y/                         # Playwright accessibility specs
    └── visual/                       # Playwright visual regression specs
```

**Not in repository:** Legacy `ControlEasy5/` (WPF) and `ControlEasyWeb/` (Blazor) referenced in `AGENTS.md` and `docs/migration/legacy-mapping.md` but not checked into this repo.

## Directory Purposes

**`src/Host/ControlEasyReborn.Api/`:**
- Purpose: Single deployable API entry point
- Contains: `Program.cs`, `appsettings.json`, hosting services
- Key files: `src/Host/ControlEasyReborn.Api/Program.cs`, `src/Host/ControlEasyReborn.Api/Hosting/FeatureEndpoints.cs`
- Subdirectories: `Hosting/` (bootstrap, feature flags)

**`src/Web/ControlEasyReborn.Web/`:**
- Purpose: Angular 18 standalone SPA
- Contains: TypeScript source, Angular config, Playwright e2e
- Key files: `src/Web/ControlEasyReborn.Web/src/main.ts`, `src/Web/ControlEasyReborn.Web/src/app/app.routes.ts`, `src/Web/ControlEasyReborn.Web/angular.json`, `src/Web/ControlEasyReborn.Web/package.json`
- Subdirectories:
  - `src/app/core/` — guards, interceptors, services, utils
  - `src/app/features/` — lazy-loaded feature pages + `*-api.service.ts`
  - `src/app/design-system/` — reusable UI components, tokens, theme
  - `src/app/layout/` — app shell, demo banner
  - `src/app/shared/` — cross-feature components (e.g. apartment picker)
  - `e2e/` — Playwright tests

**`src/BuildingBlocks/`:**
- Purpose: Shared kernel and cross-cutting infrastructure
- Contains: Multi-tenancy, demo mode, bootstrap, DBTools registration
- Key files: `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Data/ServiceCollectionExtensions.cs`, `src/BuildingBlocks/ControlEasyReborn.Infrastructure/MultiTenancy/TenantResolutionMiddleware.cs`
- Subdirectories: `SharedKernel/` (primitives, `ITenantContext`), `Infrastructure/` (middleware, interceptors, demo)

**`src/lib/DBTools_SQL/DBTools/`:**
- Purpose: Vendored data-access library (LINQ + async SQL)
- Contains: `Core/`, `Controllers/`, `Abstractions/`, `Providers/`, `Mapping/`
- Key files: `src/lib/DBTools_SQL/DBTools/Core/AsyncSqlClient.cs`, `src/lib/DBTools_SQL/DBTools/DBTools.csproj`

**`src/Modules/{Feature}/`:**
- Purpose: Vertical feature slice with Clean Architecture layers
- Contains: Four projects per module (when full slice): Domain, Application, Infrastructure, Api
- Key pattern (Residents example):
  - `ControlEasyReborn.Modules.Residents.Domain/Entities/`
  - `ControlEasyReborn.Modules.Residents.Application/Handlers/`, `Validators/`, `Contracts/`, `Abstractions/`
  - `ControlEasyReborn.Modules.Residents.Infrastructure/Persistence/`, `DI/`
  - `ControlEasyReborn.Modules.Residents.Api/Endpoints/`, `DI/`
- Note: `Apartments` is a supporting module (no dedicated Angular route; used by Residents UI)

**`docker/`:**
- Purpose: Container orchestration and database bootstrap
- Contains: Compose files, Dockerfiles, MySQL init SQL, Traefik/nginx config
- Key files: `docker/docker-compose.yml`, `docker/docker-compose.demo.yml`, `docker/api.Dockerfile`, `docker/web.Dockerfile`, `docker/mysql/init/00-schema.sql`
- Subdirectories: `mysql/init/` (ordered schema migrations), `reverse-proxy/`

**`tests/`:**
- Purpose: xUnit unit, integration, and architecture tests; Playwright visual/a11y
- Contains: Test projects mirroring module structure under `Modules/`
- Key files: `tests/ControlEasyReborn.IntegrationTests/MySqlContainerFixture.cs`, `tests/ControlEasyReborn.ArchitectureTests/LayerDependencyTests.cs`, `tests/ControlEasyReborn.UnitTests/TestDoubles/FakeAsyncSqlClient.cs`

**`.specs/`:**
- Purpose: Feature specs (requirements, design, tasks, orchestration)
- Contains: Numbered feature folders (e.g. `.specs/1 - modernization-roadmap/`, `.specs/4 - demo-mode/`)

**`docs/`:**
- Purpose: Human-readable documentation and ADRs
- Key files: `docs/getting-started.md`, `docs/migration/legacy-mapping.md`, `docs/architecture/decisions/0001-modular-monolith.md`

## Key File Locations

**Entry Points:**
- `src/Host/ControlEasyReborn.Api/Program.cs` — ASP.NET Core API startup and endpoint mapping
- `src/Web/ControlEasyReborn.Web/src/main.ts` — Angular bootstrap
- `src/ControlEasyReborn.sln` — Primary .NET solution (includes tests)

**Configuration:**
- `src/Directory.Build.props` — Shared C# compiler settings (net8.0, nullable, warnings as errors)
- `src/Directory.Packages.props` — Central Package Management (NuGet versions)
- `tests/Directory.Packages.props` — Test project package versions
- `src/Host/ControlEasyReborn.Api/appsettings.json` — API config (Db, Jwt, Serilog, Demo, Bootstrap)
- `docker/.env.example` — Docker environment variable template (do not commit secrets)
- `src/Web/ControlEasyReborn.Web/angular.json` — Angular build/serve config
- `src/Web/ControlEasyReborn.Web/tsconfig.app.json` — TypeScript strict mode
- `src/Web/ControlEasyReborn.Web/.eslintrc.json` — Angular ESLint rules
- `global.json` — .NET SDK version pin

**Core Logic:**
- `src/Modules/*/ControlEasyReborn.Modules.*.Application/Handlers/` — Use-case handlers
- `src/Modules/*/ControlEasyReborn.Modules.*.Infrastructure/Persistence/` — Repository implementations
- `src/Modules/*/ControlEasyReborn.Modules.*.Api/Endpoints/` — Minimal API route definitions
- `src/BuildingBlocks/ControlEasyReborn.Infrastructure/MultiTenancy/` — Tenant resolution and filtering
- `src/Web/ControlEasyReborn.Web/src/app/features/` — Feature pages and API services

**Testing:**
- `tests/ControlEasyReborn.UnitTests/Modules/{Module}/` — Handler and repository unit tests
- `tests/ControlEasyReborn.IntegrationTests/*EndpointTests.cs` — HTTP integration tests per module
- `tests/ControlEasyReborn.ArchitectureTests/` — NetArchTest layer rules, schema sync
- `src/Web/ControlEasyReborn.Web/e2e/` — Playwright e2e (when present)
- `tests/a11y/showcase.spec.ts`, `tests/visual/showcase.spec.ts` — Design system visual/a11y checks

**Documentation:**
- `AGENTS.md` — Project overview and conventions for AI agents
- `docs/architecture/decisions/` — ADRs (modular monolith, DBTools, multi-tenant)
- `docs/migration/legacy-mapping.md` — WPF screen → web module mapping

## Naming Conventions

**C# Projects:**
- Pattern: `ControlEasyReborn.Modules.{Feature}.{Layer}` — e.g. `ControlEasyReborn.Modules.Residents.Application`
- Layer suffixes: `.Domain`, `.Application`, `.Infrastructure`, `.Api`
- Building blocks: `ControlEasyReborn.SharedKernel`, `ControlEasyReborn.Infrastructure`

**C# Files:**
- Handlers: `{Verb}{Entity}Handler.cs` — e.g. `CreateResidentHandler.cs`, `ListResidentsHandler.cs`
- Validators: `{Request}Validator.cs` — e.g. `CreateResidentRequestValidator.cs`
- Repositories: `{Entity}Repository.cs` in `Persistence/`
- Endpoints: `{Entity}Endpoints.cs` with static `Map{Entity}Endpoints` extension
- DI extensions: `{Module}ModuleServiceCollectionExtensions.cs`, `{Module}EndpointsExtensions.cs`
- DTOs: `{Entity}Dtos.cs` or `{Entity}Response` records in `Contracts/`
- Domain exceptions: `DomainExceptions.cs` in Application `Errors/`

**C# Types:**
- PascalCase for types, methods, properties
- `sealed` classes by default for handlers, repositories, entities
- File-scoped namespaces
- Private fields: `_camelCase`
- API routes: kebab-case plural nouns under `/api/v1/` — e.g. `/api/v1/residents`, `/api/v1/service-providers`

**Angular Files:**
- Feature pages: `{feature}.page.ts` — e.g. `residents.page.ts`
- API services: `{feature}-api.service.ts` — e.g. `residents-api.service.ts`
- Components: `{name}.component.ts` in `design-system/components/`
- Guards: `{name}.guard.ts` in `core/guards/`
- Interceptors: `{name}.interceptor.ts` in `core/interceptors/`
- Spec files: `*.spec.ts` co-located with source
- Standalone components: `standalone: true` (no NgModules)

**Angular Directories:**
- kebab-case: `service-providers/`, `design-system/`
- Plural for feature collections: `residents/`, `vehicles/`

**SQL Init Scripts:**
- Ordered numeric prefix: `00-schema.sql`, `02a-residents-schema.sql`, `11-demo-seed.sql` in `docker/mysql/init/`

**Specs:**
- Folder pattern: `.specs/{number} - {feature-name}/` with `requirements.md`, `design.md`, `tasks.md`

## Where to Add New Code

**New Feature Module (full vertical slice):**
- Domain entity: `src/Modules/{Feature}/ControlEasyReborn.Modules.{Feature}.Domain/Entities/`
- Application handler: `src/Modules/{Feature}/ControlEasyReborn.Modules.{Feature}.Application/Handlers/`
- Repository interface: `src/Modules/{Feature}/ControlEasyReborn.Modules.{Feature}.Application/Abstractions/I{Entity}Repository.cs`
- Repository impl: `src/Modules/{Feature}/ControlEasyReborn.Modules.{Feature}.Infrastructure/Persistence/{Entity}Repository.cs`
- DI registration: `src/Modules/{Feature}/ControlEasyReborn.Modules.{Feature}.Infrastructure/DI/{Feature}ModuleServiceCollectionExtensions.cs`
- Endpoints: `src/Modules/{Feature}/ControlEasyReborn.Modules.{Feature}.Api/Endpoints/{Entity}Endpoints.cs`
- Host wiring: add `Add{Feature}Module()` and `Map{Feature}Endpoints()` in `src/Host/ControlEasyReborn.Api/Program.cs`
- Solution: add four projects under `src/ControlEasyReborn.sln` Modules folder
- Schema: new SQL file in `docker/mysql/init/` with next sequence number
- Tests: `tests/ControlEasyReborn.UnitTests/Modules/{Feature}/`, `tests/ControlEasyReborn.IntegrationTests/{Feature}EndpointTests.cs`

**New API Endpoint in Existing Module:**
- Handler: `src/Modules/{Module}/ControlEasyReborn.Modules.{Module}.Application/Handlers/`
- Register handler in `src/Modules/{Module}/ControlEasyReborn.Modules.{Module}.Infrastructure/DI/*ModuleServiceCollectionExtensions.cs`
- Route: add to `src/Modules/{Module}/ControlEasyReborn.Modules.{Module}.Api/Endpoints/*Endpoints.cs`

**New Angular Feature Page:**
- Page: `src/Web/ControlEasyReborn.Web/src/app/features/{feature}/{feature}.page.ts`
- API service: `src/Web/ControlEasyReborn.Web/src/app/features/{feature}/{feature}-api.service.ts`
- Route: add lazy route in `src/Web/ControlEasyReborn.Web/src/app/app.routes.ts`
- Shared UI: reuse `src/Web/ControlEasyReborn.Web/src/app/design-system/components/`

**New Design System Component:**
- Component: `src/Web/ControlEasyReborn.Web/src/app/design-system/components/{name}/{name}.component.ts`
- Export from: `src/Web/ControlEasyReborn.Web/src/app/design-system/index.ts`
- Tokens: `src/Web/ControlEasyReborn.Web/src/app/design-system/tokens/`

**Cross-Cutting Backend Concern:**
- Shared abstractions: `src/BuildingBlocks/ControlEasyReborn.SharedKernel/`
- Middleware, interceptors, hosting extensions: `src/BuildingBlocks/ControlEasyReborn.Infrastructure/`

**Cross-Module DTO Sharing:**
- Prefer referencing another module's Application `Contracts/` or `Abstractions/` (as Residents references Apartments)
- `ControlEasyReborn.Contracts` project is described in design docs but **not yet present** — do not create unless spec explicitly adds it

**Utilities:**
- Backend: colocate in module Application or SharedKernel `Primitives/` if truly shared
- Frontend: `src/Web/ControlEasyReborn.Web/src/app/core/utils/` (e.g. `cpf.util.ts`)

## Special Directories

**`src/lib/DBTools_SQL/`:**
- Purpose: Vendored fork/submodule of DBTools_SQL data-access library
- Generated: `obj/`, `bin/` build artifacts
- Committed: Source yes; build output in `.gitignore`

**`docker/mysql/init/`:**
- Purpose: Idempotent schema and seed scripts run on first MySQL start
- Generated: No
- Committed: Yes — keep in sync with repositories (verified by `tests/ControlEasyReborn.ArchitectureTests/SchemaBackfillSyncTests.cs`)

**`.planning/`:**
- Purpose: GSD workflow artifacts (codebase maps, phase plans)
- Generated: By GSD commands
- Committed: Typically yes

**`.specs/`:**
- Purpose: Spec-driven development documents per feature
- Generated: By planning agents
- Committed: Yes

**`src/Web/ControlEasyReborn.Web/dist/`:**
- Purpose: Angular production build output
- Generated: `ng build`
- Committed: No (`.gitignore`)

**`src/**/obj/`, `src/**/bin/`:**
- Purpose: .NET build artifacts
- Committed: No

**`mockup/`:**
- Purpose: Static HTML design prototypes (`mockup/showcase.html`)
- Committed: Yes — reference only, not production code

**`docs/penpot/`:**
- Purpose: Design exports (SVG screens, tokens JSON)
- Committed: Yes — design reference assets

---

*Structure analysis: 2026-06-24*
*Update when directory structure changes*
