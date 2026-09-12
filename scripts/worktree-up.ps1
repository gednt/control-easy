<#
.SYNOPSIS
    Bring up a per-feature-branch dev worktree (PowerShell edition).
.DESCRIPTION
    Windows PowerShell wrapper that performs the same work as
    scripts/worktree-up.sh. Creates the per-worktree env file,
    registers the ce-<id>.localhost hostname, generates the
    docker-compose.worktree.<id>.yml override from the template,
    and runs `docker compose up -d --build`.
.PARAMETER Branch
    Use this branch (default: current branch).
.PARAMETER NoBuild
    Skip `docker compose build` (use existing images).
.PARAMETER ResolverMode
    auto (default), nrpt, or hosts-file.
.EXAMPLE
    .\scripts\worktree-up.ps1
    .\scripts\worktree-up.ps1 -Branch feat/dashboard-live-stats
    .\scripts\worktree-up.ps1 -ResolverMode hosts-file
#>
[CmdletBinding()]
param(
    [string]$Branch,
    [switch]$NoBuild,
    [ValidateSet('auto','nrpt','hosts-file')]
    [string]$ResolverMode = 'auto'
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module (Join-Path $here 'lib/worktree.psm1') -Force
Import-Module (Join-Path $here 'lib/resolver.psm1') -Force

if ([string]::IsNullOrEmpty($Branch)) { $Branch = Get-CeCurrentBranch }

$slug    = ConvertTo-CeSlug $Branch
$project = Get-CeProjectName $Branch
$host_   = Get-CeTraefikHostname $Branch
$port    = Get-CeAllocatedPort $Branch
$httpsPort = $port + 1
$volume  = Get-CeMysqlVolumeName $Branch
$envFile = Get-CeEnvFile $Branch
$override= Get-CeOverrideFile $Branch
$template= Get-CeOverrideTemplate

Write-CeLog "Branch:        $Branch"
Write-CeLog "Project:       $project"
Write-CeLog "Hostname:      $host_"
Write-CeLog "Port:          $port"
Write-CeLog "HTTPS port:    $httpsPort"
Write-CeLog "Volume:        $volume"
Write-CeLog "Env file:      $envFile"
Write-CeLog "Override file: $override"

if (Test-CeWorktreeUp -Project $project) {
    Write-CeLog "already up (project=$project has running containers)"
    exit 0
}

# Step 3: Generate .env.worktree.<slug>
Write-CeLog "Generating $envFile"
$jwtKey = Get-CeJwtSigningKey $Branch
$envContent = @"
COMPOSE_PROJECT_NAME=$project
TRAEFIK_HOST=$host_
TRAEFIK_PORT=$port
TRAEFIK_HTTPS_PORT=$httpsPort
MYSQL_VOLUME_NAME=$volume
JWT_SIGNING_KEY=$jwtKey
"@
Set-Content -Path $envFile -Value $envContent -NoNewline -Encoding ascii

# Step 4: Register hostname via resolver shim
Write-CeLog "Registering hostname $host_ -> 127.0.0.1"
Add-CeResolver -Hostname $host_ -ResolverMode $ResolverMode

# Step 5: Generate override from template (envsubst is not available on
# Windows; do string replacement in PowerShell).
if (-not (Test-Path $template)) {
    Invoke-CeDie "Template not found: $template (run task 9.3 first)"
}
Write-CeLog "Generating $override from $template"
$tpl = Get-Content $template -Raw
$env:COMPOSE_PROJECT_NAME = $project
$env:TRAEFIK_HOST = $host_
$env:TRAEFIK_PORT = "$port"
$env:TRAEFIK_HTTPS_PORT = "$httpsPort"
$env:MYSQL_VOLUME_NAME = $volume
$env:JWT_SIGNING_KEY = $jwtKey
$rendered = $tpl `
    -replace '\$\{COMPOSE_PROJECT_NAME\}', $project `
    -replace '\$\{TRAEFIK_HOST\}', $host_ `
    -replace '\$\{TRAEFIK_PORT\}', "$port" `
    -replace '\$\{TRAEFIK_HTTPS_PORT\}', "$httpsPort" `
    -replace '\$\{MYSQL_VOLUME_NAME\}', $volume `
    -replace '\$\{JWT_SIGNING_KEY\}', $jwtKey
Set-Content -Path $override -Value $rendered -NoNewline -Encoding utf8

# Step 6: docker compose up -d --build
$env:COMPOSE_PROJECT_NAME = $project
$composeBase = "docker/docker-compose.yml"
if (-not (Test-Path $composeBase)) { Invoke-CeDie "Base compose file not found: $composeBase" }

if ($NoBuild) {
    Write-CeLog "Skipping build (-NoBuild)"
    docker compose -p $project --env-file $envFile -f $composeBase -f $override up -d
} else {
    Write-CeLog "Building and bringing up the stack"
    $env:COMPOSE_DOCKER_CLI_BUILD = '1'
    $env:DOCKER_BUILDKIT = '1'
    docker compose -p $project --env-file $envFile -f $composeBase -f $override up -d --build
}
if ($LASTEXITCODE -ne 0) { Invoke-CeDie "docker compose up failed" }

# Step 7: Print URLs
Write-CeLog "Stack is up."
Write-Host ""
Write-Host "  Web:      http://$($host_):$port/   (redirects to HTTPS)"
Write-Host "  Web HTTPS: https://$($host_):$httpsPort/  (self-signed cert)"
Write-Host "  API:      https://$($host_):$httpsPort/api/v1/health"
Write-Host "  Swagger:  https://$($host_):$httpsPort/swagger"
Write-Host ""
Write-Host "  Logs:     docker compose -p $project logs -f api"
Write-Host "  Tear down: .\scripts\worktree-down.ps1 -Branch $Branch"