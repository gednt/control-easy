<!--
Sync Impact Report (v1.3.0)
===========================
Version change: 1.2.0 → 1.3.0  (MINOR — new Principle VII + post-cleanup factual sync)

Previous report (1.1.0 → 1.2.0): "Development Workflow & Quality Gates" section
was rewritten from the actual behavior of the three workflow tools; the
"Ownership principle" was added to § 4 Seams; AGENTS.md version pins were
bumped to v1.2.0; templates were aligned. (Preserved for audit trail.)

What changed in 1.2.0 → 1.3.0:
  - Principle IV — corrected the unit-test mocking library from "Moq" to
    "NSubstitute" (matches AGENTS.md § 3, .planning/codebase/STACK.md,
    .planning/codebase/TESTING.md, and Directory.Packages.props), and added
    the concrete integration-test harness (WebApplicationFactory<Program>)
    and the architecture-test package name (NetArchTest.Rules). This is a
    factual correction, not a policy change.
  - Principle VII (NEW) — Sub-Agent Orchestration & Concurrency Budget.
    Raises AGENTS.md § 2.4 (sub-agent invocation) and § 2.5 (concurrency
    budget) to constitutional status: any agent MAY invoke any of
    GSD/spec-kit/OpenSpec as a sub-agent across seams; the default budget
    is 3 sub-agents per level (4 total in-flight including the main
    orchestrator); the budget is per agent, not per workflow tool; the
    override path requires a recorded rationale in proposal.md /
    design.md / tasks.md, audited at the next /gsd-complete-milestone.
  - Development Workflow & Quality Gates § 4 Seams — intro amended to
    cross-reference Principle VII instead of duplicating the sub-agent
    protocol inline; per-milestone gate extended to audit concurrency-
    budget override rationales.
  - Documentation Systems and Source of Truth table — three rows updated
    to reflect commit b34b4d2 (2026-07-12): (a) ADR row notes the legacy
    docs/architecture/decisions/0001-…0004-… files were deleted (kept in
    git history only); (b) Design system row notes docs/design-system/
    and docs/penpot/ were deleted (canonical homes are now
    .specs/2 - visual-design-system/ and mockup/); (c) new row added for
    docs/agent-flow-cheatsheet.md (the agent flow cheat sheet, added in
    commit 9979c1a, 2026-07-12).
  - Retirement schedule — Phase 0 (v1.1.0 deprecation) marked ✅ done;
    a new "Partial deletion (v1.3.0, commit b34b4d2, 2026-07-12)" entry
    records the early deletion of the fully-superseded ADRs, design-
    system guides, Penpot assets, and legacy-mapping doc; the next
    milestone boundary gate is narrowed to the remaining legacy docs
    (index, project-overview, development-guide, deployment-guide,
    api-contracts, data-models, integration-architecture, source-tree-
    analysis, component-inventory, architecture, project-scan-report.json).

Modified principles:
  - IV. Test-First & Verification Discipline (factual correction: Moq → NSubstitute;
    added WebApplicationFactory<Program> + NetArchTest.Rules specifics)
  - VI. Workflow Tooling (intro cross-reference to new Principle VII added)
  - VII. Sub-Agent Orchestration & Concurrency Budget (NEW)

Added sections:
  - Principle VII (Sub-Agent Orchestration & Concurrency Budget)
  - Documentation Systems and Source of Truth: row for
    docs/agent-flow-cheatsheet.md

Removed sections: none

Templates requiring updates:
  - .specify/templates/plan-template.md        ✅ no change required — the
    "Constitution Check" gate is principle-agnostic and already cites the
    constitution file by path; Principle VII is picked up at /speckit-plan
    Phase 0 automatically.
  - .specify/templates/spec-template.md         ✅ no change required.
  - .specify/templates/tasks-template.md        ✅ no change required —
    the [P] parallel markers and the wave-dependency JSON block already
    compose naturally with the concurrency budget (the budget bounds
    fan-out at runtime; [P] declares the opportunity).
  - .specify/templates/checklist-template.md    ✅ no change required.
  - .specify/templates/constitution-template.md ✅ no change required —
    the template is a placeholder skeleton with [PRINCIPLE_N_NAME] /
    [PRINCIPLE_N_DESCRIPTION] slots; the project constitution is already
    fully populated and does not re-derive from the template.

