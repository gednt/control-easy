# scripts/lib/worktree.psm1 — shared helpers for scripts/worktree-up.ps1
# and scripts/worktree-down.ps1.
#
# PowerShell equivalent of scripts/lib/worktree.sh.
# Provides: branch detection, branch->project-name conversion, worktree
# index -> port allocation, Traefik hostname derivation, and idempotency
# guards.
#
# Conventions (see .specs/devcontainers/design.md):
#   - Branch name:                feat/<spec-id>-<short-slug>
#   - Worktree path:              ..\ControlEasy.<branch-with-slashes>
#   - Compose project name:       ce-<branch-with-slashes-as-hyphens>
#   - Traefik hostname:           ce-<branch-with-slashes-as-hyphens>.localhost
#   - Published host port:        18080 + (worktree-index * 10)
#   - DB volume name:             ce-<branch-with-slashes-as-hyphens>-mysql-data

# Module-level constants
$script:CE_BASE_PORT = 18080
$script:CE_PORT_STEP = 10
$script:CE_HOST_SUFFIX = ".localhost"
$script:CE_WORKTREE_PREFIX = "ControlEasy."
$script:CE_PROJECT_PREFIX = "ce-"
$script:CE_VOLUME_SUFFIX = "-mysql-data"

function Write-CeLog  { param([string]$Message) Write-Host "[worktree] $Message" }
function Write-CeWarn { param([string]$Message) Write-Warning "[worktree][WARN] $Message" }
function Write-CeErr  { param([string]$Message) Write-Error  "[worktree][ERROR] $Message" }
function Invoke-CeDie { param([string]$Message, [int]$ExitCode = 1); Write-CeErr $Message; exit $ExitCode }

function Test-CeGitRepo {
    try { git rev-parse --git-dir *> $null; return $true } catch { return $false }
}

function Get-CeCurrentBranch {
    if (-not (Test-CeGitRepo)) { Invoke-CeDie "Not a git repository." }
    $branch = git rev-parse --abbrev-ref HEAD 2>$null
    if ([string]::IsNullOrEmpty($branch)) { Invoke-CeDie "Detached HEAD; cannot derive a worktree branch." }
    return $branch
}

function ConvertTo-CeSlug {
    param([Parameter(Mandatory)][string]$Branch)
    return ($Branch -replace '/', '-')
}

function Get-CeProjectName {
    param([Parameter(Mandatory)][string]$Branch)
    return ($script:CE_PROJECT_PREFIX + (ConvertTo-CeSlug $Branch))
}

function Get-CeTraefikHostname {
    param([Parameter(Mandatory)][string]$Branch)
    return ($script:CE_PROJECT_PREFIX + (ConvertTo-CeSlug $Branch) + $script:CE_HOST_SUFFIX)
}

function Get-CeMysqlVolumeName {
    param([Parameter(Mandatory)][string]$Branch)
    return ($script:CE_PROJECT_PREFIX + (ConvertTo-CeSlug $Branch) + $script:CE_VOLUME_SUFFIX)
}

function Get-CeWorktreeDir {
    param([Parameter(Mandatory)][string]$Branch)
    $repoRoot = (git rev-parse --show-toplevel)
    $parent = Split-Path -Parent $repoRoot
    return (Join-Path $parent ($script:CE_WORKTREE_PREFIX + $Branch))
}

function Get-CeWorktreeIndex {
    param([Parameter(Mandatory)][string]$Branch)
    $slug = ConvertTo-CeSlug $Branch
    $list = (git worktree list --porcelain) 2>$null
    if (-not $list) { Invoke-CeDie "git worktree list failed" }
    $index = 0
    foreach ($line in $list -split "`n") {
        if ($line -match '^worktree ') {
            $index++
            if ($line -like "*$($script:CE_WORKTREE_PREFIX)$slug*") {
                return ($index - 1)
            }
        }
    }
    Invoke-CeDie "Worktree for branch '$Branch' not found in 'git worktree list'."
}

function Get-CeAllocatedPort {
    param([Parameter(Mandatory)][string]$Branch)
    $index = Get-CeWorktreeIndex -Branch $Branch
    return ($script:CE_BASE_PORT + $index * $script:CE_PORT_STEP)
}

function Test-CeWorktreeUp {
    param([Parameter(Mandatory)][string]$Project)
    $count = (docker compose -p $Project ps --quiet 2>$null | Measure-Object).Count
    return ($count -gt 0)
}

function Test-CeWorktreeDown {
    param([Parameter(Mandatory)][string]$Project)
    $count = (docker compose -p $Project ps --quiet 2>$null | Measure-Object).Count
    return ($count -eq 0)
}

function Get-CeEnvFile {
    param([Parameter(Mandatory)][string]$Branch)
    $repoRoot = (git rev-parse --show-toplevel)
    return (Join-Path $repoRoot "docker/.env.worktree.$(ConvertTo-CeSlug $Branch)")
}

function Get-CeOverrideFile {
    param([Parameter(Mandatory)][string]$Branch)
    $repoRoot = (git rev-parse --show-toplevel)
    return (Join-Path $repoRoot "docker/docker-compose.worktree.$(ConvertTo-CeSlug $Branch).yml")
}

function Get-CeOverrideTemplate {
    $repoRoot = (git rev-parse --show-toplevel)
    return (Join-Path $repoRoot "docker/docker-compose.worktree.template.yml")
}

function Get-CeJwtSigningKey {
    param([Parameter(Mandatory)][string]$Branch)
    $slug = ConvertTo-CeSlug $Branch
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Branch)
    $hash = $sha.ComputeHash($bytes)
    $hex = ($hash | ForEach-Object { $_.ToString("x2") }) -join ''
    return "dev-worktree-$slug-$($hex.Substring(0,32))"
}

Export-ModuleMember -Function `
    Write-CeLog, Write-CeWarn, Write-CeErr, Invoke-CeDie, `
    Test-CeGitRepo, Get-CeCurrentBranch, ConvertTo-CeSlug, `
    Get-CeProjectName, Get-CeTraefikHostname, Get-CeMysqlVolumeName, `
    Get-CeWorktreeDir, Get-CeWorktreeIndex, Get-CeAllocatedPort, `
    Test-CeWorktreeUp, Test-CeWorktreeDown, `
    Get-CeEnvFile, Get-CeOverrideFile, Get-CeOverrideTemplate, `
    Get-CeJwtSigningKey