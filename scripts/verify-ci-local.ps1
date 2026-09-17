<#
.SYNOPSIS
    scripts/verify-ci-local.ps1 — Cross-platform PowerShell execution of the exact CI verification gate.
.DESCRIPTION
    Runs all local CI verification stages:
    1. Code formatting check (dotnet format --verify-no-changes)
    2. Solution build (dotnet build Release)
    3. Fast tests (UnitTests + ArchitectureTests)
    4. Integration tests (IntegrationTests via Testcontainers MySQL)
    5. Web container production build
    6. OpenAPI drift check
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
    $SkipDocker = $true
    $SkipOpenApi = $true
}

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
Set-Location $repoRoot

$gitDir = git rev-parse --absolute-git-dir 2>$null
if (-not $gitDir) { $gitDir = Join-Path $repoRoot ".git" }
$stampFile = Join-Path $gitDir "ci-local-passed.stamp"
$fastStampFile = Join-Path $gitDir "ci-local-fast-passed.stamp"

function Write-Log([string]$msg) { Write-Host "[ci-local] $msg" -ForegroundColor Cyan }
function Write-Pass([string]$msg) { Write-Host "[ci-local][PASS] $msg" -ForegroundColor Green }
function Write-Warn([string]$msg) { Write-Host "[ci-local][WARN] $msg" -ForegroundColor Yellow }
function Write-Fail([string]$msg) { Write-Host "[ci-local][FAIL] $msg" -ForegroundColor Red; exit 1 }

function Get-Sha256String([string]$text) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($text)
    $hasher = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $hasher.ComputeHash($bytes)
        return ([System.BitConverter]::ToString($hashBytes)).Replace("-", "").ToLowerInvariant()
    } finally {
        $hasher.Dispose()
    }
}

function Get-FreePort {
    try {
        $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
        $listener.Start()
        $port = ($listener.LocalEndpoint).Port
        $listener.Stop()
        return $port
    } catch {
        return 18099
    }
}

Write-Log "Starting local CI verification gate..."

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Fail "'dotnet' SDK was not found in PATH. Per AGENTS.md Constitution Principle V, development and verification must run inside the devcontainer (or have .NET 8 SDK installed). Please attach to the devcontainer or run inside the api container."
}

# Stage 1: Format
$hasCsChanges = [bool](git status --porcelain 2>$null | Select-String -Pattern '\.cs$')
if ($Fast -and -not $hasCsChanges) {
    Write-Pass "Stage 1/6: Skipped C# format check (no .cs files modified)."
} else {
    Write-Log "Stage 1/6: Checking C# code format..."
    dotnet restore src/ControlEasyReborn.sln --verbosity quiet --nologo
    dotnet format src/ControlEasyReborn.sln --verify-no-changes --no-restore
    if ($LASTEXITCODE -ne 0) { Write-Fail "Format check failed! Run 'dotnet format src/ControlEasyReborn.sln' to fix formatting." }
    Write-Pass "Code format verified."
}

# Stage 2: Build
Write-Log "Stage 2/6: Building solution in Release mode..."
dotnet build src/ControlEasyReborn.sln --configuration Release --nologo
if ($LASTEXITCODE -ne 0) { Write-Fail "Build failed." }
Write-Pass "Solution built successfully."

# Stage 3: Fast Tests
Write-Log "Stage 3/6: Running Unit & Architecture tests..."
dotnet test tests/ControlEasyReborn.UnitTests --configuration Release --no-build --nologo --logger "console;verbosity=minimal"
if ($LASTEXITCODE -ne 0) { Write-Fail "Unit tests failed." }
dotnet test tests/ControlEasyReborn.ArchitectureTests --configuration Release --no-build --nologo --logger "console;verbosity=minimal"
if ($LASTEXITCODE -ne 0) { Write-Fail "Architecture tests failed." }
Write-Pass "Unit and Architecture tests passed."

# Stage 4: Integration Tests
if ($Fast -or $SkipIntegration) {
    Write-Warn "Stage 4/6: Skipping integration tests."
} else {
    Write-Log "Stage 4/6: Running Integration tests (Testcontainers MySQL)..."
    dotnet test tests/ControlEasyReborn.IntegrationTests --configuration Release --no-build --nologo --logger "console;verbosity=minimal"
    if ($LASTEXITCODE -ne 0) { Write-Fail "Integration tests failed." }
    Write-Pass "Integration tests passed."
}

# Stage 5: Web Check
if ($Fast -or $SkipDocker) {
    Write-Warn "Stage 5/6: Skipping Web build check."
} else {
    Write-Log "Stage 5/6: Verifying Web production build via Docker..."
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        docker build -f docker/web.Dockerfile -t ce-local-verify-web:latest .
        if ($LASTEXITCODE -ne 0) { Write-Fail "Web build failed." }
        docker rmi ce-local-verify-web:latest 2>$null | Out-Null
        Write-Pass "Web production build verified."
    } else {
        Write-Warn "Docker command not available; skipping Web container build."
    }
}

