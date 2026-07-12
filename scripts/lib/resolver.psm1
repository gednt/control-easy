# scripts/lib/resolver.psm1 — cross-OS host resolver shim for
# ce-<id>.localhost hostnames (PowerShell edition).
#
# Provides: Add-CeResolver, Remove-CeResolver, Test-CeResolver.
#
# Modes:
#   auto        Windows: Add-DnsClientNrptRule (requires admin), fallback
#               to per-user hosts file at $HOME/.control-easy/hosts.
#   nrpt        Windows: Add-DnsClientNrptRule (requires admin).
#   hosts-file  Per-user hosts file fallback (no admin required).

function Write-ResolverLog  { param([string]$Message) Write-Host "[resolver] $Message" }
function Write-ResolverWarn { param([string]$Message) Write-Warning "[resolver][WARN] $Message" }
function Write-ResolverErr  { param([string]$Message) Write-Error  "[resolver][ERROR] $Message" }

function Test-CeResolver {
    param([Parameter(Mandatory)][string]$Hostname)
    try {
        $result = Resolve-DnsName -Name $Hostname -ErrorAction SilentlyContinue
        return ($null -ne ($result | Where-Object { $_.IPAddress -eq '127.0.0.1' }))
    } catch { return $false }
}

function Add-CeResolverHostsFile {
    param([Parameter(Mandatory)][string]$Hostname)
    $dir = Join-Path $HOME ".control-easy"
    $file = Join-Path $dir "hosts"
    $entry = "127.0.0.1 $Hostname"
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    if (Test-Path $file) {
        $existing = Get-Content $file -ErrorAction SilentlyContinue
        if ($existing -match "^\d+\.\d+\.\d+\.\d+\s+$([regex]::Escape($Hostname))$") {
            Write-ResolverLog "$Hostname already in $file"
            return
        }
    }
    Add-Content -Path $file -Value $entry
    Write-ResolverLog "Added $entry to $file"
    Write-ResolverWarn "Per-user hosts file mode. Merge the following into your"
    Write-ResolverWarn "system resolver manually, or re-run as admin for NRPT mode:"
    Write-ResolverWarn "    $file"
}

function Remove-CeResolverHostsFile {
    param([Parameter(Mandatory)][string]$Hostname)
    $dir = Join-Path $HOME ".control-easy"
    $file = Join-Path $dir "hosts"
    if (-not (Test-Path $file)) { return }
    $lines = Get-Content $file
    $pattern = "^\d+\.\d+\.\d+\.\d+\s+$([regex]::Escape($Hostname))$"
    $kept = $lines | Where-Object { $_ -notmatch $pattern }
    if ($kept.Count -ne $lines.Count) {
        Set-Content -Path $file -Value $kept
        Write-ResolverLog "Removed $Hostname from $file"
    }
}

function Add-CeResolverNrpt {
    param([Parameter(Mandatory)][string]$Hostname)
    Write-ResolverLog "Adding NRPT rule for $Hostname (requires admin)"
    try {
        Add-DnsClientNrptRule -Namespace ".$Hostname" -NameServers '127.0.0.1' `
            -Comment 'ControlEasy-worktree' -ErrorAction Stop
    } catch {
        throw "Add-DnsClientNrptRule failed (admin required). Re-run with -ResolverMode hosts-file for the per-user fallback."
    }
}

function Remove-CeResolverNrpt {
    param([Parameter(Mandatory)][string]$Hostname)
    Write-ResolverLog "Removing NRPT rule for $Hostname"
    try {
        Get-DnsClientNrptRule -ErrorAction SilentlyContinue |
            Where-Object { $_.Comment -eq 'ControlEasy-worktree' -and $_.Namespace -eq ".$Hostname" } |
            Remove-DnsClientNrptRule -Force -ErrorAction SilentlyContinue
    } catch { Write-ResolverWarn "NRPT rule removal failed: $_" }
}

function Add-CeResolver {
    param(
        [Parameter(Mandatory)][string]$Hostname,
        [ValidateSet('auto','nrpt','hosts-file')]
        [string]$ResolverMode = 'auto'
    )
    switch ($ResolverMode) {
        'auto' {
            try { Add-CeResolverNrpt -Hostname $Hostname }
            catch {
                Write-ResolverWarn "NRPT failed: $_"
                Add-CeResolverHostsFile -Hostname $Hostname
            }
        }
        'nrpt' { Add-CeResolverNrpt -Hostname $Hostname }
        'hosts-file' { Add-CeResolverHostsFile -Hostname $Hostname }
    }
}

function Remove-CeResolver {
    param(
        [Parameter(Mandatory)][string]$Hostname,
        [ValidateSet('auto','nrpt','hosts-file')]
        [string]$ResolverMode = 'auto'
    )
    switch ($ResolverMode) {
        'auto' {
            Remove-CeResolverNrpt -Hostname $Hostname
            Remove-CeResolverHostsFile -Hostname $Hostname
        }
        'nrpt' { Remove-CeResolverNrpt -Hostname $Hostname }
        'hosts-file' { Remove-CeResolverHostsFile -Hostname $Hostname }
    }
}

Export-ModuleMember -Function `
    Test-CeResolver, `
    Add-CeResolverHostsFile, Remove-CeResolverHostsFile, `
    Add-CeResolverNrpt, Remove-CeResolverNrpt, `
    Add-CeResolver, Remove-CeResolver