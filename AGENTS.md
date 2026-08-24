# AGENTS.md — ControlEasy Reborn

> **Authoritative project guide.** Binding runtime document for every
> contributor and every agent. Constitutional authority:
> `.specify/memory/constitution.md` (v1.3.0, 2026-07-12).
> Where this file and the constitution disagree, the constitution wins.

---

## 0. ⛔ AGENT PRE-FLIGHT GATE — READ BEFORE ANYTHING ELSE

> **Binding on every agent (main or sub-agent) that writes, deletes,
> or moves any file in this repository.**

### 0.1 Worktree check (mandatory before any file change)

Before writing, editing, deleting, or moving **any** file in this
repository — source code, specs, planning docs, AGENTS.md itself,
anything — an agent MUST verify it is on a feature branch inside a
git worktree — **not** on `main` or `master`.

**Step 1 — Check the branch:**
```bash
git rev-parse --abbrev-ref HEAD
```

**Step 2 — If the output is `main` or `master`, create a worktree
automatically.** Do not touch any file until the worktree exists.

1. Derive a branch name from the task context (e.g. `feat/<slug>` or
   `fix/<slug>`). If the task context is ambiguous, ask the user for
   the branch name before proceeding.
2. Compute the worktree path and create it:
   ```bash
   # BRANCH = feat/<slug>  (slashes kept in branch name)
   # PATH   = ../ControlEasy.<branch-with-slashes-as-hyphens>
   BRANCH="feat/<slug>"
   WTPATH="../ControlEasy.$(echo $BRANCH | tr / -)"
   git worktree add "$WTPATH" -b "$BRANCH"
   ```
3. All subsequent file edits MUST target the new worktree path, not
   the main checkout. Announce to the user:
   ```
   ✅ Worktree created: <WTPATH>  (branch: <BRANCH>)
   Working from the worktree for all file changes.
   ```

**Step 3 — If on a feature branch**, confirm a worktree entry exists:
```bash
git worktree list
```
If the current directory appears in the output (any line other than
the main checkout path), the check passes. If the feature branch is
checked out directly in the main clone (not a dedicated worktree),
emit this warning — the agent may create a proper worktree with
`git worktree add` if stronger isolation is needed, but need not halt:

```
⚠ WORKTREE WARNING

Branch '<branch-name>' is checked out in the main clone, not a
dedicated worktree. Isolation is reduced.
Proceeding as requested.
```

### 0.2 Scope of the gate

- **Applies to:** speckit-implement, openspec-apply-change,
  bmad-quick-dev, bmad-dev-story, bmad-dev-auto, and **any** file
  edit — source code (`src/`, `tests/`, `docker/`), specs (`.specs/`),
  planning docs (`.planning/`), documentation (`docs/`, `AGENTS.md`),
  or any other file in the repository.
- **Does NOT apply to:** read-only work and research only. The moment
  any file is written, edited, deleted, or moved — regardless of
  whether it is code or documentation — the gate applies.
- **Sub-agents inherit the gate** and MUST run the check independently.

### 0.3 Override

User may waive for a single session by saying "I know I'm on main,
proceed anyway." Agent records the waiver in the task completion note
but does not persist it here.

---

## 1. Project overview

**ControlEasy Reborn** — condominium access-control web platform
(residents, visitors, vehicles, service providers, apartments, portaria).

**Target architecture:** ASP.NET Core 8 modular monolith + Angular 18
SPA + MySQL 8 + Docker Compose + JWT bearer. Nine feature modules
(Residents, Visits, Vehicles, ServiceProviders, Apartments, Security,
Tenants, Reports, Administration), each with Clean Architecture
(Domain → Application → Infrastructure → Api).

---

## 2. Workflow tooling (GSD · spec-kit · OpenSpec)

Three systems, non-overlapping responsibility (constitution Principle VI):

