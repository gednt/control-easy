<!-- refreshed: 2026-06-24 -->
# Architecture

**Analysis Date:** 2026-06-24

## System Overview

The repository hosts **ControlEasy Reborn** — a modular monolith replacing the legacy ControlEasy 5 WPF desktop app. Legacy `ControlEasy5/` (WPF/.NET Framework 4.8) and `ControlEasyWeb/` (Blazor Server/.NET 5) are **not present in this repository**; they are referenced in `AGENTS.md`, `.specs/1 - modernization-roadmap/design.md`, and `docs/migration/legacy-mapping.md` as the Strangler Fig source being replaced.

```text
┌─────────────────────────────────────────────────────────────────────────┐
│                         Browser (Angular 18 SPA)                         │
│              `src/Web/ControlEasyReborn.Web/src/app/`                    │
├─────────────────────────────────────────────────────────────────────────┤
│  Traefik reverse proxy (`docker/reverse-proxy/`)  :8080                  │
│    /        → nginx (Angular static)   `docker/web.Dockerfile`           │
│    /api/*   → ASP.NET Core API         `docker/api.Dockerfile`           │
│    /db/*    → Adminer                  `docker/docker-compose.yml`       │
└────────────┬───────────────────────────────────────┬────────────────────┘
             │ REST /api/v1/* (JWT Bearer)             │
             ▼                                         ▼
┌──────────────────────────────┐          ┌──────────────────────────────┐
│  Host / Composition Root      │          │  MySQL 8                      │
│  `src/Host/ControlEasyReborn  │          │  `docker/mysql/init/*.sql`    │
│   .Api/Program.cs`            │─────────▶│  Schema + seed scripts        │
└──────────────┬───────────────┘          └──────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  Feature Modules (Clean Architecture per module)                         │
│  `src/Modules/{Tenants,Security,Residents,Apartments,Visits,...}/`       │
│    Api → Application (Handlers) → Domain                                 │
│    Infrastructure (Repositories via DBTools_SQL)                         │
├─────────────────────────────────────────────────────────────────────────┤
│  BuildingBlocks                                                          │
│  `src/BuildingBlocks/ControlEasyReborn.SharedKernel/`                    │
│  `src/BuildingBlocks/ControlEasyReborn.Infrastructure/`                  │
├─────────────────────────────────────────────────────────────────────────┤
│  Data Access Library (vendored)                                          │
│  `src/lib/DBTools_SQL/DBTools/` → `IAsyncSqlClient`, LINQ helpers        │
└─────────────────────────────────────────────────────────────────────────┘
```

## Component Responsibilities

| Component | Responsibility | File |
|-----------|----------------|------|
| API Host | Composition root: DI, middleware pipeline, endpoint registration, global exception mapping | `src/Host/ControlEasyReborn.Api/Program.cs` |
| Feature Module (Api) | Minimal API route groups, authorization policies, request/response binding | `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Api/Endpoints/ResidentEndpoints.cs` |
| Feature Module (Application) | Use-case handlers, FluentValidation, repository abstractions, DTOs | `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Application/Handlers/CreateResidentHandler.cs` |
| Feature Module (Domain) | Entities, value objects, domain events | `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Domain/Entities/Resident.cs` |
| Feature Module (Infrastructure) | Repository implementations, module DI registration | `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Infrastructure/Persistence/ResidentRepository.cs` |
| Shared Kernel | Cross-cutting primitives, multi-tenancy contracts, feature flags, demo personas | `src/BuildingBlocks/ControlEasyReborn.SharedKernel/` |
| Cross-cutting Infrastructure | DBTools wiring, tenant middleware, demo/bootstrap hosting | `src/BuildingBlocks/ControlEasyReborn.Infrastructure/` |
| DBTools_SQL | Multi-provider async SQL client, query interceptors, LINQ translation | `src/lib/DBTools_SQL/DBTools/Core/AsyncSqlClient.cs` |
| Angular SPA | Standalone components, feature pages, design system, HTTP interceptors | `src/Web/ControlEasyReborn.Web/src/app/` |
| Docker stack | Container build, MySQL init, Traefik routing | `docker/docker-compose.yml` |

