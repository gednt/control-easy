# AGENTS.md — ControlEasy Reborn

> **Authoritative project guide.** This file is the binding runtime
> document for every contributor and every agent working on
> ControlEasy Reborn.
>
> Constitutional authority: `.specify/memory/constitution.md`
> (currently v1.3.0, ratified 2026-07-12). Where this file and the
> constitution disagree, the constitution wins; this file is amended
> in the same change.

---

## 1. Project overview

**ControlEasy Reborn** is a condominium access-control web platform
used for managing residents, visitors, vehicles, service providers,
apartments, and gatehouse ("portaria") flow.


### Target state (ControlEasy Reborn)

A modular, web-based, containerized platform. ASP.NET Core 8 modular
monolith + Angular 18 SPA + MySQL 8 + Docker Compose + JWT bearer,
organized in 9 feature modules (Residents, Visits, Vehicles,
ServiceProviders, Apartments, Security, Tenants, Reports,
Administration) each with Clean Architecture (Domain →
Application → Infrastructure → Api).

---

## 2. Workflow tooling (GSD · spec-kit · OpenSpec)

**Read first:** § 2.4 ([Sub-agent invocation](#24-sub-agent-invocation)) and § 2.5 ([Concurrency budget](#25-concurrency-budget)) apply to every agent operating under this file.

This project runs three workflow systems, each with a single,
non-overlapping responsibility. The split is binding
(constitution v1.3.0, Principle VI).

### 2.1 GSD — planner and roadmap owner

**Home:** `.planning/`. **Owner:** GSD skills
(`.claude/skills/gsd-*` and `.agents/skills/gsd-*`).

GSD holds the project-level state. Every agent reads it before
doing anything else; every agent that changes state updates it.

- `.planning/PROJECT.md` — top-level project brief; the
  validated / active / out-of-scope lists.
- `.planning/REQUIREMENTS.md` — v1/v2 requirements with phase
  traceability.
- `.planning/ROADMAP.md` — the 14 phases.
- `.planning/STATE.md` — live execution state (what is "done"
  is what `STATE.md` says is done).
- `.planning/codebase/` — seven code-base maps (`STACK.md`,
  `STRUCTURE.md`, `ARCHITECTURE.md`, `CONVENTIONS.md`,
  `CONCERNS.md`, `INTEGRATIONS.md`, `TESTING.md`). These are
  consumed by every other tool as the source of truth about
  the code.

GSD phase boundaries (via `/gsd-transition`,
`/gsd-complete-milestone`) are the moments at which
documentation is re-validated and the constitution is
re-checked.

> **Ownership rule (constitution v1.3.0 § 4 Seams).** GSD owns
> `.planning/`. It is the single writer of the roadmap pointer,
> the phase state, and the milestone archive. spec-kit and
> OpenSpec are **writers** to `.planning/` whenever their work
> changes project state — they MUST update `STATE.md` (decisions,
> blockers, deferred items) and `PROJECT.md` (validated / active
> / out-of-scope list moves) when their work changes those facts —
> and **readers** of `.planning/` whenever they need project
> context. They MUST NOT update `ROADMAP.md`, the phase pointer,
> or the milestone archive; those are GSD's job
> (`/gsd-transition`, `/gsd-complete-milestone`). Conversely, GSD
> consumes `.specs/<feature>/` and `openspec/` artifacts but MUST
> NOT rewrite them.

### 2.2 spec-kit — per-feature spec owner

**Home:** `.specs/`, `.specify/`. **Owner:** spec-kit skills
(`.agents/skills/speckit-*`).

Every feature or bug fix lives under `.specs/<feature>/` with
the artifacts described in § 6 "Spec Structure" below.
`.specify/memory/constitution.md` is the binding governance
document for spec-kit. The spec-kit command skills are the
interface between human intent and spec-kit artifacts.

### 2.3 OpenSpec (opt-in) — change-tracking agreement layer

**Home:** `openspec/`. **Owner:** OpenSpec
([Fission-AI/OpenSpec](https://github.com/Fission-AI/OpenSpec)),
invoked **at the discretion of the user** on a per-change basis.

> **OpenSpec is opt-in.** It is NOT the default workflow for
> ControlEasy Reborn. The user invokes it on a per-change basis
> when they want the additional structure of a `proposal.md` +
> delta-specs + `tasks.md` lifecycle and the post-hoc
> "archive merge into canonical specs" step. Most changes go
> through spec-kit alone; only some go through OpenSpec as
> well. The two coexist; nothing in spec-kit is replaced by
> adopting OpenSpec on a change.

What OpenSpec adds when the user opts in:

- A `proposal.md` (why, what, scope, success criteria) in
  `openspec/changes/<id>/`.
- **Delta specs** under `openspec/changes/<id>/specs/`
  organised by domain (`auth/`, `payments/`, `ui/`, …) using
  `## ADDED Requirements`, `## MODIFIED Requirements`,
  `## REMOVED Requirements` sections. Delta specs are the
  diff against the canonical `openspec/specs/<domain>/`,
  not a rewrite.
- A `design.md` and a `tasks.md` (the lifecycle is
  `proposal → specs → design → tasks`, progressive, not
  waterfall).
- An `/opsx:apply` step that has the agent implement against
  the delta specs and the task list.
- An `/opsx:archive` step that folds the delta specs into
  the canonical `openspec/specs/<domain>/` and moves the
  change to `openspec/changes/archive/<date>-<id>/`.

When the user opts a change into OpenSpec:

- A spec-kit `.specs/<feature>/` folder is *also* created
  (spec-kit is still the implementation-side source of
  truth). The OpenSpec `proposal.md` "Why" section
  references the spec-kit folder URL.
- The OpenSpec `tasks.md` and the spec-kit `tasks.md` are
  kept in sync by the agent that runs `/opsx:apply`. The
  spec-kit `tasks.md` is the one the verification gate
  reads (see § 5).
- The canonical `openspec/specs/<domain>/` accumulates
  folded deltas over time. The spec-kit `requirements.md`
  / `design.md` files do NOT have to mirror them — they
  are independent. Cross-references are added to the
  spec-kit `requirements.md` "OpenSpec" subsection when
  relevant.

When the user does **not** opt in, the change is spec-kit
only and `openspec/` is untouched. This is the common case.

### 2.4 Sub-agent invocation

The three workflow tools in § 2.1–§ 2.3 are owned by their
respective skill sets, but **any agent — including a main
orchestrator and any in-flight sub-agent — MAY invoke any of
GSD, spec-kit, or OpenSpec skills as a sub-agent** when the
work at hand crosses the seam between the three tools.

Examples (non-exhaustive):

- A main agent running `/opsx:apply` for an OpenSpec change
 may invoke a GSD `gsd-codebase-mapper` sub-agent to refresh a
 `.planning/codebase/` map that informs a `design.md` decision.
- A GSD `gsd-plan-phase` orchestrator may invoke a spec-kit
 `speckit-specify` sub-agent to draft the
 `.specs/<feature>/requirements.md` and `design.md` for a new
 feature introduced by the phase.
- A spec-kit `speckit-implement` task that the user has opted
 into OpenSpec may invoke `/opsx:apply` as a sub-agent to drive
 the change-tracking lifecycle, keeping the spec-kit and
 OpenSpec `tasks.md` files in sync.

Sub-agent invocation MUST NOT bypass the ownership rules in
§ 2.1–§ 2.3:

- A sub-agent writes only to the directory owned by its own
 workflow tool (GSD → `.planning/`, spec-kit → `.specs/` and
 `.specify/`, OpenSpec → `openspec/`).
- A sub-agent reads from the other tools' directories but does
 not rewrite them.
- The sub-agent's parent is responsible for respecting those
 rules on the sub-agent's behalf and for catching violations
 before they land.

### 2.5 Concurrency budget

To keep fan-out predictable and the per-step latency bounded,
the default concurrency budget is:

> **Up to 3 sub-agents in flight concurrently, in addition to
> the main orchestrator.** Total in-flight count per level:
> 4 (1 main + 3 subs).

The budget is **per agent, not per workflow tool**: a main
agent running an OpenSpec apply that fans out to GSD and
spec-kit sub-agents is still capped at three concurrent
sub-agents. A sub-agent that itself spawns sub-agents gets its
own independent budget of 3 — a sub-agent may not "consume"
its parent's budget, and deeper nesting is permitted with the
budget applied at every level.

**Override path.** Exceeding the budget (4+ sub-agents in
flight at any level) is permitted only when the originating
artifact records an explicit rationale:

- `proposal.md` "Why" section, or
- `design.md` "Decisions" section, or
- `tasks.md` task description for the wave in question.

The rationale is reviewed at the next
`/gsd-complete-milestone` boundary, alongside the rest of the
work the change has produced. A rationale recorded in
`tasks.md` is the most common path for apply-time overruns; a
rationale recorded in `proposal.md` or `design.md` is for
overruns anticipated before implementation.

No environment variable, no config file, no CLI flag
overrides the budget. The override is qualitative and lives
next to the work it justifies, so the next milestone audit
sees it without a separate scan.

---

## 3. Target stack

| Layer | Choice | Notes |
|---|---|---|
| Frontend | Angular 18+ SPA, standalone components, signals, TypeScript strict mode | Domain models are NOT shared — Angular owns its own DTOs generated from OpenAPI (`ng-openapi-gen` or `nswag`). |
| Backend | ASP.NET Core 8 (LTS) Web API, C# 12, file-scoped namespaces, `record` DTOs, `sealed` classes by default | Async/await end-to-end; `IAsyncSqlClient` from DBTools is the preferred access interface. |
| Data access | [DBTools 1.4.3](https://www.nuget.org/packages/DBTools) NuGet package | LINQ-first (`LinqHelper<TModel>` / `Linq<TModel>`, deferred `IQueryable`); pinned in `src/Directory.Packages.props`; referenced from `ControlEasyReborn.Infrastructure`. |
| Database | MySQL 8 today; provider-agnostic by design | DBTools so we can later move to PostgreSQL / SQL Server / SQLite without code changes. |
| Runtime | Docker + Docker Compose (`api`, `web`, `db`, `reverse-proxy`, `adminer`, `seq`, optional `redis`) | Multi-stage Dockerfiles; Compose v2; `docker image inspect` + `docker pull` pre-warm loop. |
| Dev shell | **Devcontainer (Docker-out-of-Docker)** — canonical, see § 4 | Uniform shell across Windows / macOS / Linux; per-worktree Compose projects for parallel branches. |
| Auth | JWT bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`), BCrypt (cost ≥ 11) | Secrets from env vars / Docker secrets; never in `App.config`. |
| OpenAPI | Swashbuckle on the API | `ng-openapi-gen` at web build time. |
| Mapping | Mapster or AutoMapper | |
| Validation | FluentValidation | |
| Testing | xUnit + FluentAssertions + NSubstitute + Testcontainers.MySql + Playwright | Karma + Jasmine for Angular unit. |
| CI | **Pending** (continuous task C.1 in `.planning/PROJECT.md`) | When C.1 lands, the devcontainer image is the candidate for the CI job image (see `.specs/devcontainers/tasks.md` C.12). |

### 3.1 Naming & coding conventions

- C# 12, file-scoped namespaces, `record` DTOs, `sealed` classes by
  default.
- `PascalCase` types/methods, `_camelCase` private fields, `ALL_CAPS`
  only for `const`.
- Solution layout: `src/`, `tests/`, `docker/`, `docs/`.
- API style: REST, JSON, kebab-case routes, plural nouns
  (`/api/v1/residents`, `/api/v1/visits`).
- Module-per-feature: `Modules/<Feature>/`, each with `Domain/`,
  `Application/`, `Infrastructure/`, `Api/`.
- **No MVC controllers.** Use minimal API endpoint classes.
- **No EF Core in Reborn code.** DBTools only (ADR 0002).
- **No MediatR.** Handlers registered as scoped services directly.
- All tenant-scoped reads/writes via `ITenantAwareLinqFactory`.

### 3.2 Architectural patterns

- **Modular Monolith** (single deployable today, easy to extract
  microservices later).
- **Clean Architecture** within each module
  (Domain → Application → Infrastructure → Api).
- **CQRS-lite** for read/write separation where useful
  (e.g., reports).
- **Repository + Unit of Work** — *implemented via DBTools
  `Linq<TModel>` so the library does the heavy lifting.*
- **Strangler Fig** for the migration (legacy WPF runs behind a
  feature flag while the web UI replaces it screen by screen —
  scaffolding only; cutover was cancelled in this repo).

### 3.3 Database / DBTools integration

- All repositories take `IAsyncSqlClient` or `Linq<TModel>` in their
  constructors.
- LINQ first: queries written as
  `await _visits.WhereAsync(v => v.ApartmentId == id && v.Status == "Open")`.
- Raw SQL via `SqlClient` only for stored procedures or DB-specific
  features not covered by the LINQ provider.
- Multi-provider config in `appsettings.json`
  (`Provider = "MySQL"` today).
- Hard-coded credentials must be removed; use environment variables
  / Docker secrets.

### 3.4 Error handling & logging

- `ProblemDetails` (RFC 7807) for HTTP error responses.
- `Serilog` with structured logging, sinks: Console (JSON) +
  Seq (dev) / file (prod).
- Global exception middleware → maps domain exceptions to HTTP
  status codes.
- Domain exceptions: `NotFoundException`, `ValidationException`,
  `ConflictException`.

---

## 4. Local development

The local development loop has four modes. The canonical mode is
**Option D — Dev container + worktree**. The verification gate is
the same in every mode: `docker compose -p <project> build api web
&& docker compose -p <project> up -d --force-recreate api web`
followed by the relevant `dotnet test` slice. The devcontainer
does **not** replace Compose; it provides a uniform shell from
which Compose is invoked (Docker-out-of-Docker).

> See `.specs/devcontainers/` for the full spec
> (`requirements.md` UC-D1…D5, `design.md` decisions, `tasks.md`
> waves 1–3). This section is the operator-level reference for a
> workflow that is implemented by tasks 9.1–9.5 of that spec.
> Until those tasks land, the worktree convention (§ 4.3) is a
> target, and the main-checkout commands in § 5.1 are the ones
> that run on a fresh clone.

### 4.1 Option D — Dev container (canonical)

A single [devcontainer](https://containers.dev/) definition at
`.devcontainer/devcontainer.json` is the recommended shell for
every contributor on every OS. The dev shell:

- mounts the host's `/var/run/docker.sock` (Docker-out-of-Docker)
  so `docker compose` invocations inside the shell target the
  same engine and volumes as the host;
- pre-installs the .NET 8 SDK, Node 20 + npm 10, the Angular CLI,
  Playwright browsers, the MySQL client, and the repo's
  pre-commit hooks;
- pre-warms the same Docker base images the Compose stack uses
  (so MCR rate-limiting is avoided in the dev loop as well as the
  build loop);
- runs `docker compose -f docker/docker-compose.yml up -d --build`
  on `postCreateCommand` only if `AUTO_START_COMPOSE=true`
  (default `false` — manual bring-up is the verification gate).

Opening the repo in a devcontainer-aware IDE (VS Code, Cursor,
JetBrains Gateway, GitHub Codespaces) is the canonical entry point.
The full per-feature, per-worktree bring-up is described in § 4.3.

#### Verification gate (devcontainer mode)

```bash
# 1. The shell is the devcontainer (check $DEVCONTAINER is set).
echo "$DEVCONTAINER"  # → true

# 2. Compose stack is the same as the host.
docker compose -f docker/docker-compose.yml ps
# Expect: api, web, db, reverse-proxy, adminer, seq

# 3. Tests
dotnet test src/ControlEasyReborn.sln
```

### 4.2 Docker image caching (runtime + devcontainer)

Only pull base images if they are not present locally. Use
`docker image inspect` to check before pulling:

```bash
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
```

This skips the pull for images already cached locally, avoiding
unnecessary registry calls and MCR rate-limiting. In a fresh
environment (no local cache), all images will be pulled
automatically.

After images are present, always build without pulling to avoid
redundant registry calls:

```bash
COMPOSE_DOCKER_CLI_BUILD=1 DOCKER_BUILDKIT=1 \
  docker compose -f docker/docker-compose.yml build api web
```

If MCR is rate-limiting (HTTP 429 or 401), wait a few minutes
and retry.

### 4.3 Worktrees

Every feature branch lives in its own git worktree so that the
dev shell can be hot-reloading one branch while another is being
reviewed. The convention is:

| Item | Convention | Example (`feat/dashboard-live-stats`) |
|---|---|---|
| Branch name | `feat/<spec-id>-<short-slug>` or `fix/<spec-id>-<short-slug>` | `feat/dashboard-live-stats` |
| Worktree path | `../ControlEasy.<branch-with-slashes>` (sibling of the main checkout) | `../ControlEasy.feat-dashboard-live-stats` |
| Compose project name | `ce-<branch-with-slashes-as-hyphens>` (set via `COMPOSE_PROJECT_NAME`) | `ce-feat-dashboard-live-stats` |
| Traefik hostname | `ce-<branch>.localhost` (resolved via the host resolver shim) | `ce-feat-dashboard-live-stats.localhost` |
| Published host port range | `18080 + (worktree-index × 10)` | `18090` (worktree index 1) |
| DB volume name | `ce-<branch>-mysql-data` | `ce-feat-dashboard-live-stats-mysql-data` |

The worktree shell scripts (`scripts/worktree-up.sh`,
`scripts/worktree-down.sh`, `scripts/lib/worktree.sh`,
`scripts/lib/resolver.sh`) are future deliverables of
`.specs/devcontainers/tasks.md` tasks 9.2–9.5. When they land,
`worktree-up.sh` will automate the worktree creation, the env
file generation, the resolver entry, and the first
`docker compose up`; `worktree-down.sh` will tear it down
(deleting only the worktree's own compose project; never touching
the main checkout's stack). Until then, run the equivalent
`docker compose -p "ce-..."` commands by hand (the lines in the
verification gate below work today).

#### Why per-worktree Compose projects

- **Isolation.** A `dotnet ef migrations add` (or any schema
  change) in worktree A cannot break worktree B's database.
- **No port collisions.** Each worktree gets its own Traefik
  port and hostname, so multiple worktrees can run side-by-side
  on the same host.
- **Fast feedback.** `docker compose build api web` in worktree
  A is scoped to A's images; the main checkout's images are not
  invalidated.

#### Verification gate (worktree mode)

```bash
# 1. Confirm the worktree is the active shell.
git worktree list
# Expect: <worktree-path>  <branch>  <sha>

# 2. Confirm the compose project is the worktree's, not the main's.
docker compose -p "ce-$(git rev-parse --abbrev-ref HEAD | tr / -)" ps
# Expect: api, web, db, reverse-proxy, adminer, seq

# 3. Confirm the verification gate is green.
docker compose -p "ce-$(git rev-parse --abbrev-ref HEAD | tr / -)" \
  build api web
docker compose -p "ce-$(git rev-parse --abbrev-ref HEAD | tr / -)" \
  up -d --force-recreate api web
dotnet test src/ControlEasyReborn.sln
```

### 4.4 Retired modes (kept for reference, not for new work)

- **Option A — Full Docker on host.** The historical default.
  Falls back to the same `docker/docker-compose.yml` file as
  Option D, so the verification gate is identical. Not maintained
  as a first-class mode; use when the devcontainer is unavailable
  (e.g., inside a non-IDE terminal).
- **Option B — Hybrid (host tooling + Docker DB).** Useful for
  fast inner loops where the API and web are running on the host
  and only the DB lives in Compose. Same verification gate.
- **Option C — Demo mode.** `docker-compose.demo.yml` overlay
  on top of Option A or Option D. The demo overlay is **never**
  used in production (see `docs/demo-mode.md` security warning).

### 4.5 What the devcontainer does NOT do

- It does **not** replace the Compose stack. The runtime target
  is still the Compose-defined `api` + `web` + `db` +
  reverse-proxy; the devcontainer just provides a uniform shell
  from which to invoke Compose.
- It does **not** auto-start the stack on `postCreateCommand`
  by default. `AUTO_START_COMPOSE` is opt-in; manual bring-up
  is the verification gate.
- It does **not** provide a separate Docker engine. DinD is
  rejected (see `.specs/devcontainers/design.md` § "Decision:
  DooD vs DinD").
- It does **not** boot USB / camera devices. For
  `.specs/3 - photo-capture-hardware-integration/`, the
  developer falls back to Option A (host shell + host browser)
  — see that spec for the per-device passthrough instructions.

### 4.6 Common tasks (canonical, devcontainer mode)

The commands below are the `docker compose` invocations that
work today on any checkout. The `scripts/worktree-up.sh` /
`scripts/worktree-down.sh` wrappers (future deliverables of
`.specs/devcontainers/tasks.md` tasks 9.2–9.3) will automate
the env-file generation and resolver entry around these same
`docker compose` lines.

```bash
# Tail the API logs of the worktree's stack
docker compose -p "ce-$(git rev-parse --abbrev-ref HEAD | tr / -)" \
  logs -f api

# Run a one-off .NET CLI in the API container
# (do NOT run dotnet on the host — use the devcontainer shell)
docker compose -p "ce-$(git rev-parse --abbrev-ref HEAD | tr / -)" \
  exec api dotnet --info

# Reset the database (re-runs all init scripts in the worktree's compose)
docker compose -p "ce-$(git rev-parse --abbrev-ref HEAD | tr / -)" \
  down -v
docker compose -p "ce-$(git rev-parse --abbrev-ref HEAD | tr / -)" \
  up -d --build
```

---

## 5. Post-task verification

After completing **every implementation task**, rebuild the
affected Docker images and restart Compose before marking the
task done. **The runtime target is the Docker Compose stack;
the dev shell is the devcontainer** (per constitution v1.3.0,
Principle V). The verification gate is the same in every mode.

### 5.1 Standard verification (main checkout)

From the main checkout (`./`):

```bash
docker compose -f docker/docker-compose.yml build api web
docker compose -f docker/docker-compose.yml up -d --force-recreate api web
dotnet test src/ControlEasyReborn.sln
```

### 5.2 Demo-overlay verification

When the task touches demo mode (`.specs/4 - demo-mode/`), use
the demo overlay instead:

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml build api web
docker compose -f docker/docker-compose.yml -f docker/docker-compose.demo.yml up -d --force-recreate api web
```

### 5.3 Worktree verification

When the task is being implemented in a worktree, use the
worktree's compose project (replace `<branch>` with the
worktree's branch, slashes as hyphens):

```bash
docker compose -p "ce-<branch>" \
  -f docker/docker-compose.yml \
  -f docker/docker-compose.worktree.<branch>.yml \
  build api web
docker compose -p "ce-<branch>" \
  -f docker/docker-compose.yml \
  -f docker/docker-compose.worktree.<branch>.yml \
  up -d --force-recreate api web
dotnet test src/ControlEasyReborn.sln
```

### 5.4 Schema / seed SQL change

If the task changed MySQL init scripts, seed SQL, or database
schema, reset volumes first:

```bash
docker compose -f docker/docker-compose.yml down -v
docker compose -f docker/docker-compose.yml up -d --build
```

### 5.5 The gate

Agents must not close a task until the rebuilt stack starts
healthy and the task's verification gate passes against the
running containers. The `dotnet test` slice is the same in
every mode: at minimum, `dotnet test tests/ControlEasyReborn.UnitTests
tests/ControlEasyReborn.IntegrationTests
tests/ControlEasyReborn.ArchitectureTests`.

---

## 6. Spec structure (per feature)

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
    └── tasks.md          # Step-by-step implementation checklist with Task Dependency Graph
```

- Use **design.md** for features.
- Use **bugfix.md** for bug fixes.
- Use **docplan.md** for new documentation sets.
- Use **docchange.md** for updating existing docs.
- Use **review.md** for branch diff reviews.
- Use **analysis.md** for general code analysis.
- **tasks.md** is always required and lists implementation steps
  with verification gates. Includes a **Task Dependency Graph**
  section defining parallel execution waves:

  ```json
  {
    "waves": [
      { "wave": 1, "tasks": ["<task_id>", "..."] },
      { "wave": 2, "tasks": ["<task_id>", "..."] }
    ]
  }
  ```

  Task IDs reference the numbered tasks. Tasks in the same wave
  have no dependencies on each other and can run in parallel. A
  wave only starts after all tasks in the previous wave are
  complete.

### 6.1 Where agent skills live

The `review.md` / `analysis.md` / `docplan.md` / `docchange.md`
artifacts in a spec folder are produced by the
**GSD agent skills** (`.claude/skills/gsd-*`,
`.agents/skills/gsd-*`) and the **BMAD agent skills**
(`.claude/skills/bmad-*`, `.agents/skills/bmad-*`). The
`orchestration.md` artifact (if used) is produced by the
**BMAD orchestrator** skill. The spec-kit skills
(`.agents/skills/speckit-*`) are the interface between
human intent and the `.specs/<feature>/` artifacts.

### 6.2 When to opt a change into OpenSpec

OpenSpec is opt-in (see § 2.3). When the user decides that a
change is non-trivial enough to warrant a `proposal.md` +
delta-specs lifecycle, the spec-kit `.specs/<feature>/` folder
is created *and* an `openspec/changes/<id>/` folder is created.
The two are kept in sync by the agent that runs
`/opsx:apply`. The user is the only one who decides to opt in;
no agent should create an `openspec/changes/<id>/` folder
without explicit user instruction.

---

## 7. Documentation

This `AGENTS.md` is the canonical home for runtime
documentation. The mapping below is the constitution v1.3.0
"Documentation Systems and Source of Truth" table, reproduced
here as the agent-facing reference.

| Topic | Canonical home |
|---|---|
| Project overview | This `AGENTS.md` § 1 + `.planning/PROJECT.md` |
| Architecture (full system diagram) | This `AGENTS.md` § 3 + `.planning/codebase/ARCHITECTURE.md` |
| Modules (component inventory) | This `AGENTS.md` § 3 + `.planning/codebase/STRUCTURE.md` |
| Local development (devcontainer, worktrees, build/test commands) | This `AGENTS.md` § 4 |
| Post-task verification | This `AGENTS.md` § 5 |
| Spec structure | This `AGENTS.md` § 6 |
| Workflow tooling (GSD / spec-kit / OpenSpec split) | This `AGENTS.md` § 2 + constitution Principle VI |
| **Agent flow cheat sheet** (GSD/spec-kit/OpenSpec lifecycle diagrams, command tables, seams, concurrency budget) | `docs/agent-flow-cheatsheet.md` |
| Codebase maps (STACK, STRUCTURE, ARCHITECTURE, CONVENTIONS, CONCERNS, INTEGRATIONS, TESTING) | `.planning/codebase/*.md` |
| Per-feature spec (requirements, design, tasks, bugfix, review) | `.specs/<feature>/{requirements,design,tasks,bugfix,review,analysis}.md` |
| Constitution (binding governance) | `.specify/memory/constitution.md` |
| Roadmap (14 phases), requirements, live state | `.planning/{ROADMAP,REQUIREMENTS,STATE}.md` |
| Architecture Decision Records (ADRs) | GSD `STATE.md` holds the *decision and rationale*; spec-kit `review.md` / `analysis.md` holds the *finding* that prompted it. `docs/architecture/decisions/0001-…0004-…` are kept for historical reference only. |
| Design system (tokens, components) | `docs/design-system/`, `docs/penpot/`, `mockup/` (under `.specs/2 - visual-design-system/`) |
| Operator docs (first boot, demo mode) | `docs/getting-started.md` + `docs/demo-mode.md` |
| Deployment, hardening, env vars | `.planning/REQUIREMENTS.md` NFRs + `docs/deployment-guide.md` (legacy, redirect-only) |

### 7.1 Note on history

The legacy `docs/` files (a `bmad-document-project --mode deep`
output from 2026-07-12) carry a one-line redirect note at the
top pointing to the canonical home; the binding retirement
schedule is in the constitution.

**Do not** update the legacy `docs/` files when you change
canonical content. Update the canonical home. If a change
*only* exists in a legacy file (because the canonical home has
not been written yet), promote the legacy file to a canonical
home per the constitution's source-of-truth tiebreakers.

---

## 8. Build / Run / Test (quick reference)

```bash
# Bring up the canonical stack (Option D, main checkout)
docker compose -f docker/docker-compose.yml up -d --build

# All .NET
dotnet build src/ControlEasyReborn.sln
dotnet test  src/ControlEasyReborn.sln

# Per project
dotnet test tests/ControlEasyReborn.UnitTests
dotnet test tests/ControlEasyReborn.IntegrationTests
dotnet test tests/ControlEasyReborn.ArchitectureTests

# Filtered
dotnet test tests/ControlEasyReborn.UnitTests \
  --filter "FullyQualifiedName~CreateApartmentHandler"

# Angular unit tests
cd src/Web/ControlEasyReborn.Web
npm test
npm test -- --no-watch --browsers=ChromeHeadless

# Playwright E2E (against running stack on the worktree's Traefik port)
cd src/Web/ControlEasyReborn.Web
npm run e2e:install   # one-time
npm run e2e
E2E_BASE_URL=https://ce-<branch>.localhost:18080+10N npm run e2e
```

---

## 9. Style guardrails (enforced)

- **C#:** file-scoped namespaces, `sealed` classes, `_camelCase`
  private fields, `PascalCase` types/methods, nullable enabled,
  warnings as errors.
- **Angular:** `standalone: true`, `OnPush`, signals API, `ce-`
  prefix, kebab-case selectors, ESLint `@angular-eslint/recommended`,
  Prettier single quotes + 120 cols.
- **No MVC controllers.** Use minimal API endpoint classes.
- **No EF Core in Reborn code.** DBTools only (ADR 0002).
- **No MediatR.** Handlers registered as scoped services directly.
- **All tenant-scoped reads/writes via `ITenantAwareLinqFactory`.**

---

*Constitutional authority: `.specify/memory/constitution.md` v1.3.0.
GSD: `.planning/`. spec-kit: `.specs/` and `.specify/`. OpenSpec:
opt-in, at the discretion of the user. The constitution's
"Documentation Systems and Source of Truth" section records the
retirement schedule for the legacy `docs/` generation pipeline;
the legacy `docs/` files carry one-line redirect notes at the top
and are gated for deletion at the next `/gsd-complete-milestone`
boundary.*