| System | Home | Owner | Writes to | Reads from |
|---|---|---|---|---|
| **GSD** | `.planning/` | `.agents/skills/gsd-*` | `.planning/` (roadmap, STATE, milestone archive) | `.specs/`, `openspec/` |
| **spec-kit** | `.specs/`, `.specify/` | `.agents/skills/speckit-*` | `.specs/<feature>/`, `.specify/`; updates `STATE.md` + `PROJECT.md` when state changes | `.planning/` |
| **OpenSpec** | `openspec/` | `.agent/skills/openspec-*` | `openspec/changes/<id>/`; syncs spec-kit `tasks.md` during `/opsx:apply` | `.planning/`, `.specs/` |

**OpenSpec is opt-in.** Only the user decides to create an
`openspec/changes/<id>/` folder. Most changes are spec-kit only.

**Auto-tool selection (no explicit invocation required).** Agents MUST
read the user's intent and automatically invoke the right tool —
the user does not need to name BMAD, GSD, spec-kit, or OpenSpec
explicitly. Use this decision table:

| User intent | Tool to invoke |
|---|---|
| Implement a feature, bug fix, or refactor | **spec-kit** → `speckit-specify` → `speckit-plan` → `speckit-tasks` → `speckit-implement` |
| Review, analyse, or quality-check code | **BMAD** → `bmad-code-review` / `bmad-review-adversarial-general` / `bmad-review-edge-case-hunter` |
| Plan phases, update roadmap, transition milestones | **GSD** → appropriate `gsd-*` skill |
| Propose or implement a structured change with delta-specs | **OpenSpec** → `openspec-propose` / `openspec-apply-change` (user opt-in only) |
| Write or update documentation | **BMAD** → `bmad-agent-tech-writer` |
| Design UX / UI | **BMAD** → `bmad-ux` / `bmad-agent-ux-designer` |
| Architecture decision | **BMAD** → `bmad-architecture` or `bmad-agent-architect` |

When the intent is ambiguous, pick the most likely tool and announce
the choice: *"I'll use spec-kit to implement this — let me know if
you meant something else."* Do not wait for the user to spell out
the skill name.

**Sub-agents** may cross tool seams but MUST write only to their own
tool's directory. Sub-agents run the worktree gate independently.

**Concurrency budget:** max 3 sub-agents in flight per level (1 main +
3 subs). Exceeding the budget requires an explicit rationale recorded
in `proposal.md`, `design.md`, or `tasks.md` for the wave in question.
See `docs/agent-flow-cheatsheet.md` for lifecycle diagrams and command tables.

---

## 3. Target stack

| Layer | Choice | Hard constraints |
|---|---|---|
| Frontend | Angular 18+ SPA, standalone, signals, TS strict | DTOs from OpenAPI (`ng-openapi-gen`); no shared domain models |
| Backend | ASP.NET Core 8, C# 12, minimal API endpoints | No MVC controllers; no MediatR; no EF Core |
| Data access | DBTools 1.4.3 (`Linq<TModel>`, `IAsyncSqlClient`) | LINQ-first; raw SQL only for stored procs; no EF Core (ADR 0002) |
| Database | MySQL 8, provider-agnostic | No hard-coded credentials — env vars / Docker secrets |
| Auth | JWT bearer + BCrypt (cost ≥ 11) | Secrets never in `App.config` |
| Runtime | Docker Compose: `api`, `web`, `db`, `reverse-proxy`, `adminer`, `seq` | Multi-stage Dockerfiles; Compose v2 |
| Mapping | Mapster or AutoMapper | |
| Validation | FluentValidation | |
| Testing | xUnit + FluentAssertions + NSubstitute + Testcontainers.MySql + Playwright | Karma + Jasmine for Angular unit |
| Error handling | `ProblemDetails` (RFC 7807); Serilog → Console(JSON) + Seq | Domain exceptions: `NotFoundException`, `ValidationException`, `ConflictException` |

