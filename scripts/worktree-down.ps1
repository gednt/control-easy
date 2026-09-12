<#
.SYNOPSIS
    Tear down a per-feature-branch dev worktree (PowerShell edition).
.DESCRIPTION
    Reverses scripts/worktree-up.ps1:
      1. docker compose -p ce-<slug> down -v
      2. Remove the ce-<slug>.localhost hostname from the resolver.
      3. git worktree remove --force .worktrees\<branch-with-slashes> (unless -KeepWorktree)
    Idempotent: re-running on a torn-down worktree exits 0 with "already down".
.PARAMETER Branch
    Use this branch (default: current branch).
.PARAMETER KeepWorktree
    Do not remove the worktree directory (only tear down the stack).
.PARAMETER ResolverMode
    auto (default), nrpt, or hosts-file.
.EXAMPLE
    .\scripts\worktree-down.ps1
    .\scripts\worktree-down.ps1 -Branch feat/dashboard-live-stats -KeepWorktree
#>
[CmdletBinding()]
param(
    [string]$Branch,
    [switch]$KeepWorktree,
    [ValidateSet('auto','nrpt','hosts-file')]
    [string]$ResolverMode = 'auto'
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module (Join-Path $here 'lib/worktree.psm1') -Force
Import-Module (Join-Path $here 'lib/resolver.psm1') -Force

if ([string]::IsNullOrEmpty($Branch)) { $Branch = Get-CeCurrentBranch }

$project  = Get-CeProjectName $Branch
$host_    = Get-CeTraefikHostname $Branch
$envFile  = Get-CeEnvFile $Branch
$override = Get-CeOverrideFile $Branch

Write-CeLog "Branch:  $Branch"
Write-CeLog "Project: $project"

# Idempotency: already down?
if (Test-CeWorktreeDown -Project $project) {
    Write-CeLog "already down (project=$project has no running containers)"
    if (Test-Path $override) { Remove-Item $override -Force; Write-CeLog "Removed stale override: $override" }
    if (Test-Path $envFile)  { Remove-Item $envFile  -Force; Write-CeLog "Removed stale env: $envFile" }
    exit 0
}

# NFR-4: refuse to tear down the main checkout's project
switch ($project) {
    { $_ -in @('ce','ce-main','ce-master','controleasy') } {
        Invoke-CeDie "Refusing to tear down the main checkout's project ($project). Use 'docker compose -f docker/docker-compose.yml down -v' from the main checkout manually."
    }
}

# Step 1: docker compose -p ce-<slug> down -v
Write-CeLog "Tearing down compose project $project (containers, networks, volumes)"
$composeBase = "docker/docker-compose.yml"
if (Test-Path $override) {
    if (Test-Path $envFile) {
        docker compose -p $project --env-file $envFile -f $composeBase -f $override down -v
    } else {
        docker compose -p $project -f $composeBase -f $override down -v
    }
} else {
    docker compose -p $project -f $composeBase down -v
}
if ($LASTEXITCODE -ne 0) { Invoke-CeDie "docker compose down failed" }

# Step 2: Remove the resolver entry
Write-CeLog "Removing hostname $host_ from resolver"
try { Remove-CeResolver -Hostname $host_ -ResolverMode $ResolverMode } catch { Write-CeWarn "Resolver removal: $_" }

# Step 3: Remove generated artifacts
if (Test-Path $override) { Remove-Item $override -Force }
if (Test-Path $envFile)  { Remove-Item $envFile  -Force }

# Step 4: Remove the worktree directory (unless -KeepWorktree)
if (-not $KeepWorktree) {
    $worktreeDir = Get-CeWorktreeDir -Branch $Branch
    if (Test-Path $worktreeDir) {
        Write-CeLog "Removing worktree directory: $worktreeDir"
        git worktree remove --force $worktreeDir 2>$null
    }
}

Write-CeLog "Worktree $Branch is down."