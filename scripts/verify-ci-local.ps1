<#
.SYNOPSIS
    scripts/verify-ci-local.ps1 — Cross-platform PowerShell CI verification gate.
.DESCRIPTION
    Runs all CI stages locally. ALL dotnet/npx commands execute inside Docker
    containers — only docker CLI, git, and curl are required on the host.

    Architecture note — stages 1-4 in ONE container:
    Stages 1-4 run inside a single 'docker run' using an inner shell script
    mounted via the repo bind-mount. This ensures build artifacts produced in
    stage 2 are immediately visible to stages 3-4 without VirtioFS cross-container
    flush latency (a Docker Desktop macOS issue with separate containers).

    Security note — Docker socket bind-mount (stage 4):
    Mounting /var/run/docker.sock grants the SDK container unrestricted access to
    the host Docker daemon (equivalent to host root). Required for Testcontainers.
    Only use in trusted, developer-controlled environments.

    Stages:
      1. Format check   (dotnet format --verify-no-changes)  — mcr.microsoft.com/dotnet/sdk:8.0
      2. Build          (dotnet build Release)                — mcr.microsoft.com/dotnet/sdk:8.0
      3. Fast tests     (UnitTests + ArchitectureTests)       — mcr.microsoft.com/dotnet/sdk:8.0
      4. Integration    (Testcontainers MySQL)                — mcr.microsoft.com/dotnet/sdk:8.0 + Docker socket
      5. Web build      (Angular production image)            — docker build (host daemon)
      6. OpenAPI drift  (swagger gen + ng-openapi-gen check)  — aspnet:8.0 + node:20-slim

    Host requirements: docker CLI, git (for stamping and drift check).
#>

[CmdletBinding()]
param(
    [switch]$Fast,
    [switch]$SkipIntegration,
    [switch]$SkipDocker,
    [switch]$SkipOpenApi
)

if ($Fast) {
    $SkipIntegration = $true
    $SkipDocker      = $true
    $SkipOpenApi     = $true
}

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Split-Path -Parent $scriptDir
Set-Location $repoRoot

$gitDir       = git rev-parse --absolute-git-dir 2>$null
if (-not $gitDir) { $gitDir = Join-Path $repoRoot ".git" }
$stampFile     = Join-Path $gitDir "ci-local-passed.stamp"
$fastStampFile = Join-Path $gitDir "ci-local-fast-passed.stamp"

function Write-Log ([string]$msg)  { Write-Host "[ci-local] $msg"        -ForegroundColor Cyan  }
function Write-Pass([string]$msg)  { Write-Host "[ci-local][PASS] $msg"  -ForegroundColor Green }
function Write-Warn([string]$msg)  { Write-Host "[ci-local][WARN] $msg"  -ForegroundColor Yellow }
function Write-Fail([string]$msg)  { Write-Host "[ci-local][FAIL] $msg"  -ForegroundColor Red; exit 1 }

# SHA-256 helper — used for stamp fingerprinting only (no external tools needed)
function Get-Sha256String([string]$text) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($text)
    $hasher = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $hasher.ComputeHash($bytes)
        return ([System.BitConverter]::ToString($hashBytes)).Replace("-", "").ToLowerInvariant()
    } finally { $hasher.Dispose() }
}

# Free-port finder — .NET TcpListener, no external tools
function Get-FreePort {
    try {
        $l = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
        $l.Start()
        $p = ($l.LocalEndpoint).Port
        $l.Stop()
        return $p
    } catch { return 18099 }
}

Write-Log "Starting local CI verification gate (all dotnet/npx commands run inside Docker containers)..."

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Fail "'docker' CLI not found in PATH. This script requires Docker Engine or Docker Desktop on the host."
}

# ---------------------------------------------------------------------------
# Docker image constants
# ---------------------------------------------------------------------------
$sdkImage  = "mcr.microsoft.com/dotnet/sdk:8.0"
$rtImage   = "mcr.microsoft.com/dotnet/aspnet:8.0"
$nodeImage = "node:20-slim"
$nugetVol  = "ce-nuget-packages"   # NuGet package cache — persists between runs
$npmVol    = "ce-npm-cache"        # npm package cache — persists between runs

# ---------------------------------------------------------------------------
# Temp files — cleaned up in finally block
# ---------------------------------------------------------------------------
$ciInner    = Join-Path $repoRoot ".tmp-ci-dotnet-stages.sh"
$swaggerTemp = Join-Path $repoRoot ".tmp-ci-swagger.json"
$apiContainer = "ce-ci-swagger-$([System.Diagnostics.Process]::GetCurrentProcess().Id)"

