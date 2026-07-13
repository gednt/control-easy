# Bug Fix: CI API Readiness Timeout

## Initial Diagnosis

Thirty probes separated by two seconds give the API about 60 seconds to listen. In the failing run every probe returned HTTP 000, then diagnostics showed `Now listening` immediately after the loop ended. The application started successfully just outside the retry window.

Two downstream defects would fail next: the OpenAPI step recomputes the port from the Angular working directory, where `docker/docker-compose.yml` does not exist; and the E2E step computes a shell-local proxy port that cannot populate its separately declared `E2E_BASE_URL`.

## Confirmed Root Cause

Run 38 still returned HTTP 000 for the full 180-second deadline even though the API logged `Now listening`. The self-hosted Gitea/Act job runs inside a Docker container. `docker compose port` returns a port published on the Docker host, but `127.0.0.1` inside the job container points back to that job container. The probe therefore cannot reach the API regardless of timeout length.

## Fix

- Use a 180-second wall-clock readiness deadline and probe `127.0.0.1`.
- Retain 200/503 readiness semantics.
- Export discovered URLs through `$GITHUB_ENV` and consume them later.
- Preserve Compose log diagnostics on timeout.
- Detect whether the job shell itself is a Docker container visible through the mounted socket.
- Attach containerized jobs to `${COMPOSE_PROJECT_NAME}_default` and use Compose DNS (`api:8080`, `reverse-proxy:80`).
- Keep dynamic published-port URLs as the fallback for host-based runners.
- Disconnect the job container before Compose cleanup removes the network.

## Scope

The correction touches `.github/workflows/ci.yml`, the Compose Traefik labels, the matching file-provider router rules, and this spec folder. Application behavior is unchanged; Traefik additionally accepts its internal Compose service hostname for containerized E2E runners.
