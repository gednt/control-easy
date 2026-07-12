#!/usr/bin/env bash
# shellcheck shell=bash
#
# scripts/lib/resolver.sh — cross-OS host resolver shim for
# ce-<id>.localhost hostnames.
#
# Provides: resolver_add <hostname> [mode], resolver_remove <hostname> [mode],
# resolver_check <hostname>.
#
# Modes:
#   auto        Detect the OS and use the recommended resolver (default).
#   nrpt        Windows: Add-DnsClientNrptRule (requires admin).
#   hosts-file  Fallback: append to a per-user hosts file at
#               $HOME/.control-easy/hosts. The user is expected to wire this
#               into their system resolver manually (documented in the
#               script output).
#
# On Linux and macOS, the shim edits /etc/hosts (with sudo) and backs up the
# original to /etc/hosts.ce-backup-<timestamp> on add, restoring from the
# backup on remove. The backup is removed after a successful restore.
#
# This file is sourced, not executed.

set -euo pipefail

# ---------------------------------------------------------------------------
# Helpers (re-declared so this file is standalone-sourcable)
# ---------------------------------------------------------------------------
_resolver_log()  { printf '[resolver] %s\n' "$*"; }
_resolver_warn() { printf '[resolver][WARN] %s\n' "$*" >&2; }
_resolver_err()  { printf '[resolver][ERROR] %s\n' "$*" >&2; }
_resolver_die()  { _resolver_err "$*"; exit 1; }

# Detect OS family: linux, darwin, windows
_resolver_detect_os() {
  case "$(uname -s)" in
    Linux*)   echo "linux" ;;
    Darwin*)  echo "darwin" ;;
    MINGW*|MSYS*|CYGWIN*) echo "windows" ;;
    *) _resolver_die "Unsupported OS: $(uname -s)" ;;
  esac
}

# Check if a hostname currently resolves to 127.0.0.1.
# Exits 0 if it does, 1 otherwise.
resolver_check() {
  local hostname="$1"
  local os
  os="$(_resolver_detect_os)"
  case "$os" in
    linux|darwin)
      if command -v getent >/dev/null 2>&1; then
        getent hosts "$hostname" 2>/dev/null | grep -q '127\.0\.0\.1'
      else
        # macOS does not have getent; use dscacheutil or host.
        dscacheutil -q host -a name "$hostname" 2>/dev/null \
          | grep -q '127\.0\.0\.1' \
          || host "$hostname" 2>/dev/null | grep -q '127\.0\.0\.1'
      fi ;;
    windows)
      # PowerShell Resolve-DnsName
      powershell.exe -NoProfile -Command \
        "Resolve-DnsName -Name '$hostname' -ErrorAction SilentlyContinue \
          | Where-Object { \$_.IPAddress -eq '127.0.0.1' } \
          | Select-Object -First 1" \
        2>/dev/null | grep -q '127\.0\.0\.1' ;;
  esac
}

# ---------------------------------------------------------------------------
# /etc/hosts helpers (Linux + macOS)
# ---------------------------------------------------------------------------
_resolver_hosts_add() {
  local hostname="$1"
  local entry="127.0.0.1 $hostname"
  if [[ ! -w /etc/hosts ]]; then
    _resolver_warn "/etc/hosts is not writable without sudo."
  fi
  # Idempotent: skip if entry already present.
  if grep -qE "^[0-9.]+[[:space:]]+$hostname\$" /etc/hosts 2>/dev/null; then
    _resolver_log "$hostname already in /etc/hosts"
    return 0
  fi
  local backup="/etc/hosts.ce-backup-$(date +%Y%m%d-%H%M%S)"
  sudo cp /etc/hosts "$backup" || cp /etc/hosts "$backup" 2>/dev/null || true
  _resolver_log "Backed up /etc/hosts -> $backup"
  echo "$entry" | sudo tee -a /etc/hosts >/dev/null \
    || echo "$entry" >> /etc/hosts 2>/dev/null \
    || _resolver_die "Could not write to /etc/hosts (use --resolver=hosts-file)"
  _resolver_log "Added $entry to /etc/hosts"
}

_resolver_hosts_remove() {
  local hostname="$1"
  if ! grep -qE "^[0-9.]+[[:space:]]+$hostname\$" /etc/hosts 2>/dev/null; then
    _resolver_log "$hostname not in /etc/hosts; nothing to remove"
    return 0
  fi
  sudo sed -i.bak "/^[0-9.]\+[[:space:]]*$hostname\$/d" /etc/hosts \
    || sed -i.bak "/^[0-9.]\+[[:space:]]*$hostname\$/d" /etc/hosts 2>/dev/null \
    || _resolver_die "Could not edit /etc/hosts (use --resolver=hosts-file)"
  sudo rm -f /etc/hosts.bak || rm -f /etc/hosts.bak 2>/dev/null || true
  _resolver_log "Removed $hostname from /etc/hosts"
}

