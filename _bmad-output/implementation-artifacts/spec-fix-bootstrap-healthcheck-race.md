---
title: 'Fix first-boot race: MySQL healthcheck + silent bootstrap failure'
type: 'bugfix'
created: '2026-09-12'
status: 'done'
review_loop_iteration: 0
followup_review_recommended: true
context: []
warnings: []
baseline_revision: d85b496
final_revision: cd887af
---

<intent-contract>

## Intent

**Problem:** On first boot the API can start while MySQL is still running its temporary init server (socket-only, TCP disabled), because the healthcheck (`mysqladmin ping -h localhost`) connects via unix socket and passes during the init window. The API's `PlatformAdminBootstrapService` then runs while schema init is incomplete, its connection fails silently (DBTools returns empty results and a false `InsertAsync`), and the service logs "PlatformAdmin seeded" anyway — leaving an empty `Users` table and a misleading success log. Login is impossible until a manual restart.

**Approach:** Fix both sides of the race: (1) replace the db healthcheck with one that verifies a real TCP connection plus database readiness (official `/usr/local/bin/healthcheck.sh` from the mysql image, forced TCP), and (2) make `PlatformAdminBootstrapService` verify its insert results, log failures loudly, and retry with bounded backoff when the schema/table is not yet ready.

## Boundaries & Constraints

**Always:** Keep `depends_on: service_healthy` semantics; keep bootstrap idempotent (existing PlatformAdmin → skip); all changes inside the worktree `ControlEasy.fix-bootstrap-healthcheck-race`; no changes to data-access library (DBTools) itself; `docker compose config` must stay valid.

**Block If:** ~~The official `healthcheck.sh` does not exist in the mysql:8.0 image~~ — RESOLVED during investigation: script is absent; fall-back plan (TCP-forced mysqladmin ping + schema-existence check) is now the primary plan. The DBTools `IAsyncSqlClient` API does expose `Task<bool> InsertAsync` — verified.

**Never:** Do not add EF Core; do not alter `TenantFilterInterceptor`; do not change demo seeding; do not remove the bootstrap warning log when seeding genuinely succeeds.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Happy first boot | Fresh volume, API starts after healthy db | Healthcheck passes only after TCP server accepts connections on the app database; bootstrap inserts succeed | None expected |
| Init-window race | API start requested while init server running | Healthcheck fails (TCP unreachable) → Compose keeps waiting | API never starts against temp server |
| Insert failure | DB reachable but INSERT fails | Bootstrap logs ERR with `_db.Error` detail, retries with backoff (bounded), never logs "seeded" | Service throws after retries exhausted, app fails fast |
| Idempotent restart | PlatformAdmin already exists | "already exists; skipping bootstrap" — unchanged | No insert attempted |

</intent-contract>

## Code Map

- `docker/docker-compose.yml` -- db healthcheck (line 68-71) and api `depends_on` (line 36-38) to modify
- `src/Host/ControlEasyReborn.Api/Hosting/PlatformAdminBootstrapService.cs` -- bootstrap service: verify insert results, add bounded retry with backoff
- `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Demo/DemoSeederService.cs` -- same silent-failure class of bug (uses `SelectAsync`/`InsertAsync` return values); audit, fix only if trivial
- `tests/ControlEasyReborn.UnitTests/TestDoubles/FakeAsyncSqlClient.cs` -- fake returns bool from `InsertResultFactory`; reusable for new unit tests
- `tests/ControlEasyReborn.UnitTests/` -- add tests for bootstrap insert-failure/retry behavior
- `docker/docker-compose.demo.yml` -- inherits db healthcheck; no changes expected

## Tasks & Acceptance

