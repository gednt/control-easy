## Context

`AGENTS.md` § 2 currently defines three workflow tools (GSD, spec-kit, OpenSpec) with a clear ownership split: GSD owns `.planning/`, spec-kit owns `.specs/` + `.specify/`, OpenSpec is opt-in and owns `openspec/`. The constitution v1.2.0 Principle VI ratifies the split. Two operational rules are missing:

1. **Sub-agent invocation.** No statement of which agent is allowed to call which other agent as a sub-agent. In practice, a main agent running `/opsx:apply` routinely needs to invoke a GSD skill (e.g. `gsd-codebase-mapper`) or a spec-kit skill (e.g. `speckit-implement`) to complete an OpenSpec change. The rule is implicit.
2. **Concurrency budget.** No statement of how many sub-agents the main agent may run in parallel. The runtime used for ControlEasy Reborn (Cursor / Claude / GPT-class) works best with a small, fixed fan-out; without a written budget, fan-out is unbounded and inconsistent.

This change introduces § 2.4 (sub-agent invocation) and § 2.5 (concurrency budget) in `AGENTS.md`, plus a one-line cross-reference at the top of § 2. No code, no schema, no API, no test slice.

## Goals / Non-Goals

**Goals:**

- Codify that any agent MAY invoke any of GSD, spec-kit, or OpenSpec skills as a sub-agent, subject to the ownership rules in § 2.1–§ 2.3.
- Codify the default concurrency budget: up to 3 sub-agents + 1 main orchestrator in flight concurrently.
- Define the override path (rationale in the originating artifact, reviewed at next `/gsd-complete-milestone`).
- Add a one-line cross-reference at the top of § 2 so the new rules are discoverable.

**Non-Goals:**

- Amending `.specify/memory/constitution.md` in this change. The new rules derive from Principle VI; constitutional promotion is a follow-up at the user's discretion.
- Changing the GSD / spec-kit / OpenSpec command skills at the CLI level.
- Adding any code, schema, API, or test slice. This is documentation-only.
- Reorganising `AGENTS.md` beyond the § 2 additions. Existing § 2.1–§ 2.3 are left untouched.

## Decisions

### Decision 1 — Add two new subsections under § 2, not a new top-level section

**Choice.** Add § 2.4 ("Sub-agent invocation") and § 2.5 ("Concurrency budget") under the existing § 2 ("Workflow tooling") header, with a one-line cross-reference at the top of § 2.

**Rationale.** The new rules govern *how agents use* the three workflow tools, not what the tools do. They belong as subsections of the workflow tooling section, not as a sibling section. A new top-level section would imply they are independent of the workflow tools, which is the opposite of the intent.

**Alternatives considered.**
- New top-level § 10 ("Orchestration") — rejected: implies the rules are not part of the workflow tooling, which would dilute the GSD / spec-kit / OpenSpec ownership story in § 2.
- Embed the rules in the existing § 2.3 (OpenSpec) — rejected: the rules apply to GSD and spec-kit too, so they cannot live under OpenSpec.

### Decision 2 — Default budget of 3 sub-agents + 1 main orchestrator

**Choice.** The default in-flight cap is `3 sub-agents + 1 main orchestrator = 4 agents per level`.

**Rationale.** Empirically, the runtimes used for ControlEasy Reborn are most reliable at small fan-outs. A cap of 3 keeps the per-step latency low (each sub-agent is small and fast), avoids model-context contention, and matches the 3-task wave pattern already used in the spec-kit `tasks.md` "Task Dependency Graph" convention. A cap of 1 (no fan-out) would be too conservative for parallel waves; a cap of 5+ blows past the point of diminishing returns on the typical 4–8k-token context.

**Alternatives considered.**
- 1 sub-agent + 1 main (no fan-out) — rejected: serialises waves that should be parallel.
- 2 sub-agents + 1 main — rejected: too restrictive; many spec-kit `tasks.md` files declare 3 parallel tasks in wave 1.
- 5+ sub-agents + 1 main — rejected: latency and context contention dominate; the 3-task wave pattern is the natural ceiling.

