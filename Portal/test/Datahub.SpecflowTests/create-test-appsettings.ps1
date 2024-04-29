Write-Output "Fetching secrets from keyvault"

$resourcePrefix = "fsdh"
$vaultName = "fsdh-static-test-akv"

$tenantId = (az keyvault secret show --name "datahub-portal-tenant-id" --vault-name $vaultName --query value -o tsv)
$subscriptionId = (az keyvault secret show --name "datahub-portal-subscription-id" --vault-name $vaultName --query value -o tsv)
$domain = (az keyvault secret show --name "datahub-portal-domain" --vault-name $vaultName --query value -o tsv)


$hashTable = @{
    "AzureAd" = @{
        "Instance" = "https://login.microsoftonline.com/"
        "Domain" = $domain
        "TenantId" = $tenantId
        "SubscriptionId" = $subscriptionId
        "InfraClientId" = (az keyvault secret show --name "devops-client-id" --vault-name $vaultName --query value -o tsv)
        "InfraClientSecret" = (az keyvault secret show --name "devops-client-secret" --vault-name $vaultName --query value -o tsv)
    }
}

Write-Output "Converting secrets to json object"
$jsonObject = $hashTable | ConvertTo-Json -Depth 100

Write-Output "Writing json object to appsettings.test.json"
$jsonObject | Out-File -FilePath "appsettings.test.json"

Write-Output "Done"