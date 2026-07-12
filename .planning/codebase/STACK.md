# Technology Stack

**Analysis Date:** 2026-06-24

## Languages

**Primary:**
- C# 12 (`LangVersion: latest` in `src/Directory.Build.props`) — ASP.NET Core API, modular monolith backend, DBTools NuGet package, xUnit tests
- TypeScript 5.5 (`typescript: ~5.5.2` in `src/Web/ControlEasyReborn.Web/package.json`) — Angular 18 SPA in `src/Web/ControlEasyReborn.Web/`

**Secondary:**
- SQL — MySQL schema and seed scripts in `docker/mysql/init/*.sql`
- HTML/CSS — Angular templates and Tailwind CSS 4 styles in `src/Web/ControlEasyReborn.Web/src/`
- XAML/C# — **Not present in repository.** Legacy ControlEasy 5 WPF (`.NET Framework 4.8`) is documented in `AGENTS.md` and `.specs/1 - modernization-roadmap/design.md` but not checked into this workspace.

## Runtime

**Environment:**
- .NET 8 LTS — `TargetFramework: net8.0` in `src/Directory.Build.props`; SDK pinned in `global.json` (`8.0.0`, `rollForward: latestMajor`)
- Node.js 20 — `node:20-alpine` base image in `docker/web.Dockerfile`
- Browser — Angular SPA served as static assets via nginx

**Package Manager:**
- NuGet with Central Package Management — versions in `src/Directory.Packages.props`, shared props in `src/Directory.Build.props`
- npm 10.x (via Node 20) — lockfile: `src/Web/ControlEasyReborn.Web/package-lock.json`

## Frameworks

**Core:**
- ASP.NET Core 8 Web API — composition root at `src/Host/ControlEasyReborn.Api/Program.cs`; minimal hosting, endpoint routing, JWT auth, Swagger
- Angular 18.2 — standalone components, signals, OnPush change detection (schematics in `src/Web/ControlEasyReborn.Web/angular.json`); prefix `ce`
- DBTools 1.4.3 (NuGet) — multi-provider LINQ data access; pinned in `src/Directory.Packages.props`; wired via `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Data/ServiceCollectionExtensions.cs`

**Testing:**
- xUnit 2.9.2 — `tests/ControlEasyReborn.UnitTests/`, `tests/ControlEasyReborn.IntegrationTests/`, `tests/ControlEasyReborn.ArchitectureTests/`
- FluentAssertions 8.2.0 — assertion library across all test projects
- NSubstitute 5.1.0 — unit test mocking in `tests/ControlEasyReborn.UnitTests/`
- Testcontainers.MySql 4.0.0 — real MySQL containers in `tests/ControlEasyReborn.IntegrationTests/MySqlContainerFixture.cs`
- NetArchTest.Rules 1.3.2 — architecture constraint tests in `tests/ControlEasyReborn.ArchitectureTests/`
- Jasmine 5.2 + Karma 6.4 — Angular unit tests (`npm test` in `src/Web/ControlEasyReborn.Web/`)
- Playwright 1.49 — E2E, visual regression, and a11y tests (`src/Web/ControlEasyReborn.Web/playwright.config.ts`; test dirs include `e2e/`, `tests/visual/`, `tests/a11y/`)

**Build/Dev:**
- Angular CLI 18.2 / `@angular-devkit/build-angular` — production build to `dist/controleasy-reborn-web/`
- dotnet CLI — build/publish via `src/ControlEasyReborn.sln`
- Docker multi-stage builds — `docker/api.Dockerfile` (SDK 8.0 → aspnet 8.0), `docker/web.Dockerfile` (Node 20 → nginx:alpine)
- Tailwind CSS 4.1 — via `@tailwindcss/postcss` and `src/Web/ControlEasyReborn.Web/postcss.config.mjs`
- ng-openapi-gen 1.0.5 — OpenAPI → TypeScript client generation (`src/Web/ControlEasyReborn.Web/ng-openapi-gen.json`, output `src/app/api/`)
- ESLint 10 + `@angular-eslint/*` 22 — lint via `npm run lint`
- Prettier 3.8 — format via `npm run format`

## Key Dependencies

