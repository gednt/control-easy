# Tasks — Update `AGENTS.md` orchestration policy

> **Scope.** Documentation-only change to `AGENTS.md` § 2. No code, no
> schema, no API, no test slice. The `AGENTS.md` § 5 verification gate
> does not apply (nothing to build, restart, or test). The verification
> gate for this change is a `git diff` review of `AGENTS.md` and a
> render check that § 2.4 and § 2.5 read in isolation.

## 1. Edit `AGENTS.md` § 2

- [x] 1.1 Add a one-line cross-reference at the top of `AGENTS.md` § 2 ("Workflow tooling") pointing to the new § 2.4 and § 2.5. The cross-reference is one line of plain Markdown, introduces no new heading, and uses anchor links to the § 2.4 / § 2.5 headings.
- [x] 1.2 Append `## 2.4 Sub-agent invocation` after the existing § 2.3 (OpenSpec) section. The new section states that any agent MAY invoke any of GSD, spec-kit, or OpenSpec skills as a sub-agent, and that the ownership rules in § 2.1–§ 2.3 still apply (sub-agent invocation never bypasses them).
- [x] 1.3 Append `## 2.5 Concurrency budget` after the new § 2.4. The new section states the default fan-out (3 sub-agents + 1 main orchestrator) and the override path (rationale in the originating artifact, reviewed at next `/gsd-complete-milestone`).
- [x] 1.4 Confirm the section ordering is `§ 2.1 GSD → § 2.2 spec-kit → § 2.3 OpenSpec → § 2.4 Sub-agent invocation → § 2.5 Concurrency budget` (the new rules depend on the existing three; they MUST come last).

## 2. Verify the edit

- [x] 2.1 `git diff AGENTS.md` shows only the § 2 additions (top-of-§ 2 cross-reference + § 2.4 + § 2.5) and no other lines.
- [x] 2.2 Render `AGENTS.md` in a Markdown viewer and confirm § 2.4 and § 2.5 are reachable from the top-of-§ 2 cross-reference (anchor links resolve).
- [x] 2.3 Read § 2.4 and § 2.5 in isolation (no other context) and confirm they are self-explanatory: an agent that only reads § 2.4 or only § 2.5 understands the rule without needing § 2.1–§ 2.3.
- [x] 2.4 Confirm the new sections do not duplicate content from § 2.1–§ 2.3 (the new rules reference but do not restate the existing ownership rules).

## 3. Archive the change

- [x] 3.1 Run `openspec validate update-agents-md-orchestration-policy --strict` and confirm zero errors. (The spec-driven schema's `applyRequires` is `tasks`, so by this point all four artifacts — proposal, specs, design, tasks — should be `done`.)
- [x] 3.2 Run `openspec status --change update-agents-md-orchestration-policy` and confirm `isComplete: true`.
- [ ] 3.3 Defer the `openspec archive` step to a follow-up after the user has reviewed the `AGENTS.md` edit. The archive step folds the delta spec into the canonical `openspec/specs/agent-orchestration-policy/spec.md` and moves the change to `openspec/changes/archive/<date>-update-agents-md-orchestration-policy/`.

## Task Dependency Graph

```json
{
  "waves": [
    {
      "wave": 1,
      "tasks": ["1.1", "1.2", "1.3", "1.4"]
    },
    {
      "wave": 2,
      "tasks": ["2.1", "2.2", "2.3", "2.4"]
    },
    {
      "wave": 3,
      "tasks": ["3.1", "3.2", "3.3"]
    }
  ]
}
```

**Rationale.**
- **Wave 1 (edit).** All four sub-tasks touch the same file (`AGENTS.md` § 2) and are sequential by necessity — the cross-reference (1.1) references the § 2.4 and § 2.5 headings that 1.2 and 1.3 create. They are grouped into one wave because they are short and run as a single edit session, but the in-wave ordering matters: 1.2 → 1.3 → 1.4 → 1.1 (cross-reference last so the anchor targets already exist).
- **Wave 2 (verify).** Four parallel reads / greps over the post-edit `AGENTS.md`. No dependency on each other, all depend on Wave 1.
- **Wave 3 (archive).** Three sequential `openspec` CLI calls. Each depends on the previous one's output. `3.3` is intentionally deferred to a follow-up so the user can review the `AGENTS.md` edit before the canonical spec is folded.

**Concurrency budget compliance.** The default budget (3 sub-agents + 1 main orchestrator) is observed in every wave. Wave 1 is a single edit session, not a sub-agent fan-out. Wave 2 has four tasks but they are all reads and short; the main agent MAY run all four in parallel within the budget, OR serialise them — both are allowed. Wave 3 is serial CLI calls.

## Verification gate

This is a documentation-only change. None of `AGENTS.md` § 5.1 / § 5.2 / § 5.3 apply. The verification gate is:

1. `git diff AGENTS.md` is reviewed and contains only the § 2 additions.
2. `openspec validate update-agents-md-orchestration-policy --strict` returns 0 errors.
3. `openspec status --change update-agents-md-orchestration-policy` reports `isComplete: true`.
4. A second human/agent reader confirms § 2.4 and § 2.5 are understandable in isolation.
