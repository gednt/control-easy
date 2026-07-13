# Design — Verification Remediation Orchestration

## Objective

Establish a deterministic verification pipeline that evaluates implementation completeness strictly from `.specs/*/tasks.md`, then validates key automated gates (Playwright and demo integration tests), and finally emits an auditable compliance report.

## Design principles

1. **Tasks-first truth**: status is computed from `tasks.md` only.
2. **Evidence over inference**: every status change must be backed by command/test output.
3. **Fix setup before judging behavior**: resolve test harness issues (auth boot, host boot) before using failures as product signals.
4. **Domain isolation**: route fixes to the right domain owners (frontend test harness, backend host/test factory, QA reporting).

## Components

### 1) Spec inventory and parser

- Input: all `.specs/**/tasks.md`.
- Output:
  - normalized task rows (`spec`, `taskId`, `title`, `checkboxState`, `cancelledNote`),
  - status rollups by spec.
- Rules:
  - `[x]` with cancellation marker stays `cancelled`, not `completed`.
  - `[ ]` is `open` unless blocked annotation exists.

### 2) Playwright auth-aware verification harness

- Current gap: protected routes redirect to login, causing false-negative module page assertions.
- Target behavior:
  - use authenticated setup (UI login or API/session bootstrap),
  - then assert route-specific headings/actions.
- Output:
  - playwright results with route-level status and failure context.

### 3) Demo host boot remediation for integration tests

- Current gap: `WebApplicationFactory` path exits before host build.
- Target behavior:
  - deterministic host creation in test factory for demo-enabled and demo-disabled paths,
  - framework/runtime compatibility validated in the active environment.
- Output:
  - demo test suite results with explicit reasons for any remaining failures.

### 4) Final verification matrix and report

- Inputs: parser output + Playwright rerun + demo test rerun.
- Output:
  - consolidated matrix:
    - `Completed`, `Open`, `Cancelled`, `Blocked`,
    - failures tied to verification command evidence,
    - unresolved blockers and next owner.

## Execution phases

### Phase 1 — Playwright remediation (frontend + QA)

- Adjust protected-route tests to run with valid authentication context.
- Re-run Playwright and collect evidence.

### Phase 2 — Demo integration remediation (backend + QA)

- Fix host boot path in integration test setup and framework/runtime compatibility.
- Re-run `DemoModeTests` and collect evidence.

### Phase 3 — Full verification rerun and compliance synthesis (QA)

- Recompute `tasks.md` compliance across all specs.
- Attach test evidence from phases 1 and 2.
- Publish final matrix.

## Risks and mitigations

- **Risk**: Tests pass locally but fail in CI due to environment drift.
  - **Mitigation**: document and pin runtime assumptions in verification commands.
- **Risk**: Authentication fixture introduces brittle coupling.
  - **Mitigation**: centralize auth helper and keep assertions route-focused.
- **Risk**: Cancellation semantics are miscounted as completed.
  - **Mitigation**: explicit parser rule for cancelled items.

## Verification strategy

- Command-level evidence is required for each phase.
- Report is accepted only if:
  1. it is generated from `.specs/*/tasks.md`,
  2. Playwright rerun result is included,
  3. demo integration rerun result is included,
  4. unresolved items are listed with blockers.