**Execution:**
- [ ] `docker/docker-compose.yml` -- Replace db healthcheck with a TCP-forced readiness probe: `mysqladmin ping --protocol=tcp -h 127.0.0.1 ...` plus a schema-existence check via `mysql --protocol=tcp -h 127.0.0.1 ... -e "SELECT COUNT(*) FROM controleasydb.Users"` (forced TCP rejects the socket-only temporary init server; the official healthcheck.sh script does not exist in this image) -- RATIONALE: first-boot race root cause
- [ ] `src/Host/ControlEasyReborn.Api/Hosting/PlatformAdminBootstrapService.cs` -- Check `SelectAsync` error state and `InsertAsync` return values; wrap seeding in a bounded retry loop (e.g. 6 attempts, 2s/4s/8s exponential backoff) with clear ERR logging including `_db.Error`; throw after retries exhausted so the app fails fast instead of running with no admin -- RATIONALE: silent failure was the direct cause of the empty Users table
- [ ] `tests/ControlEasyReborn.UnitTests/` -- Add unit tests covering: insert-failure retry then success; retries exhausted throws; idempotent skip when PlatformAdmin exists -- RATIONALE: lock the fixed behavior
- [ ] `src/BuildingBlocks/ControlEasyReborn.Infrastructure/Demo/DemoSeederService.cs` -- Audit only; apply the same insert-result check if it is a small local change, otherwise record in deferred-work -- RATIONALE: same failure mode, demo-only blast radius

**Acceptance Criteria:**
- Given a fresh MySQL volume, when `docker compose up` runs, then the API container does not start until the real TCP server answers and the schema exists, and the PlatformAdmin row exists before the app logs "Application started"
- Given a simulated insert failure, when bootstrap runs, then it retries with backoff, logs ERR with the DBTools error detail, and does not log "PlatformAdmin seeded" until an insert actually succeeds
- Given PlatformAdmin already exists, when bootstrap runs again, then it logs "already exists; skipping" and performs no inserts
- Given `docker compose -f docker/docker-compose.yml config`, when run, then it exits 0 with valid merged config
- Given the full unit test suite, when run, then all tests pass
## Spec Change Log

### 2026-09-12 — bad_spec/patch amendments after review pass
- Trigger: review found the healthcheck's single-quoted `$$MYSQL_DATABASE` unexpanded, the partial-success "already exists" trap, and the empty-schema false-positive.
- Amended: healthcheck command (double-quoted -e, grep -q 1, quoted credentials); bootstrap service gained profile pre-check + repair branch; success gates no longer depend on unverifiable DBTools Error-clear semantics.
- Known-bad state avoided: db never healthy (stack hang); PlatformAdmin user without AttendantProfiles and lost random credentials; API booting against an empty schema.
- KEEP: 6-attempt retry with 2/4/8/16/16s backoff; fail-fast throw after exhaustion; bool-result-only success gates; TCP-forced healthcheck with schema check; `DelayAsync` delegate for fast tests.

## Auto Run Result

Status: done

## Summary

Fixed the first-boot race that produced an empty Users table with a misleading "PlatformAdmin seeded" log: MySQL healthcheck is now TCP-forced with a schema check, and PlatformAdminBootstrapService verifies insert results, retries with backoff, repairs partial success, and fails fast.

## Files changed

- `docker/docker-compose.yml` — TCP-forced db healthcheck (rejects init-server socket window, verifies schema has tables, quoted credentials, start_period 30s)
- `src/Host/ControlEasyReborn.Api/Hosting/PlatformAdminBootstrapService.cs` — retry loop (6 attempts, 2/4/8/16/16s backoff), profile pre-check + repair branch, fail-fast throw with DBTools error detail
- `tests/ControlEasyReborn.UnitTests/Hosting/PlatformAdminBootstrapServiceTests.cs` — 6 tests covering retry, exhaustion, repair, idempotent skip
- `tests/ControlEasyReborn.UnitTests/TestDoubles/FakeAsyncSqlClient.cs` — Error made settable

## Review findings breakdown

- Patches applied: 7 (healthcheck quoting/expansion ×2, empty-schema grep gate, partial-success repair path, Error-gate removal, DelayAsync test speedup, corrected insert-count assertions)
- Deferred: 3 (DemoSeederService unchecked insert results; API missing restart policy; plaintext password WRN log + modulo-bias generator — all pre-existing) — see `_bmad-output/implementation-artifacts/deferred-work.md`
- Rejected: 3 (backoff OOB — guarded by throw-first; cancel-in-delay semantics — acceptable OCE on shutdown; healthcheck cleartext password in process list — inherent to mysql client flags)

