# CI API Readiness Reliability

## User Story 1 - OpenAPI job waits for API startup (Priority: P1)

As a contributor, I need CI to reach the Compose API from both host-based and containerized runners so valid changes are not rejected by runner network topology.

**Independent test**: Start the CI Compose services from a containerized runner and verify readiness reaches the API through the Compose network rather than the runner's loopback interface.

### Acceptance Scenarios

1. **Given** API initialization takes more than 60 seconds, **when** it listens before the deadline, **then** CI continues.
2. **Given** the API never listens, **when** the deadline expires, **then** CI fails and prints container logs.
3. **Given** `/health` returns 200 or 503, **when** probed, **then** CI treats the API process as ready.
4. **Given** the job runs inside a Docker container, **when** services start, **then** the job container joins the Compose network and resolves the API by service name.
5. **Given** the job runs directly on a host, **when** services start, **then** CI uses the dynamically published host port.

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
- **FR-004**: CI MUST export a runner-reachable API URL for OpenAPI generation.
- **FR-005**: CI MUST export a runner-reachable reverse-proxy URL for E2E tests.
- **FR-006**: Consumers below repository root MUST NOT resolve Compose through their current directory.
- **FR-007**: Containerized job runners MUST join the Compose project network and use Docker service DNS.
- **FR-008**: Cleanup MUST disconnect a containerized runner before Compose removes its network.

## Success Criteria

- **SC-001**: An API startup taking 61-180 seconds passes readiness.
- **SC-002**: A non-starting API fails within 190 seconds and includes logs.
- **SC-003**: OpenAPI and E2E consumers receive URLs reachable from their runner topology.
- **SC-004**: Both readiness sites follow identical behavior.

## Assumptions

- Ubuntu runners provide Bash, curl, Docker, and Compose v2.
- HTTP 503 proves ASP.NET Core is serving even if a dependency is degraded.
- OpenAPI generation does not require a healthy database.
- A containerized Gitea/Act job exposes its container ID through `HOSTNAME` and the mounted Docker socket can inspect and connect that container.