**Critical:**
- MySqlConnector 2.3.7 — MySQL ADO.NET driver (pinned in `src/Directory.Packages.props`; used by DBTools MySQL provider)
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.10 — JWT bearer authentication in `src/Host/ControlEasyReborn.Api/Program.cs`
- Serilog.AspNetCore 8.0.3 + Serilog.Sinks.Seq 6.0.0 — structured logging to console and Seq
- Swashbuckle.AspNetCore 6.6.2 — OpenAPI/Swagger UI (Development only)
- FluentValidation 11.9.0 — request validation in module Application layers (e.g. `src/Modules/Security/ControlEasyReborn.Modules.Security.Application/Validators/`)
- BCrypt.Net-Next 4.0.3 — password hashing in `src/Modules/Security/ControlEasyReborn.Modules.Security.Infrastructure/Persistence/PasswordHasher.cs`
- Microsoft.FeatureManagement 4.0.0 — Strangler Fig feature flags (`FeatureManagement` section in `src/Host/ControlEasyReborn.Api/appsettings.json`)

**Infrastructure:**
- AspNetCore.HealthChecks.MySql 2.2.0 — `/health` endpoint in `Program.cs`
- Microsoft.AspNetCore.Mvc.Testing 8.0.10 — integration test host factories in `tests/ControlEasyReborn.IntegrationTests/`
- RxJS 7.8 — Angular reactive streams
- lucide-angular 0.454 — icon library in Angular UI
- Mapster 7.4.0 — pinned in `src/Directory.Packages.props` but not referenced in application code yet

**Legacy (documented, not in repo):**
- Entity Framework 6.4.4 + MySql.Data — legacy ControlEasy 5 WPF per `AGENTS.md`
- Blazor Server on .NET 5 — legacy `ControlEasyWeb` prototype per `AGENTS.md`; **not present in repository**

## Configuration

**Environment:**
- ASP.NET Core configuration hierarchy — `src/Host/ControlEasyReborn.Api/appsettings.json` plus environment variables (Docker uses `__` nesting, e.g. `Db__Host`, `Jwt__SigningKey` in `docker/docker-compose.yml`)
- Docker secrets template — `docker/.env.example` (`MYSQL_ROOT_PASSWORD`, `MYSQL_USER`, `MYSQL_PASSWORD`, `JWT_SIGNING_KEY`)
- DBTools default sample — `src/lib/DBTools_SQL/DBTools/config.json` (SqlServer sample; Reborn overrides via `Db:*` at runtime)
- Angular dev proxy — `src/Web/ControlEasyReborn.Web/proxy.conf.json` forwards `/api` → `http://localhost:8080`

**Key configs required:**
- `Db:Provider`, `Db:Host`, `Db:Port`, `Db:Database`, `Db:Username`, `Db:Password`
- `Jwt:SigningKey`, `Jwt:Issuer`, `Jwt:Audience`
- Optional: `Demo:*`, `Bootstrap:*`, `FeatureManagement:*`, `Serilog:*`, `Backup:Path`

**Build:**
- `global.json` — .NET SDK version
- `src/Directory.Build.props`, `src/Directory.Packages.props` — shared C# build and package versions
- `tests/Directory.Build.props`, `tests/Directory.Packages.props` — test project overrides
- `src/Web/ControlEasyReborn.Web/angular.json`, `tsconfig.json`, `tsconfig.app.json` — Angular build
- `src/Web/ControlEasyReborn.Web/.eslintrc.json` — ESLint rules
- `docker/docker-compose.yml`, `docker/docker-compose.demo.yml` — container orchestration
- `docker/reverse-proxy/traefik.yml`, `docker/reverse-proxy/dynamic.yml` — Traefik routing
- `docker/nginx.conf` — SPA fallback routing for production web container

## Platform Requirements

**Development:**
- Windows/macOS/Linux with .NET 8 SDK (`global.json`)
- Node.js 20+ and npm for Angular work in `src/Web/ControlEasyReborn.Web/`
- Docker Desktop (or compatible engine) for full stack via `docker compose -f docker/docker-compose.yml up`
- Optional: Seq at `http://localhost:5341` for log aggregation (service defined in `docker/docker-compose.yml`)

**Production:**
- Docker containers on any Linux host (or container platform)
- Services: `api` (ASP.NET Core on port 8080), `web` (nginx on 8080), `db` (MySQL 8.0), `reverse-proxy` (Traefik v3.1), `adminer`, `seq`
- Base images: `mcr.microsoft.com/dotnet/aspnet:8.0`, `mcr.microsoft.com/dotnet/sdk:8.0`, `node:20-alpine`, `nginx:alpine`, `mysql:8.0`, `traefik:v3.1`, `adminer:4`, `datalust/seq:latest`
- Public entry via Traefik on host port `8080` (HTTP)

---

*Stack analysis: 2026-06-24*
*Update after major dependency changes*
