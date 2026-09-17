#!/usr/bin/env bash
# shellcheck shell=bash
#
# scripts/verify-ci-local.sh — local execution of the exact CI verification gate.
#
# Runs all 5 CI stages locally:
#   1. Format & code style (dotnet format --verify-no-changes)
#   2. Build (dotnet build Release)
#   3. Fast tests (UnitTests + ArchitectureTests)
#   4. Integration tests (IntegrationTests with Testcontainers MySQL)
#   5. Web check (Angular production container build)
#   6. OpenAPI drift check (API swagger generation + ng-openapi-gen drift check)
#
# Exit codes:
#   0 - All stages passed; writes stamp to $(git rev-parse --git-dir)/ci-local-passed.stamp
#   non-zero - Verification failed at one of the stages.

set -euo pipefail

SCRIPT_DIR="$(CDPATH= cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(CDPATH= cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT"

GIT_DIR="$(git rev-parse --absolute-git-dir 2>/dev/null || echo "$REPO_ROOT/.git")"
STAMP_FILE="$GIT_DIR/ci-local-passed.stamp"
FAST_STAMP_FILE="$GIT_DIR/ci-local-fast-passed.stamp"

_log()  { printf '\033[1;34m[ci-local]\033[0m %s\n' "$*"; }
_ok()   { printf '\033[1;32m[ci-local][PASS]\033[0m %s\n' "$*"; }
_warn() { printf '\033[1;33m[ci-local][WARN]\033[0m %s\n' "$*" >&2; }
_die()  { printf '\033[1;31m[ci-local][FAIL]\033[0m %s\n' "$*" >&2; exit 1; }

compute_sha256() {
  if command -v sha256sum >/dev/null 2>&1; then
    sha256sum | awk '{print $1}'
  elif command -v shasum >/dev/null 2>&1; then
    shasum -a 256 | awk '{print $1}'
  elif command -v python3 >/dev/null 2>&1; then
    python3 -c "import hashlib, sys; print(hashlib.sha256(sys.stdin.buffer.read()).hexdigest())"
  else
    git hash-object --stdin
  fi
}

FAST_ONLY=false
SKIP_INTEGRATION=false
SKIP_DOCKER=false
SKIP_OPENAPI=false

for arg in "$@"; do
  case "$arg" in
    --fast)
      FAST_ONLY=true
      SKIP_INTEGRATION=true
      SKIP_DOCKER=true
      SKIP_OPENAPI=true
      ;;
    --skip-integration)
      SKIP_INTEGRATION=true
      ;;
    --skip-docker)
      SKIP_DOCKER=true
      ;;
    --skip-openapi)
      SKIP_OPENAPI=true
      ;;
    --help|-h)
      cat <<'EOF'
Usage: scripts/verify-ci-local.sh [OPTIONS]

Run the exact CI pipeline stages locally before considering work "done".

Options:
  --fast              Run fast checks only (format, build, unit + arch tests).
  --skip-integration  Skip Testcontainers integration tests.
  --skip-docker       Skip Docker web build.
  --skip-openapi      Skip in-process OpenAPI drift check.
  -h, --help          Show this message.
EOF
      exit 0
      ;;
    *)
      _die "Unknown argument: $arg"
      ;;
  esac
done

_log "Starting local CI verification gate..."

if ! command -v dotnet >/dev/null 2>&1; then
  _die "'dotnet' SDK was not found in PATH. Per AGENTS.md Constitution Principle V, development and verification must run inside the devcontainer (or have .NET 8 SDK installed). Please attach to the devcontainer or run inside the api container."
fi

# ---------------------------------------------------------------------------
# Stage 1: .NET Format Check
# ---------------------------------------------------------------------------
if [ "$FAST_ONLY" = true ] && ! git status --porcelain 2>/dev/null | grep -q -E '\.cs$'; then
  _ok "Stage 1/6: Skipped C# format check (no .cs files modified)."
