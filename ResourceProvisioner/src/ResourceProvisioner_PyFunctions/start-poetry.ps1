#!/usr/bin/env pwsh
param(
    [string]$Environment = $null
)

Write-Output "Starting Azure Functions host with Poetry"

if ([string]::IsNullOrWhiteSpace($Environment)) {
    $Environment = if ($env:DataHub_ENVNAME) { $env:DataHub_ENVNAME } else { 'dev' }
}
$env:DataHub_ENVNAME = $Environment

$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $projectDir "../../..")).Path
$modulePath = Join-Path $repoRoot "scripts/appsettings.psm1"

Set-Location $projectDir

if (-not (Test-Path $modulePath)) {
    Write-Error "Unable to locate appsettings module at $modulePath."
    exit 1
}

Import-Module $modulePath -Force
if (-not (Connect-FSDHAzure)) {
    exit 1
}

if (-not (Get-Command poetry -ErrorAction SilentlyContinue)) {
    Write-Error "Poetry is not installed or not available on PATH."
    exit 1
}

function Read-VaultSecret($vault, $secretId)
{
    try {
        return Get-AzKeyVaultSecret -VaultName $vault -Name $secretId -AsPlainText
    }
    catch {
        Write-Error "Error reading secret $secretId from vault $vault - do you have read access in $vault policies?"
        return
    }
}

$vaultName = Get-FSDHKeyVaultName -Environment $Environment
$env:AzureClientId = (Read-VaultSecret $vaultName "devops-client-id")
$env:AzureClientSecret = (Read-VaultSecret $vaultName "devops-client-secret")
$env:AzureTenantId = "8c1a4d93-d828-4d0e-9303-fd3bd611c822"
$env:AzureSubscriptionId = (Read-VaultSecret $vaultName "datahub-portal-subscription-id")
$env:DatahubServiceBus = (Read-VaultSecret $vaultName "service-bus-connection-string")

Write-Output "Installing dependencies with Poetry..."
poetry install
if ($LASTEXITCODE -ne 0) {
    Write-Error "Poetry install failed."
    exit $LASTEXITCODE
}

Write-Output "Starting Azure Functions host..."
poetry run func start --python
exit $LASTEXITCODE