## Pattern Overview

**Overall:** Modular Monolith with Clean Architecture (vertical slices per feature module), CQRS-lite for reads (Reports), Strangler Fig migration from legacy WPF.

**Key Characteristics:**
- Single deployable API (`ControlEasyReborn.Api`) composing nine feature modules
- Per-module four-layer structure: Domain → Application → Infrastructure → Api
- Handler-based application layer (no MediatR); handlers registered as scoped services
- Minimal APIs with extension methods (`MapResidentEndpoints`, `MapSecurityApi`)
- Multi-tenant shared schema: JWT `tenant_id` claim + `TenantFilterInterceptor` on all tenant-scoped queries
- Data access exclusively through vendored `DBTools_SQL` (`IAsyncSqlClient`); no Entity Framework in Reborn code
- Angular 18 standalone components with lazy-loaded routes and signals-based services

## Layers

**Presentation (Angular SPA):**
- Purpose: User interface, client-side routing, API consumption
- Location: `src/Web/ControlEasyReborn.Web/src/app/`
- Contains: Feature pages (`features/`), design system (`design-system/`), core services/guards/interceptors (`core/`), layout shell (`layout/`)
- Depends on: REST API at `/api/v1/*`
- Used by: End users via browser

**API / Host:**
- Purpose: HTTP entry point, cross-cutting middleware, module composition
- Location: `src/Host/ControlEasyReborn.Api/`
- Contains: `Program.cs`, `GlobalExceptionHandler`, `FeatureEndpoints.cs`, `PlatformAdminBootstrapService.cs`
- Depends on: All module Api + Infrastructure DI extensions, BuildingBlocks
- Used by: Angular SPA, integration tests

**Module Api:**
- Purpose: HTTP route definitions, authorization attributes, OpenAPI tags
- Location: `src/Modules/{Module}/ControlEasyReborn.Modules.{Module}.Api/`
- Contains: `Endpoints/*Endpoints.cs`, `DI/*Extensions.cs`, `Auth/*` (Security, Tenants)
- Depends on: Module Application (handlers, contracts)
- Used by: Host via `Map*Endpoints()` calls in `Program.cs`

**Module Application:**
- Purpose: Use-case orchestration, validation, cross-module abstractions
- Location: `src/Modules/{Module}/ControlEasyReborn.Modules.{Module}.Application/`
- Contains: `Handlers/`, `Validators/`, `Contracts/` (DTOs), `Abstractions/` (repository interfaces), `Errors/` (domain exceptions)
- Depends on: Own Domain; may reference other modules' Application abstractions (e.g. Residents → Apartments `IApartmentRepository`)
- Used by: Module Api endpoints (handler injection)

**Module Domain:**
- Purpose: Business entities and invariants
- Location: `src/Modules/{Module}/ControlEasyReborn.Modules.{Module}.Domain/`
- Contains: `Entities/`, `ValueObjects/`, `Events/`
- Depends on: SharedKernel only (when needed)
- Used by: Application handlers, Infrastructure mappers

**Module Infrastructure:**
- Purpose: Persistence, external integrations, module DI
- Location: `src/Modules/{Module}/ControlEasyReborn.Modules.{Module}.Infrastructure/`
- Contains: `Persistence/*Repository.cs`, `DI/*ModuleServiceCollectionExtensions.cs`
- Depends on: Application abstractions, BuildingBlocks Infrastructure (tenant factory), DBTools
- Used by: Host DI registration (`AddResidentsModule()`, etc.)

**BuildingBlocks:**
- Purpose: Shared cross-cutting concerns not owned by a single feature
- Location: `src/BuildingBlocks/ControlEasyReborn.SharedKernel/`, `src/BuildingBlocks/ControlEasyReborn.Infrastructure/`
- Contains: `ITenantContext`, `TenantResolutionMiddleware`, `TenantAwareLinqFactory`, demo/bootstrap extensions, DBTools registration
- Depends on: DBTools library
- Used by: All modules and Host