**Coding conventions:** `PascalCase` types/methods, `_camelCase` private
fields, `ALL_CAPS` const, file-scoped namespaces, `sealed` classes by
default, nullable enabled, warnings as errors. REST routes: kebab-case,
plural nouns (`/api/v1/residents`). Module layout:
`Modules/<Feature>/{Domain,Application,Infrastructure,Api}/`. All
tenant-scoped reads/writes via `ITenantAwareLinqFactory`. Full
conventions: `.planning/codebase/CONVENTIONS.md`.

---

## 4. Local development

Canonical mode: **devcontainer + per-feature worktree** (DooD — Docker-
out-of-Docker). Full setup guide: `docs/dev-setup.md`.

| Mode | When to use |
|---|---|
| **D — Devcontainer + worktree** (canonical) | All new work. Open repo in VS Code / Cursor / JetBrains Gateway |
| A — Full Docker on host | Devcontainer unavailable (plain terminal) |
| B — Hybrid (host tooling + Docker DB) | Fast inner loops |
| C — Demo overlay | `docker-compose.demo.yml` over A or D; **never in production** |

**Worktree naming convention:**

| Item | Pattern | Example |
|---|---|---|
| Branch | `feat/<spec-id>-<slug>` or `fix/<spec-id>-<slug>` | `feat/dashboard-live-stats` |
| Worktree path | `../ControlEasy.<branch-with-slashes>` | `../ControlEasy.feat-dashboard-live-stats` |
| Compose project | `ce-<branch-hyphens>` | `ce-feat-dashboard-live-stats` |
| Traefik host | `ce-<branch>.localhost` | `ce-feat-dashboard-live-stats.localhost` |
| Host port | `18080 + (worktree-index × 10)` | `18090` |
| DB volume | `ce-<branch>-mysql-data` | `ce-feat-dashboard-live-stats-mysql-data` |

Scripts: `scripts/worktree-up.ps1` / `.sh` (create),
`scripts/worktree-down.ps1` / `.sh` (tear down).

---

## 5. Post-task verification

After **every implementation task**, rebuild and verify before marking
done. Do **not** run `dotnet` on the host — use the devcontainer shell.

```bash
# Standard (main checkout or worktree — substitute project name for worktrees)
PROJ="ce-$(git rev-parse --abbrev-ref HEAD | tr / -)"
docker compose -p "$PROJ" -f docker/docker-compose.yml build api web
docker compose -p "$PROJ" -f docker/docker-compose.yml up -d --force-recreate api web
dotnet test tests/ControlEasyReborn.UnitTests
dotnet test tests/ControlEasyReborn.IntegrationTests
dotnet test tests/ControlEasyReborn.ArchitectureTests

# Demo overlay (tasks touching .specs/4 - demo-mode/ only)
docker compose -p "$PROJ" -f docker/docker-compose.yml -f docker/docker-compose.demo.yml build api web

# Schema/seed change — reset volumes first
docker compose -p "$PROJ" -f docker/docker-compose.yml down -v
docker compose -p "$PROJ" -f docker/docker-compose.yml up -d --build
```

A task is **not closed** until the rebuilt stack starts healthy and all
three test projects pass.

### 5.1 Pre-commit adversarial review (mandatory before any `git commit`)

**No agent may run `git commit` until both gates below pass in order.**

**Gate 1 — Build + tests green** (see commands above).

**Gate 2 — Adversarial review subagent.** After Gate 1 passes, invoke
one of the following skills as a subagent against the diff/change set:

| Situation | Skill to invoke as subagent |
|---|---|
| General feature or fix | `bmad-code-review` |
| Security-sensitive change, auth, data access | `bmad-review-adversarial-general` |
| Algorithm-heavy or branching logic | `bmad-review-edge-case-hunter` |

The subagent reviews the code and returns a findings report. The
implementing agent MUST:

1. **Block on any `CRITICAL` or `HIGH` finding** — fix it, re-run
   Gate 1, and re-invoke the review subagent before committing.
2. **Annotate `MEDIUM` findings** in the commit message or a
   follow-up task in `tasks.md` — they do not block commit.
3. **Log `LOW` / informational findings** in `tasks.md` as tech-debt
   items — they do not block commit.