# Stage 6: OpenAPI Check
if ($SkipOpenApi) {
    Write-Warn "Stage 6/6: Skipping OpenAPI drift check."
} else {
    Write-Log "Stage 6/6: Verifying OpenAPI client and swagger drift..."
    $apiPort = if ($env:CE_SWAGGER_PORT) { $env:CE_SWAGGER_PORT } else { Get-FreePort }
    $swaggerTemp = [System.IO.Path]::GetTempFileName()
    $apiDll = Join-Path $repoRoot "src/Host/ControlEasyReborn.Api/bin/Release/net8.0/ControlEasyReborn.Api.dll"
    if (-not (Test-Path $apiDll)) {
        $found = Get-ChildItem -Path (Join-Path $repoRoot "src/Host/ControlEasyReborn.Api/bin") -Filter "ControlEasyReborn.Api.dll" -Recurse | Select-Object -First 1
        if ($found) { $apiDll = $found.FullName }
        else { Write-Fail "API binary not found at $apiDll. Build may have failed." }
    }

    $prevEnv = @{
        ASPNETCORE_ENVIRONMENT = $env:ASPNETCORE_ENVIRONMENT
        Bootstrap__Enabled = $env:Bootstrap__Enabled
        Db__Provider = $env:Db__Provider
        Db__Host = $env:Db__Host
        Db__Port = $env:Db__Port
        Db__Database = $env:Db__Database
        Db__Username = $env:Db__Username
        Db__Password = $env:Db__Password
        Jwt__SigningKey = $env:Jwt__SigningKey
        Jwt__Issuer = $env:Jwt__Issuer
        Jwt__Audience = $env:Jwt__Audience
    }

    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:Bootstrap__Enabled = "false"
    $env:Db__Provider = "MySQL"
    $env:Db__Host = "127.0.0.1"
    $env:Db__Port = "3306"
    $env:Db__Database = "controleasydb"
    $env:Db__Username = "dummy"
    $env:Db__Password = "dummy"
    $env:Jwt__SigningKey = "CI-DUMMY-KEY-FOR-SWAGGER-GEN-ONLY-32-CHARS!!"
    $env:Jwt__Issuer = "ControlEasyReborn"
    $env:Jwt__Audience = "ControlEasyReborn"

    $apiBinDir = Split-Path -Parent $apiDll
    $apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "`"$apiDll`"", "--urls", "http://127.0.0.1:$apiPort", "--contentRoot", "`"$apiBinDir`"" -PassThru -NoNewWindow
    try {
        $deadline = (Get-Date).AddSeconds(45)
        $ready = $false
        while ((Get-Date) -lt $deadline) {
            try {
                $res = Invoke-WebRequest -Uri "http://127.0.0.1:$apiPort/swagger/v1/swagger.json" -TimeoutSec 2 -UseBasicParsing
                if ($res.StatusCode -eq 200) {
                    [System.IO.File]::WriteAllText($swaggerTemp, $res.Content, [System.Text.Encoding]::UTF8)
                    $ready = $true
                    break
                }
            } catch {
                Start-Sleep -Seconds 1
            }
        }
        if (-not $ready) { Write-Fail "API failed to start within 45s for swagger generation." }

        Push-Location "src/Web/ControlEasyReborn.Web"
        try {
            npx -y ng-openapi-gen --input $swaggerTemp --config ng-openapi-gen.json
            if ($LASTEXITCODE -ne 0) { Write-Fail "ng-openapi-gen generation failed with exit code $LASTEXITCODE." }
            git diff --exit-code -- src/app/api
            if ($LASTEXITCODE -ne 0) { Write-Fail "OpenAPI client drift detected! src/app/api differs from swagger." }
        } finally {
            Pop-Location
        }
    } finally {
        if ($apiProcess -and -not $apiProcess.HasExited) {
            Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
        }
        if (Test-Path $swaggerTemp) { Remove-Item $swaggerTemp -Force -ErrorAction SilentlyContinue }
        foreach ($k in $prevEnv.Keys) {
            $env:$k = $prevEnv[$k]
        }
    }
    Write-Pass "OpenAPI drift check passed."
}

# Stamp
$headSha = git rev-parse HEAD 2>$null
if (-not $headSha) { $headSha = "none" }
$diffText = git diff HEAD 2>$null | Out-String
$diffHash = Get-Sha256String $diffText
$statusText = git status --porcelain=v1 -uall 2>$null | Out-String
$statusHash = Get-Sha256String $statusText
$timestamp = (Get-Date -AsUTC -Format o)
$stampContent = "$headSha`:$diffHash`:$statusHash`:$timestamp"

if ($Fast -or $SkipIntegration -or $SkipDocker -or $SkipOpenApi) {
    $fastStampFile = Join-Path $gitDir "ci-local-fast-passed.stamp"
    Set-Content -Path $fastStampFile -Value $stampContent
    Write-Pass "Fast local CI verification checks PASSED (zero Docker downloads)!"
} else {
    Set-Content -Path $stampFile -Value $stampContent
    $fastStampFile = Join-Path $gitDir "ci-local-fast-passed.stamp"
    Set-Content -Path $fastStampFile -Value $stampContent
    Write-Pass "All local CI verification gates (including Docker web build & Testcontainers) PASSED!"
}
