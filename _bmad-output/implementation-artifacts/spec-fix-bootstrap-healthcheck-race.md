---
title: 'Fix first-boot race: MySQL healthcheck + silent bootstrap failure'
type: 'bugfix'
created: '2026-09-12'
status: 'in-progress'
review_loop_iteration: 0
followup_review_recommended: false
context: []
warnings: []
baseline_revision: d85b496
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

## Review Triage Log

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