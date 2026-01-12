param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName,
    
    [Parameter(Mandatory=$true)]
    [string]$DatabaseName,

    [Parameter(Mandatory=$false)]
    [string]$LocalPath = "C:\Temp",

    [Parameter(Mandatory=$false)]
    [switch]$SkipExport,

    [Parameter(Mandatory=$false)]
    [switch]$UseKeyVaultCredentials
)

$CurrentPath = Split-Path -Parent $MyInvocation.MyCommand.Path
#get full path from $CurrentPath

Import-Module $CurrentPath/scripts/appsettings.psm1 -Force
Import-Module $CurrentPath/scripts/dbutils.psm1 -Force

Copy-AzureDbToLocal -ServerName $ServerName -DatabaseName $DatabaseName -LocalPath $LocalPath -SkipExport:$SkipExport -UseKeyVaultCredentials:$UseKeyVaultCredentials
