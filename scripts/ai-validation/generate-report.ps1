param(
    [string]$SpecificationPath = "SPECIFICATION.md",
    [string]$SmokeReportPath = "artifacts/browser-smoke/smoke-report.json",
    [string]$OutputPath = "artifacts/ai-validation/validation-report.md"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root

$spec = Get-Content $SpecificationPath -Raw
$specHash = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($spec))).Replace("-", "").ToLowerInvariant()

$smoke = $null
if (Test-Path $SmokeReportPath) {
    $smoke = Get-Content $SmokeReportPath -Raw | ConvertFrom-Json
}

$outDir = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Path $outDir -Force | Out-Null

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# Experimental Spec Runtime Validation")
$lines.Add("")
$lines.Add("Generated: $(Get-Date -Format o)")
$lines.Add("Specification hash (SHA-256): `$specHash`")
$lines.Add("")
$lines.Add("## Mode")
$lines.Add("")
$lines.Add("This is phase-1 experimental validation. It is non-blocking and advisory.")
$lines.Add("")

if ($null -eq $smoke) {
    $lines.Add("## Smoke Input")
    $lines.Add("")
    $lines.Add("Smoke report not found at `$SmokeReportPath`.")
} else {
    $lines.Add("## Smoke Summary")
    $lines.Add("")
    $lines.Add("Base URL: $($smoke.baseUrl)")
    $lines.Add("Overall success: $($smoke.success)")
    $lines.Add("")
    $lines.Add("| Route | Status | Pass | Error |")
    $lines.Add("|---|---:|:---:|---|")
    foreach ($check in $smoke.checks) {
        $error = if ([string]::IsNullOrWhiteSpace($check.error)) { "" } else { ($check.error -replace "\|", "\\|") }
        $lines.Add("| $($check.route) | $($check.status) | $($check.ok) | $error |")
    }
}

$lines.Add("")
$lines.Add("## Requirement Mapping (Advisory)")
$lines.Add("")
$lines.Add("- Public route reachability checked against key sections in `SPECIFICATION.md`.")
$lines.Add("- This report does not replace functional tests or manual review.")
$lines.Add("- Any mismatch should be triaged against frozen spec before code changes.")

Set-Content -Path $OutputPath -Value ($lines -join [Environment]::NewLine) -Encoding UTF8
Write-Host "Validation report generated at $OutputPath"