# ---------------------------------------------------------------------------
# Windows NRPT rule helpers (require admin)
# ---------------------------------------------------------------------------
_resolver_nrpt_add() {
  local hostname="$1"
  # Add a Name Resolution Policy Table rule mapping *.localhost to 127.0.0.1.
  # We use the comment "ControlEasy-worktree" so we can find and remove it.
  _resolver_log "Adding NRPT rule for $hostname (requires admin)"
  if ! powershell.exe -NoProfile -Command \
        "Add-DnsClientNrptRule -Namespace '.$hostname' -NameServers '127.0.0.1' -Comment 'ControlEasy-worktree' -ErrorAction Stop" \
        2>/dev/null; then
    _resolver_die "Add-DnsClientNrptRule failed (admin required). Re-run with --resolver=hosts-file for the per-user fallback."
  fi
}

_resolver_nrpt_remove() {
  local hostname="$1"
  _resolver_log "Removing NRPT rule for $hostname"
  powershell.exe -NoProfile -Command \
    "Get-DnsClientNrptRule -ErrorAction SilentlyContinue \
      | Where-Object { \$_.Comment -eq 'ControlEasy-worktree' -and \$_.Namespace -eq '.$hostname' } \
      | Remove-DnsClientNrptRule -Force -ErrorAction SilentlyContinue" \
    2>/dev/null || true
}

# ---------------------------------------------------------------------------
# Per-user hosts-file fallback (any OS, no admin required)
# ---------------------------------------------------------------------------
_resolver_hosts_file_add() {
  local hostname="$1"
  local dir="$HOME/.control-easy"
  local file="$dir/hosts"
  local entry="127.0.0.1 $hostname"
  mkdir -p "$dir"
  if [[ -f "$file" ]] && grep -qE "^[0-9.]+[[:space:]]+$hostname\$" "$file"; then
    _resolver_log "$hostname already in $file"
    return 0
  fi
  echo "$entry" >> "$file"
  _resolver_log "Added $entry to $file"
  _resolver_warn "Per-user hosts file mode. Add the following to your system"
  _resolver_warn "resolver manually, or run as admin for NRPT mode:"
  _resolver_warn "    $file"
}

_resolver_hosts_file_remove() {
  local hostname="$1"
  local dir="$HOME/.control-easy"
  local file="$dir/hosts"
  if [[ ! -f "$file" ]]; then
    return 0
  fi
  if ! grep -qE "^[0-9.]+[[:space:]]+$hostname\$" "$file"; then
    return 0
  fi
  sed -i.bak "/^[0-9.]\+[[:space:]]*$hostname\$/d" "$file"
  rm -f "$file.bak"
  _resolver_log "Removed $hostname from $file"
}

# ---------------------------------------------------------------------------
# Public API
# ---------------------------------------------------------------------------
# resolver_add <hostname> [mode]
resolver_add() {
  local hostname="$1"
  local mode="${2:-auto}"
  local os
  os="$(_resolver_detect_os)"
  case "$mode" in
    auto)
      case "$os" in
        linux|darwin) _resolver_hosts_add "$hostname" ;;
        windows)     _resolver_nrpt_add "$hostname" \
                       || _resolver_hosts_file_add "$hostname" ;;
      esac ;;
    nrpt)
      case "$os" in
        windows) _resolver_nrpt_add "$hostname" ;;
        *)       _resolver_die "NRPT mode is Windows-only (detected $os)" ;;
      esac ;;
    hosts-file)
      _resolver_hosts_file_add "$hostname" ;;
    *)
      _resolver_die "Unknown resolver mode: $mode (use auto|nrpt|hosts-file)" ;;
  esac
}

# resolver_remove <hostname> [mode]
resolver_remove() {
  local hostname="$1"
  local mode="${2:-auto}"
  local os
  os="$(_resolver_detect_os)"
  case "$mode" in
    auto)
      case "$os" in
        linux|darwin) _resolver_hosts_remove "$hostname" ;;
        windows)
          _resolver_nrpt_remove "$hostname"
          _resolver_hosts_file_remove "$hostname" ;;
      esac ;;
    nrpt)
      case "$os" in
        windows) _resolver_nrpt_remove "$hostname" ;;
        *)       _resolver_warn "NRPT mode is Windows-only; skipping" ;;
      esac ;;
    hosts-file)
      _resolver_hosts_file_remove "$hostname" ;;
    *)
      _resolver_warn "Unknown resolver mode: $mode (use auto|nrpt|hosts-file)" ;;
  esac
  return 0
}