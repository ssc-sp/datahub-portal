param(
    [Parameter(Mandatory=$false)]
    [string]$LocalPath = "C:\Temp",

    [Parameter(Mandatory=$false)]
    [string]$EnvironmentName = "dev",

    [Parameter(Mandatory=$false)]
    [switch]$SkipExport
)


$CurrentPath = Split-Path -Parent $MyInvocation.MyCommand.Path
#get full path from $CurrentPath
Import-Module $CurrentPath/appsettings.psm1 -Force
Import-Module $CurrentPath/dbutils.psm1 -Force

Connect-FSDHAzure
$server = Read-VaultSecret "g2dc-cto-fsdh-$EnvironmentName-kv" "datahub-mssql-server"
$db1 = Read-VaultSecret "g2dc-cto-fsdh-$EnvironmentName-kv" "datahub-mssql-projectdb"

Copy-AzureDbToLocal -ServerName $server -DatabaseName $db1 -LocalPath $LocalPath -SkipExport:$SkipExport -EnvironmentName $EnvironmentName

$db2 = Read-VaultSecret "g2dc-cto-fsdh-$EnvironmentName-kv" "dh-portal-metadatadb"
Copy-AzureDbToLocal -ServerName $server -DatabaseName $db2 -LocalPath $LocalPath -SkipExport:$SkipExport -EnvironmentName $EnvironmentName
