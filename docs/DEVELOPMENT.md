<!-- generated-by: gsd-doc-writer -->
# Development Guide — ControlEasy Reborn

This guide outlines local development workflows, build tools, code style standards, Git worktree policies, and pull request procedures for ControlEasy Reborn.

## Local Setup

ControlEasy Reborn enforces a **Docker-first development policy**. All compilation, container execution, and integration testing happen within Docker containers or Devcontainers to guarantee environment parity.

### Prerequisites
- **Docker Engine 24+** & **Docker Compose v2**
- **Git 2.30+**

### Setting Up Your Environment
1. Clone the repository:
   ```bash
   git clone https://github.com/reisfelipe18/ControlEasy.git
   cd ControlEasy
   ```
2. Initialize local configuration:
   ```bash
   cp docker/.env.example docker/.env
   ```
3. Start the application stack:
   ```bash
   docker compose -f docker/docker-compose.yml up -d --build
   ```

---

## Build & Development Commands

### Backend (.NET 8)
Commands should be executed within container environments (or with .NET 8 SDK inside the devcontainer):

| Command | Description |
|---|---|
| `dotnet build src/ControlEasyReborn.sln` | Builds the entire .NET solution (API, BuildingBlocks, Modules) |
| `dotnet clean src/ControlEasyReborn.sln` | Cleans build artifacts and bin/obj folders |
| `dotnet test tests/ControlEasyReborn.UnitTests` | Runs unit tests across all feature modules |
| `dotnet test tests/ControlEasyReborn.IntegrationTests` | Runs integration tests with Testcontainers.MySql |
| `dotnet test tests/ControlEasyReborn.ArchitectureTests` | Runs NetArchTest architectural boundary validation |

### Frontend (Angular 18)
Executed from directory `src/Web/ControlEasyReborn.Web`:

| Command | Description |
|---|---|
| `npm start` | Starts the Angular development server (`ng serve`) |
| `npm run build` | Compiles the production Angular application bundle (runs `openapi-gen` first) |
| `npm run watch` | Builds and watches for frontend changes |
| `npm run openapi-gen` | Generates TypeScript API client models and services from backend OpenAPI spec |
| `npm run openapi-check` | Verifies generated API clients are completely up to date with Git status |
| `npm test` | Runs Angular Karma unit tests with Jasmine in ChromeHeadless |
| `npm run e2e` | Runs Playwright end-to-end tests |
| `npm run lint` | Runs ESLint analysis across all TypeScript and HTML template files |
| `npm run format` | Formats code with Prettier (single quotes, 120 columns) |

### Docker & Convenience Targets (Makefile)

| Command | Description |
|---|---|
| `make up` | Starts full Docker Compose stack with build (`docker compose up -d --build`) |
| `make down` | Shuts down containers and tears down volumes (`docker compose down -v`) |
| `make test` | Executes all test projects in sequence |
| `make build` | Rebuilds all container images |

---

## Code Standards & Style

### C# / Backend Standards
- **Language & Framework:** C# 12 on .NET 8.
- **Namespaces:** File-scoped namespaces (`namespace ControlEasyReborn.Modules.Visits.Domain;`).
- **Class Design:** Classes are `sealed` by default unless explicitly designed for inheritance.
- **Compiler Flags:** Nullable reference types enabled (`<Nullable>enable</Nullable>`) and warnings treated as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
- **Data Access:** Exclusively [DBTools](https://www.nuget.org/packages/DBTools) (`Linq<TModel>` and `IAsyncSqlClient`). **No EF Core** (ADR 0002).
- **Service Registration:** No MediatR. Register use-case handlers as scoped services in each module's `DI` extension class.
- **Route Definitions:** Minimal API endpoints using `app.MapGroup("/api/v1/<resource>")` with kebab-case, plural nouns.

### TypeScript / Angular Standards
- **Components:** Standalone components only (`standalone: true`).
- **Change Detection:** `ChangeDetectionStrategy.OnPush` on all components.
- **Reactive State:** Angular Signals API (`signal`, `computed`, `effect`).
- **Selectors:** Prefixed with `ce-` using kebab-case (e.g., `ce-resident-list`).
- **Formatting:** Prettier enforced with single quotes and 120-column print width.
- **Types:** All API models generated via `ng-openapi-gen`; no manually maintained domain DTOs duplicated on the frontend.

---

## Git Worktree Policy (Mandatory)

The repository enforces strict separation between development work and the default `main` branch:

1. **Main Branch is Pristine:**
   The main checkout is strictly read-only for active feature development. No files are edited and no commits are made directly on `main`.
2. **All Work Occurs in a Worktree:**
   Before creating or modifying code, create a dedicated Git worktree under `.worktrees/`:
   ```bash
   git worktree add ".worktrees/feat-<slug>" -b "feat/<slug>"
   ```
3. **Naming Convention:**
   - Branches: `feat/<slug>` or `fix/<slug>` (lowercase, hyphen-separated).
   - Worktree path: `.worktrees/<branch-hyphens>` (e.g., `.worktrees/feat-tenant-admin-ui`).
4. **Cleanup After Merge:**
   Once a feature branch is merged into `main`, remove the worktree:
   ```bash
   git worktree remove .worktrees/feat-<slug>
   ```

---

## Pull Request Guidelines

Before opening a pull request:
1. Ensure all .NET tests pass (`make test` or individual `dotnet test` suites).
2. Ensure Angular linter and OpenAPI checks pass:
   ```bash
   npm run lint
   npm run openapi-check
   ```
3. Verify that new code strictly adheres to Clean Architecture module boundaries (enforced by `ControlEasyReborn.ArchitectureTests`).
4. Commit messages must follow Conventional Commits (e.g., `feat(visits): add visitor fast check-in endpoint`).
5. Open PRs against the `main` branch with a clear description of the business value and verification steps.
