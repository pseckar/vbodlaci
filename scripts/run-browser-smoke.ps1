$ErrorActionPreference = "Stop"

function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory = $true)] [string] $FilePath,
        [Parameter()] [string[]] $Arguments = @()
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed: $FilePath $($Arguments -join ' ')"
    }
}

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "Vbodlaci.Web\Vbodlaci.Web.csproj"
$smokeDir = Join-Path $root "tests\browser-smoke"
$artifactDir = Join-Path $root "artifacts\browser-smoke"
$appLog = Join-Path $artifactDir "app.log"
$smokeDatabase = "vbodlaci_smoke"
$smokeConnectionString = "Host=localhost;Port=5432;Database=$smokeDatabase;Username=vbodlaci;Password=vbodlaci_dev_password"

New-Item -ItemType Directory -Path $artifactDir -Force | Out-Null

if (Test-Path $appLog) {
    Remove-Item $appLog -Force
}

Write-Host "Preparing smoke database '$smokeDatabase'..."
Invoke-NativeCommand -FilePath "docker" -Arguments @("exec", "vbodlaci-postgres", "psql", "-U", "vbodlaci", "-d", "postgres", "-c", "DROP DATABASE IF EXISTS $smokeDatabase;")
Invoke-NativeCommand -FilePath "docker" -Arguments @("exec", "vbodlaci-postgres", "psql", "-U", "vbodlaci", "-d", "postgres", "-c", "CREATE DATABASE $smokeDatabase;")

Write-Host "Starting web app for smoke test..."
$appJob = Start-Job -ArgumentList $root, $project, $appLog, $smokeConnectionString -ScriptBlock {
    param($workspaceRoot, $projectPath, $logPath, $connectionString)
    Set-Location $workspaceRoot
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:Logging__EventLog__LogLevel__Default = "None"
    $env:ConnectionStrings__DefaultConnection = $connectionString
    & dotnet run --no-launch-profile --project $projectPath --no-build --urls http://127.0.0.1:5270 *> $logPath
}

try {
    $ready = $false
    for ($i = 0; $i -lt 40; $i++) {
        Start-Sleep -Milliseconds 750
        try {
            $resp = Invoke-WebRequest -Uri "http://127.0.0.1:5270/" -UseBasicParsing -TimeoutSec 2
            if ($resp.StatusCode -ge 200 -and $resp.StatusCode -lt 500) {
                $ready = $true
                break
            }
        }
        catch {
        }
    }

    if (-not $ready) {
        if (Test-Path $appLog) {
            Write-Host "App log tail:"
            Get-Content -Tail 120 $appLog | Write-Host
        }
        throw "Web app did not become ready in time."
    }

    Push-Location $smokeDir
    try {
        Invoke-NativeCommand -FilePath "npm" -Arguments @("install")
        Invoke-NativeCommand -FilePath "npx" -Arguments @("playwright", "install", "chromium")
        $env:BASE_URL = "http://127.0.0.1:5270"
        $env:SMOKE_ARTIFACT_DIR = $artifactDir
        $env:SMOKE_REPORT_PATH = (Join-Path $artifactDir "smoke-report.json")
        Invoke-NativeCommand -FilePath "npm" -Arguments @("run", "smoke")
    }
    finally {
        Pop-Location
    }
}
finally {
    if ($appJob) {
        if ($appJob.State -eq "Running") {
            Stop-Job -Job $appJob -ErrorAction SilentlyContinue
        }

        Receive-Job -Job $appJob -Keep -ErrorAction SilentlyContinue | Out-Null
        Remove-Job -Job $appJob -Force -ErrorAction SilentlyContinue
    }
}
