# Component Inventory — ControlEasy Reborn

**Analysis date:** 2026-07-12

## Part: `api` — Backend Components

### Feature Modules (9, vertical slices)

Each module follows the pattern `ControlEasyReborn.Modules.{Module}.{Layer}` with up to 4 projects.

| Module | Domain | Application | Infrastructure | Api | Notes |
|--------|--------|-------------|----------------|-----|-------|
| Tenants | ✓ | ✓ | ✓ | ✓ | Platform tables, tenant backup endpoints |
| Security | ✓ | ✓ | ✓ | ✓ | Auth, JWT, RBAC, permissions catalog |
| Residents | ✓ | ✓ | ✓ | ✓ | CRUD, search, soft-delete; references Apartments |
| Apartments | ✓ | ✓ | ✓ | ✓ | CRUD + inline resident management (Phase 6) |
| Visits | ✓ | ✓ | ✓ | ✓ | Check-in/out, open visits list |
| Vehicles | ✓ | ✓ | ✓ | ✓ | Linked to apartments |
| ServiceProviders | ✓ | ✓ | ✓ | ✓ | CRUD |
| Administration | ✓ | ✓ | ✓ | ✓ | Audit log + config |
| Reports | ✓ | ✓ | ✓ | ✓ | CQRS-lite read projections |

### Building Blocks (cross-cutting)

- `ControlEasyReborn.SharedKernel`:
  - `ITenantContext` — per-request tenant + roles + permissions
  - `Primitives.Guard` — `AgainstNull`, `AgainstNullOrEmpty`, `AgainstNotFound`
  - `FeatureFlags` — `FeatureFlags.cs` (Microsoft.FeatureManagement wrapper)
  - `Demo/DemoOptions` — `DisableOutboundEmail`, `SeedVersion`
  - `Errors` — shared error primitives

- `ControlEasyReborn.Infrastructure`:
  - `Data/ServiceCollectionExtensions` — DBTools DI registration
  - `MultiTenancy/TenantResolutionMiddleware` — JWT claim → `ITenantContext`
  - `MultiTenancy/TenantAwareLinqFactory` — yields tenant-filtered `IAsyncSqlClient`
  - `MultiTenancy/TenantFilterInterceptor` — SQL `tenant_id` injector
  - `Demo/DemoSeederService` + `DemoEndpoints` + `AddControlEasyDemo()`
  - `Hosting/PlatformAdminBootstrapService` — first-boot seed

### Key Application Abstractions (per module)

- Handlers: `{Verb}{Entity}Handler` (e.g. `CreateResidentHandler`)
- Validators: `{Request}Validator` extending `AbstractValidator<T>`
- Repositories: `I{Entity}Repository` / `{Entity}Repository`
- Endpoints: `{Entity}Endpoints` with `Map{Entity}Endpoints` extension
- DTOs: `sealed record` in `Application/Contracts/{Entity}Dtos.cs`
- Domain exceptions: `Application/Errors/DomainExceptions.cs`

## Part: `web` — Frontend Components

### App-level (`src/app/`)
- `core/guards/` — `authGuard`, `platformAdminGuard`, `demoModeGuard`
- `core/interceptors/` — `authInterceptor`, `errorInterceptor`
- `core/services/` — `AuthService`, `TenantSessionService`, `security-api.service.ts`
- `core/utils/` — `cpf.util.ts` and shared utilities
- `features/` — lazy-loaded pages, one per route:
  - `login`, `change-password`, `dashboard`
  - `residents`, `apartments`, `visits`, `vehicles`, `service-providers`
  - `tenant-administration` (PlatformAdmin only)
  - `attendant-profiles` (TenantAdmin)
  - `audit-log` (Administration)
  - `design-system-showcase`
- `design-system/components/` — `ce-*` primitives (button, input, table, modal, etc.)
- `design-system/tokens/` — CSS custom properties (`--space-*`, `--color-*`)
- `design-system/theme/` — light/dark theme variables
- `layout/` — app shell, demo banner
- `shared/` — cross-feature components (e.g. apartment picker)
- `api/` — OpenAPI-generated client (**target, not yet present**)

### Component Conventions (Angular)

