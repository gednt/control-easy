# Orchestration Log — Verification Remediation

## Scope

- Spec folder: `.specs/6 - verification-remediation/`
- Source-of-truth policy: only `.specs/*/tasks.md` defines completion status.
- Request type: verification remediation (no product feature expansion).

## Routing decision

1. **Frontend Agent (Phase 1)**  
   Domain: Playwright protected-route auth-aware setup and route assertions.

2. **Backend Agent (Phase 2)**  
   Domain: integration host boot path and test factory/runtime remediation for demo suite.

3. **QA Agent (Phase 3)**  
   Domain: verification reruns, evidence collation, and final compliance matrix synthesis.

## Execution order and dependencies

- Phase 1 and Phase 2 can start in parallel after baseline inventory (`6.1`) is captured.
- Phase 3 starts only after:
  - Playwright rerun evidence is available (`6.3`),
  - Demo integration rerun evidence is available (`6.7`).

## Coordination checklist

- [x] Confirm agent delegation plan with user.
- [x] Capture initial baseline counts from all `.specs/*/tasks.md`.
- [x] Dispatch Frontend Agent tasks (`6.2`, `6.3`).
- [x] Dispatch Backend Agent tasks (`6.4`, `6.5`, `6.6`, `6.7`).
- [x] Dispatch QA Agent tasks (`6.8`, `6.9`, `6.10`).
- [x] Review cross-phase consistency and unresolved blockers.
- [x] Publish final remediation report.

## Conflict handling policy

- If Playwright setup requirements conflict with auth/security constraints, escalate with options and trade-offs.
- If host-boot fixes require entrypoint behavior changes that affect production startup semantics, escalate before merge.
- If test evidence contradicts `tasks.md` checkbox state, keep `tasks.md` as authoritative and report contradiction as a blocker.