## Data Flow

### Primary Request Path (Authenticated API)

1. Browser sends HTTP request with `Authorization: Bearer {jwt}` — Angular `authInterceptor` in `src/Web/ControlEasyReborn.Web/src/app/core/interceptors/auth.interceptor.ts` attaches token
2. Traefik routes `/api/*` to API container (`docker/docker-compose.yml` labels)
3. ASP.NET Core pipeline runs: CORS → Authentication (JWT) → `TenantResolutionMiddleware` → Authorization — `src/Host/ControlEasyReborn.Api/Program.cs`
4. `TenantResolutionMiddleware` (`src/BuildingBlocks/ControlEasyReborn.Infrastructure/MultiTenancy/TenantResolutionMiddleware.cs`) extracts `tenant_id`, `profile_id`, roles, permissions from JWT claims into `ITenantContext`
5. Minimal API endpoint invokes scoped handler (e.g. `CreateResidentHandler.HandleAsync`) — `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Api/Endpoints/ResidentEndpoints.cs`
6. Handler validates via FluentValidation, applies business rules, calls repository abstraction — `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Application/Handlers/CreateResidentHandler.cs`
7. Repository obtains tenant-scoped `IAsyncSqlClient` from `TenantAwareLinqFactory`, executes SQL; `TenantFilterInterceptor` appends `WHERE tenant_id = @ctx_tenant` — `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Infrastructure/Persistence/ResidentRepository.cs`
8. Response serialized as JSON; errors caught by `GlobalExceptionHandler` → RFC 7807 ProblemDetails

### Login / Session Establishment

1. `POST /api/v1/security/auth/login` (public, no tenant middleware enforcement for unauthenticated)
2. `LoginHandler` validates credentials, resolves `AttendantProfile` or platform/tenant admin — `src/Modules/Security/ControlEasyReborn.Modules.Security.Application/Handlers/LoginHandler.cs`
3. `IJwtTokenService` issues JWT with `tenant_id`, `profile_id`, `roles`, `permissions` claims
4. Angular `AuthService` stores tokens; subsequent requests carry tenant context automatically

### Angular Feature Page Load

1. Route matched in `src/Web/ControlEasyReborn.Web/src/app/app.routes.ts` (lazy `loadComponent`)
2. `authGuard` / `platformAdminGuard` / `demoModeGuard` gate access — `src/Web/ControlEasyReborn.Web/src/app/core/guards/`
3. Feature page injects `*ApiService` (e.g. `ResidentsApiService`) — `src/Web/ControlEasyReborn.Web/src/app/features/residents/residents-api.service.ts`
4. HTTP client calls `/api/v1/{resource}`; `errorInterceptor` maps ProblemDetails to user-facing toasts

**State Management:**
- Server: Stateless per request; tenant/user context in scoped `HttpTenantContext`
- Client: Angular signals in services (`AuthService`, `TenantSessionService`); no NgRx
- Database: MySQL persistent state; demo overlay via `docker/docker-compose.demo.yml` and `DemoSeederService`

## Key Abstractions

**Handler (Application use case):**
- Purpose: Single-responsibility command/query executor
- Examples: `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Application/Handlers/CreateResidentHandler.cs`, `src/Modules/Reports/ControlEasyReborn.Modules.Reports.Application/Handlers/GetDashboardStatsHandler.cs`
- Pattern: Plain sealed class with `HandleAsync`; registered scoped in module DI extension

**Repository:**
- Purpose: Persistence boundary for aggregates
- Examples: `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Infrastructure/Persistence/ResidentRepository.cs`, `src/Modules/Tenants/ControlEasyReborn.Modules.Tenants.Infrastructure/Persistence/TenantRepository.cs`
- Pattern: Interface in Application `Abstractions/`; implementation uses `ITenantAwareLinqFactory.Create(_ctx)` + manual row mapping

