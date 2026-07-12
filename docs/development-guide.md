# Development Guide — ControlEasy Reborn

**Analysis date:** 2026-07-12

## Prerequisites

- **.NET 8 SDK** (8.0.0 pinned in `global.json`; `rollForward: latestMajor`)
- **Node.js 20+** and **npm 10.x**
- **Docker Desktop** (or any Docker Engine with Compose v2)
- Optional: **Seq** at `http://localhost:5341` for log aggregation (auto-started via `docker compose`)
- OS: Windows, macOS, or Linux

## First-Time Setup

```bash
# 1. Clone & enter
git clone <repo> && cd ControlEasy

# 2. Copy secrets template
cp docker/.env.example docker/.env
# Edit docker/.env: set MYSQL_ROOT_PASSWORD, MYSQL_USER, MYSQL_PASSWORD, JWT_SIGNING_KEY

# 3. Pre-warm Docker images (avoid MCR rate-limiting on rebuilds)
for img in \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  mcr.microsoft.com/dotnet/aspnet:8.0 \
  node:20-alpine \
  nginx:alpine \
  mysql:8.0 \
  adminer:4 \
  traefik:v3.1; do
  docker image inspect "$img" >/dev/null 2>&1 || docker pull "$img"
done

# 4. Start the full stack
docker compose -f docker/docker-compose.yml up -d --build
# Web UI:  http://localhost:8080
# API:     http://localhost:8080/api/v1
# Swagger: http://localhost:8080/swagger (Development env only)
# Adminer: http://localhost:8080/db
# Traefik: http://localhost:8082
# Seq:     http://localhost:5341 (compose-internal)

# 5. Bootstrap credentials on first boot
# PlatformAdmin: GET http://localhost:8080/api/v1/security/bootstrap
# Returns: { pending: true, email, password } (password changes after first login)
```

## Development Modes

### Option A: Full Docker (recommended for end-to-end work)
```bash
docker compose -f docker/docker-compose.yml up -d --build
# Edit code, then:
docker compose -f docker/docker-compose.yml build api web
docker compose -f docker/docker-compose.yml up -d --force-recreate api web
```

### Option B: Hybrid (host tooling + Docker DB)
```bash
# 1. Start only the DB
docker compose -f docker/docker-compose.yml up -d db

# 2. Run the API on the host
dotnet run --project src/Host/ControlEasyReborn.Api
# Listens on http://localhost:8080 (verify in launchSettings.json)

# 3. Run the web app on the host (proxies /api to localhost:8080)
cd src/Web/ControlEasyReborn.Web
npm install
npm start
# Open http://localhost:4200
```

### Option C: Demo mode (one-command seeded stack)
```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build
# Demo personas with fixed credentials (see docs/demo-mode.md)
```

## Build Commands

```bash
# All .NET (api + tests)
dotnet build src/ControlEasyReborn.sln

# Web
cd src/Web/ControlEasyReborn.Web && npm run build
# Output: dist/controleasy-reborn-web/

# Docker images
COMPOSE_DOCKER_CLI_BUILD=1 DOCKER_BUILDKIT=1 \
  docker compose -f docker/docker-compose.yml build api web
```

## Test Commands

```bash
# All .NET tests
dotnet test src/ControlEasyReborn.sln

# Per project
dotnet test tests/ControlEasyReborn.UnitTests/ControlEasyReborn.UnitTests.csproj
dotnet test tests/ControlEasyReborn.IntegrationTests/ControlEasyReborn.IntegrationTests.csproj
dotnet test tests/ControlEasyReborn.ArchitectureTests/ControlEasyReborn.ArchitectureTests.csproj

# Filtered
dotnet test tests/ControlEasyReborn.UnitTests --filter "FullyQualifiedName~CreateApartmentHandler"

# Angular unit tests
cd src/Web/ControlEasyReborn.Web
npm test                                # Karma, watch mode
npm test -- --no-watch --browsers=ChromeHeadless   # CI-style

# Playwright E2E (against running stack on :8080)
cd src/Web/ControlEasyReborn.Web
npm run e2e:install                     # one-time
npm run e2e
E2E_BASE_URL=http://localhost:8080 npm run e2e
```

## Lint & Format

```bash
# C#: warnings as errors (no separate lint pass)
dotnet build src/ControlEasyReborn.sln

# Angular: ESLint
cd src/Web/ControlEasyReborn.Web
npm run lint

# Prettier
cd src/Web/ControlEasyReborn.Web
npm run format
```

## OpenAPI Client Regeneration (local, manual today)

```bash
# 1. Start the API (Docker or host)
docker compose -f docker/docker-compose.yml up -d api

# 2. Generate
cd src/Web/ControlEasyReborn.Web
npm run openapi-gen
# Reads https://localhost/swagger/v1/swagger.json (or http://localhost:8080/swagger/...)
# Writes to src/app/api/
```

> **Note:** `openapi-gen` is **not yet part of the Docker web build** (gap captured in `CONCERNS.md` and `REQUIREMENTS.md` CI-04). Until wired, feature services hand-maintain DTOs.

## Project Structure (cheat sheet)

```
src/Host/ControlEasyReborn.Api/      # ASP.NET Core composition root
src/Modules/{Feature}/                # 9 feature modules × 4 projects
src/BuildingBlocks/                   # SharedKernel + Infrastructure
src/Web/ControlEasyReborn.Web/       # Angular SPA
tests/                                # 4 test projects
docker/                               # Docker Compose, MySQL init, Traefik
```