### Decision 3 — Override path is a written rationale, not a config flag

**Choice.** Exceeding the 3-sub-agent budget requires a rationale in the originating artifact (`proposal.md` Why section, `design.md` Decision section, or `tasks.md` task description). The rationale is reviewed at the next `/gsd-complete-milestone`. No environment variable, no config file, no CLI flag.

**Rationale.** The override is rare and qualitative. A config flag would either be over-used (because flags are cheap) or under-used (because flag updates require a code change). A written rationale surfaces the override at the same place the work is described, so the next milestone audit sees it without a separate scan.

**Alternatives considered.**
- `ORCHESTRATION_MAX_SUBAGENTS` env var — rejected: env vars are easy to set and forget, and the override should be deliberate.
- A workflow-tool-specific override (e.g. OpenSpec-only) — rejected: the budget is per agent, not per tool.

### Decision 4 — The new rules are NOT promoted to constitutional status in this change

**Choice.** The new rules live in `AGENTS.md` only. `.specify/memory/constitution.md` v1.2.0 is not amended.

**Rationale.** Constitutional amendments deserve a separate change with its own proposal, design, and review. This change establishes the rules as a derived guidance (deriving from Principle VI, not amending it). A follow-up change can promote them to constitutional status if the user wants them binding across all agents, including those that don't read `AGENTS.md`.

**Alternatives considered.**
- Amend Principle VI in the same change — rejected: the change scope is too small to justify a constitutional amendment, and mixing the two would obscure the audit trail.
- Promote in a follow-up change — accepted as the default path.

## Risks / Trade-offs

- **[Risk] A parent agent may still over-spawn sub-agents in practice.** `AGENTS.md` is guidance; it has no runtime enforcement. → **Mitigation.** The rationale-at-override rule plus the `/gsd-complete-milestone` review surface overruns after the fact. A follow-up change could add a runtime check (e.g. a pre-tool-use hook in `.claude/hooks/`) if the user wants enforcement.
- **[Risk] Cross-references in `AGENTS.md` may drift as the file evolves.** A future edit to § 2.1–§ 2.3 could move the section numbering, breaking the new cross-reference. → **Mitigation.** The cross-reference is a single line of Markdown, easy to grep for in a CI check. The OpenSpec `tasks.md` includes a verification step that renders `AGENTS.md` and checks that § 2.4 and § 2.5 are reachable from the top of § 2.
- **[Risk] Constitution drift.** The new rules derive from Principle VI but are not in the constitution. An agent that only reads the constitution may not see them. → **Mitigation.** `AGENTS.md` is the binding runtime document for every contributor and every agent (per its own § 1). The constitution v1.2.0 already defers to `AGENTS.md` on operational details.
- **[Trade-off] The override path is qualitative, not quantitative.** A written rationale is easier to write than to enforce. → **Accepted** because quantitative enforcement (e.g. a hard cap) is the wrong tool for the rare case where 4+ sub-agents is the right call.

## Migration Plan

No migration. This is a documentation-only change.

1. Apply: edit `AGENTS.md` to add § 2.4 and § 2.5, plus the top-of-§ 2 cross-reference.
2. Rollback: revert the `AGENTS.md` edit. No state to roll back.
3. Audit: the next `/gsd-complete-milestone` reviews (a) the new rules and (b) any override rationales that accumulated since the previous milestone.

## Open Questions

- Should the override path also require a `tasks.md` link (so the rationale is one click away from the verification gate)? — _Deferred to a follow-up change if the user wants stricter traceability._
- Should the budget be configurable per workflow tool (e.g. GSD gets 5, OpenSpec gets 3)? — _Deferred: the spec says per-agent, not per-tool; revisit if a real workload demonstrates the need._
- Should the runtime enforce the budget (e.g. a hook that counts in-flight sub-agents and aborts the 4th)? — _Deferred to a follow-up change. The current change is guidance only._