**ITenantContext / TenantAwareLinqFactory:**
- Purpose: Per-request tenant isolation for all DB operations
- Examples: `src/BuildingBlocks/ControlEasyReborn.SharedKernel/MultiTenancy/ITenantContext.cs`, `src/BuildingBlocks/ControlEasyReborn.Infrastructure/MultiTenancy/TenantAwareLinqFactory.cs`, `src/BuildingBlocks/ControlEasyReborn.Infrastructure/MultiTenancy/TenantFilterInterceptor.cs`
- Pattern: Middleware sets context; factory wraps `AsyncSqlClient` with interceptor; platform-admin operations bypass filter

**Module DI Extension:**
- Purpose: Encapsulate module service registration
- Examples: `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Infrastructure/DI/ResidentsModuleServiceCollectionExtensions.cs`
- Pattern: `Add{Module}Module()` extension on `IServiceCollection`; called from `Program.cs`

**Endpoint Extension:**
- Purpose: Map module routes onto `WebApplication`
- Examples: `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Api/Endpoints/ResidentEndpoints.cs`, `src/Modules/Security/ControlEasyReborn.Modules.Security.Api/DI/SecurityEndpointsExtensions.cs`
- Pattern: Static class with `Map{Entity}Endpoints(this IEndpointRouteBuilder app)` returning route group under `/api/v1/`

**Feature ApiService (Angular):**
- Purpose: Typed HTTP client per feature
- Examples: `src/Web/ControlEasyReborn.Web/src/app/features/residents/residents-api.service.ts`, `src/Web/ControlEasyReborn.Web/src/app/features/dashboard/dashboard-api.service.ts`
- Pattern: `@Injectable({ providedIn: 'root' })` with local TypeScript interfaces (OpenAPI gen configured but DTOs often hand-maintained)

## Entry Points

**ASP.NET Core API:**
- Location: `src/Host/ControlEasyReborn.Api/Program.cs`
- Triggers: HTTP requests to `/api/v1/*`, `/health`, demo/bootstrap endpoints
- Responsibilities: Compose modules, configure JWT/Swagger/Serilog, register middleware pipeline, map all endpoint groups

**Angular bootstrap:**
- Location: `src/Web/ControlEasyReborn.Web/src/main.ts`
- Triggers: Browser load of SPA
- Responsibilities: Bootstrap standalone `AppComponent` with `appConfig` providers (`src/Web/ControlEasyReborn.Web/src/app/app.config.ts`)

**Docker Compose:**
- Location: `docker/docker-compose.yml` (demo overlay: `docker/docker-compose.demo.yml`)
- Triggers: `docker compose up`
- Responsibilities: Start api, web, db, traefik, adminer, seq services

**MySQL schema initialization:**
- Location: `docker/mysql/init/*.sql` (ordered `00-` through `11-`)
- Triggers: First container start (empty volume)
- Responsibilities: Create tables, views, tenant seed, demo data

**Integration test host:**
- Location: `tests/ControlEasyReborn.IntegrationTests/TenantAwareWebApplicationFactory.cs`
- Triggers: xUnit test run
- Responsibilities: Spin up in-memory API with Testcontainers MySQL

## Architectural Constraints

- **Threading:** ASP.NET Core request-per-thread async model; no background workers except `PlatformAdminBootstrapService` hosted service
- **Global state:** No static mutable application state; `TenantAwareLinqFactory` and DBTools clients are singleton/scoped per DI rules
- **Layer dependencies:** Enforced by NetArchTest — Domain must not reference Infrastructure; Infrastructure must not reference EF Core or MySql.Data — `tests/ControlEasyReborn.ArchitectureTests/LayerDependencyTests.cs`
- **Multi-tenancy:** All tenant-scoped reads/writes must go through `ITenantAwareLinqFactory`; platform tables bypass via `__bypassTenantFilter`
- **Cross-module references:** Application layers may reference other modules' Application abstractions only (not Infrastructure)
- **Legacy coexistence:** Strangler Fig assumes legacy WPF may still write to same MySQL schema; Reborn uses new normalized tables in init scripts, not legacy EF models

## Anti-Patterns

### Direct IAsyncSqlClient injection in handlers