Only after the review subagent returns with zero `CRITICAL`/`HIGH`
blockers may the agent run `git commit`.

---

## 6. Spec structure (per feature)

```text
.specs/
└── <feature-or-bug-fix>/
    ├── requirements.md   # User stories UC[n]
    ├── design.md         # Feature design (use for features)
    ├── bugfix.md         # Bug analysis (use for bug fixes)
    ├── review.md         # Branch review findings
    ├── analysis.md       # General code analysis
    ├── docplan.md        # New documentation plan
    ├── docchange.md      # Existing documentation update plan
    └── tasks.md          # Implementation checklist + Task Dependency Graph
```

`tasks.md` always includes a dependency graph (`{ "waves": [...] }`).
Tasks in the same wave are parallel; a wave starts only when all tasks
in the prior wave are complete.

Artifacts are produced by: GSD skills (`gsd-*`), BMAD skills
(`bmad-*`), spec-kit skills (`speckit-*`). OpenSpec opt-in: only the
user creates `openspec/changes/<id>/` — no agent does this
autonomously.

---

## 7. Documentation source-of-truth map

| Topic | Canonical home |
|---|---|
| Project overview | This file § 1 + `.planning/PROJECT.md` |
| Architecture | This file § 3 + `.planning/codebase/ARCHITECTURE.md` |
| Modules | `.planning/codebase/STRUCTURE.md` |
| Local dev / worktrees | `docs/dev-setup.md` |
| Post-task verification | This file § 5 |
| Spec structure | This file § 6 |
| Workflow tooling / lifecycle | This file § 2 + `docs/agent-flow-cheatsheet.md` |
| Codebase maps | `.planning/codebase/*.md` |
| Per-feature spec | `.specs/<feature>/{requirements,design,tasks,...}.md` |
| Constitution | `.specify/memory/constitution.md` |
| Roadmap / requirements / state | `.planning/{ROADMAP,REQUIREMENTS,STATE}.md` |
| ADRs | `GSD STATE.md` (decision + rationale); `docs/architecture/decisions/` (historical) |
| Design system | `docs/design-system/`, `.specs/2 - visual-design-system/` |
| Operator docs | `docs/getting-started.md`, `docs/demo-mode.md` |

**Do not** update legacy `docs/` files. Update the canonical home
listed above. Legacy files carry redirect notes; retirement is gated
on `/gsd-complete-milestone`.

---

## 8. Build / Run / Test (quick reference)

```bash
# Stack
docker compose -f docker/docker-compose.yml up -d --build

# .NET
dotnet build src/ControlEasyReborn.sln
dotnet test  src/ControlEasyReborn.sln
dotnet test tests/ControlEasyReborn.UnitTests --filter "FullyQualifiedName~CreateApartmentHandler"

# Angular
cd src/Web/ControlEasyReborn.Web
npm test -- --no-watch --browsers=ChromeHeadless

# Playwright E2E
npm run e2e:install  # one-time
E2E_BASE_URL=https://ce-<branch>.localhost:<port> npm run e2e
```

---

## 9. Style guardrails (enforced)

- **C#:** file-scoped namespaces, `sealed` classes, `_camelCase` private
  fields, `PascalCase` types/methods, nullable enabled, warnings as errors.
- **Angular:** `standalone: true`, `OnPush`, signals API, `ce-` prefix,
  kebab-case selectors, ESLint `@angular-eslect/recommended`, Prettier
  single quotes + 120 cols.
- **No MVC controllers.** Minimal API endpoint classes only.
- **No EF Core.** DBTools only (ADR 0002).
- **No MediatR.** Handlers registered as scoped services directly.
- **All tenant-scoped reads/writes via `ITenantAwareLinqFactory`.**

---

*Constitutional authority: `.specify/memory/constitution.md` v1.3.0.
GSD owns `.planning/`. spec-kit owns `.specs/` and `.specify/`.
OpenSpec is opt-in at the user's discretion.*
