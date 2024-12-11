#!/usr/bin/env pwsh
Write-Output "Setting environment variables from Azure Key Vault"

# Check if the Az.KeyVault module is installed
if (-not (Get-Module -ListAvailable -Name Az.KeyVault)) {
    Write-Output "Az.KeyVault module not found. Installing..."
    Install-Module -Name Az.KeyVault -Force -Scope CurrentUser
} else {
    Write-Output "Az.KeyVault module is already installed."
}

#check if user is signed in on azure
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
	} catch {
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
Invoke-Expression $dockerCommand
