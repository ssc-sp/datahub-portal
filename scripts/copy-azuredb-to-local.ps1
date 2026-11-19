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

$portalProject = Join-Path $CurrentPath "..\Portal\src\Datahub.Portal\Datahub.Portal.csproj"
foreach ($secret in @(
    "ConnectionStrings:datahub_mssql_project",
    "ConnectionStrings:datahub_mssql_metadata"
)) {
    & dotnet user-secrets remove $secret --project $portalProject
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to remove user secret '$secret' from Datahub.Portal. dotnet user-secrets returned exit code $LASTEXITCODE."
    }
}

Copy-AzureDbToLocal -ServerName $ServerName -DatabaseName $DatabaseName -LocalPath $LocalPath -SkipExport:$SkipExport -UseKeyVaultCredentials:$UseKeyVaultCredentials
