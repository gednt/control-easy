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

