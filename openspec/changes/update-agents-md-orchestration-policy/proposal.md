## Why

`AGENTS.md` § 2 currently prescribes a clean GSD / spec-kit / OpenSpec seam: GSD owns `.planning/`, spec-kit owns `.specs/`, OpenSpec is opt-in. The split is correct, but it leaves two operational gaps:

1. **Sub-agent invocation is implicit.** When a parent agent (e.g. the main agent running `/opsx:apply`, or a BMAD orchestrator) needs a GSD map of the codebase, a spec-kit review, or an OpenSpec artifact, the rule for *which agent is allowed to call which other agent as a sub-agent* is not written down. Without it, every parent reinvents the rule, and the constitution's "single non-overlapping responsibility" guarantee is only enforced socially.
2. **Concurrency budget is unbounded.** Nothing in `AGENTS.md` says how many sub-agents the main agent may run in parallel. In practice the runtimes used for ControlEasy Reborn (Cursor / Claude / GPT-class) work best with a small, fixed fan-out. Without a cap, the main agent tends to either over-spawn (paying latency for work that could have been one task) or under-spawn (serialising what could have been parallel).

This change makes those two rules explicit and binding in `AGENTS.md` so that any agent, regardless of which workflow tool (GSD, spec-kit, OpenSpec) it lives in, follows the same orchestration contract.

## What Changes

- **`AGENTS.md` § 2 — Workflow tooling.** Add a new subsection **"2.4 Sub-agent invocation"** stating that any agent MAY invoke any of GSD, spec-kit, or OpenSpec skills as a sub-agent when the work at hand crosses the seam. The owning tool's authorship and writer/reader rules from § 2.1–§ 2.3 are preserved; sub-agent invocation never bypasses them.
- **`AGENTS.md` § 2 — Workflow tooling.** Add a new subsection **"2.5 Concurrency budget"** stating the default fan-out: at most **3 sub-agents + 1 main orchestrator** (i.e. the main agent plus up to three concurrent sub-agents) in flight at any moment. Exceeding the budget requires an explicit rationale in the originating artifact (`proposal.md` Why section, `design.md` Decision section, or `tasks.md` task description) and the rationale is reviewed at the next `/gsd-complete-milestone`.
- **`AGENTS.md` § 2 — Workflow tooling.** Add a one-line cross-reference at the top of § 2 pointing to the new § 2.4 / § 2.5, so an agent reading "workflow tooling" first sees the orchestration rules.
- **No change** to `.specify/memory/constitution.md` in this change. The constitution v1.2.0 already records the GSD / spec-kit / OpenSpec ownership rule (Principle VI). This change derives sub-agent invocation and concurrency rules from that principle; the constitution itself is not amended. A follow-up change can promote the new rules to constitutional status if the user wants it there.

## Capabilities

### New Capabilities

- `agent-orchestration-policy`: the rules that govern (a) which workflow tools an agent may invoke as a sub-agent, and (b) how many sub-agents may run in parallel. Anchored in `AGENTS.md` § 2.4 and § 2.5.

### Modified Capabilities

_None._ No existing canonical `openspec/specs/<domain>/` spec changes in this change. The new `agent-orchestration-policy` capability is a fresh addition.

## Impact

- **`AGENTS.md`** — two new subsections (§ 2.4, § 2.5) plus a one-line cross-reference at the top of § 2. Net delta: ~30–50 lines.
- **Constitution** — unchanged in this change. The new rules are derived from Principle VI; promotion to constitutional status is deferred to a follow-up change at the user's discretion.
- **Workflow skills** — GSD (`/gsd-*`), spec-kit (`/speckit-*`), and OpenSpec (`/opsx-*`) command skills are unaffected at the CLI level. Their runtime behaviour already allows sub-agent invocation; this change just makes the rule explicit in `AGENTS.md`.
- **No code, no API, no DB schema, no test slice.** This is documentation-only.
- **Verification gate.** None of § 5.1 / § 5.2 / § 5.3 applies because no code, image, or schema is touched. The verification gate for this change is a `git diff` review of `AGENTS.md` and a render check that the two new subsections read in isolation.
