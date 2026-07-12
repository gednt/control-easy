#!/usr/bin/env bash
# shellcheck shell=bash
#
# scripts/worktree-down.sh — tear down a per-feature-branch dev worktree
#
# Usage:
#   scripts/worktree-down.sh [--branch <branch>] [--keep-worktree]
#   scripts/worktree-down.sh --help
#
# This script reverses scripts/worktree-up.sh:
#   1. docker compose -p ce-<slug> down -v  (removes containers, networks, volumes)
#   2. Remove the ce-<slug>.localhost hostname via scripts/lib/resolver.sh.
#   3. git worktree remove --force ../ControlEasy.<branch>  (unless --keep-worktree)
#
# Idempotent: re-running on a torn-down worktree exits 0 with "already down".
#
# NFR-4: never operates on the main checkout's Compose project unless
# explicitly invoked from the main checkout itself (we refuse to remove
# the main checkout's volume).

set -euo pipefail

SCRIPT_DIR="$(CDPATH= cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=lib/worktree.sh
. "$SCRIPT_DIR/lib/worktree.sh"
# shellcheck source=lib/resolver.sh
. "$SCRIPT_DIR/lib/resolver.sh"

# ---------------------------------------------------------------------------
# Usage
# ---------------------------------------------------------------------------
ce_usage() {
  cat <<'EOF'
Usage: scripts/worktree-down.sh [OPTIONS]

Tear down a per-feature-branch dev worktree (the inverse of
scripts/worktree-up.sh).

Options:
  --branch <name>   Use this branch (default: current branch).
  --keep-worktree   Do not remove the worktree directory (only tear down the stack).
  --resolver=<m>    Resolver mode: auto (default), nrpt, hosts-file.
  --help, -h        Show this help.
EOF
}

# ---------------------------------------------------------------------------
# Parse args
# ---------------------------------------------------------------------------
CE_BRANCH=""
CE_KEEP_WORKTREE=false
CE_RESOLVER_MODE="auto"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --branch)
      CE_BRANCH="$2"; shift 2 ;;
    --branch=*)
      CE_BRANCH="${1#--branch=}"; shift ;;
    --keep-worktree)
      CE_KEEP_WORKTREE=true; shift ;;
    --resolver=*)
      CE_RESOLVER_MODE="${1#--resolver=}"; shift ;;
    --resolver)
      CE_RESOLVER_MODE="$2"; shift 2 ;;
    --help|-h)
      ce_usage; exit 0 ;;
    *)
      ce_die "Unknown argument: $1 (try --help)" ;;
  esac
done

if [[ -z "$CE_BRANCH" ]]; then
  CE_BRANCH="$(ce_current_branch)"
fi

SLUG="$(ce_branch_to_slug "$CE_BRANCH")"
PROJECT="$(ce_project_name "$CE_BRANCH")"
HOSTNAME="$(ce_traefik_hostname "$CE_BRANCH")"
ENV_FILE="$(ce_env_file "$CE_BRANCH")"
OVERRIDE_FILE="$(ce_override_file "$CE_BRANCH")"

ce_log "Branch:        $CE_BRANCH"
ce_log "Project:       $PROJECT"

# ---------------------------------------------------------------------------
# Idempotency: already down?
# ---------------------------------------------------------------------------
if ce_is_worktree_down "$PROJECT"; then
  ce_log "already down (project=$PROJECT has no running containers)"
  # Still clean up artifacts so re-running after a manual `down -v` is safe.
  if [[ -f "$OVERRIDE_FILE" ]]; then
    rm -f "$OVERRIDE_FILE"
    ce_log "Removed stale override: $OVERRIDE_FILE"
  fi
  if [[ -f "$ENV_FILE" ]]; then
    rm -f "$ENV_FILE"
    ce_log "Removed stale env: $ENV_FILE"
  fi
  exit 0
fi

# ---------------------------------------------------------------------------
# NFR-4: refuse to remove the main checkout's project (ce or controleasy)
# ---------------------------------------------------------------------------
case "$PROJECT" in
  ce|ce-main|ce-master|controleasy)
    ce_die "Refusing to tear down the main checkout's project ($PROJECT). Use docker compose -f docker/docker-compose.yml down -v from the main checkout manually."
    ;;
esac

# ---------------------------------------------------------------------------
# Step 1: docker compose -p ce-<slug> down -v
# ---------------------------------------------------------------------------
ce_log "Tearing down compose project $PROJECT (containers, networks, volumes)"
COMPOSE_FILE_BASE="docker/docker-compose.yml"
if [[ -f "$OVERRIDE_FILE" ]]; then
  docker compose -p "$PROJECT" \
    -f "$COMPOSE_FILE_BASE" \
    -f "$OVERRIDE_FILE" \
    down -v
else
  # Override file may have been removed manually; tear down with just the base.
  docker compose -p "$PROJECT" \
    -f "$COMPOSE_FILE_BASE" \
    down -v
fi

# ---------------------------------------------------------------------------
# Step 2: Remove the resolver entry
# ---------------------------------------------------------------------------
ce_log "Removing hostname $HOSTNAME from resolver"
resolver_remove "$HOSTNAME" "$CE_RESOLVER_MODE" || true

# ---------------------------------------------------------------------------
# Step 3: Remove generated artifacts
# ---------------------------------------------------------------------------
rm -f "$OVERRIDE_FILE" "$ENV_FILE"
ce_log "Removed $OVERRIDE_FILE and $ENV_FILE"

# ---------------------------------------------------------------------------
# Step 4: Remove the worktree directory (unless --keep-worktree)
# ---------------------------------------------------------------------------
if [[ "$CE_KEEP_WORKTREE" == "false" ]]; then
  WORKTREE_DIR="$(ce_worktree_dir "$CE_BRANCH")"
  if [[ -d "$WORKTREE_DIR" ]]; then
    ce_log "Removing worktree directory: $WORKTREE_DIR"
    git worktree remove --force "$WORKTREE_DIR" || true
  fi
fi

ce_log "Worktree $CE_BRANCH is down."