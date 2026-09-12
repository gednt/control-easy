<!-- agents-template-schema 1 -->

# AGENTS.md — ControlEasy Reborn

> **Authoritative project guide.** Binding runtime document for every
> contributor and every agent. Constitutional authority:
> `.specify/memory/constitution.md` (v1.3.0, 2026-07-12).
> Where this file and the constitution disagree, the constitution wins.

---

## Reader contract (minimal)

An agent reading `AGENTS.md` alone (without `AGENTS-SPEC.md`) MUST:

1. Treat sections without an `<!-- owner:... -->` comment as **project-owned**: their content is binding for this repository and the agent obeys it.
2. Treat sections marked `<!-- owner:<workflow> -->` as **workflow-owned**: the agent obeys the *non-empty* blocks for workflows it knows it is running. Empty blocks (between `<!-- owner:<workflow> -->` and `<!-- /owner -->` with no content) are inert.
3. Honor a freshness stamp (`<!-- verified YYYY-MM-DD against <sha> -->` or `<!-- approved YYYY-MM-DD -->`) on workflow blocks as a cache-invalidation signal. Orientation sections do not carry stamps.
4. Treat the `## Skill overlays` list as a set of pointers to sibling files. The agent reads those sibling files directly; it does not assume any overlay owns anything in `AGENTS.md`.
5. Treat the schema header (`<!-- agents-template-schema 1 -->`) as the version this file conforms to. Unknown section names do not error; the agent ignores them.

---

## Project

ControlEasy Reborn is a condominium access-control web platform for managing residents, visitors, vehicles, service providers, apartments, and gatehouse ("portaria") operations. Built for condominium administrators, portaria staff, and residents.

**Target architecture:** ASP.NET Core 8 modular monolith + Angular 18 SPA + MySQL 8 + Docker Compose + JWT bearer. Nine feature modules (Residents, Visits, Vehicles, ServiceProviders, Apartments, Security, Tenants, Reports, Administration), each with Clean Architecture (Domain, Application, Infrastructure, Api).

---

## Repo anatomy

Orientation for the top-level layout so an agent can find things without guesswork.

- `src/` — ASP.NET Core 8 solution, Angular 18 SPA, shared building blocks, and nine feature modules. Tracked source.
  - `src/Host/ControlEasyReborn.Api/` — API host (minimal API endpoints, DI registration).
  - `src/Web/ControlEasyReborn.Web/` — Angular 18 SPA (standalone components, signals).
  - `src/BuildingBlocks/` — shared cross-cutting code (auth, error handling, middleware).
  - `src/Modules/<Feature>/{Domain,Application,Infrastructure,Api}/` — nine feature modules.
  - `src/Directory.Build.props`, `src/Directory.Packages.props` — central build properties and package versioning.
  - `src/ControlEasyReborn.sln` — the solution file.
- `tests/` — xUnit test projects. Tracked source.
  - `tests/ControlEasyReborn.UnitTests/` — unit tests.
  - `tests/ControlEasyReborn.IntegrationTests/` — integration tests (Testcontainers.MySql).
  - `tests/ControlEasyReborn.ArchitectureTests/` — NetArchTest architecture rules.
  - `tests/a11y/`, `tests/visual/` — accessibility and visual regression tests.
- `docker/` — Docker Compose files, Dockerfiles, MySQL init scripts. Tracked source.
  - `docker/docker-compose.yml` — main stack (api, web, db, reverse-proxy, adminer, seq).
  - `docker/docker-compose.demo.yml` — demo overlay (pre-populated data, fixed credentials; never in production).
  - `docker/docker-compose.worktree.template.yml` — per-worktree override template.
- `.planning/` — GSD-core workflow directory. Roadmap, state, phases, milestones. Workflow-owned.
- `.specs/` — spec-kit per-feature specifications (requirements / design / tasks). Workflow-owned (spec-kit).
- `.specify/` — spec-kit configuration and memory (constitution). Workflow-owned (spec-kit).
- `openspec/` — OpenSpec change-tracking (opt-in). Workflow-owned.
- `_bmad/` — BMAD workflow configuration and skills. Workflow-owned.
- `_bmad-output/` — BMAD planning artifacts. Workflow-owned.
- `agents-init/` — agents-template onboarding skill (canonical SKILL.md). Overlay-owned.
- `docs/` — operator and developer documentation. Tracked source.
- `scripts/` — worktree management and utility scripts. Tracked source.
- `mockup/` — UI mockup reference files. Tracked source.
- `Makefile` — convenience targets (up, down, test, build, restore, clean).
- `global.json` — .NET SDK version pinning.
- `AGENTS-TEMPLATE.md`, `AGENTS-SPEC.md` — agents-template schema artifacts (read before editing this file).

