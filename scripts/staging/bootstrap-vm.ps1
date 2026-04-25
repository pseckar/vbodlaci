param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectId,
    [Parameter(Mandatory = $true)]
    [string]$Zone,
    [Parameter(Mandatory = $true)]
    [string]$InstanceName,
    [string]$Domain = "35-231-76-42.nip.io",
    [string]$LetsEncryptEmail = "petrsec@gmail.com",
    [string]$DeployUser = "vbodlaci",
    [string]$AdminEmail = "petrsec@gmail.com",
    [string]$DeployPublicKey = "",
    [switch]$VerboseGcloud
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$remoteScript = "/tmp/bootstrap-vm.sh"
$localScript = ".\scripts\staging\bootstrap-vm.sh"

function Resolve-GcloudPath {
    $cmd = Get-Command gcloud -ErrorAction SilentlyContinue
    if ($cmd) {
        return $cmd.Source
    }

    $candidate = Join-Path $env:LOCALAPPDATA "Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd"
    if (Test-Path $candidate) {
        return $candidate
    }

    throw "gcloud CLI not found in PATH or default Cloud SDK location."
}

function Invoke-Gcloud {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    Write-Host ""
    Write-Host ">>> gcloud $($Arguments -join ' ')"
    if ($VerboseGcloud) {
        & $script:GcloudPath @Arguments --verbosity=debug
    }
    else {
        & $script:GcloudPath @Arguments
    }

    if ($LASTEXITCODE -ne 0) {
        throw "gcloud command failed with exit code $LASTEXITCODE"
    }
}

if (-not (Test-Path $localScript)) {
    throw "Local bootstrap script not found at: $localScript"
}

$script:GcloudPath = Resolve-GcloudPath
Write-Host "Using gcloud binary: $script:GcloudPath"

Invoke-Gcloud -Arguments @("--version")

$activeAccount = (& $script:GcloudPath auth list "--filter=status:ACTIVE" "--format=value(account)") | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($activeAccount)) {
    throw "No active gcloud account. Run 'gcloud auth login' first."
}
Write-Host "Active gcloud account: $activeAccount"

Invoke-Gcloud -Arguments @(
    "compute", "scp",
    $localScript,
    "${InstanceName}:${remoteScript}",
    "--project", $ProjectId,
    "--zone", $Zone
)

$bootstrapCommand = @"
set -euxo pipefail
chmod +x ${remoteScript}
sudo DOMAIN='${Domain}' LETSENCRYPT_EMAIL='${LetsEncryptEmail}' DEPLOY_USER='${DeployUser}' ADMIN_EMAIL='${AdminEmail}' DEPLOY_PUBLIC_KEY='${DeployPublicKey}' bash ${remoteScript}
"@

try {
    Invoke-Gcloud -Arguments @(
        "compute", "ssh", $InstanceName,
        "--project", $ProjectId,
        "--zone", $Zone,
        "--command", $bootstrapCommand
    )
}
catch {
    Write-Host ""
    Write-Host "Bootstrap command failed. Attempting to fetch /tmp/bootstrap-vm.log..."
    try {
        Invoke-Gcloud -Arguments @(
            "compute", "ssh", $InstanceName,
            "--project", $ProjectId,
            "--zone", $Zone,
            "--command", "sudo tail -n 200 /tmp/bootstrap-vm.log || true"
        )
    }
    catch {
        Write-Host "Could not fetch remote bootstrap log."
    }

    throw
}

Write-Host ""
Write-Host "Bootstrap command finished. Running post-checks..."
Invoke-Gcloud -Arguments @(
    "compute", "ssh", $InstanceName,
    "--project", $ProjectId,
    "--zone", $Zone,
    "--command", "set -e; sudo ls -l /etc/systemd/system/vbodlaci-staging.service; sudo ls -l /etc/vbodlaci/staging.env; sudo nginx -t; systemctl is-enabled nginx postgresql vbodlaci-staging.service"
)