Runtime guidance:
  - AGENTS.md                                   ✅ updated — version pin
        bumped to v1.3.0; the testing row already names NSubstitute
        (no edit needed for the Principle IV correction); § 2.4 and
        § 2.5 already contain the sub-agent invocation and concurrency
        budget text that Principle VII elevates (no edit needed for the
        Principle VII addition, only the version pin); documentation map
        already lists docs/agent-flow-cheatsheet.md.
  - docs/agent-flow-cheatsheet.md               ✅ updated — version pin
        bumped to v1.3.0 in the § 10 versions block; the cheat sheet
        already references the constitution as the governance anchor.

Deferred items (stable conditions, not point-in-time snapshots):
  - OpenSpec adoption: openspec/ is opt-in. An `openspec/changes/<id>/`
    folder is created only when the user explicitly invokes `/opsx:new`
    (or equivalent) on a change. No automatic promotion.
  - Legacy docs retirement: the partial deletion in commit b34b4d2
    removed the ADRs, design-system guides, Penpot assets, and legacy-
    mapping doc. The remaining legacy docs (index, project-overview,
    development-guide, deployment-guide, api-contracts, data-models,
    integration-architecture, source-tree-analysis, component-inventory,
    architecture, project-scan-report.json) are gated for deletion at
    the next /gsd-complete-milestone boundary.
  - STATE.md drift: .planning/STATE.md still records "Current focus:
    Phase 8" with last activity 2026-06-24, predating the 2026-07-12
    governance refactor. Updating STATE.md is GSD's writer-of-record job
    (via /gsd-* commands), not spec-kit's; flagged for the next
    /gsd-transition or /gsd-complete-milestone run.
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
Refactor). Unit tests (xUnit + FluentAssertions + NSubstitute), integration
tests (against Testcontainers-spun MySQL, with `WebApplicationFactory<Program>`
helpers), and architecture tests (NetArchTest.Rules) are mandatory for: new
entities, new endpoints, cross-tenant paths, raw-SQL sites, and any change to
the tenant filter interceptor. Every implementation task has a verification
gate — most tasks ship a `docker compose build && up -d` smoke plus a
`dotnet test` slice. The post-task Docker rebuild rule from `AGENTS.md`
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
  deltas. It is opt-in: an `openspec/changes/<id>/` folder is created
  only when the user explicitly invokes `/opsx:new` (or equivalent) on
  a change. Promoting a per-feature spec from `.specs/<feature>/` to
  `openspec/changes/<id>/` is a one-way migration triggered
  explicitly; it is NOT automatic.

