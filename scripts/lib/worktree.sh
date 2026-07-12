#!/usr/bin/env bash
# shellcheck shell=bash
#
# scripts/lib/worktree.sh — shared helpers for scripts/worktree-up.sh
# and scripts/worktree-down.sh.
#
# Provides: branch detection, branch→project-name conversion, worktree
# index → port allocation, Traefik hostname derivation, and idempotency
# guards.
#
# Conventions (see .specs/devcontainers/design.md):
#   - Branch name:                feat/<spec-id>-<short-slug>
#   - Worktree path:              ../ControlEasy.<branch-with-slashes>
#   - Compose project name:       ce-<branch-with-slashes-as-hyphens>
#   - Traefik hostname:           ce-<branch-with-slashes-as-hyphens>.localhost
#   - Published host port:        18080 + (worktree-index * 10)
#   - DB volume name:             ce-<branch-with-slashes-as-hyphens>-mysql-data
#
# This file is sourced, not executed.

set -euo pipefail

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------
readonly CE_BASE_PORT=18080
readonly CE_PORT_STEP=10
readonly CE_HOST_SUFFIX=".localhost"
readonly CE_WORKTREE_PREFIX="ControlEasy."
readonly CE_PROJECT_PREFIX="ce-"
readonly CE_VOLUME_SUFFIX="-mysql-data"
readonly CE_TRAEFIK_PORT_NAME="traefik"  # service name in docker-compose.yml
readonly CE_RESOLVER_FILE="${HOME}/.control-easy/hosts"

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
ce_log()  { printf '[worktree] %s\n' "$*"; }
ce_warn() { printf '[worktree][WARN] %s\n' "$*" >&2; }
ce_err()  { printf '[worktree][ERROR] %s\n' "$*" >&2; }
ce_die()  { ce_err "$*"; exit 1; }

# Detect the operating system family: linux, darwin, or windows (under Git
# Bash / WSL).
ce_detect_os() {
  case "$(uname -s)" in
    Linux*)   echo "linux" ;;
    Darwin*)  echo "darwin" ;;
    MINGW*|MSYS*|CYGWIN*) echo "windows" ;;
    *) ce_die "Unsupported OS: $(uname -s)" ;;
  esac
}

# Return the current git branch, with a sanity check that we are NOT on the
# main checkout's default branch (the scripts must never operate on the
# main checkout's Compose project unless explicitly invoked from the main
# checkout itself).
ce_current_branch() {
  local branch
  branch="$(git rev-parse --abbrev-ref HEAD)" || ce_die "Not a git repository."
  [[ -n "$branch" ]] || ce_die "Detached HEAD; cannot derive a worktree branch."
  echo "$branch"
}

# Convert a branch name to a slug with slashes as hyphens:
#   feat/dashboard-live-stats -> feat-dashboard-live-stats
ce_branch_to_slug() {
  local branch="$1"
  printf '%s' "$branch" | tr '/' '-'
}

# Compose project name for a branch: ce-<branch-with-slashes-as-hyphens>
ce_project_name() {
  local branch="$1"
  printf '%s%s' "$CE_PROJECT_PREFIX" "$(ce_branch_to_slug "$branch")"
}

# Traefik hostname for a branch: ce-<branch-with-slashes-as-hyphens>.localhost
ce_traefik_hostname() {
  local branch="$1"
  printf '%s%s%s' \
    "$CE_PROJECT_PREFIX" \
    "$(ce_branch_to_slug "$branch")" \
    "$CE_HOST_SUFFIX"
}

# MySQL volume name for a branch: ce-<...>-mysql-data
ce_mysql_volume_name() {
  local branch="$1"
  printf '%s%s%s' \
    "$CE_PROJECT_PREFIX" \
    "$(ce_branch_to_slug "$branch")" \
    "$CE_VOLUME_SUFFIX"
}

# Worktree directory name (sibling of the main checkout):
#   ../ControlEasy.<branch-with-slashes>
ce_worktree_dir() {
  local branch="$1"
  local repo_root
  repo_root="$(git rev-parse --show-toplevel)"
  local parent
  parent="$(dirname "$repo_root")"
  printf '%s/%s%s' "$parent" "$CE_WORKTREE_PREFIX" "$branch"
}

# Allocate a port for this worktree based on the worktree index (the
# position of this worktree in `git worktree list`). The main checkout
# is index 0 and uses the standard :8080 (NOT 18080); worktrees start at
# index 1 and use 18080 + index*10.
#
# Outputs the port number to stdout. Aborts with a clear message on
# collision detection (NFR-5).
ce_allocate_port() {
  local branch="$1"
  local index
  index="$(ce_worktree_index "$branch")" || return 1
  local port=$(( CE_BASE_PORT + index * CE_PORT_STEP ))
  echo "$port"
}

# Return the 1-based worktree index for the given branch (the main
# checkout is index 0). The index is the ordinal position in
# `git worktree list --porcelain`, excluding the main checkout line.
ce_worktree_index() {
  local branch="$1"
  local slug
  slug="$(ce_branch_to_slug "$branch")"
  local list
  list="$(git worktree list --porcelain)" || ce_die "git worktree list failed"
  local index=0
  local found=0
  # shellcheck disable=SC2034
  while IFS= read -r line; do
    if [[ "$line" == "worktree "* ]]; then
      ((index++)) || true
      if [[ "$line" == *"$CE_WORKTREE_PREFIX$slug"* ]]; then
        found=1
        # The main checkout is index 1 (git worktree list's first entry).
        # Worktrees start at index 2, so we subtract 1.
        echo $(( index - 1 ))
        return 0
      fi
    fi
  done <<< "$list"
  ce_die "Worktree for branch '$branch' not found in 'git worktree list'."
}

# Idempotency guard: is this worktree already up?
# We treat "already up" as: the compose project has at least one running
# container.
ce_is_worktree_up() {
  local project="$1"
  local count
  count="$(docker compose -p "$project" ps --quiet 2>/dev/null | wc -l)"
  [[ "$count" -gt 0 ]]
}

# Idempotency guard: is this worktree already down?
ce_is_worktree_down() {
  local project="$1"
  local count
  count="$(docker compose -p "$project" ps --quiet 2>/dev/null | wc -l)"
  [[ "$count" -eq 0 ]]
}

# Path to the per-worktree env file: docker/.env.worktree.<slug>
ce_env_file() {
  local branch="$1"
  local repo_root
  repo_root="$(git rev-parse --show-toplevel)"
  printf '%s/docker/.env.worktree.%s' "$repo_root" "$(ce_branch_to_slug "$branch")"
}

# Path to the per-worktree compose override: docker/docker-compose.worktree.<slug>.yml
ce_override_file() {
  local branch="$1"
  local repo_root
  repo_root="$(git rev-parse --show-toplevel)"
  printf '%s/docker/docker-compose.worktree.%s.yml' \
    "$repo_root" "$(ce_branch_to_slug "$branch")"
}

# Path to the checked-in override template.
ce_override_template() {
  local repo_root
  repo_root="$(git rev-parse --show-toplevel)"
  printf '%s/docker/docker-compose.worktree.template.yml' "$repo_root"
}

# Generate a deterministic, per-worktree JWT signing key so worktrees do
# NOT reuse the main checkout's key (design.md "Worktree bring-up sequence"
# step 3). NOT a production secret — it is a dev-only key.
ce_jwt_signing_key() {
  local branch="$1"
  printf 'dev-worktree-%s-%s' \
    "$(ce_branch_to_slug "$branch")" \
    "$(printf '%s' "$branch" | sha256sum | cut -c1-32)"
}