#!/usr/bin/env pwsh
param(
    [string]$Environment = $null
)

Write-Output "Setting environment variables from Azure Key Vault"

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

function Read-VaultSecret($vault, $secretId)
{
    try {
        return Get-AzKeyVaultSecret -VaultName $vault -Name $secretId -AsPlainText
	} catch {
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
$env:AzureWebJobsStorage = (Read-VaultSecret $vaultName "datahub-storage-queue-conn-str")
$env:AzureWebJobsDashboard = $env:AzureWebJobsStorage
$env:AzureWebJobsAzureStorageQueueConnectionString = $env:AzureWebJobsStorage

$dockerCommand = "docker run -p 8080:80 " +
    "-e AzureClientId=$env:AzureClientId " +
    "-e AzureClientSecret=`"$env:AzureClientSecret`" " +
    "-e AzureTenantId=$env:AzureTenantId " +
    "-e AzureSubscriptionId=$env:AzureSubscriptionId " +
    "-e DatahubServiceBus=`"$env:DatahubServiceBus`" " +
    "-e DataHub_ENVNAME=$env:DataHub_ENVNAME " +
    "fsdh-pyfunction:latest"

Write-Output "Running the Docker container with the following command:"
Write-Output $dockerCommand

$dockerArgs = @(
    'run',
    '--name', 'fsdh-pyfunction',
    '--rm',
    '-it',
    '-p', '8080:80',
    '-e', "AzureClientId=$env:AzureClientId",
    '-e', "AzureClientSecret=$env:AzureClientSecret",
    '-e', "AzureTenantId=$env:AzureTenantId",
    '-e', "AzureSubscriptionId=$env:AzureSubscriptionId",
    '-e', "DatahubServiceBus=$env:DatahubServiceBus",
    '-e', "DataHub_ENVNAME=$env:DataHub_ENVNAME",    '-e', "AzureWebJobsStorage=$env:AzureWebJobsStorage",
    '-e', "AzureWebJobsDashboard=$env:AzureWebJobsDashboard",
    '-e', "AzureWebJobsAzureStorageQueueConnectionString=$env:AzureWebJobsAzureStorageQueueConnectionString",    'fsdh-pyfunction:latest'
)

try {
    & docker @dockerArgs
    exit $LASTEXITCODE
}
catch {
    Write-Error $_
    exit 1
}