The **legacy `docs/` generation pipeline** (the output of
`bmad-document-project --mode deep` runs) is **deprecated as a source
of truth** and is on a retirement schedule (see § "Documentation
Systems and Source of Truth" below).

**Rationale:** Three overlapping spec systems (GSD + spec-kit + OpenSpec)
is one too many for a small team. Pinning each to a single
responsibility is cheaper than consolidating to one tool: GSD's
roadmap + codebase maps are hard to replicate, spec-kit's per-feature
template is hard to replicate, and OpenSpec's proposal/scenario model
is future-looking. The legacy `docs/` generation pipeline duplicates
`.planning/codebase/` and `.specs/<feature>/design.md` content and
drifts; retirement is cheaper than keeping it in sync.

### VII. Sub-Agent Orchestration & Concurrency Budget

Any agent — including a main orchestrator and any in-flight sub-agent —
MAY invoke any of GSD, spec-kit, or OpenSpec skills as a sub-agent when
the work at hand crosses the seam between the three tools (see §
"Development Workflow & Quality Gates" → "Seams"). Sub-agent
invocation MUST NOT bypass the ownership rules in Principle VI: a
sub-agent writes only to the directory owned by its own workflow tool
(GSD → `.planning/`, spec-kit → `.specs/` and `.specify/`, OpenSpec →
`openspec/`); it reads from the other tools' directories but does not
rewrite them; the sub-agent's parent is responsible for catching
ownership violations before they land.

**Concurrency budget.** The default concurrency budget is **up to 3
sub-agents in flight concurrently, in addition to the main
orchestrator** (total in-flight count per level: 4 = 1 main + 3 subs).
The budget is **per agent, not per workflow tool**: a sub-agent that
itself spawns sub-agents gets its own independent budget of 3 — a
sub-agent may not "consume" its parent's budget. Exceeding the budget
(4+ sub-agents in flight at any level) is permitted only when the
originating artifact records an explicit rationale in its
`proposal.md` "Why" section, `design.md` "Decisions" section, or
`tasks.md` task description for the wave in question. The rationale is
reviewed at the next `/gsd-complete-milestone` boundary. No environment
variable, config file, or CLI flag overrides the budget — the override
is qualitative and lives next to the work it justifies.

**Rationale:** Without a bound on fan-out, an autonomous agent can
spawn a fan-in cascade that exhausts the context budget or produces
unbounded per-step latency. A per-agent, per-level budget of 3 keeps
fan-out predictable while still permitting the parallelism that
wave-based GSD plans and spec-kit `[P]` task markers rely on. The
qualitative override path (rationale recorded in the originating
artifact) ensures that an exception is visible at the next milestone
audit without a separate scan — the rationale lives next to the work
it justifies.

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

The project runs three workflow tools — **GSD**, **spec-kit**, and **OpenSpec** —
each with a single, non-overlapping responsibility (Principle VI), and any agent
MAY invoke any of them as a sub-agent within the concurrency budget
(Principle VII). This section describes how each tool works day-to-day and,
critically, the **seams** where they hand off to each other so the three
compose into one development loop rather than three parallel ones.

### 1. GSD — planner and roadmap owner (`.planning/`)

GSD owns the project-level state: the roadmap, the requirements, the live
execution state, and the seven codebase maps. Every agent reads it before
doing anything else; every agent that changes state updates it.

**Phase lifecycle.** The roadmap is a sequence of phases. A phase moves through
a standard lifecycle, each step a `/gsd-*` command:

1. `/gsd-discuss-phase <N>` — surface scope, assumptions, and decisions;
   produce `CONTEXT.md`.
2. `/gsd-plan-phase <N>` — spawn `gsd-phase-researcher` (writes `RESEARCH.md`)
   then `gsd-planner` (writes `NN-PLAN.md`); verify with `gsd-plan-checker`
   until the plan passes.
3. `/gsd-execute-phase <N>` — wave-based parallel execution; each plan gets a
   `gsd-executor` subagent that runs tasks atomically and commits per task.
4. `/gsd-verify-work <N>` — conversational UAT; produce `NN-UAT.md`; if gaps
   are found, `/gsd-execute-phase <N> --gaps-only` closes them.
5. `/gsd-validate-phase <N>` — retroactive Nyquist audit of test coverage;
   produce `VALIDATION.md` + generated tests.
6. `/gsd-transition` — advance the roadmap pointer; re-validate `PROJECT.md`,
   `REQUIREMENTS.md`, and this constitution against what shipped.
7. `/gsd-complete-milestone <version>` — at a milestone boundary: archive
   roadmap + requirements to `.planning/milestones/`, update `PROJECT.md`,
   tag the release, and trigger the legacy-`docs/` retirement schedule
   (see "Documentation Systems and Source of Truth" below).

**Artifacts GSD owns:** `PROJECT.md`, `REQUIREMENTS.md`, `ROADMAP.md`,
`STATE.md`, `.planning/codebase/{STACK,STRUCTURE,ARCHITECTURE,CONVENTIONS,
CONCERNS,INTEGRATIONS,TESTING}.md`, and per-phase folders under
`.planning/phases/<NN>-<slug>/` (`CONTEXT.md`, `RESEARCH.md`, `NN-PLAN.md`,
`NN-SUMMARY.md`, `NN-VERIFICATION.md`, `VALIDATION.md`, `NN-UAT.md`).

**Decisions and rationale.** GSD `STATE.md` is the canonical home for the
*decision and rationale* of any architectural choice; spec-kit `review.md` /
`analysis.md` holds the *finding* that prompted it (see "Documentation
Systems and Source of Truth").

### 2. spec-kit — per-feature spec owner (`.specs/`, `.specify/`)

spec-kit owns the per-feature implementation contract. Every feature or bug
fix lives under `.specs/<feature>/`. The spec-kit command skills
(`.agents/skills/speckit-*`) are the interface between human intent and the
spec artifacts. The pipeline is four commands, run in order:

1. `/speckit-specify "<description>"` — create `.specs/<feature>/spec.md`
   from the feature description; generate a quality checklist under
   `.specs/<feature>/checklists/`; resolve up to 3 `[NEEDS CLARIFICATION]`
   markers with the user. Output: `spec.md` (user stories with priorities,
   functional requirements, success criteria, edge cases).
2. `/speckit-plan` — fill `plan.md` from the template: Technical Context,
   Constitution Check (read from this file), Phase 0 `research.md`,
   Phase 1 `data-model.md` + `contracts/` + `quickstart.md`. Re-evaluate
   the Constitution Check after design.
3. `/speckit-tasks` — generate `tasks.md` organized by user story (Setup →
   Foundational → one phase per story in priority order → Polish), with
   `[P]` parallel markers, `[USn]` story labels, exact file paths, and a
   dependency graph.
4. `/speckit-implement` — execute `tasks.md` phase-by-phase, marking each
   task `[X]` as it lands; respect `[P]` parallelism and TDD ordering where
   tests are requested.

**Auxiliary commands:** `/speckit-clarify` (resolve remaining
clarifications), `/speckit-checklist` (generate domain checklists — UX,
security, test, etc.), `/speckit-converge` (consolidation),
`/speckit-analyze` (code analysis → `analysis.md`),
`/speckit-constitution` (amend this file).

**This constitution is the binding governance document for spec-kit.**
`/speckit-plan` Phase 0 reads `.specify/memory/constitution.md` and fills
the `plan.md` "Constitution Check" section from it; any violation that
cannot be justified in the plan's Complexity Tracking table blocks
planning. spec-kit does NOT own the roadmap or the phase state — it owns
the per-feature contract that GSD phases consume.

### 3. OpenSpec — opt-in change-tracking layer (`openspec/`)

OpenSpec is **opt-in**, at the discretion of the user, on a per-change
basis. It is NOT the default workflow. Most changes go through spec-kit
alone; only some go through OpenSpec as well. The two coexist; nothing in
spec-kit is replaced by adopting OpenSpec on a change.

When the user opts a change into OpenSpec, the agent creates an
`openspec/changes/<id>/` folder with a `proposal.md` + delta-specs +
`design.md` + `tasks.md` lifecycle:

1. `proposal.md` — why, what, scope, success criteria. The "Why" section
   cross-references the spec-kit `.specs/<feature>/` folder.
2. **Delta specs** under `openspec/changes/<id>/specs/<domain>/` using
   `## ADDED Requirements`, `## MODIFIED Requirements`,
   `## REMOVED Requirements` sections — the diff against the canonical
   `openspec/specs/<domain>/`, not a rewrite.
3. `design.md` and `tasks.md` — the lifecycle is
   `proposal → specs → design → tasks`, progressive, not waterfall.
4. `/opsx:apply <id>` — implement against the delta specs and the task
   list; the agent keeps the OpenSpec `tasks.md` and the spec-kit
   `tasks.md` in sync. **The spec-kit `tasks.md` is the one the
   verification gate reads** (see Quality Gates below).
5. `/opsx:archive <id>` — fold the delta specs into the canonical
   `openspec/specs/<domain>/` and move the change to
   `openspec/changes/archive/<date>-<id>/`.

`openspec/` is opt-in: no agent creates an `openspec/changes/<id>/`
folder without explicit user instruction. The canonical
`openspec/specs/<domain>/` accumulates folded deltas over time; the
spec-kit `requirements.md` / `design.md` do NOT have to mirror them —
they are independent, with cross-references added to the spec-kit
`requirements.md` "OpenSpec" subsection when relevant.

### 4. Seams — how the three tools compose

The three tools are designed to hand off to each other at well-defined
points. The seams below are non-negotiable. Sub-agent invocation across
these seams is governed by Principle VII; the ownership rules in this
section apply equally to sub-agents as to main orchestrators. The sub-agent invocation
protocol and concurrency budget that govern cross-tool fan-out are in
Principle VII (and reproduced as operator guidance in `AGENTS.md`
§ 2.4 and § 2.5, and in `docs/agent-flow-cheatsheet.md` § 4 and § 5). The seams
below assume that protocol and do not repeat it.

**Ownership principle.** GSD owns `.planning/` — it is the single writer
of the roadmap pointer, the phase state, and the milestone archive, and
the single reader-of-record for project-level state. The other two tools
are **writers** to `.planning/` whenever their work changes project
state, and **readers** of `.planning/` whenever they need project
context. Concretely: spec-kit and OpenSpec MUST update
`.planning/STATE.md` (decisions, blockers, deferred items) and
`.planning/PROJECT.md` (validated/active/out-of-scope lists) when their
work changes those facts; they MUST NOT update `ROADMAP.md`, the phase
pointer, or the milestone archive — those are GSD's job
(`/gsd-transition`, `/gsd-complete-milestone`). Conversely, GSD consumes
`.specs/<feature>/` and `openspec/` artifacts but MUST NOT rewrite them
— e.g., GSD editing a `.specs/<feature>/tasks.md` checkbox is a
constitutional violation (that is spec-kit's job via
`/speckit-implement`).

- **GSD → spec-kit.** A GSD phase references one or more spec-kit
  `.specs/<feature>/` folders in its phase detail block
  (`ROADMAP.md` → "Phase N" → "Specs:"). The GSD planner reads the
  spec-kit `tasks.md` to size the phase; the GSD executor reads the same
  `tasks.md` to run tasks. GSD does NOT rewrite the spec-kit artifacts —
  it consumes them.
- **spec-kit → GSD.** When a spec-kit task completes, the executor
  updates `.planning/STATE.md` (decisions, blockers, deferred items)
  and, if a requirement is validated or invalidated, `.planning/
  PROJECT.md` (move between the Active / Validated / Out-of-Scope
  lists) so the next GSD command sees fresh state. spec-kit does NOT
  advance the roadmap pointer or mark phases complete — that is GSD's
  job (`/gsd-transition`, `/gsd-complete-milestone`).
- **spec-kit ↔ OpenSpec.** When a change is opted into OpenSpec, the
  spec-kit `.specs/<feature>/` folder is created as usual AND an
  `openspec/changes/<id>/` folder is created. The agent that runs
  `/opsx:apply` keeps both `tasks.md` files in sync. The spec-kit
  `tasks.md` is the verification-gate source of truth; the OpenSpec
  `tasks.md` is the change-tracking source of truth. On `/opsx:archive`,
  the delta specs fold into `openspec/specs/<domain>/` and the
  `openspec/changes/<id>/` folder moves to archive; the spec-kit
  `.specs/<feature>/` folder stays in place as the implementation
  record. If an OpenSpec change validates or invalidates a requirement,
  the executor updates `.planning/STATE.md` and `.planning/PROJECT.md`
  under the same ownership rule as spec-kit (see above).
- **GSD ↔ OpenSpec.** GSD does not directly read `openspec/`. The
  cross-reference flows through spec-kit: the spec-kit
  `requirements.md` "OpenSpec" subsection links the feature to its
  OpenSpec change id, and GSD reads that subsection when planning the
  phase. At milestone boundaries (`/gsd-complete-milestone`), the
  canonical `openspec/specs/<domain>/` is a candidate for re-validation
  alongside the legacy-`docs/` retirement schedule.
- **Constitution.** This file is the shared governance contract for all
  three. spec-kit reads it at `/speckit-plan` Phase 0; GSD re-validates
  it at every phase transition (`/gsd-transition`) and milestone
  (`/gsd-complete-milestone`); OpenSpec delta specs MUST NOT contradict
  it (a delta spec that violates a principle is a blocker, not a
  proposal).

### 5. Quality gates

The verification gate is the same in every mode and is owned by the
runtime target — the Docker Compose stack — not by any of the three
workflow tools. The tools produce the code; the gate verifies the code.

- **Per-task gate (spec-kit `/speckit-implement`).** A task is "done"
  only when: (1) the code lands on the branch, (2) the `tasks.md`
  checkbox is marked `[X]`, (3) the affected Docker services are rebuilt
  and observed healthy
  (`docker compose build api web && up -d --force-recreate api web`),
  and (4) the task's test slice passes (`dotnet test` for the affected
  test projects). Architecture tests are non-skippable for any change
  touching the tenant filter, an entity, or an endpoint.
- **Per-phase gate (GSD `/gsd-verify-work`, `/gsd-validate-phase`).**
  Before a phase transitions, the phase's UAT
  (`NN-UAT.md`) must be clean or its gaps closed via
  `/gsd-execute-phase <N> --gaps-only`, and the Nyquist validation
  (`VALIDATION.md`) must be green.
- **Per-milestone gate (GSD `/gsd-complete-milestone`).** Before a
  milestone is archived, `/gsd-audit-milestone` must pass; the
  legacy-`docs/` retirement schedule is re-checked; the constitution is
  re-validated against what shipped; and any concurrency-budget
  override rationales recorded during the milestone (Principle VII)
  are audited.
- **Per-PR gate.** Every PR MUST pass `dotnet test` for the affected
  test projects (Unit, Integration, Architecture) and the Docker rebuild
  smoke. A `.specs/.../review.md` (or a PR comment linking to one) is
  required for any change that touches a contract, an entity, or the
  tenant filter. Commit messages reference the spec-kit task ID
  (e.g., `T014: add resident filter to visits query`). No force-pushes
  to `main`/`master`.
- **Per-constitutional-change gate.** Any change to this file requires
  a Sync Impact Report at the top, a version bump per semantic
  versioning, and maintainer approval (see Governance). The agent that
  amends the constitution MUST propagate the change to
  `AGENTS.md` runtime guidance and flag any affected templates in the
  Sync Impact Report.

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
| Architecture Decision Records (ADRs) | **Two homes, two scopes:** GSD STATE.md holds the *decision and rationale*; spec-kit `review.md` / `analysis.md` files hold the *finding* that prompted the decision. | GSD + spec-kit | The legacy `docs/architecture/decisions/0001-…0004-…` ADRs were deleted in commit `b34b4d2` (2026-07-12) — kept for historical reference only in git history; new ADRs are recorded in GSD STATE.md and cross-referenced from the relevant `.specs/<feature>/` file. |
| Design system (tokens, components) | `.specs/2 - visual-design-system/` + `mockup/` | spec-kit | Tokens contract enforced by `.planning/REQUIREMENTS.md` trace rows and continuous task C.7. The legacy `docs/design-system/` and `docs/penpot/` assets were deleted in commit `b34b4d2` (2026-07-12); the spec under `.specs/2 - visual-design-system/` and `mockup/` are the canonical homes going forward. |
| Agent flow cheat sheet (GSD/spec-kit/OpenSpec lifecycle diagrams, command tables, seams, concurrency budget) | `docs/agent-flow-cheatsheet.md` | spec-kit | Quick-reference card; **not** a substitute for this constitution. The constitution wins on any conflict. Added 2026-07-12. |
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

- **Phase 0 (v1.1.0):** mark the legacy docs as deprecated. Update each
  legacy doc with a one-line redirect note pointing to its canonical
  home in `AGENTS.md` and `.planning/`. ✅ Done at v1.1.0 ratification.
- **Partial deletion (v1.3.0, commit `b34b4d2`, 2026-07-12):** the
  following were deleted ahead of the next milestone boundary because
  they were fully superseded and no longer load-bearing:
  `docs/architecture/decisions/0001…0004-*.md` (legacy ADRs — moved to
  GSD `STATE.md` + spec-kit `review.md`/`analysis.md`),
  `docs/design-system/` (design system usage guides — moved to
  `.specs/2 - visual-design-system/`),
  `docs/penpot/` (Penpot design system assets — moved to `mockup/`),
  `docs/migration/legacy-mapping.md` (legacy mapping — superseded by
  the Strangler Fig notes in `AGENTS.md` § 3.2). This deletion is
  reflected in the source-of-truth table above.
- **Next milestone boundary** (per `/gsd-complete-milestone`): delete
  the remaining legacy `docs/{index,project-overview,development-guide,
  deployment-guide,api-contracts,data-models,integration-architecture,
  source-tree-analysis,component-inventory,architecture}.md` files and
  `docs/project-scan-report.json`. These still carry one-line redirect
  notes at the top and are gated for deletion at the next milestone.
- **GSD phase transitions** are the trigger for re-validating this
  retirement schedule. If a phase boundary finds that a remaining
  legacy doc is still load-bearing, that doc is promoted to a
  canonical home (this section is amended) and the migration is
  paused.

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

**Version**: 1.3.0 | **Ratified**: 2026-07-12 | **Last Amended**: 2026-07-12
