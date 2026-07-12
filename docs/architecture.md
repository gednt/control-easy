# Architecture — ControlEasy Reborn

**Analysis date:** 2026-07-12
**Parts:** `api` (backend) + `web` (web)
**Pattern:** Modular Monolith · Clean Architecture per module · CQRS-lite (Reports) · Strangler Fig (scaffolding only — legacy not in repo)

## System Diagram

```text
┌─────────────────────────────────────────────────────────────────────────┐
│                         Browser (Angular 18 SPA)                         │
│         src/Web/ControlEasyReborn.Web/src/app/  (Part: web)              │
├─────────────────────────────────────────────────────────────────────────┤
│  Traefik reverse proxy (docker/reverse-proxy/)   :8080                   │
│    /        → nginx (Angular static)              docker/web.Dockerfile  │
│    /api/*   → ASP.NET Core API                    docker/api.Dockerfile  │
│    /db/*    → Adminer                             docker/docker-compose  │
└────────────┬───────────────────────────────────────┬────────────────────┘
             │ REST /api/v1/* (JWT Bearer)             │
             ▼                                         ▼
┌──────────────────────────────┐          ┌──────────────────────────────┐
│  Host / Composition Root      │          │  MySQL 8                      │
│  src/Host/ControlEasyReborn   │          │  docker/mysql/init/*.sql      │
│  .Api/Program.cs (Part: api)  │─────────▶│  Schema + seed scripts        │
└──────────────┬───────────────┘          └──────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  Feature Modules (Clean Architecture per module)                         │
│  src/Modules/{Tenants,Security,Residents,Apartments,Visits,              │
│               Vehicles,ServiceProviders,Administration,Reports}/          │
│    Api → Application (Handlers, Validators) → Domain                     │
│    Infrastructure (Repositories via DBTools NuGet 1.4.3)                 │
├─────────────────────────────────────────────────────────────────────────┤
│  BuildingBlocks                                                          │
│  src/BuildingBlocks/ControlEasyReborn.SharedKernel/                       │
│  src/BuildingBlocks/ControlEasyReborn.Infrastructure/                     │
│    • DBTools DI registration                                              │
│    • TenantResolutionMiddleware                                           │
│    • TenantFilterInterceptor + TenantAwareLinqFactory                     │
│    • DemoSeederService, PlatformAdminBootstrapService                     │
└─────────────────────────────────────────────────────────────────────────┘
```

## Layer Responsibilities (per module)

| Layer | Project | Purpose |
|-------|---------|---------|
| Domain | `ControlEasyReborn.Modules.{Module}.Domain` | Entities, value objects, domain events. Depends only on SharedKernel. |
| Application | `ControlEasyReborn.Modules.{Module}.Application` | Use-case handlers, FluentValidation validators, DTO contracts, repository abstractions, domain exceptions. May reference other modules' Application abstractions only. |
| Infrastructure | `ControlEasyReborn.Modules.{Module}.Infrastructure` | Repository implementations, module DI. Uses `ITenantAwareLinqFactory` from BuildingBlocks.Infrastructure. |
| Api | `ControlEasyReborn.Modules.{Module}.Api` | Minimal API endpoint groups (`Map{Entity}Endpoints`); authorization policies; OpenAPI tags. |

## Data Flow — Authenticated Request

1. **Browser** sends `Authorization: Bearer {jwt}`. `authInterceptor` (`src/Web/.../core/interceptors/auth.interceptor.ts`) attaches the token.
2. **Traefik** routes `/api/*` → `api:8080` (`docker/docker-compose.yml` labels).
3. **ASP.NET Core pipeline** runs: CORS → JWT authentication → `TenantResolutionMiddleware` → authorization.
4. `TenantResolutionMiddleware` (`src/BuildingBlocks/.../MultiTenancy/TenantResolutionMiddleware.cs`) extracts `tenant_id`, `profile_id`, `roles`, `permissions` from JWT into scoped `ITenantContext`.
5. **Minimal API** invokes scoped handler (e.g. `CreateResidentHandler.HandleAsync`).
6. Handler validates via FluentValidation, applies business rules, calls `I{Entity}Repository`.
7. Repository obtains tenant-scoped `IAsyncSqlClient` from `TenantAwareLinqFactory`; `TenantFilterInterceptor` appends `WHERE tenant_id = @ctx_tenant`.
8. Response JSON; errors mapped by `GlobalExceptionHandler` → RFC 7807 `ProblemDetails`.
9. Angular `errorInterceptor` surfaces API errors to UI.

## Multi-Tenancy

- **Strategy:** Shared schema, JWT-claim-based.
- **Enforcement:** `TenantFilterInterceptor` rewrites tenant-scoped SQL to add `WHERE tenant_id = @ctx_tenant`.
- **Bypass:** Platform tables (auth, tenants registry, platform admin) use raw `IAsyncSqlClient` without the interceptor. Marked via `__bypassTenantFilter` convention.
- **Known fragility:** Interceptor does naive `string.Contains` on SQL text; multi-table JOINs without explicit aliases can return empty data (see `CONCERNS.md` — `occupiedApartments` bug was caused by this).
- **Architecture test:** `LayerDependencyTests` enforces Domain ↛ Infrastructure; `SchemaBackfillSyncTests` enforces `docker/mysql/init/*.sql` ↔ `03-tenant-backfill.sql` marker sync.

## Error Handling

- Per-module `Application/Errors/DomainExceptions.cs`:
  - `NotFoundException` → 404
  - `ConflictException` → 409
  - `ValidationException` (with `IReadOnlyDictionary<string, string[]>`) → 400
  - Security module adds `UnauthorizedException` → 401
