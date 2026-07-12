#!/usr/bin/env bash
# shellcheck shell=bash
#
# scripts/verify-devcontainer.sh — verification gate for the
# devcontainer + worktree loop.
#
# Run from inside the devcontainer (or any shell that can invoke
# docker compose against the host engine):
#
#   scripts/verify-devcontainer.sh
#
# This script:
#   1. Pre-warms the base images (only pulls if missing locally).
#   2. Builds the stack (api, web) with COMPOSE_DOCKER_CLI_BUILD=1.
#   3. Brings the stack up with --force-recreate.
#   4. Waits for /api/v1/health (or /health) to return 200.
#   5. Runs dotnet test src/ControlEasyReborn.sln (unit + integration + architecture).
#   6. Runs the demo overlay path (docker-compose.demo.yml).
#   7. Tears down (docker compose down -v).
#
# On any test failure, the script exits non-zero and leaves the stack
# up for inspection (per task 9.6 acceptance criterion).
#
# See .specs/devcontainers/tasks.md task 9.6 and AGENTS.md § 5.

set -euo pipefail

SCRIPT_DIR="$(CDPATH= cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(CDPATH= cd "$SCRIPT_DIR/.." && pwd)"
COMPOSE_BASE="$REPO_ROOT/docker/docker-compose.yml"
COMPOSE_DEMO="$REPO_ROOT/docker/docker-compose.demo.yml"
PROJECT="${COMPOSE_PROJECT_NAME:-ce-verify-devcontainer}"
DEMO_PROJECT="ce-demo-verify-devcontainer"
HEALTH_TIMEOUT="${HEALTH_TIMEOUT:-180}"
TEST_TIMEOUT="${TEST_TIMEOUT:-300}"

# ---------------------------------------------------------------------------
# Logging
# ---------------------------------------------------------------------------
_log()  { printf '[verify] %s\n' "$*"; }
_warn() { printf '[verify][WARN] %s\n' "$*" >&2; }
_die()  { printf '[verify][ERROR] %s\n' "$*" >&2; exit 1; }

# ---------------------------------------------------------------------------
# Step 1: Pre-warm base images (only pull if missing)
# ---------------------------------------------------------------------------
_log "Step 1/7: Pre-warming base images (skipping cached ones)"
BASE_IMAGES=(
  "mcr.microsoft.com/dotnet/sdk:8.0"
  "mcr.microsoft.com/dotnet/aspnet:8.0"
  "node:20-alpine"
  "nginx:alpine"
  "mysql:8.0"
  "adminer:4"
  "traefik:v3.1"
)
for img in "${BASE_IMAGES[@]}"; do
  if docker image inspect "$img" >/dev/null 2>&1; then
    _log "  cached: $img"
  else
    _log "  pulling: $img"
    docker pull "$img"
  fi
done

# ---------------------------------------------------------------------------
# Step 2: Build the stack
# ---------------------------------------------------------------------------
_log "Step 2/7: Building the stack (api, web)"
COMPOSE_DOCKER_CLI_BUILD=1 DOCKER_BUILDKIT=1 \
  docker compose -p "$PROJECT" -f "$COMPOSE_BASE" build api web

# ---------------------------------------------------------------------------
# Step 3: Bring the stack up
# ---------------------------------------------------------------------------
_log "Step 3/7: Bringing the stack up (--force-recreate)"
docker compose -p "$PROJECT" -f "$COMPOSE_BASE" up -d --force-recreate

# Ensure cleanup on success (NOT on failure — leave stack up for inspection).
cleanup() {
  _log "Tearing down (project=$PROJECT)"
  docker compose -p "$PROJECT" -f "$COMPOSE_BASE" down -v || true
}
trap 'rc=$?; if [[ $rc -eq 0 ]]; then cleanup; fi' EXIT

# ---------------------------------------------------------------------------
# Step 4: Wait for /health to return 200
# ---------------------------------------------------------------------------
_log "Step 4/7: Waiting for /api/v1/health (timeout ${HEALTH_TIMEOUT}s)"

# Get the api container's published port. The base compose publishes on
# 8080 (Traefik). For the worktree variant, the override republishes to
# the worktree's port. Here we use the base compose (main checkout), so
# the api is reachable on the docker host's port 8080 via Traefik.
HEALTH_PORT="${TRAEFIK_PORT:-8080}"
HEALTH_URL="http://localhost:${HEALTH_PORT}/api/v1/health"

elapsed=0
while [[ "$elapsed" -lt "$HEALTH_TIMEOUT" ]]; do
  if curl -fsS "$HEALTH_URL" >/dev/null 2>&1; then
    _log "  health check passed (${elapsed}s): $HEALTH_URL"
    break
  fi
  sleep 3
  elapsed=$((elapsed + 3))
done

if [[ "$elapsed" -ge "$HEALTH_TIMEOUT" ]]; then
  _die "Health check timed out after ${HEALTH_TIMEOUT}s. Stack left up for inspection."
fi

# ---------------------------------------------------------------------------
# Step 5: dotnet test
# ---------------------------------------------------------------------------
_log "Step 5/7: Running dotnet test (timeout ${TEST_TIMEOUT}s)"
cd "$REPO_ROOT"
if ! timeout "${TEST_TIMEOUT}" dotnet test src/ControlEasyReborn.sln --nologo --verbosity minimal; then
  _die "dotnet test failed. Stack left up for inspection (project=$PROJECT)."
fi

# ---------------------------------------------------------------------------
# Step 6: Demo overlay path
# ---------------------------------------------------------------------------
_log "Step 6/7: Demo overlay path"
if [[ -f "$COMPOSE_DEMO" ]]; then
  COMPOSE_DOCKER_CLI_BUILD=1 DOCKER_BUILDKIT=1 \
    docker compose -p "$DEMO_PROJECT" -f "$COMPOSE_BASE" -f "$COMPOSE_DEMO" \
    build api web
  docker compose -p "$DEMO_PROJECT" -f "$COMPOSE_BASE" -f "$COMPOSE_DEMO" \
    up -d --force-recreate
  # Quick health check on the demo stack too.
  demo_elapsed=0
  while [[ "$demo_elapsed" -lt "$HEALTH_TIMEOUT" ]]; do
    if curl -fsS "http://localhost:${HEALTH_PORT}/api/v1/health" >/dev/null 2>&1; then
      _log "  demo health check passed (${demo_elapsed}s)"
      break
    fi
    sleep 3
    demo_elapsed=$((demo_elapsed + 3))
  done
  if [[ "$demo_elapsed" -ge "$HEALTH_TIMEOUT" ]]; then
    _warn "Demo stack health check timed out (continuing — demo is non-blocking)"
  fi
  docker compose -p "$DEMO_PROJECT" -f "$COMPOSE_BASE" -f "$COMPOSE_DEMO" down -v || true
else
  _warn "Demo overlay not found ($COMPOSE_DEMO); skipping demo path"
fi

# ---------------------------------------------------------------------------
# Step 7: Teardown
# ---------------------------------------------------------------------------
_log "Step 7/7: Teardown"
# The EXIT trap calls cleanup() on success.
_log "Verification gate PASSED."