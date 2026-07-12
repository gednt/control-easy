<!--
Sync Impact Report
==================
Version change: 1.0.0 → 1.1.0  (MINOR — new principle + new governance section + Principle V split)
Modified principles:
  - V. Observability, Security, and Container Parity
      → V. Observability, Security, Runtime Container Parity
        + "Dev shell" sub-bullet (devcontainer is the canonical local shell; Compose remains the runtime target; verification gate runs against Compose, not the devcontainer)
Added principles:
  - VI. Workflow Tooling (GSD / spec-kit / OpenSpec split; legacy docs/ generation pipeline is deprecated; .planning/ is the source of truth for the roadmap, .specs/<feature>/ is the source of truth for per-feature specs, openspec/ is a forward-compatible slot)
Added sections:
  - Documentation Systems and Source of Truth (canonical homes for roadmap, per-feature specs, ADRs, design system, code maps, operator docs, and the explicit retirement of the legacy docs/ generation pipeline and the now-redundant docs/architecture/decisions/ ADRs in favor of GSD STATE.md rationale and spec-kit review.md findings)
Removed sections: none
Templates requiring updates:
  - .specify/templates/plan-template.md        ⚠ pending — confirm "Constitution Check" section is re-evaluated for Principle VI
  - .specify/templates/spec-template.md         ✅ aligned
  - .specify/templates/tasks-template.md        ✅ aligned
  - .specify/templates/checklist-template.md    ⚠ pending — confirm scope covers docs-retirement audit
Follow-up TODOs:
  - [RESOLVED 2026-07-12] CONSTITUTION_V_DEMOTED_RUNTIME_DOCKER_PROSE: AGENTS.md "Post-task verification" prose has been rewritten without the "(Docker)" parenthetical and with an explicit "runtime target is the Docker Compose stack; dev shell is the devcontainer" sentence. See new `AGENTS.md` § 4.1 and § 5.
  - [TODO(CONSTITUTION_VI_OPENSPEC_ADOPTION_DATE)]: openspec/ is a stub today. Per the rewritten `AGENTS.md` § 2.3, OpenSpec is opt-in on a per-change basis at the discretion of the user; it is not a default workflow. The trigger for promoting a change from `.specs/<feature>/` to `openspec/changes/<id>/` is the user explicitly invoking `/opsx:new` on that change. No automatic promotion.
  - [TODO(CONSTITUTION_VI_DOCS_RETIREMENT_DATE)]: docs/index.md, docs/project-overview.md, docs/development-guide.md, docs/architecture.md, docs/component-inventory.md, docs/deployment-guide.md, docs/api-contracts.md, docs/data-models.md, docs/integration-architecture.md, docs/source-tree-analysis.md, and docs/project-scan-report.json are all bmad-document-project outputs. They have been marked DEPRECATED in this change (one-line redirect note at the top of each, with `_deprecated_redirect` field in the JSON). Actual deletion is gated on the next `/gsd-complete-milestone` run.
-->

# ControlEasy Reborn Constitution

## Core Principles

### I. Spec-Driven Development

Every shipped change starts with a spec under `.specs/<feature>/` and is implemented
against the checklist in `tasks.md`. Specs are the contract between intent and code;
the implementation plan (`plan.md`), task list (`tasks.md`), and review artifacts
(`review.md`, `analysis.md`) MUST be kept in sync with the code they describe. No
feature work begins without an approved spec folder. Strangler Fig migration
contracts — what is in, out, deferred, and how the legacy WPF app is replaced —
are recorded here and re-validated at every phase boundary.

**Rationale:** The legacy modernization spans 19+ spec folders, ~308 checkbox tasks,
and a long-running migration from WPF. Without spec-first discipline, the team
loses the single source of truth and ships accidental regressions. `AGENTS.md`
mandates the spec-driven workflow; this principle makes it non-negotiable.

### II. Multi-Tenant Isolation by Default

