# Tasks — Code Review Agent (Phase 3)

- [x] 1. Read `AGENTS.md` (repo root), CodeReviewAgent `AGENTS.md`, and `orchestration.md`.
- [x] 2. Read `requirements.md`, `design.md`, `tasks.md`, and the source-of-truth delta `tenant-and-attendant-deltas.md`.
- [x] 3. Grep for emojis, `condominium_id`, `Porteiro` (leftover old role names) across the three updated files.
- [x] 4. Grep for `tenant_id` references to confirm coverage of the new naming.
- [x] 5. Verify the 5 ambiguities the orchestrator flagged.
- [x] 6. Cross-check user-story coverage in design.md and tasks.md (UC-9 updated, UC-21..UC-25).
- [x] 7. Cross-check endpoint paths (especially `POST /api/v1/security/tenant-switch`) across the three files.
- [x] 8. Verify the policy-list update in design.md (Ambiguity #5) is consistent with the new role set.
- [x] 9. Confirm glossary coverage for body-used role/concept names.
- [x] 10. Write the review document at `.specs/1 - modernization-roadmap/review.md`.
- [x] 11. Present findings (summary, ambiguities, blockers, follow-up edits) to the orchestrator.

## Findings (summary)

- Major: M1 (tasks.md 2.1 duplicate), M2 (1.0b -> 1.15 NetArchTest rule), M3 (tenant-switch missing from endpoint list).
- Minor: M4 (design.md:310 intro wording), m1 (pre-existing duplicated `### Frontend` heading), m2 (glossary sort-order rationale in orchestration log is inaccurate), m3 (orchestration change map stale on 2.7a/3.9a), m4 (glossary missing the `Attendant` role string).
- Nit: n1, n2.
- Critical: none.

## Ambiguity verdicts

1. 1.0b / 1.15 NetArchTest rule: **Needs fix** (M2).
2. C.6 -> C.1 `test` job: **OK**.
3. Glossary sort order: **Recommendation (no doc change)**.
4. 2.7a vs 2.7: **OK**.
5. Policy list: **OK (keep as merged)**.

## Status

Complete. Review document published at `.specs/1 - modernization-roadmap/review.md`.
