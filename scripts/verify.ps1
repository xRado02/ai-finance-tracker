[CmdletBinding()]
param(
    [switch]$SkipRestore
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

function Invoke-QualityStep {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    Write-Host ""
    Write-Host "==> $Name" -ForegroundColor Cyan
    & $Action

    if ($LASTEXITCODE -ne 0) {
        throw "$Name failed with exit code $LASTEXITCODE."
    }
}

Push-Location $repoRoot

try {
    if (-not $SkipRestore) {
        Invoke-QualityStep "Restore backend" {
            dotnet restore .\ai-finance-tracker.csproj
        }
        Invoke-QualityStep "Restore backend tests" {
            dotnet restore .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj
        }
    }

    Invoke-QualityStep "Build backend" {
        dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false
    }
    Invoke-QualityStep "Test backend" {
        dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj --no-restore -p:UseAppHost=false
    }

    Push-Location .\frontend

    try {
        Invoke-QualityStep "Typecheck frontend" {
            npm run typecheck
        }
        Invoke-QualityStep "Build frontend" {
            npm run build
        }
    }
    finally {
        Pop-Location
    }

    Write-Host ""
    Write-Host "All quality gates passed." -ForegroundColor Green
}
finally {
    Pop-Location
}