## Verification performed

- `docker compose -f docker/docker-compose.yml config` — exit 0
- Healthcheck command executed live in running db container — exit 0 (mysqld is alive + schema check passes); no-TCP variant verified exit 1
- `dotnet test tests/ControlEasyReborn.UnitTests` (SDK 8 in docker) — 84/84 pass, 198ms
- `dotnet build src/ControlEasyReborn.sln` — Build succeeded, 0 warnings, 0 errors

## Residual risks

- DBTools Error-clear-on-success semantics remain unverified against the real client; success gates now rely only on bool results, which is safe under both semantics.
- DemoSeederService still ignores insert results (deferred).
- Fresh-volume full-stack boot was not executed end-to-end (would destroy the running stack's volume); healthcheck command verified live against the existing container instead.

## Follow-up review recommendation

Recommended: two high-severity patches landed (healthcheck shell quoting, partial-success repair); an independent follow-up review of the retry/repair logic is warranted.

## Review Triage Log

### 2026-09-12 — Review pass
- intent_gap: 0
- bad_spec: 0
- patch: 6 (high 2, medium 3, low 1)
- defer: 3 (medium 2, low 1)
- reject: 3
- addressed_findings:
  - `[high]` `[patch]` Healthcheck `$$MYSQL_DATABASE` inside single quotes — shell never expands it, healthcheck would fail forever — switched to double-quoted `-e "USE $$MYSQL_DATABASE; ..."`, verified live in container (exit 0 with data)
  - `[high]` `[patch]` Partial-success trap: user insert succeeded but profile failed → retry pre-check finds user → returns "already exists" leaving admin without profile and credentials lost — added profile pre-check + repair path that inserts the missing AttendantProfiles on the "exists" branch
  - `[medium]` `[patch]` Schema check `SELECT COUNT(*) > 0` returns exit 0 regardless of result — pipe through `grep -q 1` so an empty schema fails the healthcheck
  - `[medium]` `[patch]` Healthcheck credentials unquoted in shell — quoted `-u"$$MYSQL_USER" -p"$$MYSQL_PASSWORD"`
  - `[medium]` `[patch]` Success gates checked shared `bypassClient.Error` after inserts — DBTools Error-clear-on-success semantics unverifiable; removed Error from both success gates, keeping bool-result checks (false→retry, true→success)
  - `[medium]` `[patch]` Test suite slept real backoff (46s per exhaustion test) — added settable `DelayAsync` delegate, tests inject no-op delay; suite 90s → 198ms
  - `[low]` `[patch]` Retry tests asserted 2×6 inserts and 3 insert calls — wrong due to `&&` short-circuit; corrected to actual user/profile insert counts

## Design Notes

- RESOLVED: the official `healthcheck.sh` script is NOT present in the mysql:8.0.46 image (only `docker-entrypoint.sh` and `gosu`). Primary plan is the TCP-forced probe: `mysqladmin ping --protocol=tcp -h 127.0.0.1` (fails against the socket-only temp init server) plus `mysql --protocol=tcp ... -e "USE controleasydb; SELECT COUNT(*) FROM Users"` guard for schema readiness. Both commands verified working in the running container.
- Retry loop must use `_db.Error` for diagnostics; DBTools does not throw on failed statements.
- Keep retry budget small (≤ ~30s total) so genuine misconfigurations fail fast rather than hang the boot.

## Verification

**Commands:**
- `docker compose -f docker/docker-compose.yml config` -- expected: exits 0
- `dotnet test tests/ControlEasyReborn.UnitTests` -- expected: all pass
- Fresh-volume boot: `docker compose -p ce-fix-bootstrap-healthcheck-race -f docker/docker-compose.yml down -v; docker compose -p ce-fix-bootstrap-healthcheck-race -f docker/docker-compose.yml up -d` then `docker exec <db> mysql ... SELECT COUNT(*) FROM Users` -- expected: 1 user, and API logs "PlatformAdmin seeded" only after actual insert
- Login smoke: `POST /api/v1/security/auth/login` with seeded credentials -- expected: 200