# Coding Conventions

**Analysis Date:** 2026-06-24

## Scope Note

This repository's active code lives in **ControlEasy Reborn** (`src/`, `tests/`, `docker/`). Legacy **ControlEasy 5** (WPF / .NET Framework 4.8) and **ControlEasyWeb** (Blazor Server / .NET 5) are **not present** on disk; they are referenced only in specs (`README.md`, `.specs/`) and migration docs (`docs/migration/legacy-mapping.md`). All new work follows Reborn conventions below.

A build-free HTML prototype lives in `mockup/` (manual smoke checklist in `mockup/SMOKE.md`); it is not part of the production stack.

---

## ControlEasy Reborn — C# Backend

### Naming Patterns

**Projects / assemblies:**
- Pattern: `ControlEasyReborn.Modules.{Module}.{Layer}` (e.g. `ControlEasyReborn.Modules.Apartments.Application`)
- Layers: `Domain`, `Application`, `Infrastructure`, `Api`
- Building blocks: `ControlEasyReborn.SharedKernel`, `ControlEasyReborn.Infrastructure`
- Host: `ControlEasyReborn.Api` in `src/Host/ControlEasyReborn.Api/`

**Files:**
- One primary type per file; file name matches type name (e.g. `CreateApartmentHandler.cs`)
- Group related DTOs in `*Dtos.cs` or `*Contracts.cs` under `Application/Contracts/` (e.g. `src/Modules/Apartments/ControlEasyReborn.Modules.Apartments.Application/Contracts/ApartmentDtos.cs`)

**Types:**
- `sealed` classes by default for handlers, repositories, validators, endpoints
- Domain entities: `sealed class` with private setters and explicit constructors (e.g. `src/Modules/Apartments/ControlEasyReborn.Modules.Apartments.Domain/Entities/Apartment.cs`)
- DTOs / API contracts: `sealed record` with positional parameters (e.g. `CreateApartmentRequest`, `ApartmentResponse` in `ApartmentDtos.cs`)
- Handlers: `{Verb}{Entity}Handler` (e.g. `CreateApartmentHandler`, `ListResidentsHandler`) in `Application/Handlers/`
- Validators: `{Request}Validator` extending `AbstractValidator<T>` in `Application/Validators/`
- Repository interfaces: `I{Entity}Repository` in `Application/Abstractions/`; implementations `{Entity}Repository` in `Infrastructure/Persistence/`
- Endpoints: static class `{Entity}Endpoints` with `Map{Entity}Endpoints` extension in `Api/Endpoints/`
- DI registration: `{Module}ServiceCollectionExtensions` or `*Module.cs` under `Infrastructure/DI/` or `Api/DI/`

**Methods:**
- Handler entry point: `HandleAsync(...)` — always async, always accepts `CancellationToken ct` (or `CancellationToken cancellationToken`)
- Repository methods: async with `Async` suffix (`FindAsync`, `AddAsync`, `ListAsync`)
- Endpoint lambdas: inject handler + `CancellationToken ct` via minimal API DI

**Variables / fields:**
- Private instance fields: `_camelCase` (e.g. `_apartments`, `_validator` in `CreateApartmentHandler`)
- Parameters and locals: `camelCase`
- Constants: `PascalCase` or `ALL_CAPS` for true constants (e.g. `TableName` in repositories)

**Namespaces:**
- File-scoped namespaces matching folder structure:
  `namespace ControlEasyReborn.Modules.Apartments.Application.Handlers;`
- No block-style `namespace { }` wrappers

### Code Style

**Compiler / build settings** (`src/Directory.Build.props`, mirrored in `tests/Directory.Build.props`):
- Target: `net8.0`
- `LangVersion`: `latest`
- `Nullable`: `enable`
- `TreatWarningsAsErrors`: `true`
- `ImplicitUsings`: `enable`
- Central package versions: `src/Directory.Packages.props` (`ManagePackageVersionsCentrally`)

