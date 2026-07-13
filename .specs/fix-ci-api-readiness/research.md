# Research: CI API Readiness Reliability

## Wall-clock deadline

**Decision**: Probe until Bash `SECONDS` reaches a deadline 180 seconds away.  
**Rationale**: The observed startup completed just after 60 seconds; a deadline expresses the bound independently of curl duration.  
**Alternatives**: More retries couple timing to each probe; Compose `--wait` requires a new API healthcheck and broadens scope.

## Cross-step URLs

**Decision**: Write complete service URLs to `$GITHUB_ENV`.  
**Rationale**: Later steps receive stable values regardless of working directory.  
**Alternatives**: Recomputing ports duplicates discovery; step outputs add needless same-job YAML.

## Health semantics

**Decision**: Accept 200 or 503.  
**Rationale**: Either proves the API is serving; OpenAPI does not require database health.

## Runner network topology

**Decision**: If `docker inspect "$HOSTNAME"` identifies the job container, connect it to `${COMPOSE_PROJECT_NAME}_default` and use Compose service DNS. Otherwise use published host ports.
**Rationale**: Loopback inside a Gitea/Act job container cannot reach ports published on the Docker host. Joining the project network gives the job direct, stable access to `api:8080` and `reverse-proxy:80` without platform-specific host gateway names.
**Alternatives**: Longer timeouts never repair unreachable loopback; `host.docker.internal` is not portable across Linux runner configurations; spawning a curl sidecar would not make the runner-hosted OpenAPI generator or Playwright browser network-reachable.

## Cleanup

**Decision**: Persist the runner container/network identifiers and disconnect during `if: always()` cleanup.
**Rationale**: Compose cannot remove its default network while the external job container remains attached.
