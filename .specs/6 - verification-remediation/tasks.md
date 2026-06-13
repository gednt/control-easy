# Tasks — Verification Remediation Orchestration

> Companion to `requirements.md` and `design.md`. This orchestration uses `.specs/*/tasks.md` as the only source of truth for implementation completeness.

---

## Phase 1 — Playwright auth-aware remediation

- [x] **6.1** Build the spec inventory baseline from `.specs/*/tasks.md` and publish initial counts (`completed/open/cancelled/blocked`) per spec folder.
  - **Verification:** Baseline report exists and every count is traceable to checkbox rows in `tasks.md`.

- [x] **6.2** Refactor Playwright protected-route checks so tests run with a valid authenticated context before asserting module headings/actions.
  - **Verification:** Protected-route tests navigate to target pages (not login redirect), and assertion failures (if any) are page-specific.

- [x] **6.3** Re-run Playwright suite and persist evidence artifacts (terminal output, failure context, and passed route list).
  - **Verification:** Playwright run completes with a deterministic pass/fail summary and actionable logs.

---

## Phase 2 — Demo integration host-boot remediation

- [x] **6.4** Diagnose `WebApplicationFactory` host boot failure path for demo tests and document concrete root cause.
  - **Verification:** Root-cause note maps failing stack trace to a specific host/test-factory startup path.

- [x] **6.5** Implement host boot remediation in integration test setup so demo-enabled and demo-disabled scenarios both construct an `IHost`.
  - **Verification:** Boot no longer fails with "entry point exited without ever building an IHost".

- [x] **6.6** Resolve framework/runtime execution mismatch affecting demo integration test target frameworks in the local/CI execution path.
  - **Verification:** Target framework used for the demo suite runs without framework-missing abort.

- [x] **6.7** Re-run `DemoModeTests` and persist evidence artifacts (summary + failing test diagnostics if any remain).
  - **Verification:** Test run completes and returns deterministic pass/fail status with traceable output.

---

## Phase 3 — Full verification matrix rerun

- [x] **6.8** Recompute compliance matrix from `.specs/*/tasks.md` after Phases 1 and 2, preserving cancellation semantics separately from completion.
  - **Verification:** Matrix includes per-spec totals for `completed`, `open`, `cancelled`, `blocked`.

- [x] **6.9** Merge automation evidence (Playwright + demo integration) into the matrix and annotate blockers by domain owner (frontend/backend/qa/devops).
  - **Verification:** Every blocker row references command/test evidence and an owner domain.

- [x] **6.10** Publish final remediation report with: current compliance state, what was fixed, what remains open per `tasks.md`, and recommended next orchestration wave.
  - **Verification:** Report is review-ready and reproducible from repository state plus logged commands.

---

## Verification gate (Phase 6)

- Baseline and final compliance matrices are generated solely from `.specs/*/tasks.md`.
- Playwright protected-route verification executes with auth-aware setup.
- Demo integration suite host boot issue is remediated or explicitly documented with concrete blocker evidence.
- Final report lists unresolved work only as represented by unchecked items in `tasks.md` (plus explicit cancelled items).

---

## Task Dependency Graph

```json
{
  "waves": [
    { "wave": 1, "tasks": ["6.1", "6.2", "6.4"] },
    { "wave": 2, "tasks": ["6.3", "6.5", "6.6"] },
    { "wave": 3, "tasks": ["6.7", "6.8"] },
    { "wave": 4, "tasks": ["6.9", "6.10"] }
  ]
}
```

### Wave rationale

- **Wave 1**: establish baseline, fix frontend verification setup, and diagnose backend host-boot root cause in parallel.
- **Wave 2**: execute first reruns after remediation code paths are updated.
- **Wave 3**: finish demo rerun and recompute matrix from source-of-truth tasks.
- **Wave 4**: synthesize final blocker ownership and publish final report.
