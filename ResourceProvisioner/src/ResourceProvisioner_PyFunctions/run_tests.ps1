#!/usr/bin/env pwsh
param(
    [ValidateSet("test", "dev", "int", "poc")]
    [string]$Environment = $null,

    [switch]$ConfigureOnly
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

if ([string]::IsNullOrWhiteSpace($Environment)) {
    $Environment = if ($env:DataHub_ENVNAME) { $env:DataHub_ENVNAME } else { 'dev' }
}
$env:DataHub_ENVNAME = $Environment

Write-Output "Setting environment variables from Azure Key Vault"

$repoRoot = (Resolve-Path (Join-Path $scriptDir "../../..")).Path
$modulePath = Join-Path $repoRoot "scripts/appsettings.psm1"

if (-not (Test-Path $modulePath)) {
    Write-Error "Unable to locate appsettings module at $modulePath."
    exit 1
}

Import-Module $modulePath -Force
if (-not (Connect-FSDHAzure)) {
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
$azureContext = Get-AzContext -ErrorAction Stop
$env:AzureTenantId = if ($script:AzureTenantId) { $script:AzureTenantId } else { $azureContext.Tenant.Id }
$env:AzureSubscriptionId = if ($script:AzureSubscriptionId) { $script:AzureSubscriptionId } else { $azureContext.Subscription.Id }
$env:AzureClientId = (Read-VaultSecret $vaultName "devops-client-id")
$env:AzureClientSecret = (Read-VaultSecret $vaultName "devops-client-secret")
$env:DatahubServiceBus = (Read-VaultSecret $vaultName "service-bus-connection-string")
$env:AzureWebJobsStorage = (Read-VaultSecret $vaultName "datahub-storage-queue-conn-str")
$env:AzureWebJobsDashboard = $env:AzureWebJobsStorage
$env:AzureWebJobsAzureStorageQueueConnectionString = $env:AzureWebJobsStorage

Write-Output "Environment variables set - service bus is $($env:DatahubServiceBus)"
Write-Output "Logging to ACR"

if (-not (Get-Module -ListAvailable -Name Az.ContainerRegistry)) {
    Write-Output "Az.ContainerRegistry module not found. Installing..."
    Install-Module -Name Az.ContainerRegistry -Force -Scope CurrentUser
} else {
    Write-Output "Az.ContainerRegistry module is already installed."
}


if ($ConfigureOnly) {
    Write-Output "Environment configured. Skipping test execution."
    return
}

Write-Host "Ensuring Poetry is using Python 3.12..."
poetry env use python3.12
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Installing project dependencies..."
poetry install
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Running Python unit tests..."
poetry run python -m unittest discover -s tests -v
exit $LASTEXITCODE
