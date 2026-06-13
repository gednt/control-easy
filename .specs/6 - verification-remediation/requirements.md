# Requirements — Verification Remediation Orchestration

## Overview

This spec defines a remediation orchestration to recover verification confidence across the repository, using only `.specs/*/tasks.md` as the authoritative completion source.

The orchestration is limited to:

1. making Playwright verification runnable and meaningful for protected routes,
2. restoring demo integration test host boot and deterministic execution,
3. re-running the full verification matrix and publishing a compliance report.

No feature-scope expansion is allowed in this spec. Work is limited to verification and testability alignment.

## Source-of-truth rule

- Task completion status MUST be derived from `.specs/*/tasks.md` checkboxes only.
- `requirements.md`, `design.md`, commit messages, and inferred code presence are supporting context only.
- If code appears implemented but `tasks.md` is unchecked, the status is `Not Completed`.

## User stories

- **UC1**: As a maintainer, I want Playwright checks to validate protected module pages with valid authentication, so verification failures reflect product issues rather than test setup.
- **UC2**: As a maintainer, I want demo integration tests to boot the host deterministically, so demo-mode tasks can be verified from automated evidence.
- **UC3**: As a maintainer, I want a full spec compliance matrix generated from all `.specs/*/tasks.md`, so I can see exactly what is completed, missing, cancelled, or blocked.
- **UC4**: As a maintainer, I want remediation work sequenced and dependency-aware, so reruns only happen after prerequisite fixes land.
- **UC5**: As a maintainer, I want verification outputs persisted as artifacts/logs, so review and audits are reproducible.

## Functional requirements

- **FR1**: Build a `tasks.md` inventory pass that indexes all spec task files and classifies each task as `completed`, `open`, `cancelled`, or `blocked` based on checkbox state and inline cancellation notes.
- **FR2**: Update Playwright verification flow for protected routes:
  - either authenticate in test setup and navigate to target routes,
  - or provide test-only seed/auth hook for stable login in CI/local.
- **FR3**: Ensure Playwright reports include failing selectors/route context and are linkable to specific task IDs where applicable.
- **FR4**: Fix demo integration test host boot so `WebApplicationFactory` initializes correctly in both configured target frameworks used by the test project.
- **FR5**: Re-run demo integration tests scoped to demo suite and capture pass/fail evidence.
- **FR6**: Re-run Playwright verification suite and capture pass/fail evidence.
- **FR7**: Produce consolidated compliance report:
  - per spec folder,
  - per task status counts,
  - list of unresolved blockers and owner domain.

## Non-functional requirements

- **NFR1**: No production feature behavior changes beyond what is required to make verification deterministic.
- **NFR2**: Verification reruns must be scriptable and repeatable on a clean environment.
- **NFR3**: Report generation must be deterministic for the same repository state.
- **NFR4**: All newly added checks must complete within existing local/CI practical limits.

## Out of scope

- Implementing unchecked product features from unrelated specs.
- Rewriting legacy specs or changing historical task semantics.
- Reclassifying unchecked tasks as done based only on code inspection.

## Acceptance criteria

1. A `tasks.md`-driven compliance matrix is produced for all `.specs` folders.
2. Playwright protected-route checks execute in an auth-aware manner and provide actionable failures.
3. Demo integration suite boots host and runs deterministically, with explicit pass/fail outputs.
4. Final rerun matrix is executed and captured after remediation fixes.
5. Remaining incomplete tasks are reported exactly as represented in `.specs/*/tasks.md`.