- `GlobalExceptionHandler` (in `Program.cs`) implements `IExceptionHandler`, maps to RFC 7807 `ProblemDetails`.
- Angular `errorInterceptor` translates to user-facing toasts.

## Authentication & Authorization

- **Provider:** Custom JWT (HMAC-SHA256, symmetric). No Auth0, Azure AD, or OAuth2.
- **Token issuance:** `JwtTokenService` (`src/Modules/Security/.../Persistence/JwtTokenService.cs`).
- **Endpoints:** `POST /api/v1/security/auth/login`, `POST /api/v1/security/auth/refresh`.
- **Password hashing:** BCrypt (`PasswordHasher.cs`).
- **Refresh tokens:** Persisted in MySQL; rotation on use.
- **Claims:** `tenant_id`, `profile_id`, `roles[]`, `permissions[]`.
- **Roles:** `PlatformAdmin`, `TenantAdmin`, `Attendant`.
- **Permissions:** Fine-grained; registered dynamically from `Permissions.All`; `[RequirePermission("Residents.Write")]` on write endpoints.

## Cross-Cutting Concerns

| Concern | Implementation |
|---------|----------------|
| Logging | Serilog → console JSON + Seq at `http://seq:5341`; `UseSerilogRequestLogging()` |
| Validation | FluentValidation per request type, invoked at start of `HandleAsync` |
| AuthN/Z | JWT bearer + role/permission policies + custom `RequirePermissionAuthorizationHandler` |
| Feature flags | Microsoft.FeatureManagement; flags registered in `appsettings.json` (currently all `false`; legacy WPF no longer in scope) |
| Demo mode | `AddControlEasyDemo()`, `DemoSeederService`, `DemoEndpoints`, `docker-compose.demo.yml` overlay |
| Bootstrap | `PlatformAdminBootstrapService` hosted service seeds `platform-admin@controleasy.local` + matching `AttendantProfiles` row |
| Tenant resolution | `TenantResolutionMiddleware` → `ITenantContext` (scoped) |
| Tenant filtering | `TenantFilterInterceptor` (string-contains SQL rewrite) |
| Health checks | `GET /health` via `AspNetCore.HealthChecks.MySql` |
| OpenAPI | Swashbuckle at `/swagger` (Development only); `ng-openapi-gen` configured but not yet wired into web Docker build |

## Architectural Constraints (enforced by `LayerDependencyTests`)

1. **Domain must not reference Infrastructure** or other modules' Infrastructure.
2. **Infrastructure must not reference EntityFramework or MySql.Data** (only DBTools).
3. **Cross-module references** are allowed only between Application layers of different modules.
4. **All tenant-scoped reads/writes** go through `ITenantAwareLinqFactory`.
5. **Handlers registered as scoped** services; no MediatR.
6. **Endpoints via Minimal API only** (no MVC `[ApiController]`).

## Anti-Patterns to Avoid

- **Direct `IAsyncSqlClient` injection in handlers** — bypasses repository boundary and tenant interceptor.
- **Manual `WHERE tenant_id =` in repositories** — relies on human discipline; the interceptor already does this.
- **Adding MediatR or EF Core to modules** — conflicts with ADR 0002.
- **Adding MVC controllers** — modules use static endpoint classes; Host has no controller registration.
- **Forgetting `dbtools-nuget-migration` invariants** — `DBTools` must be a NuGet package reference, not vendored.

## Key Abstractions (for AI agents)

| Abstraction | Where | Purpose |
|-------------|-------|---------|
| `HandleAsync` (handler) | `Modules/*/Application/Handlers/` | Single-responsibility use-case executor |
| `I{Entity}Repository` | `Modules/*/Application/Abstractions/` | Persistence boundary; tenant-scoped |
| `ITenantContext` | `BuildingBlocks/SharedKernel/MultiTenancy/` | Per-request tenant + roles + permissions |
| `TenantAwareLinqFactory` | `BuildingBlocks/Infrastructure/MultiTenancy/` | Yields `IAsyncSqlClient` wrapped in `TenantFilterInterceptor` |
| `Map{Entity}Endpoints` | `Modules/*/Api/Endpoints/` | Static class registering route group under `/api/v1/` |
| `{Module}ModuleServiceCollectionExtensions` | `Modules/*/Infrastructure/DI/` | One-call module DI registration from `Program.cs` |
| `FeatureApiService` (Angular) | `Web/.../src/app/features/{feature}/` | Typed HTTP client per feature; manual DTOs until `ng-openapi-gen` is wired |

## Where to Add New Code

- **New feature module:** see `STRUCTURE.md` § "Where to Add New Code" — full 4-project slice + endpoint + tests + SQL init script + `Program.cs` registration.
- **New endpoint in existing module:** add handler in `Application/Handlers/`, register in `{Module}ModuleServiceCollectionExtensions`, route in `Endpoints/{Entity}Endpoints.cs`.
- **New Angular feature page:** add `{feature}.page.ts` + `{feature}-api.service.ts` in `src/app/features/`, register lazy route in `app.routes.ts`, reuse `ce-*` design-system components.
- **New design-system component:** add to `src/app/design-system/components/`, export from `design-system/index.ts`, add co-located `.spec.ts`.
- **Cross-cutting backend concern:** add to `BuildingBlocks/SharedKernel` (primitives) or `BuildingBlocks/Infrastructure` (middleware, DI, interceptors).

---

*Architecture document: 2026-07-12 · Consolidated from GSD `ARCHITECTURE.md` (2026-06-24) + ADRs 0001–0004. Source for AI agents — read alongside the per-topic maps in `.planning/codebase/`.*
