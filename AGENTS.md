# Project Overview

**ControlEasy Reborn** is the modernization of a legacy condominium access-control desktop application ("ControlEasy 5") used for managing residents, visitors, vehicles, service providers, apartments, and gatehouse ("portaria") flow.

## Legacy State (ControlEasy 5)
- **UI:** WPF / XAML (C#, .NET Framework 4.8) on Windows.
- **Pattern:** MVC-like manual structure under `ControlEasy5/` (`Controller/`, `Model/`, `View/`).
- **Backend:** WPF-coupled business logic with `EntityFramework 6.4.4` and `MySql.Data` (hard-coded credentials in `App.config`).
- **Database:** MySQL (file `db/controlEasyDB.db` and Access `config.mdb` in repo).
- **Existing web project:** `ControlEasyWeb` — a half-started **Blazor Server** app on **.NET 5** (EOL), with placeholder pages (`Counter`, `FetchData`, `PrestadoresServico`) and a project reference to the legacy `ControlEasy5` WPF project.

## Target State (ControlEasy Reborn)
A modular, web-based, containerized platform.

## Technical Details

### Target Stack
- **Frontend:** **Angular 18+** (standalone components, signals) SPA, communicating with a RESTful API. TypeScript strict mode. C# domain models are not shared — Angular has its own TypeScript DTOs/interfaces generated from the OpenAPI schema.
- **Backend:** ASP.NET Core 8 (LTS) Web API, organized in **Clean Architecture / Vertical Slices / Modular Monolith** layers.
- **ORM / Data Access:** [`DBTools_SQL`](https://github.com/gednt/DBTools_SQL) — a multi-provider (SQL Server, PostgreSQL, MySQL, SQLite) data-access library that **prioritizes LINQ** via `LinqHelper<TModel>` / `Linq<TModel>` (lambda predicates, property selectors, JOINs, deferred `IQueryable`).
- **Database:** MySQL 8 (existing schema), abstracted by `DBTools_SQL` so it can later move to PostgreSQL/SQL Server without code changes.
- **Containerization:** Docker + Docker Compose with services: `web` (Angular served by nginx), `api` (ASP.NET Core), `db` (MySQL), `reverse-proxy` (Traefik or Nginx), `adminer` (DB UI), and optional `redis` for caching.
- **Auth:** JWT bearer tokens (ASP.NET Core `Microsoft.AspNetCore.Authentication.JwtBearer`).
- **OpenAPI / contract:** Swashbuckle on the API; Angular client types generated with `ng-openapi-gen` (or `nswag`) at build time.
- **Mapping:** Mapster or AutoMapper.
- **Validation:** FluentValidation.
- **Testing:** xUnit + FluentAssertions + Moq + Testcontainers for integration tests.
- **CI:** GitHub Actions building containers and running tests.

### Naming & Coding Conventions
- C# 12, file-scoped namespaces, `record` types for DTOs, `sealed` classes by default.
- Async/await end-to-end; `IAsyncSqlClient` from DBTools_SQL is the preferred access interface.
- PascalCase types/methods, _camelCase private fields, ALL_CAPS only for const.
- Solution layout: `src/`, `tests/`, `docker/`, `docs/`.
- API style: REST, JSON, kebab-case routes, plural nouns (`/api/residents`, `/api/visits`).
- Module-per-feature: `Modules/Residents/`, `Modules/Visits/`, etc., each with `Domain/`, `Application/`, `Infrastructure/`, `Api/`.

### Architectural Patterns
- **Modular Monolith** (single deployable today, easy to extract microservices later).
- **Clean Architecture** within each module (Domain → Application → Infrastructure → Api).
- **CQRS-lite** for read/write separation where useful (e.g., reports).
- **Repository + Unit of Work** — *implemented via DBTools_SQL's `Linq<TModel>` so the library does the heavy lifting*.
- **Strangler Fig** for the migration (legacy WPF runs behind a feature flag while the web UI replaces it screen by screen).

### Database / DBTools_SQL Integration
- All repositories take `IAsyncSqlClient` or `Linq<TModel>` in their constructors.
- LINQ first: queries written as `await _visits.WhereAsync(v => v.ApartmentId == id && v.Status == "Open")`.
- Raw SQL via `SqlClient` only for stored procedures or DB-specific features not covered by the LINQ provider.
- Multi-provider config in `appsettings.json` (`Provider = "MySQL"` today).
- Hard-coded credentials must be removed; use environment variables / Docker secrets.

### Error Handling & Logging
- `ProblemDetails` (RFC 7807) for HTTP error responses.
- `Serilog` with structured logging, sinks: Console (JSON) + Seq (dev) / file (prod).
- Global exception middleware → maps domain exceptions to HTTP status codes.
- Domain exceptions: `NotFoundException`, `ValidationException`, `ConflictException`.

### Build / Run (target)
```bash
docker compose -f docker/docker-compose.yml up -d   # starts api, web, db, reverse-proxy
dotnet build src/ControlEasyReborn.sln
dotnet test  tests/ControlEasyReborn.Tests.sln
```

### Post-task verification (Docker)
After completing **every implementation task**, rebuild the affected Docker images and restart Compose before marking the task done. Run from the repo root:

```bash
docker compose -f docker/docker-compose.yml build api web
docker compose -f docker/docker-compose.yml up -d --force-recreate api web
```

When the task touches demo mode (`.specs/4 - demo-mode/`), use the demo overlay instead:

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml build api web
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --force-recreate api web
```

If the task changed MySQL init scripts, seed SQL, or database schema, reset volumes first:

```bash
docker compose -f docker/docker-compose.yml down -v
docker compose -f docker/docker-compose.yml up -d --build
```

Agents must not close a task until the rebuilt stack starts healthy and the task's verification gate passes against the running containers.

## Spec Structure

```text
.specs/
└── <feature-or-bug-fix>/
    ├── requirements.md   # User stories (UC[n]: As a user, I want/need to...)
    ├── design.md         # Feature design (overview, glossary, architecture, diagrams)
    ├── bugfix.md         # Bug analysis (overview, condition, examples, fix plan)
    ├── review.md         # Branch review findings (Code Review Agent)
    ├── analysis.md       # General code analysis (Code Review Agent)
    ├── docplan.md       # New documentation structure plan (Documentation Agent)
    ├── docchange.md     # Existing documentation update plan (Documentation Agent)
    ├── orchestration.md  # Multi-agent coordination log (Orchestrator)
    └── tasks.md          # Step-by-step implementation checklist with Task Dependency Graph
```

- Use **design.md** for features
- Use **bugfix.md** for bug fixes
- Use **docplan.md** for new documentation sets
- Use **docchange.md** for updating existing docs
- Use **review.md** for branch diff reviews
- Use **analysis.md** for general code analysis
- **tasks.md** is always required and lists implementation steps with verification gates. Includes a **Task Dependency Graph** section defining parallel execution waves:
    ```json
    {
      "waves": [
        { "wave": 1, "tasks": ["<task_id>", "..."] },
        { "wave": 2, "tasks": ["<task_id>", "..."] }
      ]
    }
    ```
    Task IDs reference the numbered tasks. Tasks in the same wave have no dependencies on each other and can run in parallel. A wave only starts after all tasks in the previous wave are complete.