else
  _log "Stage 1/6: Checking C# code format..."
  dotnet restore src/ControlEasyReborn.sln --verbosity quiet --nologo
  if ! dotnet format src/ControlEasyReborn.sln --verify-no-changes --no-restore; then
    _die "Format check failed! Run 'dotnet format src/ControlEasyReborn.sln' to fix formatting."
  fi
  _ok "Code format verified."
fi

# ---------------------------------------------------------------------------
# Stage 2: Solution Build (Release)
# ---------------------------------------------------------------------------
_log "Stage 2/6: Building solution in Release mode..."
dotnet build src/ControlEasyReborn.sln --configuration Release --nologo
_ok "Solution built successfully."

# ---------------------------------------------------------------------------
# Stage 3: Fast Tests (Unit + Architecture)
# ---------------------------------------------------------------------------
_log "Stage 3/6: Running Unit & Architecture tests..."
dotnet test tests/ControlEasyReborn.UnitTests --configuration Release --no-build --nologo --logger "console;verbosity=minimal"
dotnet test tests/ControlEasyReborn.ArchitectureTests --configuration Release --no-build --nologo --logger "console;verbosity=minimal"
_ok "Unit and Architecture tests passed."

# ---------------------------------------------------------------------------
# Stage 4: Integration Tests
# ---------------------------------------------------------------------------
if [ "$SKIP_INTEGRATION" = true ]; then
  _warn "Stage 4/6: Skipping integration tests (--skip-integration specified)."
else
  _log "Stage 4/6: Running Integration tests (Testcontainers MySQL)..."
  dotnet test tests/ControlEasyReborn.IntegrationTests --configuration Release --no-build --nologo --logger "console;verbosity=minimal"
  _ok "Integration tests passed."
fi

# ---------------------------------------------------------------------------
# Stage 5: Web Production Build Check
# ---------------------------------------------------------------------------
if [ "$SKIP_DOCKER" = true ]; then
  _warn "Stage 5/6: Skipping web build check (--skip-docker specified)."
else
  _log "Stage 5/6: Verifying Web production build via Docker..."
  if command -v docker >/dev/null 2>&1; then
    docker build -f docker/web.Dockerfile -t ce-local-verify-web:latest .
    docker rmi ce-local-verify-web:latest >/dev/null 2>&1 || true
    _ok "Web production build verified."
  else
    _warn "Docker is not available; skipping Web container build check."
  fi
fi

# ---------------------------------------------------------------------------
# Stage 6: OpenAPI Drift Check
# ---------------------------------------------------------------------------
if [ "$SKIP_OPENAPI" = true ]; then
  _warn "Stage 6/6: Skipping OpenAPI drift check (--skip-openapi specified)."
