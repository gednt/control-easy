# Agent Flow Cheat Sheet — GSD · spec-kit · OpenSpec

> **Canonical home (dev/operator reference):** this file.
> **Governance anchor:** `.specify/memory/constitution.md` v1.5.0
> (Principle VI — "Workflow Tooling").
> **Runtime anchor:** `AGENTS.md` § 2 (Workflow tooling).
>
> This document is a quick-reference card for the three workflow
> systems that run ControlEasy Reborn. It is **not** a substitute
> for the constitution; the constitution wins on any conflict.
> Source-of-truth map: see the "Documentation Systems and Source
> of Truth" table in the constitution.

---

## 0. The mental model in 30 seconds

| | GSD | spec-kit | OpenSpec |
|---|---|---|---|
| **Owns** | The roadmap & project state | The per-feature spec | The change-tracking diff |
| **Home directory** | `.planning/` | `.specs/<feature>/`, `.specify/` | `openspec/` |
| **Command prefix** | `/gsd-*` | `/speckit-*` | `/opsx:*` |
| **Mandatory?** | Yes — every phase runs through it | Yes — every feature/bug starts here | **No — opt-in, per change** |
| **Cadence** | Per phase (14 phases) | Per feature | Per opted change |
| **Output is** | Plans, summaries, UAT, validation | `spec.md`, `plan.md`, `tasks.md` | `proposal.md`, delta specs, `tasks.md` |
| **Skill home** | `.claude/skills/gsd-*`, `.agents/skills/gsd-*` | `.agents/skills/speckit-*` | `.claude/skills/openspec-*` |
| **Single sentence** | "Where are we on the 14-phase rollout?" | "What does this feature look like end-to-end?" | "Show me the diff against the canonical specs." |

The constitution's **ownership principle** (Principle VI § 4 Seams) is
the one rule that keeps the three from drifting:

- **GSD owns `.planning/`.** Other tools may **write** to it (to
  record decisions, blockers, validated/invalidated requirements)
  but must not update `ROADMAP.md`, the phase pointer, or the
  milestone archive — those are GSD's.
- **spec-kit owns `.specs/<feature>/` and `.specify/`.** No one
  rewrites a spec-kit `tasks.md` checkbox except
  `/speckit-implement`.
- **OpenSpec owns `openspec/`.** No agent creates
  `openspec/changes/<id>/` without explicit user instruction.

A canonical test for "who owns this artifact?": if it lives in
`.planning/`, ask GSD; if in `.specs/` or `.specify/`, ask spec-kit;
if in `openspec/`, ask OpenSpec. Three homes, three owners, zero
overlap.

---

## 1. GSD — planner and roadmap owner (`.planning/`)

### 1.1 What GSD owns (and what it does not)

