# Project Overview

**ControlEasy Reborn** is a greenfield rewrite of the legacy ControlEasy 5 WPF desktop system. The repo today contains the **spec-only** skeleton: AGENTS.md (root), `agents/`, `docs/penpot/`, `mockup/`, and `.specs/`. The legacy `ControlEasy5/` (WPF) and `ControlEasyWeb/` (Blazor prototype) folders are already gone from disk; this PR must not reintroduce references to them. Target: a modular monolith on **ASP.NET Core 8 (LTS)** with an **Angular 18+ SPA** (standalone components + signals) frontend, containerized via Docker + Compose, with **DBTools_SQL** as the sole data-access library. Multi-tenant from day one (shared schema, `tenant_id` discriminator enforced by a global query interceptor).

# Technical details

## Stack confirmed
- **Backend:** .NET 8 (only the 8.0 runtime is installed locally; the only SDK is 10.0 which still supports `<TargetFramework>net8.0</TargetFramework>`).
- **DBTools_SQL:** vendored under `src/lib/DBTools_SQL/DBTools/` as a faithful stub that exposes the exact public surface the Tenants module and `TenantFilterInterceptor` require. The vendor stub:
  - implements `IAsyncSqlClient`, `Linq<TModel>`, `LinqHelper<TModel>`, `IQueryInterceptor`
  - ships `AddDbTools(Action<DbToolsOptions>)` + `DbToolsOptions` + `DatabaseProvider` enum
  - ships a trivial in-memory `IDbProvider` for unit tests (canned rows, no real SQL)
  - carries a `// TODO(migration): replace with real DBTools_SQL` marker at the top of the main type so the swap point is obvious
  - inherits `LangVersion=latest`, `Nullable=enable`, `TreatWarningsAsErrors=true` via a local `Directory.Build.props` link
  - is consumed via `<ProjectReference>` (vendored projects do not appear in CPM)
- **MySQL provider package:** `MySqlConnector` 2.3.7 (declared in `src/Directory.Packages.props` for completeness even though the vendor stub does not yet bind to it).
- **CPM:** `src/Directory.Packages.props` (pinned versions) + `src/Directory.Build.props` (compiler settings). Both applied via the standard MSBuild import search up the tree.
- **Module layout:** `src/Modules/{Module}/{Domain,Application,Infrastructure,Api}` with one .csproj per layer, plus a `TenantAwareLinqFactory` in `src/BuildingBlocks/Infrastructure/MultiTenancy/`.
- **Project naming convention** (per design.md line 95+): `ControlEasyReborn.Modules.{Module}.{Layer}`.

## Discovered environment facts
- `dotnet --list-sdks` → `10.0.203`; `dotnet --list-runtimes` → `Microsoft.NETCore.App 8.0.26` and `10.0.7`. The 8.0 runtime is present, so `net8.0` targets build and run.
- NuGet cache (`~/.nuget/packages/`) has the basics but **not** `DBTools_SQL`, `MySqlConnector`, `FluentValidation`, `Swashbuckle`, `xunit`, `Serilog`, `Testcontainers`, `Mapster`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `NSubstitute`. The first `dotnet restore` will fetch them; the machine has internet.
- Legacy `ControlEasy5/`, `ControlEasyWeb/`, `db/` are not present in the working tree; do not add references to them. The new `mockup/` is the build-free HTML prototype owned by the visual-design-system spec — do not modify.
- `dotnet new sln` and `dotnet sln add` work on macOS via the installed SDK.

## Conventions (confirmed from repo-root AGENTS.md)
- C# 12, file-scoped namespaces, `record` types for DTOs, `sealed` classes by default, `_camelCase` private fields, `PascalCase` types/methods, ALL_CAPS only for const.
- Async end-to-end via `IAsyncSqlClient`.
- Routes: kebab-case, plural nouns, versioned `/api/v1/...`.
- Errors: `ProblemDetails` (RFC 7807).
- Multi-tenant: per-tenant entities carry `TenantId` (non-null). `Platform*` entities are exempt.
- **No** raw SQL, no `MySql.Data`, no `EntityFramework` anywhere under `src/` or `tests/`.
- **No emojis** in code, commits, or docs.

## Orchestrator decisions (binding)
- **DBTools_SQL:** vendored under `src/lib/DBTools_SQL/`. Not on NuGet; no placeholder project reference. Vendor stub carries the swap-point marker.
- **IQueryInterceptor shape:** isolated in `MultiTenancy/TenantFilterInterceptor.cs` (one file, one swap point).
- **Thin async extension methods on `Linq<TModel>`:** accepted (`LinqTenantExtensions`).
- **Factory-resolved `Linq<T>` per request:** accepted.

## Open issues handed off to Wave 2
- `TenantRepository` uses the in-memory vendor stub; will need real DBTools_SQL swap before the integration tests in 1.14 land.
- The `PlatformAdminOnly` policy is registered in the Tenants module's API project (so the endpoints compile and tests can exercise the policy) but the *handler* currently checks the `roles` claim only; the `Host/Program.cs` wiring (Serilog, JWT, Swagger, `AddDbTools`, health checks, `Microsoft.FeatureManagement`) is owned by task 1.4.
- The `DefaultTenantSeeder` (design.md) is owned by task 1.0c.
- The `docker/mysql/init/02-tenants-seed.sql` (which creates the `Tenants` table) is owned by task 1.0b.
- The NetArchTest rule from task 1.0b (shipped in the same PR as 1.0b) is owned by Wave 6 (task 1.15).