Rules of thumb:

- Generated output (`bin/`, `obj/`, `node_modules/`, `dist/`) is gitignored; do not document it.
- Configuration files at the repo root are project-owned; do not put policy or conventions in them.

---

## Build, test, and development commands

Canonical commands an agent should run. The project uses Docker Compose for the full stack and .NET / npm for individual layers.

### Build

- `dotnet build src/ControlEasyReborn.sln` — build the entire .NET solution.
- `cd src/Web/ControlEasyReborn.Web && npm run build` — build the Angular SPA (runs `ng-openapi-gen` first via `prebuild`).
- `docker compose -f docker/docker-compose.yml build` — build all container images.

### Test

- `dotnet test tests/ControlEasyReborn.UnitTests` — unit tests (xUnit + FluentAssertions + NSubstitute).
- `dotnet test tests/ControlEasyReborn.IntegrationTests` — integration tests (Testcontainers.MySql).
- `dotnet test tests/ControlEasyReborn.ArchitectureTests` — architecture rules (NetArchTest.Rules).
- `cd src/Web/ControlEasyReborn.Web && npm test -- --no-watch --browsers=ChromeHeadless` — Angular unit tests (Karma + Jasmine).
- `cd src/Web/ControlEasyReborn.Web && npm run e2e` — Playwright E2E tests. Set `E2E_BASE_URL=https://ce-<branch>.localhost:<port>`.

### Lint / format

- `cd src/Web/ControlEasyReborn.Web && npm run lint` — ESLint (`@angular-eslint/recommended`).
- `cd src/Web/ControlEasyReborn.Web && npm run format` — Prettier (single quotes, 120 cols).
- `cd src/Web/ControlEasyReborn.Web && npm run openapi-check` — verify OpenAPI-generated types are up to date.

### Local development

- `docker compose -f docker/docker-compose.yml up -d --build` — full stack (canonical first boot).
- `docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --build` — demo mode overlay.
- `cd src/Web/ControlEasyReborn.Web && npm start` — Angular dev server only.
- `make up` / `make down` / `make test` / `make build` — Makefile convenience targets.

Containers: the compose file is `docker/docker-compose.yml`. The project-name convention is `ce-<branch-hyphens>` (e.g., `ce-feat-dashboard-live-stats`). Worktree compose overrides use `docker/docker-compose.worktree.template.yml`. See `docs/dev-setup.md` for the full devcontainer + worktree guide.

---

## Policy

Hard rules an agent must not violate, beyond what the workflows enforce.

- **Docker-only development.** All build, run, test, lint, and dependency-restore work happens **inside Docker containers** (canonical: `docker compose -f docker/docker-compose.yml`, Makefile targets, or the devcontainer). Agents MUST NOT install anything on the host machine: no .NET SDKs, no Node.js, no global tools, and no project dependencies (`dotnet restore`, `npm install`, `dotnet tool install`, `pip`, etc. on the host). `dotnet` / `npm` / `ng` commands are only valid inside the containers (e.g., `docker compose exec`, `docker run`, or the devcontainer). If a container is missing a tool or dependency, fix the Dockerfile / compose setup — never the host.
- **No EF Core.** DBTools (`Linq<TModel>`, `IAsyncSqlClient`) is the only data-access library. LINQ-first; raw SQL only for stored procs (ADR 0002).
- **No MediatR.** Handlers are registered as scoped services directly.
- **No MVC controllers.** Minimal API endpoint classes only.
- **All tenant-scoped reads/writes via `ITenantAwareLinqFactory`.** No cross-tenant data access.
- **Secrets never in `App.config`.** Use environment variables or Docker secrets.
- **BCrypt cost >= 11** for password hashing.
- **`ProblemDetails` (RFC 7807)** for all error responses. Domain exceptions: `NotFoundException`, `ValidationException`, `ConflictException`.
- **Serilog** structured logging to Console (JSON) + Seq.
- **spec-kit** is the per-feature specification system. Specs live in `.specs/<feature>/`. Configuration and constitutional memory in `.specify/`. spec-kit owns `.specs/` and `.specify/` and updates `STATE.md` + `PROJECT.md` when state changes.
- **Three workflow systems, non-overlapping responsibility:** GSD owns `.planning/`, spec-kit owns `.specs/` and `.specify/`, OpenSpec is opt-in at user discretion (only the user creates `openspec/changes/<id>/`).

