$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet build .\Vbodlaci.sln -c Debug
    dotnet test .\Vbodlaci.sln -c Debug
    .\scripts\run-browser-smoke.ps1
}
finally {
    Pop-Location
}
