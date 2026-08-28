#!/usr/bin/env pwsh
Write-Output "Starting Azure Functions host with Poetry"

$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $projectDir

if (-not (Get-Command poetry -ErrorAction SilentlyContinue)) {
    Write-Error "Poetry is not installed or not available on PATH."
    exit 1
}

if (-not (Get-Module -ListAvailable -Name Az.KeyVault)) {
    Write-Output "Az.KeyVault module not found. Installing..."
    Install-Module -Name Az.KeyVault -Force -Scope CurrentUser
} else {
    Write-Output "Az.KeyVault module is already installed."
}

Import-Module Az.KeyVault -Force -NoClobber
$domain = "163oxygen.onmicrosoft.com"
$context = Get-AzContext
if ($null -eq $context) {
    connect-azaccount -Domain $domain -DeviceCode
} else {
    Write-Output "User $($context.Account.Id) is signed in."
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

$env:AzureClientId = (Read-VaultSecret "fsdh-key-dev" "devops-client-id")
$env:AzureClientSecret = (Read-VaultSecret "fsdh-key-dev" "devops-client-secret")
$env:AzureTenantId = "8c1a4d93-d828-4d0e-9303-fd3bd611c822"
$env:AzureSubscriptionId = (Read-VaultSecret "fsdh-key-dev" "datahub-portal-subscription-id")
$env:DatahubServiceBus = (Read-VaultSecret "fsdh-key-dev" "service-bus-connection-string")
$env:DataHub_ENVNAME = "dev"

Write-Output "Installing dependencies with Poetry..."
poetry install
if ($LASTEXITCODE -ne 0) {
    Write-Error "Poetry install failed."
    exit $LASTEXITCODE
}

Write-Output "Starting Azure Functions host..."
poetry run func start --python
exit $LASTEXITCODE