### Git worktree rule (mandatory)

- **The main tree stays pristine.** The main checkout (`<repo-root>` on its default branch) is read-only for agents until work is committed: no file edits, no generated artifacts, no build/test outputs, no formatting, no `.gitignore` churn, and no commits. Before starting any work, an agent MUST first create a worktree (see below) and do all reading, editing, building, and testing inside it. Read-only commands (`git status`, `git log`, `git diff`, grep/search) are the only operations permitted on the main checkout.
- **All Git work happens in a worktree.** When an agent (AI or human) is making a change, never commit, branch, push, or merge on `main`. The only permitted operation on `main` is fast-forwarding `main` to a feature branch's tip after a human has reviewed and approved the change. Create a dedicated Git worktree under `<repo-root>/.worktrees/` on a branch `feat/<slug>` or `fix/<slug>`, where `<slug>` is a semantic, lowercase, hyphen-separated name describing the change.
- **Worktrees live inside the repo, under `.worktrees/`.** Never create worktrees as siblings of the repo (e.g., `../ControlEasy.<branch>`) or anywhere outside `<repo-root>/.worktrees/`. Ensure `.worktrees/` is listed in `.gitignore` so worktree contents are never tracked. The canonical path is `<repo-root>/.worktrees/<branch-with-slashes-as-hyphens>` (e.g., `.worktrees/feat-dashboard-live-stats`).
- **The agent reports done; the human merges on `main`.** The agent commits, branches, and pushes inside the worktree, then stops. The agent MUST NOT run `git merge` on `main` for any reason. The only exception: the human explicitly says, in the current turn, "merge on main."
- **Pre-flight check before creating a worktree.** Use only read-only Git commands to confirm that the branch does not already exist and that the worktree path is not already in use. If either exists, abort and pick a different slug.
- **One task, one worktree, one branch.** Do not reuse a slug, a branch, or a worktree path between concurrent tasks.
- **Canonical command.** `git worktree add ".worktrees/$(echo $BRANCH | tr / -)" -b "$BRANCH"` (run from the repo root) is the canonical way to create the worktree.
- **Clean up after merge.** After a change merges to `main` and is verified, remove only the worktree you created: `git worktree remove .worktrees/<branch-with-slashes-as-hyphens>`. Do not touch unrelated worktrees, branches, or paths.
- **No force-push, no rewriting shared history.** Do not rewrite commits that have been pushed or that other worktrees share. Local-only history rewrites (interactive rebase before push) are allowed when no one else depends on the branch.

#### Worktree naming convention

| Item | Pattern | Example |
|---|---|---|
| Branch | `feat/<spec-id>-<slug>` or `fix/<spec-id>-<slug>` | `feat/dashboard-live-stats` |
| Worktree path | `.worktrees/<branch-with-slashes-as-hyphens>` | `.worktrees/feat-dashboard-live-stats` |
| Compose project | `ce-<branch-hyphens>` | `ce-feat-dashboard-live-stats` |
| Traefik host | `ce-<branch>.localhost` | `ce-feat-dashboard-live-stats.localhost` |
| Host port | `18080 + (worktree-index x 10)` | `18090` |
| DB volume | `ce-<branch>-mysql-data` | `ce-feat-dashboard-live-stats-mysql-data` |

Scripts: `scripts/worktree-up.ps1` / `.sh` (create), `scripts/worktree-down.ps1` / `.sh` (tear down).

---

## Workflow blocks

Each block below is owned by one workflow. The block is **inert** when empty — an agent that does not run that workflow ignores it. Workflows may replace the contents of their own block on refresh. Workflows MUST NOT modify another workflow's block.

