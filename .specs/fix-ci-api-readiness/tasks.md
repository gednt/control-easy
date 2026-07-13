# Tasks: CI API Readiness Reliability

## Phase 1: Specification and diagnosis

- [X] T001 Document the timeout and port propagation defects in `.specs/fix-ci-api-readiness/bugfix.md`
- [X] T002 Document decisions and validation in `.specs/fix-ci-api-readiness/plan.md`, `.specs/fix-ci-api-readiness/research.md`, and `.specs/fix-ci-api-readiness/quickstart.md`

## Phase 2: User Story 1 - Reliable startup wait (P1)

**Independent Test**: Both readiness blocks tolerate startup beyond 60 seconds, accept 200/503, and fail with logs after 180 seconds.

- [X] T003 [US1] Replace count-based readiness with wall-clock deadlines in `.github/workflows/ci.yml`
- [X] T004 [US1] Validate both readiness shell blocks and diagnostics in `.github/workflows/ci.yml`

## Phase 3: User Story 2 - Dynamic URL propagation (P2)

**Independent Test**: OpenAPI and E2E steps use exported URLs without Compose calls from the Angular directory.

- [X] T005 [US2] Export API and proxy URLs through GitHub Actions environment state in `.github/workflows/ci.yml`
- [X] T006 [US2] Consume exported URLs in OpenAPI and Playwright steps in `.github/workflows/ci.yml`

## Phase 4: Verification

- [X] T007 Run workflow structural checks and inspect the focused `.github/workflows/ci.yml` diff
- [X] T008 Rebuild Compose API/web and run Unit, Integration, and Architecture tests per `AGENTS.md`

## Task Dependency Graph

```json
{"waves":[{"wave":1,"tasks":["T001","T002"]},{"wave":2,"tasks":["T003","T005"]},{"wave":3,"tasks":["T004","T006"]},{"wave":4,"tasks":["T007"]},{"wave":5,"tasks":["T008"]},{"wave":6,"tasks":["T009","T010"]},{"wave":7,"tasks":["T011"]},{"wave":8,"tasks":["T012"]}]}
```

## Implementation Strategy

T003-T004 resolve the reported failure; T005-T006 remove the next deterministic failures in the same jobs; verification closes the task.

## Phase 5: Corrective Follow-up - Containerized Runner Networking

**Independent Test**: A simulated job container attached to the Compose network reaches `api:8080`, while host execution retains its published-port fallback.

- [X] T009 [US1] Detect and attach containerized job runners to the Compose project network in `.github/workflows/ci.yml`
- [X] T010 [US2] Export Compose-DNS URLs for containerized runners and published-port URLs for host runners in `.github/workflows/ci.yml`
- [X] T011 [US2] Disconnect containerized runners during always-run cleanup in `.github/workflows/ci.yml`
- [X] T012 Validate YAML, simulated container networking, Compose health, and all three .NET test projects
