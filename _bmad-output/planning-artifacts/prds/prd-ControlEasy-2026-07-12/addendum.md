# Addendum — Dev Containers & Worktrees PRD

> Operational details, runbook-style guidance, and rejected alternatives that support the PRD but do not belong in its main narrative.

## 1. Post-Task Cleanup Convention

Every agent, automated test, or contributor that creates ephemeral Docker artifacts must leave the engine in a clean state. The canonical cleanup command is:

```bash
docker system prune -f
```

### 1.1 When to run it

- After a successful verification gate, once the temporary stack is no longer needed.
- After any automated test job that creates ephemeral containers, networks, or build cache.
- Before a clean verification run when stale artifacts from previous tasks may interfere.
- When the active worktree's `docker compose up` behaves unexpectedly due to dangling images, networks, or volumes.

### 1.2 Common maintenance tasks

| Task | Command |
|---|---|
| Remove dangling images, networks, and build cache (keep volumes) | `docker system prune -f --volumes=false` |
| Remove only dangling images | `docker image prune -f` |
| Reset a worktree database (intentionally destructive) | `docker compose -p ce-<id> down -v` |
| Tail API logs of the active worktree | `docker compose -p ce-<id> logs -f api` |

### 1.3 What to preserve

- Named volumes that contain intentional data (e.g., the main checkout's `mysql-data` if the team keeps seed data there).
- Running containers from other worktrees or from the main checkout.

> **Caution:** `docker system prune -f` removes **all** dangling artifacts on the host engine, not only those created by the current task. Run it only after confirming that no other critical container or volume is in use.

## 2. Rejected alternatives

### 2.1 Docker-in-Docker (DinD)

A devcontainer running `docker:dind` would fully isolate Docker state from the host. It was rejected because it creates a second engine that can drift from the host's engine, violating the Constitution v1.3.0 Principle V that the verification gate must produce the same result on the dev shell as on the host runtime.

### 2.2 No Docker inside the devcontainer

A devcontainer containing only SDK + Node + Playwright, shelling out to the host's `docker compose` via `host.docker.internal` or an SSH wrapper, was rejected as the default because it is slower and more surprising for contributors. It remains acceptable as an opt-in `scripts/worktree-verify-from-host.sh` for CI parity.

## 3. Fallback for USB / camera passthrough

`.specs/3 - photo-capture-hardware-integration/` requires USB / camera passthrough, which devcontainers do not support portably. The devcontainer's `runArgs` must not request `--privileged` or `--device`. That spec keeps its documented host-shell + host-browser fallback.