## Adding a New Feature Module (full vertical slice)

1. **Domain entity** in `src/Modules/{Feature}/.../Domain/Entities/{Entity}.cs`
2. **Application handlers** in `Application/Handlers/{Verb}{Entity}Handler.cs`
3. **Validators** in `Application/Validators/{Request}Validator.cs`
4. **Contracts (DTOs)** in `Application/Contracts/{Entity}Dtos.cs` (sealed records)
5. **Abstraction** `Application/Abstractions/I{Entity}Repository.cs`
6. **Implementation** `Infrastructure/Persistence/{Entity}Repository.cs` (use `ITenantAwareLinqFactory`)
7. **DI extension** `Infrastructure/DI/{Feature}ModuleServiceCollectionExtensions.cs`
8. **Endpoints** `Api/Endpoints/{Entity}Endpoints.cs` with `Map{Entity}Endpoints`
9. **Register in `Program.cs`:** add `Add{Feature}Module()` and `Map{Feature}Endpoints()`
10. **Add 4 projects to `src/ControlEasyReborn.sln`**
11. **Schema:** new SQL file in `docker/mysql/init/` with next sequence number; update `03-tenant-backfill.sql` markers (or `SchemaBackfillSyncTests` will fail)
12. **Tests:** `tests/.../UnitTests/Modules/{Feature}/` + `tests/.../IntegrationTests/{Feature}EndpointTests.cs` (include at least one `CrossTenant_*` test)

## Adding a New API Endpoint to an Existing Module

1. Add handler + validator in `Application/Handlers/`, `Application/Validators/`
2. Register in `{Module}ModuleServiceCollectionExtensions`
3. Add route in `Api/Endpoints/{Entity}Endpoints.cs`
4. Add tests (unit + integration + cross-tenant)

## Adding an Angular Feature Page

1. Page: `src/Web/ControlEasyReborn.Web/src/app/features/{feature}/{feature}.page.ts` (standalone, `OnPush`)
2. API service: `{feature}-api.service.ts` (signal-based, manual DTOs until openapi-gen is wired)
3. Route: lazy entry in `src/app/app.routes.ts`
4. Use `ce-*` design-system components; do not duplicate CSS
5. Add Playwright E2E in `e2e/{feature}.spec.ts`

## Adding a Design-System Component

1. Component: `src/Web/ControlEasyReborn.Web/src/app/design-system/components/{name}/{name}.component.ts` (selector `ce-{name}`)
2. Co-located spec: `{name}.component.spec.ts`
3. Export from `src/app/design-system/index.ts`
4. If new tokens, add to `src/app/design-system/tokens/`

## Common Tasks

```bash
# Reset the database (re-runs all init scripts)
docker compose -f docker/docker-compose.yml down -v
docker compose -f docker/docker-compose.yml up -d --build

# Tail API logs
docker compose -f docker/docker-compose.yml logs -f api

# Inspect MySQL data
docker exec -it controleasy-db-1 mysql -ucontroleasy -pcontroleasy_dev controleasydb

# Run a one-off script in the API container
docker compose -f docker/docker-compose.yml exec api dotnet ef ...
# (Note: no EF Core in this project — use DBTools or raw SQL init scripts)

# Regenerate tenant backfill markers after schema change
node scripts/generate-tenant-backfill.sql
```

## Troubleshooting

| Symptom | Likely cause | Fix |
|---------|--------------|-----|
| `GET /api/v1/dashboard/stats` returns zero `occupiedApartments` | JOIN ambiguity in `tenant_id` (fixed once, regression-prone) | Use aliased `a.tenant_id` / `r.tenant_id`; add integration test |
| `Login failed: No active attendant profile found for user` | First-boot PlatformAdmin missing `AttendantProfiles` row | `PlatformAdminBootstrapService` now inserts both; verify the service ran |
| `WebApplicationFactory` test fails with "entry point exited without building IHost" | Serilog bootstrap pre-load + working directory | Keep `optional: true` on bootstrap config; set content root in test factory |
| `npm run openapi-gen` fails | API not running or wrong URL | Start API first; check `ng-openapi-gen.json` |
| MCR rate-limit (HTTP 429/401) on `docker build` | Cold image cache, large parallel builds | Pre-warm with `docker image inspect` + `docker pull` loop; wait + retry |

## Style Guardrails (enforced)

- **C#:** file-scoped namespaces, `sealed` classes, `_camelCase` private fields, `PascalCase` types/methods, nullable enabled, warnings as errors.
- **Angular:** `standalone: true`, `OnPush`, signals API, `ce-` prefix, kebab-case selectors, ESLint `@angular-eslint/recommended`, Prettier single quotes + 120 cols.
- **No MVC controllers.** Use minimal API endpoint classes.
- **No EF Core in Reborn code.** DBTools only (ADR 0002).
- **No MediatR.** Handlers registered as scoped services directly.
- **All tenant-scoped reads/writes via `ITenantAwareLinqFactory`.**

---

*Development guide: 2026-07-12 · Sources: GSD `TESTING.md` + `STACK.md` (2026-06-24) + `AGENTS.md` + live compose files.*
