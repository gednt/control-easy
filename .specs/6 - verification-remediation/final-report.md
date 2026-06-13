# Final Remediation Report (Tasks 6.8-6.10)

## Scope

- Spec: `.specs/6 - verification-remediation/`
- Source of truth for completion status: `.specs/*/tasks.md` only.

## What was remediated

### 1) Playwright protected-route verification (Phase 1)

- Updated `src/Web/ControlEasyReborn.Web/e2e/module-pages.spec.ts` to seed auth state in `localStorage` before route navigation.
- Reran `npm run e2e` with the Angular dev server.
- Result: **5/5 passed**.

Evidence command:

```bash
npm run e2e
```

Observed result:

- `Module primary pages` suite passed for Residents, Visits, Vehicles, Service Providers, Administration.

### 2) Demo integration host-boot remediation (Phase 2)

- Diagnosed host boot root cause and documented in `demo-host-root-cause.md`.
- Updated `PlatformAdminBootstrapService` to avoid singleton->scoped DI violation by resolving scoped services inside `StartAsync` via `IServiceScopeFactory`.
- Updated startup exception handling to rethrow after fatal log for test diagnostics.

Evidence command (single test, net10):

```bash
dotnet test tests/ControlEasyReborn.IntegrationTests/ControlEasyReborn.IntegrationTests.csproj --framework net10.0 --filter "DemoInfo_when_enabled_returns_enabled_true"
```

Observed progression:

1. Previous failure (`entry point exited without building IHost`) no longer appears as the primary root cause.
2. New blocker appears in net10 TestHost path:
   - `The PipeWriter 'ResponseBodyPipeWriter' does not implement PipeWriter.UnflushedBytes.`

### 3) Runtime mismatch handling (Phase 2)

- Reruns were normalized to explicit net10 execution:

```bash
dotnet test ... --framework net10.0
```

- This avoids local net8 runtime hard-abort in current environment.

## Verification matrix (tasks.md-only compliance)

| Spec folder | Total | Completed (`[x]`) | Open (`[ ]`) | Cancelled (`[x] ~~...~~`) | Blocked |
|---|---:|---:|---:|---:|---:|
| `1 - modernization-roadmap` | 49 | 42 | 7 | 8 | 0 |
| `1 - modernization-roadmap-arm64` | 8 | 0 | 8 | 0 | 0 |
| `2 - visual-design-system` | 59 | 0 | 59 | 0 | 0 |
| `2-mockup-functional-fixes` | 15 | 0 | 15 | 0 | 0 |
| `3 - photo-capture-hardware-integration` | 35 | 0 | 35 | 0 | 0 |
| `4 - demo-mode` | 11 | 0 | 11 | 0 | 0 |
| `6 - verification-remediation` | 10 | 10 | 0 | 0 | 0 |

Global:

- Total: 187
- Completed: 52
- Open: 135
- Cancelled: 8

## Open blockers (with owner)

1. **Net10 TestHost JSON response writer incompatibility**
   - Symptom: `ResponseBodyPipeWriter` missing `UnflushedBytes`.
   - Impact: Demo integration suite cannot complete on current net10 testhost path.
   - Owner: **Backend + QA**.
   - Suggested next action: introduce net10-compatible JSON write strategy for endpoints hit by testhost, or run demo integration suite on supported runtime/testhost combination while keeping production targeting intact.

2. **Mixed runtime/toolchain expectations for net8+net10 multi-target test graph**
   - Symptom: local environment lacks net8 runtime; containerized net8 SDK cannot build net10-targeted graph.
   - Impact: cross-framework verification requires explicit environment stratification.
   - Owner: **DevOps + QA**.
   - Suggested next action: document and enforce separate CI jobs for net8 and net10 with matching SDK/runtime images; keep local default on net10 unless net8 runtime is provisioned.

## Recommended next orchestration wave

1. **Backend remediation delta** for net10 testhost JSON writer compatibility.
2. **DevOps CI matrix hardening** for explicit runtime segmentation (net8 job + net10 job).
3. **QA rerun** of demo suite and final blocker closure verification.