Every persisted entity, every query, and every API request is scoped to a tenant
(`TenantId` / `tenant_id` column + JWT `tenant_id` claim + `TenantFilterInterceptor`).
Cross-tenant data leakage is a P0 incident. PlatformAdmin-only paths (e.g.,
condominium administration, `/api/v1/demo/reset`) are the only legitimate
exceptions and MUST be guarded by `[Authorize(Roles = "PlatformAdmin")]` and an
explicit allowlist. Architecture tests enforce that no new entity ships without
`tenant_id` (with a documented `exemptTables` set for true platform tables such
as `DemoMetadata`, `Tenants`, `Migrations`).

**Rationale:** Condominium data is regulated personal data hosted in a shared
schema. A single missing filter in one repository is a privacy breach for an
entire condominium. Defending in depth — schema, interceptor, JWT claim, and
architecture test — is the only acceptable posture.

### III. LINQ-First Data Access (DBTools)

All data access goes through the DBTools `IAsyncSqlClient` / `Linq<TModel>`
primitives, preferring LINQ expressions (`WhereAsync`, `Select`, joins via
property selectors) for every query. Raw SQL via `SqlClient` is permitted only
for stored procedures or DB-specific features not covered by the LINQ provider,
and every raw-SQL site MUST carry a comment justifying the bypass. The target
provider is MySQL 8 today; code MUST NOT depend on MySQL-specific syntax in
LINQ paths so the system can move to PostgreSQL or SQL Server without code
changes. The library is consumed as the [DBTools 1.4.3 NuGet package](https://www.nuget.org/packages/DBTools);
the vendored copy under `src/lib/DBTools_SQL/` is being retired.

**Rationale:** `AGENTS.md` pins the data-access stack and `Directory.Packages.props`
pins the version. LINQ-first is a deliberate design constraint that gives
multi-provider portability and async/await end-to-end. Bypassing it for
convenience erodes both guarantees at once.

### IV. Test-First & Verification Discipline

Tests are written, observed to fail, and only then implemented (Red → Green →
Refactor). Unit tests (xUnit + FluentAssertions + Moq), integration tests
(against Testcontainers-spun MySQL), and architecture tests (NetArchTest) are
mandatory for: new entities, new endpoints, cross-tenant paths, raw-SQL sites,
and any change to the tenant filter interceptor. Every implementation task has
a verification gate — most tasks ship a `docker compose build && up -d` smoke
plus a `dotnet test` slice. The post-task Docker rebuild rule from `AGENTS.md`
(non-optional) is a constitutional requirement: no task is "done" until the
affected containers are rebuilt, restarted, and observed healthy.

**Rationale:** A modular monolith with 9 feature modules, a multi-tenant data
model, and a Strangler Fig migration in flight is a regression factory without
disciplined tests. TDD is the cheapest place to catch a tenant leak, a missing
migration, or a broken contract.

### V. Observability, Security, and Runtime Container Parity

The system is observable, secure, and identical between local and production.
Structured logging via Serilog (Console JSON + Seq/file sinks) is mandatory for
every backend request path. Errors surface as RFC 7807 `ProblemDetails` mapped
from domain exceptions (`NotFoundException`, `ValidationException`,
`ConflictException`). Authentication is JWT bearer (HS256 minimum, RS256
preferred) with secrets from environment variables or Docker secrets — never
hard-coded. The Docker Compose stack (`api`, `web`, `db`, `reverse-proxy`,
`adminer`, optional `seq`, optional `redis`) is the source of truth for the
**runtime target**: anything that "works on my machine but not in Compose" is a
bug. The demo overlay (`docker-compose.demo.yml`) and its JWT signing key are
**never** used in production.

**Dev shell (sub-bullet):** The local **developer shell** is a
[devcontainer](https://containers.dev/) (see `.specs/devcontainers/`). The
devcontainer mounts the host's Docker socket (Docker-out-of-Docker) so that
`docker compose` invocations inside the dev shell target the same engine and
volumes as the host. **The verification gate is still the Compose stack**;
the devcontainer is a uniform shell, not a parallel runtime. A worktree per
feature branch (see `AGENTS.md` "Dev containers & worktrees") is the standard
shape for parallel work.

**Rationale:** Condominium operators are not developers; when something breaks
at the gatehouse, the on-call engineer must be able to read the logs, replay
the request, and roll back without leaving Docker. Production parity is the
cheapest way to ensure that. The dev shell is separate from the runtime
target: we want a uniform, reproducible dev experience (devcontainer) without
giving up "Compose is the runtime" (the verification gate is unchanged).

### VI. Workflow Tooling (GSD / spec-kit / OpenSpec)

The project runs three workflow systems, each with a single, non-overlapping
responsibility:

- **GSD** (`.planning/`) is the **planner and roadmap owner**. `PROJECT.md`
  holds the high-level brief and the validated/active/out-of-scope lists.
  `REQUIREMENTS.md`, `ROADMAP.md`, and `STATE.md` track the 14-phase rollout
  and the live execution state. `.planning/codebase/` holds the seven
  code-base maps (STACK, STRUCTURE, ARCHITECTURE, CONVENTIONS, CONCERNS,
  INTEGRATIONS, TESTING) consumed by every other tool. GSD phase boundaries
  (via `/gsd-transition`, `/gsd-complete-milestone`) are the moments at which
  documentation is re-validated.
- **spec-kit** (`.specs/`, `.specify/`) is the **per-feature spec owner**.
  Every feature or bug fix lives under `.specs/<feature>/` with the
  artifacts defined in `AGENTS.md` (the `requirements.md` / `design.md` /
  `tasks.md` triple, with optional `bugfix.md` / `review.md` / `analysis.md`).
  `.specify/memory/constitution.md` (this file) is the binding governance
  document for spec-kit. The speckit-command skills (`.agents/skills/`)
  are the interface between human intent and spec-kit artifacts.
- **OpenSpec** (`openspec/`) is a **forward-compatible slot** for the day
  the team chooses to adopt change-tracking with formal proposal/spec
  deltas. Today it is a stub (`openspec/config.yaml` only, `schema:
  spec-driven`, no `changes/` directory). Promoting a per-feature spec from
  `.specs/<feature>/` to `openspec/changes/<id>/` is a one-way migration
  triggered explicitly (see `TODO(CONSTITUTION_VI_OPENSPEC_ADOPTION_DATE)`
  in the Sync Impact Report above); it is NOT automatic.

The **legacy `docs/` generation pipeline** (the output of `bmad-document-project
--mode deep` runs) is **deprecated as a source of truth** and is on a
retirement schedule (see § "Documentation Systems and Source of Truth"
below). It remains in the tree as a historical snapshot for the
2026-07-12 scan only.

**Rationale:** Three overlapping spec systems (GSD + spec-kit + OpenSpec) is
one too many for a small team. Pinning each to a single responsibility is
cheaper than consolidating to one tool: GSD's roadmap + codebase maps are
hard to replicate, spec-kit's per-feature template is hard to replicate, and
OpenSpec's proposal/scenario model is future-looking. The legacy `docs/`
generation pipeline was useful once (it bootstrapped the project when the team
had no GSD codebase maps) but it now duplicates `.planning/codebase/` and
`.specs/<feature>/design.md` content, and drifts. Retirement is cheaper than
keeping it in sync.

## Stack & Architecture Constraints

The stack is fixed by `AGENTS.md` and the modernization roadmap. New code MUST
conform.

- **Frontend:** Angular 18+ SPA, standalone components, signals, TypeScript
  strict mode. Domain models are NOT shared with the backend — Angular owns its
  own DTOs generated from the OpenAPI schema (`ng-openapi-gen` or `nswag`).
- **Backend:** ASP.NET Core 8 (LTS) Web API, C# 12, file-scoped namespaces,
  `record` DTOs, `sealed` classes by default, async/await end-to-end.
- **Architecture:** Modular monolith with Clean Architecture per module
  (`Domain` → `Application` → `Infrastructure` → `Api`). Module-per-feature
  under `src/Modules/<Feature>/`. Vertical slices where they reduce coupling.
  CQRS-lite is permitted for read/write-heavy paths (e.g., reports).
- **Layout:** `src/` (Host, Web, BuildingBlocks, Modules), `tests/`, `docker/`,
  `docs/`. API routes are kebab-case, plural nouns, versioned at
  `/api/v1/...`.
- **Mapping:** Mapster or AutoMapper; **Validation:** FluentValidation;
  **OpenAPI:** Swashbuckle on the API.
- **Containerization:** Docker + Docker Compose. Base images are checked
  with `docker image inspect` before pulling to avoid MCR rate-limiting. Builds
  run with `COMPOSE_DOCKER_CLI_BUILD=1 DOCKER_BUILDKIT=1`.
- **Cross-cutting:** Serilog for logging, FluentValidation for input, Mapster
  for mapping, Swashbuckle for OpenAPI, JWT bearer for auth. No Entity
  Framework, no `MySql.Data` in Reborn code.

## Security & Compliance Requirements

- **Secrets** (DB connection string, JWT signing key, third-party API keys)
  are loaded from environment variables or Docker secrets. The legacy
  hard-coded credentials in `App.config` are not present in Reborn and MUST
  never reappear.
- **Authentication** is JWT bearer; passwords are BCrypt-hashed (cost ≥ 11).
  PlatformAdmin bootstrap on first boot is a one-shot hosted service and is
  disabled in demo mode.
- **Authorization** is role-based plus per-tenant scope. Roles include
  `PlatformAdmin`, `TenantAdmin`, `Porteiro`, `Resident`, `ServiceProvider`,
  `Multi`. Cross-role access is denied by default and granted only with an
  explicit policy.
- **Personal data** is limited to what the gatehouse operation requires
  (residents, visitors, vehicles, service providers, apartments). Data
  retention rules are deferred to the separate data-migration spec.
- **Demo data and credentials are not production data.** The demo overlay
  is segregated by `docker-compose.demo.yml`, its JWT key is fixed and
  well-known, and a prominent banner in the UI tells every signed-in user
  they are on a demo stack. `POST /api/v1/demo/reset` is PlatformAdmin-only
  and returns 404 when demo mode is off.

## Development Workflow & Quality Gates

- **Spec folder is the unit of work.** Every feature or bug fix lives under
  `.specs/<feature>/` with at minimum `tasks.md` and either `design.md`
  (feature) or `bugfix.md` (fix). Optional artifacts: `requirements.md`,
  `review.md`, `analysis.md`, `docplan.md`, `docchange.md`,
  `orchestration.md`.
- **Plan–Spec–Tasks alignment.** `plan.md` MUST cite the spec, declare a
  Constitution Check pass, and document any intentional deviations. The
  `tasks.md` checklist and its Task Dependency Graph (`waves` JSON) are the
  execution contract.
- **Task numbering.** Numbered tasks reference the spec phase (e.g., `1.0a`,
  `8.5`). A `C.*` (continuous) prefix is reserved for cross-cutting tasks
  (CI, Docker rebuild, architecture tests) that run across the whole project.
- **Definition of done.** A task is done only when: (1) the code lands on the
  branch, (2) the spec checkbox is updated, (3) the affected Docker services
  are rebuilt and observed healthy, (4) the task's verification gate (unit,
  integration, or manual smoke) passes, and (5) the Sync Impact Report (for
  any constitutional change) is regenerated.
- **Code review.** Every PR must include a `.specs/.../review.md` (or a
  comment linking to one) and a passing `dotnet test` for the affected test
  projects. Architecture tests are non-skippable.
- **Commits.** Commit messages reference the task ID (`8.5: add demo
  banner to app shell`). No force-pushes to `main`/`master`.

## Documentation Systems and Source of Truth

Every documentation system has a single canonical home. Conflicts between
systems are resolved in favor of the system that owns the artifact in
question; if two systems claim the same artifact, the newer system is
correct and the older one is stale.

| Artifact | Canonical home | Owner | Notes |
|---|---|---|---|
| Project brief, validated/active/out-of-scope lists | `.planning/PROJECT.md` | GSD | Re-validated at every phase transition. |
| Roadmap (14 phases), requirements, live state | `.planning/ROADMAP.md`, `.planning/REQUIREMENTS.md`, `.planning/STATE.md` | GSD | Updated by `/gsd-*` skills. |
| Codebase maps (STACK, STRUCTURE, ARCHITECTURE, CONVENTIONS, CONCERNS, INTEGRATIONS, TESTING) | `.planning/codebase/*.md` | GSD | Source for every other tool. |
| Per-feature spec (requirements, design, tasks, bugfix, review) | `.specs/<feature>/{requirements,design,tasks,bugfix,review,analysis}.md` | spec-kit | Required artifacts vary per spec type. |
| Constitution (binding governance) | `.specify/memory/constitution.md` | spec-kit | This file. Amended by `/speckit-constitution`. |
| Per-feature OpenAPI / contracts | `.specs/<feature>/design.md` (section "Contracts") | spec-kit | Generated artifacts land under `src/Host/.../wwwroot/swagger/`. |
| Architecture Decision Records (ADRs) | **Two homes, two scopes:** GSD STATE.md holds the *decision and rationale*; spec-kit `review.md` / `analysis.md` files hold the *finding* that prompted the decision. | GSD + spec-kit | The legacy `docs/architecture/decisions/0001-…0004-…` ADRs are kept for historical reference but new ADRs are recorded in GSD STATE.md and cross-referenced from the relevant `.specs/<feature>/` file. |
| Design system (tokens, components) | `docs/design-system/`, `docs/penpot/`, `mockup/` | spec-kit (`.specs/2 - visual-design-system/`) | Tokens contract enforced by `.planning/REQUIREMENTS.md` trace rows and continuous task C.7. |
| Operator docs (first boot, demo mode) | `AGENTS.md` "Operator reference" + `docs/getting-started.md` + `docs/demo-mode.md` | spec-kit | When the doc describes a one-command procedure, it is in `AGENTS.md`. |
| Local development (dev shell, devcontainer, worktrees, build/test commands) | `AGENTS.md` "Local development" | spec-kit | This is the home for Option D. The legacy `docs/development-guide.md` is being retired (see Retirement schedule below). |
| Deployment, hardening, env vars | `AGENTS.md` "Deployment" + `.planning/REQUIREMENTS.md` (NFRs) | spec-kit + GSD | The legacy `docs/deployment-guide.md` is being retired. |
| API contracts (route catalog, key endpoints) | `AGENTS.md` "API surface" + `.specs/<feature>/design.md` (contracts section) | spec-kit | The legacy `docs/api-contracts.md` is being retired. |
| Data models (schema, dual-column tenancy, repositories) | `AGENTS.md` "Data model" + `.specs/<feature>/design.md` (data model section) | spec-kit | The legacy `docs/data-models.md` is being retired. |
| Integration architecture (cross-part communication, auth flow) | `AGENTS.md` "Integration architecture" + `.planning/codebase/INTEGRATIONS.md` | spec-kit + GSD | The legacy `docs/integration-architecture.md` is being retired. |
| Project overview (executive summary, parts, tech-stack table) | `README.md` + `AGENTS.md` "Project overview" | spec-kit | The legacy `docs/project-overview.md` is being retired. |
| Source-tree analysis | `.planning/codebase/STRUCTURE.md` | GSD | The legacy `docs/source-tree-analysis.md` is being retired. |
| Component inventory | `AGENTS.md` "Modules" + `.planning/codebase/STRUCTURE.md` | spec-kit + GSD | The legacy `docs/component-inventory.md` is being retired. |
| Architecture (full system diagram) | `AGENTS.md` "Architecture" + `.planning/codebase/ARCHITECTURE.md` | spec-kit + GSD | The legacy `docs/architecture.md` is being retired. |
| Doc-gen pipeline state | **No home — retired.** | n/a | The legacy `docs/project-scan-report.json` is the historical artifact of a `bmad-document-project --mode deep` run on 2026-07-12. |

**Retirement schedule** (for the legacy `docs/` generation pipeline and the
now-redundant `docs/architecture/decisions/` ADRs):

- **Phase 0 (this constitution bump, v1.1.0):** mark the legacy docs as
  deprecated. Update each legacy doc with a one-line redirect note pointing
  to its canonical home in `AGENTS.md` and `.planning/`.
- **Next milestone boundary** (per `/gsd-complete-milestone`): delete the
  legacy `docs/{index,project-overview,development-guide,deployment-guide,
  api-contracts,data-models,integration-architecture,source-tree-analysis,
  component-inventory,architecture}.md` files and `docs/project-scan-report.json`.
  Keep `docs/architecture/decisions/` for historical reference, but stop
  writing new ADRs there.
- **GSD phase transitions** are the trigger for re-validating this
  retirement schedule. If a phase boundary finds that a legacy doc is still
  load-bearing, that doc is promoted to a canonical home (this section is
  amended) and the migration is paused.

**Source-of-truth tiebreakers:**

1. Code is the lowest-level truth. If `AGENTS.md` and the code disagree,
   the code wins and `AGENTS.md` is updated.
2. `.planning/STATE.md` is the highest-level truth for the *current* state.
   If `.specs/<feature>/tasks.md` and `.planning/STATE.md` disagree on
   what is "done", `STATE.md` wins (it is updated by the agent that
   finished the work).
3. `.specify/memory/constitution.md` (this file) is non-negotiable within
   the scope of `/speckit-*` analysis. If a principle needs to change,
   that is a separate, explicit constitution update — never silent
   reinterpretation.

## Governance

This Constitution supersedes all other practices, ad-hoc conventions, and
informal agreements for the **ControlEasy Reborn** project. Where this
document conflicts with `AGENTS.md` or any spec, this Constitution wins
unless an amendment is in flight.

- **Amendments.** Any change to a principle, section, or governance rule
  requires (1) a pull request that updates `.specify/memory/constitution.md`
  and the version line, (2) a Sync Impact Report at the top of the file
  listing every modified principle, added/removed section, and template
  affected, and (3) approval from a project maintainer.
- **Versioning.** Constitution versions follow semantic versioning.
  **MAJOR** = backward-incompatible principle removal or redefinition;
  **MINOR** = new principle or materially expanded guidance; **PATCH** =
  clarifications, wording, typo fixes.
- **Compliance review.** Every PR review MUST verify compliance with the
  active principles. Any violation that survives review is a blocker;
  the `Complexity Tracking` section of `plan.md` exists solely to justify
  exceptions, not to normalize them.
- **Runtime guidance.** `AGENTS.md` is the runtime development guide and
  MUST be updated whenever a principle changes in a way that affects day-
  to-day contributor workflow (e.g., new test gate, new Docker command).
- **Living document.** This Constitution is reviewed at every milestone
  boundary (via `/gsd-complete-milestone`) and at every phase transition
  (via `/gsd-transition`). "What This Is" and "Out of Scope" drift are
  treated as constitutional concerns, not just documentation hygiene.

**Version**: 1.1.0 | **Ratified**: 2026-07-12 | **Last Amended**: 2026-07-12
