[CmdletBinding()]
param(
    [switch]$NoBrowser,
    [switch]$Silent
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$frontendRoot = Join-Path $repoRoot "frontend"
$runtimeRoot = Join-Path $env:LOCALAPPDATA "AI Finance Tracker\launcher"
$statePath = Join-Path $runtimeRoot "processes.json"
$backendUrl = "http://localhost:5218/api/categories"
$frontendUrl = "http://127.0.0.1:5173"

function Show-LauncherMessage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message,

        [int]$Icon = 64
    )

    if ($Silent) {
        Write-Error $Message
        return
    }

    $shell = New-Object -ComObject WScript.Shell
    [void]$shell.Popup($Message, 0, "AI Finance Tracker", $Icon)
}

function Test-Url {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url,

        [string]$ExpectedText
    )

    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2
        if ($response.StatusCode -lt 200 -or $response.StatusCode -ge 400) {
            return $false
        }

        return [string]::IsNullOrEmpty($ExpectedText) -or $response.Content.Contains($ExpectedText)
    }
    catch {
        return $false
    }
}

function Get-ProcessRecord {
    param(
        [Parameter(Mandatory = $true)]
        [System.Diagnostics.Process]$Process
    )

    return [ordered]@{
        id = $Process.Id
        name = $Process.ProcessName
        startedAt = $Process.StartTime.ToUniversalTime().ToString("O")
    }
}

function Stop-LauncherProcess {
    param(
        [System.Diagnostics.Process]$Process
    )

    if ($null -eq $Process -or $Process.HasExited) {
        return
    }

    & taskkill.exe /PID $Process.Id /T /F | Out-Null
}

function Wait-ForApplication {
    param(
        [System.Diagnostics.Process]$BackendProcess,
        [System.Diagnostics.Process]$FrontendProcess
    )

    $deadline = (Get-Date).AddSeconds(60)

    while ((Get-Date) -lt $deadline) {
        if ($null -ne $BackendProcess -and $BackendProcess.HasExited) {
            throw "Backend zakonczyl dzialanie przed uruchomieniem aplikacji."
        }

        if ($null -ne $FrontendProcess -and $FrontendProcess.HasExited) {
            throw "Frontend zakonczyl dzialanie przed uruchomieniem aplikacji."
        }

        $backendReady = Test-Url -Url $backendUrl
        $frontendReady = Test-Url -Url $frontendUrl -ExpectedText "<title>AI Finance Tracker</title>"
        if ($backendReady -and $frontendReady) {
            return
        }

        Start-Sleep -Milliseconds 500
    }

    throw "Aplikacja nie uruchomila sie w ciagu 60 sekund."
}

$backendProcess = $null
$frontendProcess = $null

try {
    New-Item -ItemType Directory -Path $runtimeRoot -Force | Out-Null

    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnetCommand) {
        throw "Nie znaleziono .NET SDK. Zainstaluj .NET SDK 9 i sprobuj ponownie."
    }

    $npmCommand = Get-Command npm.cmd -ErrorAction SilentlyContinue
    if ($null -eq $npmCommand) {
        throw "Nie znaleziono npm. Zainstaluj Node.js i sprobuj ponownie."
    }

    if (-not (Test-Path -LiteralPath (Join-Path $frontendRoot "node_modules"))) {
        throw "Brakuje zaleznosci frontendu. Uruchom raz: cd frontend; npm install --no-audit --no-fund"
    }

    $existingState = $null
    if (Test-Path -LiteralPath $statePath) {
        try {
            $existingState = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json
        }
        catch {
            Remove-Item -LiteralPath $statePath -Force -ErrorAction SilentlyContinue
        }
    }

    $backendReady = Test-Url -Url $backendUrl
    $frontendReady = Test-Url -Url $frontendUrl -ExpectedText "<title>AI Finance Tracker</title>"

    if ($backendReady -and $frontendReady) {
        if (-not $NoBrowser) {
            Start-Process $frontendUrl
        }

        exit 0
    }

    if (-not $backendReady) {
        $backendProcess = Start-Process `
            -FilePath $dotnetCommand.Source `
            -ArgumentList @(
                "run",
                "--project",
                (Join-Path $repoRoot "ai-finance-tracker.csproj"),
                "--launch-profile",
                "http",
                "-p:UseAppHost=false"
            ) `
            -WorkingDirectory $repoRoot `
            -WindowStyle Hidden `
            -RedirectStandardOutput (Join-Path $runtimeRoot "backend.out.log") `
            -RedirectStandardError (Join-Path $runtimeRoot "backend.error.log") `
            -PassThru
    }

    if (-not $frontendReady) {
        $frontendProcess = Start-Process `
            -FilePath $npmCommand.Source `
            -ArgumentList @(
                "run",
                "dev",
                "--",
                "--host",
                "127.0.0.1",
                "--port",
                "5173",
                "--strictPort"
            ) `
            -WorkingDirectory $frontendRoot `
            -WindowStyle Hidden `
            -RedirectStandardOutput (Join-Path $runtimeRoot "frontend.out.log") `
            -RedirectStandardError (Join-Path $runtimeRoot "frontend.error.log") `
            -PassThru
    }

    $state = [ordered]@{
        createdAt = (Get-Date).ToUniversalTime().ToString("O")
        backend = if ($null -ne $backendProcess) {
            Get-ProcessRecord -Process $backendProcess
        }
        elseif ($null -ne $existingState) {
            $existingState.backend
        }
        else {
            $null
        }
        frontend = if ($null -ne $frontendProcess) {
            Get-ProcessRecord -Process $frontendProcess
        }
        elseif ($null -ne $existingState) {
            $existingState.frontend
        }
        else {
            $null
        }
    }
    $state | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $statePath -Encoding UTF8

    Wait-ForApplication -BackendProcess $backendProcess -FrontendProcess $frontendProcess

    if (-not $NoBrowser) {
        Start-Process $frontendUrl
    }
}
catch {
    Stop-LauncherProcess -Process $frontendProcess
    Stop-LauncherProcess -Process $backendProcess
    Remove-Item -LiteralPath $statePath -Force -ErrorAction SilentlyContinue

    $message = @"
Nie udalo sie uruchomic aplikacji.

$($_.Exception.Message)

Logi:
$runtimeRoot
"@
    Show-LauncherMessage -Message $message -Icon 16
    exit 1
}
