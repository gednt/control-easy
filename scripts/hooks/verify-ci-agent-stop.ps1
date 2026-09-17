<#
.SYNOPSIS
    scripts/hooks/verify-ci-agent-stop.ps1 — Agent Stop lifecycle hook (PowerShell / Windows).
#>

$ErrorActionPreference = "SilentlyContinue"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
Set-Location $repoRoot

$inputRaw = ""
if ([System.Console]::IsInputRedirected) {
    $inputRaw = [System.Console]::In.ReadToEnd()
}

$inputJson = $null
if ($inputRaw) {
    try {
        $inputJson = $inputRaw | ConvertFrom-Json -ErrorAction SilentlyContinue
    } catch {}
}

# Recursion guard
if ($inputJson -and $inputJson.stop_hook_active -eq $true) {
    Write-Output "{}"
    exit 0
}

$gitDir = git rev-parse --git-dir 2>$null
if (-not $gitDir) { $gitDir = Join-Path $repoRoot ".git" }
$stampFile = Join-Path $gitDir "ci-local-passed.stamp"
$fastStampFile = Join-Path $gitDir "ci-local-fast-passed.stamp"

$isDirty = [bool](git status --porcelain 2>$null)
$isAhead = $false

$baseRef = $null
if (git rev-parse --verify "origin/main" 2>$null) {
    $baseRef = "origin/main"
} elseif (git rev-parse --verify "main" 2>$null) {
    $baseRef = "main"
} elseif (git rev-parse --verify "origin/HEAD" 2>$null) {
    $baseRef = "origin/HEAD"
}

if ($baseRef) {
    $commitsAhead = git rev-list "$baseRef..HEAD" --count 2>$null
    if ($commitsAhead -and [int]$commitsAhead -gt 0) {
        $isAhead = $true
    }
} else {
    if (git rev-parse --verify HEAD 2>$null) {
        $isAhead = $true
    }
}

if (-not $isDirty -and -not $isAhead) {
    Write-Output "{}"
    exit 0
}

$headSha = git rev-parse HEAD 2>$null
$diffText = git diff HEAD 2>$null | Out-String
$bytes = [System.Text.Encoding]::UTF8.GetBytes($diffText)
$diffHash = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData($bytes)).Replace("-", "").ToLowerInvariant()
$expectedPrefix = "$headSha`:$diffHash"

$verifyScript = Join-Path $repoRoot "scripts\verify-ci-local.ps1"
$verifyOutput = $null

if ($isDirty) {
    # Active editing / intermediate turn: fast checks only (zero Docker downloads)
    if (Test-Path $fastStampFile) {
        $fastContent = (Get-Content -Path $fastStampFile -TotalCount 1 2>$null)
        if ($fastContent -and $fastContent.StartsWith($expectedPrefix)) {
            Write-Output "{}"
            exit 0
        }
    }
    if (Test-Path $stampFile) {
        $stampContent = (Get-Content -Path $stampFile -TotalCount 1 2>$null)
        if ($stampContent -and $stampContent.StartsWith($expectedPrefix)) {
            Write-Output "{}"
            exit 0
        }
    }

    $verifyOutput = & pwsh -File $verifyScript -Fast 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Output "{}"
        exit 0
    }
} else {
    # Working tree clean and commits ahead: task completion / end of all tasks completion
    if (Test-Path $stampFile) {
        $stampContent = (Get-Content -Path $stampFile -TotalCount 1 2>$null)
        if ($stampContent -and $stampContent.StartsWith($expectedPrefix)) {
            Write-Output "{}"
            exit 0
        }
    }

    # Run full verification gate once per task completion
    $verifyOutput = & pwsh -File $verifyScript 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Output "{}"
        exit 0
    }
}

$lastLines = ($verifyOutput | Select-Object -Last 25 | Out-String)
$reason = "MANDATORY LOCAL CI GATE FAILED: Local CI verification jobs failed. Per project rules, an agent MUST NOT consider any modification done until all local CI jobs pass. Please fix: $lastLines"

$isAntigravity = ($inputJson -and ($inputJson.terminationReason -or $inputJson.artifactDirectoryPath))
$decision = if ($isAntigravity) { "continue" } else { "block" }

$response = @{
    decision = $decision
    reason = $reason
}

$response | ConvertTo-Json -Compress

if ($isAntigravity) {
    exit 0
} else {
    exit 2
}
