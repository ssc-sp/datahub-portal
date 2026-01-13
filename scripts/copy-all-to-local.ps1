param(
    [Parameter(Mandatory=$false)]
    [string]$LocalPath = "C:\Temp",

    [Parameter(Mandatory=$false)]
    [switch]$SkipExport
)


$CurrentPath = Split-Path -Parent $MyInvocation.MyCommand.Path
#get full path from $CurrentPath
Import-Module $CurrentPath/appsettings.psm1 -Force
Import-Module $CurrentPath/dbutils.psm1 -Force

Connect-FSDHAzure

$server = Read-VaultSecret "fsdh-key-dev" "datahub-mssql-server"
$db1 = Read-VaultSecret "fsdh-key-dev" "datahub-mssql-projectdb"

Copy-AzureDbToLocal -ServerName $server -DatabaseName $db1 -LocalPath $LocalPath -SkipExport:$SkipExport -UseKeyVaultCredentials:$True

$db2 = Read-VaultSecret "fsdh-key-dev" "dh-portal-metadatadb"

Copy-AzureDbToLocal -ServerName $server -DatabaseName $db2 -LocalPath $LocalPath -SkipExport:$SkipExport -UseKeyVaultCredentials:$True