try {
    # ---------------------------------------------------------------------------
    # Write the inner dotnet stages script (stages 1-4).
    # This POSIX sh script is executed INSIDE the SDK container via the bind-mount.
    # Combining stages 1-4 in one container eliminates VirtioFS cross-container
    # artifact visibility issues on macOS Docker Desktop.
    # ---------------------------------------------------------------------------
    $innerScript = @'
#!/bin/sh
# Inner script: stages 1-4 inside the dotnet SDK container.
# Variables injected by docker run -e:
#   FAST_ONLY           (true/false)
#   INCLUDE_INTEGRATION (true/false)
# NOTE: base image uses dash for /bin/sh, which does NOT support `set -o pipefail`.
# Use `-eu` only; pipeline exit status is the last command's status.
set -eu

_log()  { printf '\033[1;34m[ci-local]\033[0m %s\n' "$*"; }
_ok()   { printf '\033[1;32m[ci-local][PASS]\033[0m %s\n' "$*"; }
_fail() { printf '\033[1;31m[ci-local][FAIL]\033[0m %s\n' "$*" >&2; exit 1; }

has_cs_changes() {
  git status --porcelain 2>/dev/null | grep -qE '\.cs$'
}

# Stage 1: Format check
if [ "${FAST_ONLY:-false}" = "true" ] && ! has_cs_changes; then
  _ok "Stage 1/6: Skipped C# format check (no .cs files modified)."
else
  _log "Stage 1/6: Checking C# code format..."
  dotnet restore src/ControlEasyReborn.sln --verbosity quiet --nologo
  dotnet format src/ControlEasyReborn.sln --verify-no-changes --no-restore \
    || _fail "Format check failed! Run 'dotnet format src/ControlEasyReborn.sln' to fix formatting."
  _ok "Code format verified."
fi

# Stage 2: Build
_log "Stage 2/6: Building solution in Release mode..."
dotnet build src/ControlEasyReborn.sln --configuration Release --nologo \
  || _fail "Build failed."
_ok "Solution built successfully."

# Stage 3: Unit + Architecture tests
_log "Stage 3/6: Running Unit & Architecture tests..."
dotnet test tests/ControlEasyReborn.UnitTests \
    --configuration Release --no-build --nologo --logger "console;verbosity=minimal" \
  || _fail "Unit tests failed."
dotnet test tests/ControlEasyReborn.ArchitectureTests \
    --configuration Release --no-build --nologo --logger "console;verbosity=minimal" \
  || _fail "Architecture tests failed."
_ok "Unit and Architecture tests passed."

# Stage 4: Integration tests (Testcontainers MySQL)
if [ "${INCLUDE_INTEGRATION:-false}" = "true" ]; then
  _log "Stage 4/6: Running Integration tests (Testcontainers MySQL)..."
  dotnet test tests/ControlEasyReborn.IntegrationTests \
      --configuration Release --no-build --nologo --logger "console;verbosity=minimal" \
    || _fail "Integration tests failed."
  _ok "Integration tests passed."
fi
'@
    # Write with Unix line endings so the sh shebang is honored inside the Linux container
    $innerBytes = [System.Text.Encoding]::UTF8.GetBytes($innerScript.Replace("`r`n", "`n"))
    [System.IO.File]::WriteAllBytes($ciInner, $innerBytes)

    # ---------------------------------------------------------------------------
    # Stages 1-3 (fast) OR 1-4 (full): one SDK container
    # ---------------------------------------------------------------------------
    if ($SkipIntegration) {
        Write-Log "Running stages 1-3 inside SDK container..."
        & docker run --rm `
            -v "${repoRoot}:/workspace" `
            -v "${nugetVol}:/root/.nuget/packages" `
            -e "FAST_ONLY=$($Fast.IsPresent.ToString().ToLowerInvariant())" `
            -e "INCLUDE_INTEGRATION=false" `
            -w /workspace `
            $sdkImage `
            sh /workspace/.tmp-ci-dotnet-stages.sh
        if ($LASTEXITCODE -ne 0) { Write-Fail "Stages 1-3 failed inside SDK container." }
    } else {
        # Full path — bind Docker socket for Testcontainers.
        # --network host on Linux makes sibling containers reachable at 127.0.0.1.
        # On macOS/Windows Docker Desktop --network host is a no-op; use
        # TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal instead.
        Write-Log "Running stages 1-4 inside SDK container (Docker socket bound for Testcontainers)..."
        $dockerSock   = "/var/run/docker.sock"
        $networkFlags = if ($IsLinux) { @("--network", "host") } else { @() }
        $tcEnv        = if (-not $IsLinux) {
            Write-Warn "Non-Linux host: --network host not used; setting TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal for Docker Desktop."
            @("-e", "TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal")
        } else { @() }
        # Honor an already-running MySQL via CE_ITEST_MYSQL so the fixture's
        # Testcontainers-spawn path can be bypassed (Docker-in-Docker is fragile
        # on Windows/macOS Docker Desktop).
        $ceItestMysql = if ($env:CE_ITEST_MYSQL) { @("-e", "CE_ITEST_MYSQL=$env:CE_ITEST_MYSQL") } else { @() }

        & docker run --rm @networkFlags `
            -v "${repoRoot}:/workspace" `
            -v "${nugetVol}:/root/.nuget/packages" `
            -v "${dockerSock}:${dockerSock}" `
            -e "DOCKER_HOST=unix://${dockerSock}" `
            @tcEnv `
            @ceItestMysql `
            -e "FAST_ONLY=false" `
            -e "INCLUDE_INTEGRATION=true" `
            -w /workspace `
            $sdkImage `
            sh /workspace/.tmp-ci-dotnet-stages.sh
        if ($LASTEXITCODE -ne 0) { Write-Fail "Stages 1-4 failed inside SDK container." }
    }

    # ---------------------------------------------------------------------------
    # Stage 5: Web Production Build (host Docker daemon — already container-native)
    # ---------------------------------------------------------------------------
    if ($SkipDocker) {
        Write-Warn "Stage 5/6: Skipping Web build check."
    } else {
        Write-Log "Stage 5/6: Verifying Web production build (docker build)..."
        & docker build -f docker/web.Dockerfile -t ce-local-verify-web:latest .
        if ($LASTEXITCODE -ne 0) { Write-Fail "Web production Docker build failed." }
        & docker rmi ce-local-verify-web:latest 2>$null | Out-Null
        Write-Pass "Web production build verified."
    }

    # ---------------------------------------------------------------------------
    # Stage 6: OpenAPI Drift Check
    # ---------------------------------------------------------------------------
    if ($SkipOpenApi) {
        Write-Warn "Stage 6/6: Skipping OpenAPI drift check."
    } else {
        Write-Log "Stage 6/6: Verifying OpenAPI client and swagger drift..."

        $apiPort    = if ($env:CE_SWAGGER_PORT) { [int]$env:CE_SWAGGER_PORT } else { Get-FreePort }
        $apiDllRel  = "src/Host/ControlEasyReborn.Api/bin/Release/net8.0/ControlEasyReborn.Api.dll"
        $apiDll     = Join-Path $repoRoot $apiDllRel

        if (-not (Test-Path $apiDll)) {
            Write-Fail "API binary not found at $apiDllRel. Make sure stages 1-4 succeeded."
        }

        # Pin ng-openapi-gen to the version declared in package.json.
        # Read it inside a node container so no host-side node/npm needed.
        Write-Log "Resolving ng-openapi-gen version from package.json..."
        $pkgJsonPath = Join-Path $repoRoot "src/Web/ControlEasyReborn.Web/package.json"
        try {
            $pkgJson = Get-Content $pkgJsonPath -Raw | ConvertFrom-Json
            $ngGenRaw = $pkgJson.devDependencies.'ng-openapi-gen'
            if (-not $ngGenRaw) { $ngGenRaw = $pkgJson.dependencies.'ng-openapi-gen' }
            if (-not $ngGenRaw) { $ngGenRaw = "latest" }
            # Strip semver range prefix (^, ~)
            $ngGenVersion = $ngGenRaw -replace '^[\^~]', ''
        } catch {
            $ngGenVersion = "latest"
        }
        Write-Log "Using ng-openapi-gen@$ngGenVersion"

        # Determine host user for --user flag (preserves file ownership; prevents root-owned generated files)
        $nodeUserFlags = @()
        if ($IsLinux -or $IsMacOS) {
            try {
                $uid = (& id -u 2>$null).Trim()
                $gid = (& id -g 2>$null).Trim()
                if ($uid -and $gid -and $uid -ne "0") {
                    $nodeUserFlags = @("--user", "${uid}:${gid}")
                }
            } catch {}
        }

        Write-Log "Starting API container for swagger generation (host port $apiPort)..."
        & docker run -d `
            --name $apiContainer `
            -p "${apiPort}:8080" `
            -v "${repoRoot}:/workspace:ro" `
            -e ASPNETCORE_ENVIRONMENT=Development `
            -e Bootstrap__Enabled=false `
            -e Db__Provider=MySQL `
            -e "Db__Host=127.0.0.1" `
            -e Db__Port=3306 `
            -e Db__Database=controleasydb `
            -e Db__Username=dummy `
            -e Db__Password=dummy `
            -e Storage__Provider=Local `
            -e Storage__Local__Path=/tmp/photos `
            -e "Jwt__SigningKey=CI-DUMMY-KEY-FOR-SWAGGER-GEN-ONLY-32-CHARS!!" `
            -e Jwt__Issuer=ControlEasyReborn `
            -e Jwt__Audience=ControlEasyReborn `
            $rtImage `
            dotnet "/workspace/$apiDllRel" --urls "http://0.0.0.0:8080" | Out-Null

        # Poll swagger endpoint until ready
        $deadline = (Get-Date).AddSeconds(45)
        $ready    = $false
        while ((Get-Date) -lt $deadline) {
            try {
                $res = Invoke-WebRequest -Uri "http://127.0.0.1:$apiPort/swagger/v1/swagger.json" `
                    -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
                if ($res.StatusCode -eq 200) {
                    [System.IO.File]::WriteAllText($swaggerTemp, $res.Content, [System.Text.Encoding]::UTF8)
                    $ready = $true
                    break
                }
            } catch { Start-Sleep -Seconds 1 }
        }
        if (-not $ready) { Write-Fail "API container failed to start within 45s. Check: docker logs $apiContainer" }

        & docker rm -f $apiContainer 2>$null | Out-Null
        $apiContainer = ""  # prevent double-cleanup in finally

        # Run ng-openapi-gen in node container.
        # --user ensures generated files are owned by the host user, not root.
        # swagger temp file is inside $repoRoot (= /workspace) so container can read it.
        Write-Log "Running ng-openapi-gen@$ngGenVersion drift check (inside node container)..."
        & docker run --rm @nodeUserFlags `
            -v "${repoRoot}:/workspace" `
            -v "${npmVol}:/tmp/.npm" `
            -e NPM_CONFIG_CACHE=/tmp/.npm `
            -w /workspace/src/Web/ControlEasyReborn.Web `
            $nodeImage `
            sh -c "npx -y ng-openapi-gen@$ngGenVersion --input /workspace/.tmp-ci-swagger.json --config ng-openapi-gen.json"
        if ($LASTEXITCODE -ne 0) { Write-Fail "ng-openapi-gen execution failed! Verify swagger schema and ng-openapi-gen.json config." }

        # git diff on host — git is always available
        & git diff --exit-code -- src/Web/ControlEasyReborn.Web/src/app/api
        if ($LASTEXITCODE -ne 0) { Write-Fail "OpenAPI client drift detected! Generated types in src/app/api differ from swagger. Commit the regenerated files." }

        Write-Pass "OpenAPI drift check passed."
    }

    # ---------------------------------------------------------------------------
    # Stamp success state — uses only PS built-in .NET SHA256 + git (host utils)
    # PowerShell under ErrorActionPreference=Stop treats any stderr from an
    # external command as an error, including harmless git CRLF warnings
    # when core.autocrlf is true. Temporarily relax the preference and
    # capture only stdout.
    # ---------------------------------------------------------------------------
    $prevPref = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $headSha    = (git rev-parse HEAD 2>&1 | Out-String).Trim(); if (-not $headSha) { $headSha = "none" }
        $diffText   = (git diff HEAD 2>&1        | Out-String)
        $statusText = (git status --porcelain=v1 -uall 2>&1 | Out-String)
    } finally {
        $ErrorActionPreference = $prevPref
    }
    $diffHash   = Get-Sha256String $diffText
    $statusHash = Get-Sha256String $statusText
    $timestamp  = (Get-Date).ToUniversalTime().ToString('o')
    $stampContent = "$headSha`:$diffHash`:$statusHash`:$timestamp"

    if ($Fast -or $SkipIntegration -or $SkipDocker -or $SkipOpenApi) {
        $null = New-Item -ItemType Directory -Force -Path (Split-Path $fastStampFile)
        Set-Content -Path $fastStampFile -Value $stampContent
        Write-Pass "Fast local CI verification checks PASSED!"
    } else {
        $null = New-Item -ItemType Directory -Force -Path (Split-Path $stampFile)
        Set-Content -Path $stampFile -Value $stampContent
        Set-Content -Path $fastStampFile -Value $stampContent
        Write-Pass "All local CI verification gates PASSED (Testcontainers + Docker web build + OpenAPI drift)!"
    }

} finally {
    # Cleanup temp files and any lingering API container.
    # Wrap docker rm in a try/catch so the cleanup never aborts the run when
    # the container doesn't exist (e.g., when -Fast skips stage 6).
    if ($apiContainer) {
        try {
            & docker rm -f $apiContainer 2>$null | Out-Null
        } catch {
            # Container already gone or never started — ignore.
        }
    }
    if (Test-Path $ciInner)     { Remove-Item $ciInner     -Force -ErrorAction SilentlyContinue }
    if (Test-Path $swaggerTemp) { Remove-Item $swaggerTemp -Force -ErrorAction SilentlyContinue }
}