| Owns | Does NOT own |
|---|---|
| `.planning/PROJECT.md` (validated / active / out-of-scope) | `.specs/<feature>/` artifacts (read-only consumer) |
| `.planning/REQUIREMENTS.md` | `.specify/memory/constitution.md` (constitution is spec-kit's) |
| `.planning/ROADMAP.md` (14 phases) | The spec-kit `tasks.md` checkbox (spec-kit's job) |
| `.planning/STATE.md` (live state, decisions, blockers) | Per-PR review (that's a code review concern) |
| `.planning/codebase/*.md` (7 maps: STACK, STRUCTURE, ARCHITECTURE, CONVENTIONS, CONCERNS, INTEGRATIONS, TESTING) | — |
| `.planning/phases/<NN>-<slug>/` (CONTEXT, RESEARCH, NN-PLAN, NN-SUMMARY, NN-VERIFICATION, VALIDATION, NN-UAT) | — |

### 1.2 The phase lifecycle

```
/gsd-discuss-phase N
        │
        ▼
/gsd-plan-phase N ── spawns ──▶ gsd-phase-researcher (RESEARCH.md)
        │                            └─▶ gsd-planner (NN-PLAN.md)
        │                            └─▶ gsd-plan-checker (loop until pass)
        ▼
/gsd-execute-phase N ── wave-based parallel execution ──▶ gsd-executor
        │                                                  (atomic commits, per task)
        ▼
/gsd-verify-work N ── conversational UAT ──▶ NN-UAT.md
        │                                       (gaps → /gsd-execute-phase N --gaps-only)
        ▼
/gsd-validate-phase N ── Nyquist test audit ──▶ VALIDATION.md + generated tests
        │
        ▼
/gsd-transition ── advance roadmap pointer; re-validate PROJECT, REQUIREMENTS, constitution
        │
        ▼ (at milestone boundaries)
/gsd-complete-milestone <version> ── archive roadmap + requirements,
                                      update PROJECT.md, tag release,
                                      trigger legacy-docs retirement
```

### 1.3 GSD command cheat sheet

| Command | When | Output artifact | Sub-agents it spawns |
|---|---|---|---|
| `/gsd-discuss-phase N` | Before planning phase N | `CONTEXT.md` | — |
| `/gsd-plan-phase N` | Before executing phase N | `RESEARCH.md`, `NN-PLAN.md` | `gsd-phase-researcher`, `gsd-planner`, `gsd-plan-checker` |
| `/gsd-execute-phase N` | When the plan is approved | `NN-SUMMARY.md`, commits | `gsd-executor` (wave-based) |
| `/gsd-execute-phase N --gaps-only` | When UAT found gaps | closes the gaps | `gsd-executor` |
| `/gsd-verify-work N` | After execution | `NN-UAT.md` | conversational; no sub-agent |
| `/gsd-validate-phase N` | After UAT passes | `VALIDATION.md` | `gsd-nyquist-auditor` |
| `/gsd-transition` | When the phase is complete | updated phase pointer | — |
| `/gsd-complete-milestone <v>` | At a milestone boundary | `.planning/milestones/` archive | legacy-docs retirement |
| `/gsd-audit-milestone` | Before archive | audit report | — |
| `/gsd-quick`, `/gsd-fast`, `/gsd-autonomous`, `/gsd-help` | Ad-hoc driver commands | varies | varies |

### 1.4 The wave structure (within a plan)

`gsd-executor` runs `NN-PLAN.md` task-by-task in waves, where a
"wave" is a set of tasks with no inter-dependencies that can
fan out in parallel. The GSD concurrency budget for sub-agents
(**§ 2.5 of `AGENTS.md`**) applies here too: at most 3
sub-agents in flight concurrently, on top of the main
orchestrator. Wave boundaries are declared in
`.specs/<feature>/tasks.md` as a `Task Dependency Graph` JSON
block (`{"waves": [...]}`).

### 1.5 When to invoke GSD as a sub-agent

Per `AGENTS.md` § 2.4, any agent — including a main
orchestrator or an in-flight spec-kit or OpenSpec sub-agent —
MAY invoke a `/gsd-*` skill when the work crosses the seam:

- A spec-kit `speckit-implement` task that needs a fresh
  `.planning/codebase/ARCHITECTURE.md` reading before it
  can decide on a structure.
- An OpenSpec `/opsx:apply` that needs to know the current
  phase (`STATE.md`) to know which phase's UAT gate to respect.
- A BMAD orchestrator that needs the live roadmap pointer.

GSD does **not** rewrite the caller's artifacts; the caller's
workflow tool's ownership rule wins.

---

## 2. spec-kit — per-feature spec owner (`.specs/`, `.specify/`)

### 2.1 The 4-command pipeline

```
/speckit-specify "<description>"
        │   creates .specs/<feature>/spec.md
        │   generates .specs/<feature>/checklists/
        │   resolves up to 3 [NEEDS CLARIFICATION] markers
        ▼
[ gate: review spec ]
        │
        ▼
/speckit-plan
        │   fills plan.md from .specify/templates/plan-template.md
        │   reads .specify/memory/constitution.md (Phase 0)
        │   generates research.md, data-model.md, contracts/, quickstart.md
        │   re-evaluates "Constitution Check" after design
        ▼
[ gate: review plan ]
        │
        ▼
/speckit-tasks
        │   fills tasks.md from .specify/templates/tasks-template.md
        │   organized by user story (Setup → Foundational → USn → Polish)
        │   [P] parallel markers, [USn] story labels, exact file paths
        ▼
/speckit-implement
        │   executes tasks.md phase-by-phase
        │   marks [X] as each task lands
        │   respects [P] parallelism and TDD ordering
        ▼
[ tasks.md all [X] ] → ready for /gsd-verify-work (if part of a GSD phase)
```

### 2.2 Auxiliary commands

| Command | When | Output |
|---|---|---|
| `/speckit-clarify` | Spec still has `[NEEDS CLARIFICATION]` markers | resolves them |
| `/speckit-checklist` | Need a domain checklist (UX, security, test) | `<domain>-checklist.md` |
| `/speckit-converge` | After implementation, consolidate drift | convergence report |
| `/speckit-analyze` | Periodic code analysis | `.specs/<feature>/analysis.md` |
| `/speckit-constitution` | Amend `.specify/memory/constitution.md` | new version + Sync Impact Report |

### 2.3 The per-feature folder

```
.specs/<feature>/
├── spec.md           # /speckit-specify output (user stories, FRs, success criteria)
├── plan.md           # /speckit-plan output (Constitution Check, data model, contracts)
├── tasks.md          # /speckit-tasks output (phased, [P] [USn] formatted)
├── checklists/       # /speckit-checklist output (domain quality)
├── design.md         # feature design (overview, glossary, architecture, diagrams)
├── bugfix.md         # only for bug-fix specs
├── review.md         # produced by GSD code-review agents
├── analysis.md       # produced by /speckit-analyze or GSD
├── docplan.md        # new documentation set plan (Doc agent)
├── docchange.md      # existing-documentation update plan (Doc agent)
├── orchestration.md  # produced by BMAD orchestrator (when used)
└── requirements.md   # legacy: see .specs/1 - modernization-roadmap/
```

`requirements.md` and `design.md` predate the spec-kit pipeline in
this repo (legacy from before the spec-kit adoption). New
specs follow spec-kit's native shape: `spec.md` + `plan.md` +
`tasks.md`. The two coexist; the agent picks the shape that
matches the spec's lineage.

### 2.4 Task format

```
[ID] [P?] [Story] Description with exact file paths
```

- **`[P]`** — can run in parallel (different files, no
  dependencies).
- **`[Story]`** — user-story label (`US1`, `US2`, `US3`).
- **Phases** are top-level `## Phase N: <Name>` sections,
  typically: Setup → Foundational → US1 (P1) → US2 (P2) → … →
  Polish.
- **Wave graph** at the bottom of `tasks.md` (in `.specs/`
  here) or in `plan.md` (in spec-kit's native shape) is a
  JSON block:

  ```json
  { "waves": [
      { "wave": 1, "tasks": ["T001", "T002"] },
      { "wave": 2, "tasks": ["T003"] }
  ] }
  ```

### 2.5 The Constitution Check

`/speckit-plan` reads `.specify/memory/constitution.md` at
Phase 0 and fills the `plan.md` "Constitution Check" section
from it. Any principle violation that cannot be justified in
`plan.md` "Complexity Tracking" **blocks planning**. The
nine principles of the constitution v1.5.0 are the binding
contract; the agent does not interpret them — it cites them. (v1.5.0
adds Principle IX on BMAD as the fourth workflow tool; v1.4.0 added
Principle VIII on host-OS / shell-aware command execution; v1.3.0
added Principle VII on sub-agent orchestration.)

### 2.6 spec-kit's relationship to GSD and OpenSpec

- **spec-kit → GSD.** When a `/speckit-implement` task lands,
  the executor updates `.planning/STATE.md` (decisions,
  blockers, deferred items) and, if a requirement was
  validated or invalidated, `.planning/PROJECT.md` (move
  between Active / Validated / Out-of-Scope). The executor
  does **not** advance the roadmap pointer or mark phases
  complete — that's GSD's job.
- **spec-kit ↔ OpenSpec.** When a change is opted into
  OpenSpec, the spec-kit `.specs/<feature>/` folder is
  created as usual **and** an `openspec/changes/<id>/` folder
  is created. The agent that runs `/opsx:apply` keeps both
  `tasks.md` files in sync. **The spec-kit `tasks.md` is the
  one the verification gate reads.**

### 2.7 When to invoke spec-kit as a sub-agent

- A GSD `gsd-plan-phase` orchestrator that needs a fresh
  per-feature spec drafted before it can size the phase.
- An OpenSpec `/opsx:apply` that needs spec-kit's
  review/analysis artifacts to inform the delta spec.
- A BMAD orchestrator running a story that crosses multiple
  features and wants one spec-kit per feature.

---

## 3. OpenSpec — opt-in change-tracking layer (`openspec/`)

### 3.1 The opt-in rule

> OpenSpec is opt-in, at the discretion of the user, on a
> per-change basis. It is NOT the default workflow.
> Most changes go through spec-kit alone; only some go
> through OpenSpec as well.
>
> — `AGENTS.md` § 2.3

Concretely: an `openspec/changes/<id>/` folder is created
**only** when the user explicitly invokes `/opsx:new` (or
`/opsx:propose`) on a change. No agent creates that folder
on its own initiative.

### 3.2 The change lifecycle

```
[ USER DECIDES TO OPT IN ]
        │
        ▼
/opsx:propose "<change-name>"  (or /opsx:new "<name>")
        │   creates openspec/changes/<id>/
        │   generates proposal.md, design.md, tasks.md
        ▼
[ artifacts may be augmented: specs/<domain>/spec.md with delta sections ]
        │
        ▼
/opsx:apply <id>
        │   reads openspec status, openspec instructions apply
        │   implements against proposal + delta specs + design + tasks
        │   marks tasks [- [x]] as they land
        │   KEEPS openspec/tasks.md AND .specs/<feature>/tasks.md IN SYNC
        ▼
[ all tasks [x] ]
        │
        ▼
/opsx:archive <id>
        │   folds delta specs into canonical openspec/specs/<domain>/
        │   moves openspec/changes/<id>/ to openspec/changes/archive/YYYY-MM-DD-<id>/
        ▼
[ change archived; spec-kit .specs/<feature>/ remains as implementation record ]
```

### 3.3 The change folder

```
openspec/changes/<id>/
├── .openspec.yaml       # change metadata
├── proposal.md          # Why, What, Capabilities, Impact
├── design.md            # How (architecture, decisions)
├── tasks.md             # Implementation tasks (the change-tracking source of truth)
└── specs/
    └── <domain>/
        └── spec.md      # Delta spec: ## ADDED, ## MODIFIED, ## REMOVED sections
```

The proposal.md "Why" section cross-references the spec-kit
`.specs/<feature>/` folder (the two-track record). The
**delta spec is a diff against the canonical
`openspec/specs/<domain>/spec.md`**, not a rewrite — that's
the whole point of OpenSpec's proposal/scenario model.

### 3.4 `/opsx:propose` and `/opsx:apply` at a glance

#### `/opsx:propose` (creates the change)

| Step | What it does |
|---|---|
| 1 | Derives a kebab-case name from the description |
| 2 | `openspec new change "<name>"` → scaffolds `openspec/changes/<id>/` |
| 3 | `openspec status --change "<name>" --json` → returns artifact dependency graph |
| 4 | For each `ready` artifact, runs `openspec instructions <id> --change "<name>" --json` to get template + context, then writes the artifact |
| 5 | Stops when every `applyRequires` artifact is `done` |

#### `/opsx:apply` (implements the change)

| Step | What it does |
|---|---|
| 1 | Selects the change (infer / auto / AskUserQuestion) |
| 2 | `openspec status --change "<name>" --json` → schema, paths, progress |
| 3 | `openspec instructions apply --change "<name>" --json` → contextFiles + task list + dynamic instruction |
| 4 | Reads every context file (proposal, specs, design, tasks) |
| 5 | For each pending task: implement, mark `- [x]`, continue |
| 6 | Pause on ambiguity / design issue / blocker |
| 7 | On done: suggest archive |

#### `/opsx:archive` (folds the change)

| Step | What it does |
|---|---|
| 1 | Selects the change (always AskUserQuestion if not provided) |
| 2 | Checks artifact + task completion status (warns, does not block) |
| 3 | Assesses delta-spec sync state (adds, modifications, removals, renames) |
| 4 | `mv <changeRoot> <changesDir>/archive/YYYY-MM-DD-<id>/` |
| 5 | Preserves `.openspec.yaml` (it moves with the directory) |

### 3.5 OpenSpec's relationship to spec-kit and GSD

- **OpenSpec ↔ spec-kit.** Two tracks: the spec-kit
  `.specs/<feature>/` folder is the implementation record;
  the OpenSpec `openspec/changes/<id>/` folder is the
  change-tracking diff. The agent that runs `/opsx:apply`
  keeps both `tasks.md` files in sync. **The spec-kit
  `tasks.md` is the verification-gate source of truth** (see
  `AGENTS.md` § 5).
- **OpenSpec → GSD.** Cross-references flow through
  spec-kit: the spec-kit `requirements.md` "OpenSpec"
  subsection links the feature to its OpenSpec change id,
  and GSD reads that subsection when planning the phase.
  At milestone boundaries (`/gsd-complete-milestone`), the
  canonical `openspec/specs/<domain>/` is a candidate for
  re-validation alongside the legacy-`docs/` retirement
  schedule.
- **Constitution.** OpenSpec delta specs MUST NOT contradict
  `.specify/memory/constitution.md`. A delta that violates a
  principle is a blocker, not a proposal.

### 3.6 When to invoke OpenSpec as a sub-agent

- A main agent implementing a `/speckit-implement` task
  that the user has opted into OpenSpec: spawn
  `/opsx:apply` as a sub-agent to drive the change-tracking
  lifecycle and keep the two `tasks.md` files in sync.
- A GSD `gsd-codebase-mapper` that needs a snapshot of
  in-flight change deltas before mapping a code area.
- A BMAD orchestrator that runs `/opsx:propose` on a
  multi-agent story and wants the artifacts drafted
  concurrently.

---

## 4. Sub-agent invocation — `AGENTS.md` § 2.4

### 4.1 The rule

> Any agent — including a main orchestrator and any in-flight
> sub-agent — MAY invoke any of GSD, spec-kit, or OpenSpec
> skills as a sub-agent when the work at hand crosses the
> seam between the three tools.

The ownership rules in § 1.1 / § 2.1 / § 3 are preserved.
Concretely:

- A sub-agent **writes only to the directory owned by its own
  workflow tool** (GSD → `.planning/`, spec-kit →
  `.specs/` and `.specify/`, OpenSpec → `openspec/`).
- A sub-agent **reads from the other tools' directories** but
  does not rewrite them.
- The sub-agent's parent is responsible for catching
  ownership violations before they land.

### 4.2 Worked examples

| Parent | Sub-agent | Why |
|---|---|---|
| Main agent running `/opsx:apply` | `gsd-codebase-mapper` | The OpenSpec apply needs a fresh `.planning/codebase/` map to make a design decision. |
| GSD `gsd-plan-phase` orchestrator | `speckit-specify` | The phase introduces a new feature that needs a spec drafted before planning. |
| spec-kit `speckit-implement` task that the user has opted into OpenSpec | `/opsx:apply` | Drive the change-tracking lifecycle alongside the spec-kit implementation, keeping both `tasks.md` files in sync. |
| BMAD orchestrator running a multi-feature story | `speckit-specify` (per feature) | One spec per feature, drafted concurrently. |

### 4.3 Anti-patterns (these are violations)

- A GSD sub-agent editing a `.specs/<feature>/tasks.md`
  checkbox — that's `/speckit-implement`'s job.
- A spec-kit sub-agent editing `.planning/ROADMAP.md` or
  advancing the phase pointer — that's GSD's.
- An OpenSpec sub-agent creating `openspec/changes/<id>/`
  without the user invoking `/opsx:new` / `/opsx:propose` —
  OpenSpec is opt-in, period.
- A sub-agent of **any** kind rewriting a file owned by a
  different workflow tool without going through that tool's
  command.

---

## 5. Concurrency budget — `AGENTS.md` § 2.5

### 5.1 The default

> Up to **3 sub-agents in flight concurrently, in addition
> to the main orchestrator.** Total in-flight count per
> level: 4 (1 main + 3 subs).

### 5.2 The per-level rule

The budget is **per agent, not per workflow tool**. A main
agent running an OpenSpec apply that fans out to GSD and
spec-kit sub-agents is still capped at three concurrent
sub-agents. A sub-agent that itself spawns sub-agents gets
**its own independent budget of 3** — a sub-agent may not
"consume" its parent's budget, and deeper nesting is
permitted with the budget applied at every level.

### 5.3 The override path

Exceeding the budget (4+ sub-agents in flight at any level)
is permitted only when the originating artifact records an
explicit rationale:

- `proposal.md` "Why" section (for overruns anticipated
  before implementation), or
- `design.md` "Decisions" section (for overruns anticipated
  during design), or
- `tasks.md` task description (for the wave in question — the
  most common path for apply-time overruns).

The rationale is reviewed at the next
`/gsd-complete-milestone` boundary, alongside the rest of
the work the change has produced.

No environment variable, no config file, no CLI flag
overrides the budget. The override is qualitative and lives
next to the work it justifies, so the next milestone audit
sees it without a separate scan.

### 5.4 Quick check

> "Am I about to fan out to ≥ 4 sub-agents at once?"
> - **No** → proceed.
> - **Yes, and no rationale is recorded** → split into
>   multiple waves or record the rationale in the relevant
>   artifact first.
> - **Yes, and the rationale is already recorded** → proceed;
>   flag it to the next milestone audit.

---

## 6. The seams — how the three compose

### 6.1 The single page diagram

```
            ┌─────────────────────────────────────────────┐
            │                  USER                       │
            │   (opt-in decision for OpenSpec)            │
            └────────────────────┬────────────────────────┘
                                 │
            ┌────────────────────▼────────────────────────┐
            │                GSD (always on)              │
            │  .planning/ — PROJECT, REQUIREMENTS,        │
            │  ROADMAP, STATE, codebase/, phases/         │
            └───────┬─────────────────────────┬──────────┘
                    │ reads & plans           │ archives
                    │ against                  │ (milestone)
                    │                         │
   ┌────────────────▼───────────┐  ┌─────────▼──────────────┐
   │         spec-kit            │  │      OpenSpec          │
   │ .specs/<feature>/           │  │ openspec/changes/<id>/│
   │   spec, plan, tasks         │◀─┤  proposal, design,    │
   │ .specify/memory/            │  │  tasks, delta specs   │
   │   constitution.md           │  │ openspec/specs/       │
   │                             │  │  canonical (folded)   │
   └────────────────┬────────────┘  └─────────┬─────────────┘
                    │ writes to .planning/      │ writes to .planning/
                    │ (STATE, PROJECT)          │ (STATE, PROJECT)
                    │                           │
                    └─────────┬─────────────────┘
                              │
                              ▼
            ┌─────────────────────────────────────────────┐
            │      DOCKER COMPOSE STACK (runtime)         │
            │  api · web · db · reverse-proxy · adminer   │
            │  Verification gate (AGENTS.md § 5):         │
            │  docker compose build api web &&            │
            │  docker compose up -d --force-recreate      │
            │  && dotnet test                            │
            └─────────────────────────────────────────────┘
```

### 6.2 The handoff rules (one-liners)

| Direction | When | Artifact that moves | Who updates `.planning/STATE.md` |
|---|---|---|---|
| **GSD → spec-kit** | Phase references a feature | spec-kit `tasks.md` is read; GSD does not rewrite it | GSD |
| **spec-kit → GSD** | A spec-kit task lands | (none — read-only) | spec-kit (writes decision/blocker) |
| **spec-kit → GSD** | Requirement validated/invalidated | (none) | spec-kit (moves PROJECT.md list) |
| **spec-kit ↔ OpenSpec** | Change opted into OpenSpec | both `tasks.md` files kept in sync | spec-kit (writes decision) |
| **OpenSpec → GSD** | Change validates/invalidates a requirement | (none) | OpenSpec (writes decision; moves PROJECT.md list) |
| **GSD ↔ OpenSpec** | Phase references an OpenSpec change | (none — read via spec-kit) | (no handoff; cross-refs only) |

### 6.3 The verification gate (every mode, every change)

The verification gate is owned by the **runtime target** (the
Docker Compose stack), not by any of the three workflow
tools. The tools produce the code; the gate verifies the code.

```bash
# Per-task gate (after every /speckit-implement or /opsx:apply task)
docker compose -f docker/docker-compose.yml build api web
docker compose -f docker/docker-compose.yml up -d --force-recreate api web
dotnet test src/ControlEasyReborn.sln
# (slice to the affected test projects at minimum)
```

No task is "done" until the rebuilt stack starts healthy and
the task's `dotnet test` slice passes against the running
containers. `STATE.md` is updated by the agent that finished
the work.

---

## 7. Common day-to-day questions

> **"I want to add a new feature. Where do I start?"**
> Run `/speckit-specify "<description>"` — the four-command
> pipeline is the default. OpenSpec is not invoked unless
> you explicitly want change-tracking on top of it.

> **"I want to fix a bug. Where do I start?"**
> Same pipeline (`/speckit-specify` → `…` → `tasks.md`),
> but `tasks.md` is built around a `bugfix.md` analysis
> rather than a `spec.md` user-story set.

> **"I want a formal change-tracking record with delta
> specs."**
> Opt the change into OpenSpec by running
> `/opsx:propose "<name>"` (or `/opsx:new`). Then run
> `/speckit-specify` on the same change so the spec-kit
> implementation record exists alongside. `/opsx:apply`
> keeps both `tasks.md` files in sync.

> **"I'm mid-phase. Can I update the spec?"**
> Yes — `requirements.md` and `design.md` are living
> documents. A change is `docchange.md` (for an existing
> doc) or `docplan.md` (for a new doc). Update
> `.planning/STATE.md` with the change.

> **"I have a 4-agent parallel plan. Is that allowed?"**
> Only if the originating artifact records the rationale
> (proposal/design/tasks). The default is 3 sub-agents
> in flight; >3 needs justification that survives the
> next `/gsd-complete-milestone` audit.

> **"What file do I open to know the current state?"**
> `.planning/STATE.md` — it is the highest-level truth for
> the *current* state, per the constitution's
> source-of-truth tiebreakers.

> **"Who decides whether OpenSpec is on or off for a
> change?"**
> The user. Always. An agent MUST NOT create
> `openspec/changes/<id>/` without explicit user
> instruction.

---

## 8. Reference table — artifacts, owners, and homes

| Artifact | Canonical home | Owner | How to amend |
|---|---|---|---|
| Project brief, validated/active/out-of-scope lists | `.planning/PROJECT.md` | GSD | Re-validated at every `/gsd-transition` |
| Roadmap (14 phases) | `.planning/ROADMAP.md` | GSD | `/gsd-transition`, `/gsd-complete-milestone` |
| Requirements (v1/v2, traceability) | `.planning/REQUIREMENTS.md` | GSD | `/gsd-transition` |
| Live state (decisions, blockers, deferred) | `.planning/STATE.md` | GSD **writer-of-record**; spec-kit & OpenSpec **writers** | Update whenever the fact changes |
| 7 codebase maps (STACK, STRUCTURE, ARCHITECTURE, CONVENTIONS, CONCERNS, INTEGRATIONS, TESTING) | `.planning/codebase/*.md` | GSD | `/gsd-map-codebase` |
| Per-phase folder (CONTEXT, RESEARCH, NN-PLAN, NN-SUMMARY, NN-VERIFICATION, VALIDATION, NN-UAT) | `.planning/phases/<NN>-<slug>/` | GSD | The phase-lifecycle commands |
| Per-feature spec (spec, plan, tasks, design, bugfix, review, analysis) | `.specs/<feature>/` | spec-kit | `/speckit-*` |
| Constitution (binding governance) | `.specify/memory/constitution.md` | spec-kit | `/speckit-constitution` |
| OpenSpec change (proposal, design, tasks, delta specs) | `openspec/changes/<id>/` | OpenSpec | `/opsx:propose`, `/opsx:apply`, `/opsx:archive` |
| Canonical spec (after a change is archived) | `openspec/specs/<domain>/` | OpenSpec | Folded from delta specs at archive time |
| ADR decision & rationale | `.planning/STATE.md` | GSD | When the decision is made |
| ADR finding that prompted the decision | `.specs/<feature>/{review,analysis}.md` | spec-kit | When the finding lands |

---

## 9. Source-of-truth tiebreakers (constitution, last section)

1. **Code is the lowest-level truth.** If `AGENTS.md` and the
   code disagree, the code wins and `AGENTS.md` is updated.
2. **`.planning/STATE.md` is the highest-level truth for the
   current state.** If `.specs/<feature>/tasks.md` and
   `.planning/STATE.md` disagree on what is "done",
   `STATE.md` wins (it is updated by the agent that finished
   the work).
3. **`.specify/memory/constitution.md` is non-negotiable
   within the scope of `/speckit-*` analysis.** If a
   principle needs to change, that is a separate, explicit
   constitution update — never silent reinterpretation.

---

## 10. Versions & last amended

- **Constitution:** v1.5.0 (ratified 2026-07-12,
  last amended 2026-09-15)
- **AGENTS.md:** version pins to v1.5.0
- **This cheat sheet:** v1.3 — v1.3 on 2026-09-15 aligns with
  constitution v1.5.0 (Principle IX added BMAD as the fourth workflow
  tool with review/analysis/adversarial-quality scope; Principle VI
  retitled to "Workflow Tooling (GSD / spec-kit / OpenSpec / BMAD)";
  the § 4 Seams section grew three new BMAD seams; the § 5 source-of-
  truth table grew five BMAD rows). Promote to v1.x on any change to
  the four-tool ownership rule, the concurrency budget, the canonical
  bring-up procedure, the host-OS rule, or BMAD's scope.
- **OpenSpec schema in this repo:** `spec-driven`
  (`openspec/config.yaml`)
- **spec-kit workflow version:** 1.0.0
  (`.specify/workflows/workflow-registry.json`)

---

*This document is a cheat sheet, not a governance artifact.
The constitution at `.specify/memory/constitution.md` is the
binding governance document; the runtime guide at
`AGENTS.md` is the binding operator reference. When in doubt,
read those two first.*