- **Class name:** `Ce{Name}Component` (PascalCase, `Ce` prefix)
- **Selector:** `ce-{name}` (kebab-case element)
- **Attribute directive selector:** `ce{Name}` (camelCase)
- **Standalone:** `standalone: true` (no NgModules)
- **Change detection:** `OnPush`
- **Styles:** `style: css` (no SCSS by default)
- **i18n source locale:** `pt-BR`
- **Inputs:** Angular signals API — `input()`, `output()`, `computed()`
- **Test pattern:** co-located `*.component.spec.ts` for design-system primitives; **feature pages have no specs yet**
- **State:** Angular signals in services; no NgRx
- **HTTP:** `inject(HttpClient)` (not constructor injection, where consistent)
- **Conditional classes:** `clsx`

### Iconography
- `lucide-angular 0.454.0` — Lucide icon library

## Test Components

### Unit Tests (`tests/ControlEasyReborn.UnitTests/`)
- Organized by module: `tests/.../UnitTests/Modules/{Module}/`
- Cross-cutting: `MultiTenancy/`, `Hosting/`, `Demo/`
- Test doubles: `TestDoubles/FakeAsyncSqlClient.cs`, `FakeTenantContext`, `FakeTenantAwareLinqFactory`
- Conventions: xUnit + FluentAssertions + NSubstitute; `_sut` field; `{Method}_with_{condition}_{expected}`

### Integration Tests (`tests/ControlEasyReborn.IntegrationTests/`)
- `MySqlContainerFixture` — Testcontainers MySQL
- `TestcontainersWebApplicationFactory` / `TenantAwareWebApplicationFactory` / `DemoWebApplicationFactory` — `WebApplicationFactory<Program>` with JWT helpers
- Per-module: `{Module}EndpointTests.cs`
- Cross-tenant naming enforced: classes touching repositories must include a `CrossTenant_*` `[Fact]`

### Architecture Tests (`tests/ControlEasyReborn.ArchitectureTests/`)
- `LayerDependencyTests` — Domain ↛ Infrastructure; Infrastructure ↛ EF/MySql.Data
- `CrossTenantTestNamingTests` — integration class naming
- `SchemaBackfillSyncTests` — `docker/mysql/init/*.sql` ↔ `03-tenant-backfill.sql` markers
- `TenantIdPropertyTests` — entity tenant ID conventions

### Frontend Tests
- **Karma + Jasmine** (Angular unit): `src/Web/.../src/**/*.spec.ts` — primarily design-system components
- **Playwright E2E** (`e2e/`): `first-boot-login`, `apartments-crud`, `resident-create`, `vehicle-create`, `visit-checkin-checkout`, `platform-condominiums`, `module-pages`, `apartment-residents`
- **Playwright Visual** (`tests/visual/`): snapshot regressions for design system at multiple viewports × themes
- **Playwright A11y** (`tests/a11y/`): `AxeBuilder` with WCAG 2.x tags; fail on `serious`/`critical`

## Reusable vs Specific

| Reusable (cross-module / cross-feature) | Specific (one feature) |
|------------------------------------------|--------------------------|
| `ce-*` design-system primitives | Feature pages (`residents.page.ts`, etc.) |
| `Guard` (SharedKernel.Primitives) | Per-module `DomainExceptions` |
| `TenantFilterInterceptor` + `TenantAwareLinqFactory` | Per-module repository implementations |
| `GlobalExceptionHandler` (Host) | Per-module `*ProblemDetailsWriter` (legacy; prefer global) |
| `AuthService` (Angular) + `AuthInterceptor` | `ResidentsApiService` (per feature) |
| `ErrorInterceptor` (Angular) | |
| `CpfUtil` (Angular) | |
| `Mockup` showcase (`mockup/showcase.html`) | |

## Inventory Gaps (not yet implemented)

- `src/app/api/` (OpenAPI-generated client) — planned via `ng-openapi-gen`, not yet wired into Docker build
- `.github/workflows/*.yml` (CI pipeline) — `REQUIREMENTS.md` CI-01
- Redis service — referenced in `AGENTS.md` as optional, not yet in compose
- Email provider — `Demo:DisableOutboundEmail` flag exists but no implementation
- Photo capture / storage abstraction (`.specs/3 - photo-capture-hardware-integration/`)
- Hardware framework (biometrics, cameras, intercoms)

---

*Component inventory: 2026-07-12 · Sources: GSD `STACK.md` + `STRUCTURE.md` + `TESTING.md` (2026-06-24) + live cross-check of `src/`.*
