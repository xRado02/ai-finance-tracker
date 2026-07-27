[CmdletBinding()]
param(
    [switch]$Silent
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$runtimeRoot = Join-Path $env:LOCALAPPDATA "AI Finance Tracker\launcher"
$statePath = Join-Path $runtimeRoot "processes.json"

function Show-LauncherMessage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message,

        [int]$Icon = 64
    )

    if ($Silent) {
        return
    }

    $shell = New-Object -ComObject WScript.Shell
    [void]$shell.Popup($Message, 0, "AI Finance Tracker", $Icon)
}

function Stop-RecordedProcess {
    param(
        [object]$Record
    )

    if ($null -eq $Record) {
        return $false
    }

    $process = Get-Process -Id ([int]$Record.id) -ErrorAction SilentlyContinue
    if ($null -eq $process -or $process.ProcessName -ne [string]$Record.name) {
        return $false
    }

    $recordedStart = [DateTime]::Parse([string]$Record.startedAt).ToUniversalTime()
    $actualStart = $process.StartTime.ToUniversalTime()
    if ([Math]::Abs(($actualStart - $recordedStart).TotalSeconds) -gt 2) {
        return $false
    }

    & taskkill.exe /PID $process.Id /T /F | Out-Null
    return $true
}

try {
    if (-not (Test-Path -LiteralPath $statePath)) {
        Show-LauncherMessage -Message "Nie znaleziono procesow uruchomionych przez launcher."
        exit 0
    }

    $state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json
    $stoppedFrontend = Stop-RecordedProcess -Record $state.frontend
    $stoppedBackend = Stop-RecordedProcess -Record $state.backend
    Remove-Item -LiteralPath $statePath -Force

    if ($stoppedFrontend -or $stoppedBackend) {
        Show-LauncherMessage -Message "AI Finance Tracker zostal zatrzymany."
    }
    else {
        Show-LauncherMessage -Message "Procesy launchera nie byly juz uruchomione."
    }
}
catch {
    Show-LauncherMessage -Message "Nie udalo sie zatrzymac aplikacji: $($_.Exception.Message)" -Icon 16
    exit 1
}
