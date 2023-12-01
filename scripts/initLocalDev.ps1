# Initial your local development environment

az login
az account set -s "NRCan Dev"

$rgName = "ciosb-dev-datahub-ca-central"
$envName = "dev"
$kvName = ("datahub-key-"+$envName.ToLower())

$tenantId=az account show --query "tenantId"
$sqlUser=az keyvault secret show --vault-name $kvName -n datahub-mssql-admin --query "value"
$sqlPassword=az keyvault secret show --vault-name $kvName -n datahub-mssql-password --query "value"
$clientId=az keyvault secret show --vault-name $kvName -n datahubportal-client-id --query "value"
$clientSecret=az keyvault secret show --vault-name $kvName -n datahubportal-client-secret --query "value"
$instruKey=az webapp config appsettings list -g $rgName -n ("dh-portal-app-" + $envName.ToLower()) --query "[?name=='APPINSIGHTS_INSTRUMENTATIONKEY'].{value:value}[0].value"

$files=Get-Childitem -Path WebPortal,DatahubIntakeForms,.vscode -Include  appsettings*json-tmpl,launch*json-tmpl -File -Recurse -ErrorAction SilentlyContinue  | Where {$_.FullName -notlike "*\Debug\*"}
foreach ($file in $files){
    $fileRendered=$file.FullName.trim("-tmpl")
    Write-Host "Processing file: ${file} > $fileRendered"

    Copy-Item $file.FullName -Force -Destination $fileRendered

    ((Get-Content -path $fileRendered -Raw) -replace "###MSSQL_PASSWORD###", $sqlPassword.trim('"')) | Set-Content -Path $fileRendered
    ((Get-Content -path $fileRendered -Raw) -replace "###MSSQL_USER###", $sqlUser.trim('"')) | Set-Content -Path $fileRendered
    ((Get-Content -path $fileRendered -Raw) -replace "###DH_CLIENT_ID###", $clientId.trim('"')) | Set-Content -Path $fileRendered
    ((Get-Content -path $fileRendered -Raw) -replace "###DH_CLIENT_SECRET###", $clientSecret.trim('"')) | Set-Content -Path $fileRendered  
    ((Get-Content -path $fileRendered -Raw) -replace "###DH_TENANT_ID###", $tenantId.trim('"')) | Set-Content -Path $fileRendered  
    ((Get-Content -path $fileRendered -Raw) -replace "###DH_INSTRU_KEY###", $instruKey.trim('"')) | Set-Content -Path $fileRendered      

    $fileRelativePath = ($fileRendered|Resolve-Path -Relative).Replace("\", "/").trim("./")
    if ((Get-Content .gitignore | Select-String -SimpleMatch -Pattern $fileRelativePath).length -lt 1) {
        Add-Content .gitignore "$fileRelativePath"
    }
}