### BMAD

<!-- owner:bmad -->
<!-- BMAD's project-context skill writes its durable context here. Delete this block if the project does not use BMAD. -->
<!-- /owner -->

### GSD-core

<!-- owner:gsd -->
<!-- GSD's onboarding skill writes its state slice here. Delete this block if the project does not use GSD-core. -->
<!-- /owner -->

### openspec

<!-- owner:openspec -->
<!-- openspec's onboarding writes its change-proposal pointer here. Delete this block if the project does not use openspec. -->
<!-- /owner -->

### Other workflows

Add a new `<!-- owner:<workflow> -->` block here when adopting a new workflow. Document the block in `AGENTS-SPEC.md` (see Extension protocol).

---

## Skill overlays

Skill overlays do not own any block in `AGENTS.md`. They are **sibling files** that the agent reads directly when needed.

- **agents-template** — `AGENTS-TEMPLATE.md` (canonical template), `AGENTS-SPEC.md` (reader contract spec), `agents-init/SKILL.md` (onboarding skill). Read template + spec before editing this file; run `agents-init` to scaffold or refresh `AGENTS.md`.

If a project stops using an overlay, remove its line. Do not leave stale pointers.

---

## Language conventions

### C#

- File-scoped namespaces.
- `sealed` classes by default.
- `PascalCase` types and methods; `_camelCase` private fields; `ALL_CAPS` constants.
- Nullable enabled (`<Nullable>enable</Nullable>`).
- Warnings as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
- Central package versioning (`Directory.Packages.props`).
- REST routes: kebab-case, plural nouns (`/api/v1/residents`).
- Module layout: `Modules/<Feature>/{Domain,Application,Infrastructure,Api}/`.

### Angular / TypeScript

- Standalone components (`standalone: true`).
- `OnPush` change detection.
- Signals API for reactive state.
- `ce-` component prefix; kebab-case selectors.
- ESLint `@angular-eslint/recommended`; Prettier single quotes + 120 cols.
- TypeScript strict mode.
- DTOs generated from OpenAPI via `ng-openapi-gen`; no shared domain models between frontend and backend.

---

## Conventions that differ from defaults

- **Stack:** ASP.NET Core 8 (C# 12, minimal API endpoints) + Angular 18 SPA + MySQL 8 + Docker Compose + JWT bearer.
- **Data access:** DBTools 1.4.3 (`Linq<TModel>`, `IAsyncSqlClient`). No EF Core (ADR 0002). LINQ-first; raw SQL only for stored procs.
- **Mapping:** Mapster.
- **Validation:** FluentValidation.
- **Testing:** xUnit + FluentAssertions + NSubstitute + Testcontainers.MySql + Playwright. Karma + Jasmine for Angular unit tests.
- **Runtime:** Docker Compose services: `api`, `web`, `db`, `reverse-proxy`, `adminer`, `seq`. Multi-stage Dockerfiles. Compose v2.
- **Error handling:** `ProblemDetails` (RFC 7807); Serilog to Console (JSON) + Seq. Domain exceptions: `NotFoundException`, `ValidationException`, `ConflictException`.
- **Auto-tool selection:** Agents read the user's intent and automatically invoke the right tool (spec-kit for implementation, BMAD for review/analysis, GSD for phase planning, OpenSpec for structured delta-specs at user opt-in). See `docs/agent-flow-cheatsheet.md` for lifecycle diagrams.
- **Concurrency budget:** Max 3 sub-agents in flight per level (1 main + 3 subs). Exceeding requires explicit rationale in the wave's `proposal.md`, `design.md`, or `tasks.md`.
- **Post-task verification:** After every implementation task, rebuild and verify: `docker compose -p "$PROJ" -f docker/docker-compose.yml build api web && up -d --force-recreate api web`, then run all three test projects. A task is not closed until the rebuilt stack starts healthy and all tests pass.
- **Pre-commit adversarial review:** No agent may `git commit` until (1) build + tests are green and (2) an adversarial review subagent returns zero CRITICAL/HIGH findings. See existing AGENTS.md archive for full gate procedure.

---

<!-- agents-template-schema 1 -->