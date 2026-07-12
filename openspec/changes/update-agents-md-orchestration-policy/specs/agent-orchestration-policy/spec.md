## ADDED Requirements

### Requirement: Any agent MAY invoke GSD, spec-kit, or OpenSpec skills as sub-agents
The system SHALL allow any agent — including a main orchestrator and any other in-flight sub-agent — to invoke a GSD skill (`.claude/skills/gsd-*` or `.agents/skills/gsd-*`), a spec-kit skill (`.agents/skills/speckit-*`), or an OpenSpec command skill (`/opsx-*`) as a sub-agent when the work at hand crosses the seam between the three workflow tools.

The invocation MUST NOT bypass the ownership rules in `AGENTS.md` § 2.1–§ 2.3 (GSD owns `.planning/`, spec-kit owns `.specs/` and `.specify/`, OpenSpec is opt-in and owns `openspec/`). The sub-agent's parent is responsible for respecting those rules on the sub-agent's behalf.

#### Scenario: Main agent needs a GSD map of the codebase while running an OpenSpec change
- **WHEN** a main agent running `/opsx:apply` for a change needs a fresh `.planning/codebase/ARCHITECTURE.md` summary to inform a design decision
- **THEN** the main agent invokes the GSD `gsd-codebase-mapper` skill as a sub-agent and feeds the result back into the OpenSpec `design.md` artifact
- **AND** the GSD sub-agent writes to `.planning/codebase/` per GSD's ownership rule
- **AND** the OpenSpec sub-agent does NOT write to `.planning/codebase/` (it only reads it)

#### Scenario: GSD phase planning delegates spec drafting to spec-kit
- **WHEN** a GSD `gsd-plan-phase` orchestrator is preparing a phase plan and the phase introduces a new feature spec
- **THEN** the GSD orchestrator MAY invoke the spec-kit `speckit-specify` skill as a sub-agent to draft the `.specs/<feature>/requirements.md` and `design.md`
- **AND** the spec-kit sub-agent writes the artifacts under `.specs/<feature>/`
- **AND** the GSD orchestrator only cross-references those artifacts in the phase plan and does NOT rewrite them

#### Scenario: spec-kit verification delegates change-tracking lifecycle to OpenSpec
- **WHEN** the user has opted a change into OpenSpec and the spec-kit `speckit-implement` task is ready to be applied
- **THEN** the spec-kit orchestrator MAY invoke `/opsx:apply` as a sub-agent to drive the OpenSpec change
- **AND** the spec-kit `tasks.md` and the OpenSpec `tasks.md` are kept in sync by the agent that runs `/opsx:apply`
- **AND** neither sub-agent rewrites the other tool's home directory

### Requirement: Default concurrency budget is 3 sub-agents plus 1 main orchestrator
The system SHALL enforce a default concurrency budget of at most **three (3) sub-agents in flight concurrently, in addition to the main orchestrator**, for any agent operating under `AGENTS.md`.

The budget is per agent, not per workflow tool: a main agent running an OpenSpec apply that fans out to GSD and spec-kit is still capped at three concurrent sub-agents.

Exceeding the budget (e.g. four or more sub-agents in flight) is permitted only when the originating artifact (`proposal.md` Why section, `design.md` Decision section, or `tasks.md` task description) records an explicit rationale; the rationale is reviewed at the next `/gsd-complete-milestone` boundary.

#### Scenario: Default apply run with no budget override
- **WHEN** `/opsx:apply` is invoked on a change whose `tasks.md` has three parallel tasks in wave 1
- **THEN** the main agent spawns up to three sub-agents in wave 1 (one per task)
- **AND** the main agent itself counts as the orchestrator
- **AND** the total in-flight count never exceeds 4 (1 main + 3 subs)

#### Scenario: Apply run with more than 3 parallel tasks
- **WHEN** `/opsx:apply` is invoked on a change whose `tasks.md` declares four parallel tasks in wave 1
- **THEN** the main agent MAY spawn a fourth sub-agent ONLY IF the change's `proposal.md` Why section or `design.md` Decision section records a rationale (e.g. "the four tasks are independent reads, no shared state")
- **AND** the rationale is reviewed at the next `/gsd-complete-milestone`

#### Scenario: Sub-agent spawns its own sub-agents
- **WHEN** a sub-agent running on behalf of the main orchestrator needs to delegate further (e.g. the OpenSpec sub-agent needs a GSD map)
- **THEN** the sub-agent's own fan-out is also capped at 3 concurrent sub-agents
- **AND** the total in-flight count across the full tree never exceeds 4 (the originating agent plus its 3 subs) at any single level
- **AND** deeper nesting is permitted but each level's fan-out budget is independent — a sub-agent may not "consume" its parent's budget

### Requirement: Cross-references between the new and existing sections of `AGENTS.md` are stable
The system SHALL keep the new § 2.4 ("Sub-agent invocation") and § 2.5 ("Concurrency budget") sections of `AGENTS.md` cross-referenced from the top of § 2 ("Workflow tooling") with a one-line pointer, so an agent reading "workflow tooling" first sees the orchestration rules.

The cross-reference MUST be one line of plain Markdown, MUST NOT introduce a new section heading, and MUST remain on the same level (##) as the existing § 2 header.

#### Scenario: Top-of-§ 2 cross-reference is present
- **WHEN** an agent reads `AGENTS.md` § 2 from the top
- **THEN** the first non-heading content under § 2 is a one-line pointer to § 2.4 and § 2.5
- **AND** the pointer does not duplicate the content of § 2.4 or § 2.5
- **AND** the pointer's anchor links resolve to the § 2.4 and § 2.5 headings

#### Scenario: New sections are inserted in dependency order
- **WHEN** `AGENTS.md` § 2 is rendered
- **THEN** § 2.1 (GSD), § 2.2 (spec-kit), and § 2.3 (OpenSpec) appear before § 2.4 (Sub-agent invocation) and § 2.5 (Concurrency budget)
- **AND** § 2.4 appears before § 2.5 (the concurrency rule depends on the invocation rule)