**What happens:** Handlers take `IAsyncSqlClient` and write SQL inline.
**Why it's wrong:** Bypasses repository boundary and tenant interceptor setup; violates Clean Architecture.
**Do this instead:** Define `I{Entity}Repository` in Application `Abstractions/`; implement in Infrastructure using `ITenantAwareLinqFactory` — pattern in `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Infrastructure/Persistence/ResidentRepository.cs`.

### Manual tenant_id in every repository query

**What happens:** Each repository method adds `WHERE tenant_id = ...` manually.
**Why it's wrong:** Easy to forget on new queries; cross-tenant data leak risk.
**Do this instead:** Rely on `TenantFilterInterceptor` via `TenantAwareLinqFactory.Create(_ctx)` — `src/BuildingBlocks/ControlEasyReborn.Infrastructure/MultiTenancy/TenantFilterInterceptor.cs`.

### Adding MediatR or EF Core to modules

**What happens:** Introducing alternate dispatch or ORM alongside DBTools.
**Why it's wrong:** Conflicts with project ADR (`docs/architecture/decisions/0002-dbtools-sql-as-only-data-access.md`) and architecture tests.
**Do this instead:** Register handlers directly in `{Module}ModuleServiceCollectionExtensions`; use DBTools LINQ/SQL APIs.

### Controllers instead of Minimal APIs

**What happens:** Adding MVC `[ApiController]` classes for new endpoints.
**Why it's wrong:** Existing modules consistently use static endpoint classes; Host has no controller registration.
**Do this instead:** Add methods to `Endpoints/{Entity}Endpoints.cs` and register via `MapGroup` — `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Api/Endpoints/ResidentEndpoints.cs`.

## Error Handling

**Strategy:** Domain-specific exceptions thrown from handlers; global `IExceptionHandler` maps to HTTP status + ProblemDetails JSON.

**Patterns:**
- Per-module `Errors/DomainExceptions.cs` with `NotFoundException`, `ValidationException`, `ConflictException`, `UnauthorizedException` — e.g. `src/Modules/Residents/ControlEasyReborn.Modules.Residents.Application/Errors/DomainExceptions.cs`
- `GlobalExceptionHandler` in `src/Host/ControlEasyReborn.Api/Program.cs` pattern-matches exceptions to 400/401/404/409; validation errors include `errors` extension dictionary
- Unhandled 500s fall through (returns false from handler for default ASP.NET behavior)
- Angular `errorInterceptor` (`src/Web/ControlEasyReborn.Web/src/app/core/interceptors/error.interceptor.ts`) surfaces API errors to UI

## Cross-Cutting Concerns

**Logging:**
- Serilog configured in `Program.cs`; structured JSON to console; Seq sink in Docker (`docker/docker-compose.yml`)
- `UseSerilogRequestLogging()` for HTTP request logs

**Validation:**
- FluentValidation validators co-located in Application `Validators/`; invoked at start of handler `HandleAsync`
- ASP.NET `[FromBody]` binding for request DTOs at endpoint level

**Authentication:**
- JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) configured in `Program.cs`
- Permission policies registered dynamically from `Permissions.All` — `src/Modules/Security/ControlEasyReborn.Modules.Security.Application/Permissions.cs`
- `[RequirePermission("Residents.Write")]` / policy `"Permission_Residents.Write"` on write endpoints

**Authorization:**
- Role-based: `PlatformAdmin` bypasses tenant requirement
- Permission-based: fine-grained claims in JWT `permissions` array
- Handlers: `RequirePermissionAuthorizationHandler` — `src/Modules/Security/ControlEasyReborn.Modules.Security.Api/Auth/RequirePermissionAuthorizationHandler.cs`

**Feature flags:**
- Microsoft.FeatureManagement; endpoints in `src/Host/ControlEasyReborn.Api/Hosting/FeatureEndpoints.cs`

**Demo mode:**
- `AddControlEasyDemo()` / demo compose overlay; `DemoSeederService`, `DemoEndpoints` in BuildingBlocks Infrastructure

---

*Architecture analysis: 2026-06-24*
*Update when major patterns change*