else
  _log "Stage 6/6: Verifying OpenAPI client and swagger drift..."
  SWAGGER_TEMP="$(mktemp /tmp/ce-swagger-XXXXXX.json)"
  find_free_port() {
    if command -v python3 >/dev/null 2>&1; then
      python3 -c 'import socket; s=socket.socket(); s.bind(("", 0)); print(s.getsockname()[1]); s.close()' 2>/dev/null && return 0
    fi
    local hash
    hash="$(basename "$REPO_ROOT" | compute_sha256 | tr -dc '0-9' | cut -c1-4)"
    echo "$(( 18100 + (hash % 800) ))"
  }
  API_PORT="${CE_SWAGGER_PORT:-$(find_free_port)}"
  API_DLL="$REPO_ROOT/src/Host/ControlEasyReborn.Api/bin/Release/net8.0/ControlEasyReborn.Api.dll"
  API_BIN_DIR="$(dirname "$API_DLL")"

  if [ ! -f "$API_DLL" ]; then
    _die "API binary not found at $API_DLL. Build may have failed."
  fi

  API_PID=""
  stop_api() {
    if [ -n "$API_PID" ] && kill -0 "$API_PID" 2>/dev/null; then
      kill "$API_PID" 2>/dev/null || true
      wait "$API_PID" 2>/dev/null || true
      API_PID=""
    fi
  }

  api_cleanup() {
    stop_api
    rm -f "$SWAGGER_TEMP"
  }
  trap api_cleanup EXIT

  # Start API compiled DLL directly in background (avoids dotnet run wrapper PID leak)
  ASPNETCORE_ENVIRONMENT=Development \
  Bootstrap__Enabled="false" \
  Db__Provider=MySQL \
  Db__Host=127.0.0.1 \
  Db__Port="3306" \
  Db__Database=controleasydb \
  Db__Username=dummy \
  Db__Password=dummy \
  Jwt__SigningKey=CI-DUMMY-KEY-FOR-SWAGGER-GEN-ONLY-32-CHARS!! \
  Jwt__Issuer=ControlEasyReborn \
  Jwt__Audience=ControlEasyReborn \
  dotnet "$API_DLL" --urls "http://127.0.0.1:${API_PORT}" --contentRoot "$API_BIN_DIR" >/dev/null 2>&1 &
  API_PID=$!

  deadline=$((SECONDS + 45))
  READY=false
  while (( SECONDS < deadline )); do
    STATUS=$(curl --connect-timeout 2 --max-time 5 --silent -o /dev/null -w "%{http_code}" "http://127.0.0.1:${API_PORT}/swagger/v1/swagger.json" || true)
    if [ "$STATUS" = "200" ]; then
      READY=true
      break
    fi
    sleep 1
  done

  if [ "$READY" = false ]; then
    api_cleanup
    trap - EXIT
    _die "API failed to start within 45s for swagger generation."
  fi

  curl --fail --silent --show-error "http://127.0.0.1:${API_PORT}/swagger/v1/swagger.json" -o "$SWAGGER_TEMP"
  stop_api

  # Run generator and drift check
  (
    cd src/Web/ControlEasyReborn.Web
    if ! npx -y ng-openapi-gen --input "$SWAGGER_TEMP" --config ng-openapi-gen.json >/dev/null 2>&1; then
      api_cleanup
      trap - EXIT
      _die "ng-openapi-gen execution failed! Verify swagger schema and ng-openapi-gen installation."
    fi
    git diff --exit-code -- src/app/api || {
      api_cleanup
      trap - EXIT
      _die "OpenAPI client drift detected! Generated types in src/app/api differ from swagger."
    }
  )
  api_cleanup
  trap - EXIT
  _ok "OpenAPI drift check passed."
fi

# ---------------------------------------------------------------------------
# Stamp Success State
# ---------------------------------------------------------------------------
HEAD_SHA="$(git rev-parse HEAD 2>/dev/null || echo "none")"
DIFF_HASH="$(git diff HEAD 2>/dev/null | compute_sha256)"
STATUS_HASH="$(git status --porcelain=v1 -uall 2>/dev/null | compute_sha256)"
TIMESTAMP="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
STAMP_CONTENT="${HEAD_SHA}:${DIFF_HASH}:${STATUS_HASH}:${TIMESTAMP}"

if [ "$FAST_ONLY" = true ] || [ "$SKIP_INTEGRATION" = true ] || [ "$SKIP_DOCKER" = true ] || [ "$SKIP_OPENAPI" = true ]; then
  FAST_STAMP_FILE="$GIT_DIR/ci-local-fast-passed.stamp"
  mkdir -p "$(dirname "$FAST_STAMP_FILE")"
  printf '%s\n' "$STAMP_CONTENT" > "$FAST_STAMP_FILE"
  _ok "Fast local CI verification checks PASSED (zero Docker downloads)!"
else
  mkdir -p "$(dirname "$STAMP_FILE")"
  printf '%s\n' "$STAMP_CONTENT" > "$STAMP_FILE"
  # Full pass also satisfies fast stamp
  FAST_STAMP_FILE="$GIT_DIR/ci-local-fast-passed.stamp"
  printf '%s\n' "$STAMP_CONTENT" > "$FAST_STAMP_FILE"
  _ok "All local CI verification gates (including Docker web build & Testcontainers) PASSED!"
fi
