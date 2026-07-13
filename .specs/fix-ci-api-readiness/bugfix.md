# Bug Fix: CI API Readiness Timeout

## Root Cause

Thirty probes separated by two seconds give the API about 60 seconds to listen. In the failing run every probe returned HTTP 000, then diagnostics showed `Now listening` immediately after the loop ended. The application started successfully just outside the retry window.

Two downstream defects would fail next: the OpenAPI step recomputes the port from the Angular working directory, where `docker/docker-compose.yml` does not exist; and the E2E step computes a shell-local proxy port that cannot populate its separately declared `E2E_BASE_URL`.

## Fix

- Use a 180-second wall-clock readiness deadline and probe `127.0.0.1`.
- Retain 200/503 readiness semantics.
- Export discovered URLs through `$GITHUB_ENV` and consume them later.
- Preserve Compose log diagnostics on timeout.

## Scope

Only `.github/workflows/ci.yml` and this spec folder change. Runtime application behavior is unchanged.

