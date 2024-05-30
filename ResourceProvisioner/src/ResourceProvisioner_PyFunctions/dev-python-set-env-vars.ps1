Write-Output "Setting environment variables from Azure Key Vault"
#check if user is signed in on azure
Import-Module Az.KeyVault -Force -NoClobber
$domain = "163oxygen.onmicrosoft.com"
$context = Get-AzContext
if ($null -eq $context) {
    connect-azaccount -Domain $domain
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
$env:Datahub_ENVNAME = "dev"
Write-Output "Environment variables set"
Write-Output "Use 'func start' to start the function app locally"
