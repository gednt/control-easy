# CI API Readiness Reliability

## User Story 1 - OpenAPI job waits for API startup (Priority: P1)

As a contributor, I need CI to allow enough time for the containerized API to start so valid changes are not rejected by a startup timing race.

**Independent test**: Start the CI Compose services with dynamic ports and verify readiness succeeds when the API listens after 60 seconds but within the configured deadline.

### Acceptance Scenarios

1. **Given** API initialization takes more than 60 seconds, **when** it listens before the deadline, **then** CI continues.
2. **Given** the API never listens, **when** the deadline expires, **then** CI fails and prints container logs.
3. **Given** `/health` returns 200 or 503, **when** probed, **then** CI treats the API process as ready.

## User Story 2 - Later steps reuse dynamic ports (Priority: P2)

As a contributor, I need later OpenAPI and E2E steps to reuse ports discovered at repository root so their Angular working directory cannot break Compose path resolution or lose the selected port.

**Independent test**: Verify both consumers use exported URLs without invoking Compose from the Angular directory.

### Acceptance Scenarios

1. **Given** OpenAPI generation runs from the Angular directory, **then** it uses the API URL exported by readiness.
2. **Given** E2E tests run from the Angular directory, **then** Playwright receives the exported reverse-proxy URL.

## Edge Cases

- A port is not published: readiness fails with diagnostics.
- The API reports degraded database health: 503 proves the API is listening.
- Startup exceeds the deadline: CI remains bounded and emits logs.

## Functional Requirements

- **FR-001**: CI MUST wait up to 180 seconds for the API process to listen.
- **FR-002**: CI MUST accept health responses 200 and 503 as process readiness.
- **FR-003**: CI MUST emit logs when readiness expires.
- **FR-004**: CI MUST export the discovered API URL for OpenAPI generation.
- **FR-005**: CI MUST export the reverse-proxy URL for E2E tests.
- **FR-006**: Consumers below repository root MUST NOT resolve Compose through their current directory.

## Success Criteria

- **SC-001**: An API startup taking 61-180 seconds passes readiness.
- **SC-002**: A non-starting API fails within 190 seconds and includes logs.
- **SC-003**: OpenAPI and E2E consumers receive localhost URLs with numeric dynamic ports.
- **SC-004**: Both readiness sites follow identical behavior.

## Assumptions

- Ubuntu runners provide Bash, curl, Docker, and Compose v2.
- HTTP 503 proves ASP.NET Core is serving even if a dependency is degraded.
- OpenAPI generation does not require a healthy database.