**Formatting:**
- 4-space indentation (default for C#)
- Braces on new line for types/methods; expression-bodied members for simple mappers (e.g. `ToResponse` in handlers)
- `using` directives: System first, then third-party, then project namespaces (implicit usings reduce boilerplate)

**Patterns to follow:**
- Prefer `is not null` / `is null` over `!= null` where idiomatic
- Use `ct` as the standard `CancellationToken` parameter name in handlers and repositories
- Use `Guid` for entity IDs; `DateTime.UtcNow` for timestamps
- Multi-tenancy: every tenant-scoped entity carries `TenantId`; resolve via `ITenantContext` in endpoints

### Module Layout (per feature)

```
src/Modules/{Module}/
├── ControlEasyReborn.Modules.{Module}.Domain/
│   └── Entities/, Events/
├── ControlEasyReborn.Modules.{Module}.Application/
│   ├── Abstractions/     # I*Repository
│   ├── Contracts/        # sealed record DTOs
│   ├── Errors/           # NotFoundException, ValidationException, ConflictException
│   ├── Handlers/
│   └── Validators/
├── ControlEasyReborn.Modules.{Module}.Infrastructure/
│   ├── Persistence/      # *Repository
│   └── DI/
└── ControlEasyReborn.Modules.{Module}.Api/
    ├── Endpoints/
    └── DI/
```

### API Conventions

**Routes:**
- Prefix: `/api/v1/{resource}` (plural nouns, kebab-case implied by lowercase segments)
- Example: `/api/v1/apartments`, `/api/v1/residents` (`src/Modules/Apartments/ControlEasyReborn.Modules.Apartments.Api/Endpoints/ApartmentEndpoints.cs`)
- Use `MapGroup` + `WithTags` + `RequireAuthorization`
- Write operations: `.RequireAuthorization("Permission_{Module}.{Action}")` (e.g. `Permission_Apartments.Write`)
- POST success: `Results.Created($"/api/v1/apartments/{response.Id}", response)`

**Handlers over controllers:**
- No MVC controllers; use minimal API endpoint classes
- Handlers registered in DI and injected into route delegates

### Data Access

- Use `IAsyncSqlClient` / `ITenantAwareLinqFactory` from DBTools_SQL (`src/lib/DBTools_SQL/DBTools/`)
- Repositories take `ITenantContext` + `ITenantAwareLinqFactory` in constructor
- LINQ-first; raw SQL only when LINQ provider cannot express the query
- Table names: PascalCase string constants (e.g. `"Apartments"` in `ApartmentRepository`)

### Validation

- FluentValidation `AbstractValidator<T>` per request type
- Handlers call `_validator.ValidateAsync(request, ct)` and throw module `ValidationException` with grouped errors:
  ```csharp
  throw new Errors.ValidationException(result.Errors
      .GroupBy(e => e.PropertyName)
      .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
  ```
- Validators registered via `AddValidatorsFromAssembly` in module DI

### Error Handling

**Domain exceptions** (per module, in `Application/Errors/DomainExceptions.cs`):
- `NotFoundException` → HTTP 404
- `ConflictException` → HTTP 409
- `ValidationException` (with `IReadOnlyDictionary<string, string[]> Errors`) → HTTP 400
- Security module adds `UnauthorizedException` → HTTP 401

**Handler pattern:**
- Validate first, then check existence/conflicts, then mutate, then return DTO
- Throw domain exceptions; do not return error result objects from handlers
- Example: `src/Modules/Apartments/ControlEasyReborn.Modules.Apartments.Application/Handlers/CreateApartmentHandler.cs`

**HTTP mapping:**
- Global: `GlobalExceptionHandler` implements `IExceptionHandler` in `src/Host/ControlEasyReborn.Api/Program.cs` — maps all module exceptions to RFC 7807 `ProblemDetails`
- `builder.Services.AddProblemDetails()` + `app.UseExceptionHandler()`
- Some modules also define `*ProblemDetailsWriter` middleware (e.g. `ResidentProblemDetailsWriter` in `ResidentEndpoints.cs`) — prefer the global handler for new code

**Guard helpers:**
- Use `ControlEasyReborn.SharedKernel.Primitives.Guard` for argument validation (`AgainstNull`, `AgainstNullOrEmpty`, `AgainstNotFound`)
- Tenant resolution failures: `throw new InvalidOperationException("Tenant context is not resolved.")`

### Logging

**Framework:** Serilog (`Serilog.AspNetCore`)

**Configuration:**
- Bootstrap logger in `Program.cs`; `builder.Host.UseSerilog(...)` reads from `appsettings.json`
- `app.UseSerilogRequestLogging()` for HTTP request logging
- Structured logging via Serilog configuration; enrich from log context

**Patterns:**
- Log fatal startup failures in `Program.cs` catch block
- Do not use `Console.WriteLine` in production API code
- Handler/repository layers generally rely on exception propagation rather than verbose logging

### Comments

**When to comment:**
- Explain non-obvious business rules or migration shims
- XML doc comments on public library APIs (DBTools_SQL uses `///` extensively)
- Avoid narrating obvious code

**TODO:**
- No enforced format; prefer linking to `.specs/` tasks or GitHub issues

---

## ControlEasy Reborn — Angular Frontend

Location: `src/Web/ControlEasyReborn.Web/`

### Naming Patterns

**Files:**
- Components: `{name}.component.ts`, `{name}.component.html` (inline templates common in design system)
- Services: `{feature}-api.service.ts` (e.g. `src/app/features/vehicles/vehicles-api.service.ts`)
- Tests: co-located `*.component.spec.ts`, `*.service.spec.ts`
- E2E: `e2e/*.spec.ts`

**Components:**
- Class: `Ce{Name}Component` (PascalCase with `Ce` prefix)
- Selector: `ce-{name}` (kebab-case element), enforced by ESLint
- Directive selector: `ce{Name}` (camelCase attribute)
- Project prefix `ce` configured in `angular.json`

**Services:**
- `{Feature}ApiService` with `@Injectable({ providedIn: 'root' })`
- Use `inject(HttpClient)` instead of constructor injection where consistent with existing services

**Types:**
- Interfaces for API DTOs: `{Entity}Response`, `Create{Entity}Request`, `Update{Entity}Request` (PascalCase)
- Mirror backend JSON shape; do not import C# types

**Signals / inputs:**
- Use Angular signals API: `input()`, `output()`, `computed()` (e.g. `button.component.ts`)
- Read signal values in templates with `()` — `variant()`, `loading()`

### Code Style

**TypeScript** (`tsconfig.json`):
- `strict: true` plus `noImplicitReturns`, `noUncheckedIndexedAccess`, `exactOptionalPropertyTypes`
- Target `ES2022`, `moduleResolution: bundler`
- Path aliases: `@app/*` → `./src/app/*`, `@design-system/*` → `./src/app/design-system/*`

**Formatting** (`.prettierrc`, `.editorconfig`):
- Prettier: single quotes, semicolons, `printWidth: 120`, `trailingComma: all`, `tabWidth: 2`
- EditorConfig: 2-space indent, UTF-8, final newline, trim trailing whitespace
- Run: `npm run format` in `src/Web/ControlEasyReborn.Web/`

**Linting** (`.eslintrc.json`):
- `@angular-eslint/recommended` for `*.ts` and `*.html`
- Component selector: element `ce` + kebab-case
- Directive selector: attribute `ce` + camelCase
- Run: `npm run lint`

**Angular schematics defaults** (`angular.json`):
- `standalone: true`
- `changeDetection: OnPush`
- `skipTests: true` for generated components (design-system components add specs manually)
- `style: css`
- i18n source locale: `pt-BR`

### Component Design

- Standalone components; import dependencies in `imports: [...]` array
- Prefer inline `template` and `styles` for small design-system primitives
- Use `@if` control flow (not `*ngIf`) in new templates
- CSS: design tokens as CSS variables (`var(--color-primary)`, `var(--space-2)`)
- `clsx` for conditional class names

### API Services

- Base URL: relative `/api/v1/{resource}` (proxied in dev via `proxy.conf.json`)
- Return `Observable<T>` from `HttpClient` methods
- Build query params with `HttpParams` (`skip`, `take`, optional `search`)
- Example: `src/app/features/vehicles/vehicles-api.service.ts`

### Error Handling

- HTTP errors surface via Angular `HttpClient` observables; subscribe with error callbacks or use `async` pipe + component-level handling
- No global Angular error handler detected; handle per feature as needed
- E2E tests assert HTTP status and UI feedback via Playwright

### Import Organization

1. Angular core / common / router / forms
2. Third-party (`rxjs`, `clsx`, `lucide-angular`)
3. `@app/*` / `@design-system/*` aliases
4. Relative imports (`./`, `../`)

Blank line between groups; no enforced auto-sort beyond ESLint.

---

## DBTools_SQL (vendored library)

Location: `src/lib/DBTools_SQL/DBTools/`

- Separate `Directory.Build.props`; not all Reborn `TreatWarningsAsErrors` rules apply
- Public APIs use XML documentation comments
- Interfaces: `IAsyncSqlClient`, `IQueryInterceptor`
- Throw `ArgumentNullException` / `ArgumentException` / `InvalidOperationException` for programmer errors
- Do not reference Entity Framework or `MySql.Data` from Reborn infrastructure (enforced by architecture tests)

---

## Docker / Scripts

- MySQL init scripts: `docker/mysql/init/*.sql` (ordered by filename prefix)
- Regenerate tenant backfill: `scripts/generate-tenant-backfill.sql`
- Compose: `docker/docker-compose.yml`; demo overlay `docker/docker-compose.demo.yml`

---

*Convention analysis: 2026-06-24*
*Update when patterns change*
