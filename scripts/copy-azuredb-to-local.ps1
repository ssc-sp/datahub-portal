param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName,
    
    [Parameter(Mandatory=$true)]
    [string]$DatabaseName,

    [Parameter(Mandatory=$false)]
    [string]$LocalPath = "C:\Temp",

    [Parameter(Mandatory=$false)]
    [switch]$SkipExport,

    # Either supply a PSCredential, or set this switch to prompt for one at runtime.
    [Parameter(Mandatory=$false)]
    [System.Management.Automation.PSCredential]$Credential,

    [Parameter(Mandatory=$false)]
    [switch]$PromptForCredential
)

# Validate credential inputs: mutually exclusive
if ($Credential -and $PromptForCredential) {
    Write-Error "Specify either -Credential or -PromptForCredential, not both."
    exit 1
}

# Variables
$targetInstance = "(localdb)\MSSQLLocalDB"
$dacpacFileName = "$DatabaseName.dacpac"
$dacpacPath = Join-Path $LocalPath $dacpacFileName
$tempDir = $LocalPath

# Check if dacpac already exists
if (-not $SkipExport -and (Test-Path $dacpacPath)) {
    Write-Host "Existing dacpac found at $dacpacPath. Skipping export phase." -ForegroundColor Yellow
    $SkipExport = $true
} elseif (-not $SkipExport) {
    Write-Host "No existing dacpac found; will perform export." -ForegroundColor Cyan
}

Write-Host "`nLooking for SqlPackage.exe..." -ForegroundColor Cyan

# Find SqlPackage.exe
$sqlpackagePath = $null
$possiblePaths = @(
    "C:\tools\sqlpackage\sqlpackage.exe",
    "C:\Program Files\Microsoft SQL Server\160\DAC\bin\sqlpackage.exe",
    "C:\Program Files\Microsoft SQL Server\150\DAC\bin\sqlpackage.exe",
    "C:\Program Files (x86)\Microsoft SQL Server\160\DAC\bin\sqlpackage.exe",
    "C:\Program Files (x86)\Microsoft SQL Server\150\DAC\bin\sqlpackage.exe",
    "$env:LOCALAPPDATA\Microsoft\WinGet\Packages\Microsoft.SqlPackage_Microsoft.Winget.Source_8wekyb3d8bbwe\sqlpackage.exe"
)

foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $sqlpackagePath = $path
        break
    }
}

if (-not $sqlpackagePath) {
    Write-Error "SqlPackage.exe not found. Please install SQL Server Data Tools or SqlPackage:"
    Write-Host "  - Via Visual Studio Installer: Add 'Data storage and processing' workload" -ForegroundColor Yellow
    Write-Host "  - Via Winget: winget install Microsoft.SqlPackage" -ForegroundColor Yellow
    Write-Host "  - Or download standalone: https://aka.ms/sqlpackage-windows" -ForegroundColor Yellow
    exit 1
}

Write-Host "Using SqlPackage at: $sqlpackagePath" -ForegroundColor Green

# Create temp directory if it doesn't exist
if (-not (Test-Path $tempDir)) {
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    Write-Host "Created directory: $tempDir" -ForegroundColor Green
}

# Export from Azure SQL Database
if (-not $SkipExport) {
    Write-Host "`nExtracting database from Azure..." -ForegroundColor Cyan
    
    $serverFqdn = "$ServerName.database.windows.net"

    # If user asked to be prompted, request credential now
    if ($PromptForCredential -and -not $Credential) {
        try {
            $Credential = Get-Credential -Message "Enter SQL username and password for $serverFqdn"
        } catch {
            Write-Error "Credential prompt cancelled or failed: $($_.Exception.Message)"
            exit 1
        }
    }

    # Build source connection. If a PSCredential is available, use SQL auth; otherwise use AD interactive
    if ($Credential) {
        $user = $Credential.UserName
        $pass = $Credential.GetNetworkCredential().Password
        $sourceConn = "Server=$serverFqdn;Database=$DatabaseName;User ID=$user;Password=$pass;"
        Write-Host "Server: $serverFqdn" -ForegroundColor Gray
        Write-Host "Database: $DatabaseName" -ForegroundColor Gray
        Write-Host "Authentication: SQL Username/Password (from PSCredential)" -ForegroundColor Gray
    } else {
        $sourceConn = "Server=$serverFqdn;Database=$DatabaseName;Authentication=Active Directory Interactive;"
        Write-Host "Server: $serverFqdn" -ForegroundColor Gray
        Write-Host "Database: $DatabaseName" -ForegroundColor Gray
        Write-Host "Authentication: Active Directory Interactive" -ForegroundColor Gray
    }
    
    & $sqlpackagePath /Action:Extract `
        /SourceConnectionString:$sourceConn `
        /TargetFile:$dacpacPath `
        /p:ExtractAllTableData=True `
        /p:IgnorePermissions=True `
        /p:IgnoreUserLoginMappings=True
    
    if ($LASTEXITCODE -ne 0) { 
        Write-Error "Extract failed with exit code $LASTEXITCODE"
        exit 1
    }

    Write-Host "Extract completed: $dacpacPath" -ForegroundColor Green
} else {
    Write-Host "`nSkipping extract; using existing file $dacpacPath" -ForegroundColor Yellow
}

# Import to LocalDB
Write-Host "`nPublishing dacpac to LocalDB..." -ForegroundColor Cyan
Write-Host "Target Instance: $targetInstance" -ForegroundColor Gray
Write-Host "Target Database: $DatabaseName" -ForegroundColor Gray

# Ensure contained database authentication is enabled on LocalDB instance (idempotent)
# This aligns closer with Azure SQL which often uses contained database users.
# LocalDB supports partial containment; enabling this lets contained users function after publish.
try {
    if (Get-Command Invoke-Sqlcmd -ErrorAction SilentlyContinue) {
        $containedStatus = Invoke-Sqlcmd -ServerInstance $targetInstance -Database master -Query "SELECT value_in_use FROM sys.configurations WHERE name='contained database authentication';" | Select-Object -ExpandProperty value_in_use -ErrorAction Stop
        if ($containedStatus -ne 1) {
            Write-Host "Enabling contained database authentication on instance $targetInstance..." -ForegroundColor Cyan
            Invoke-Sqlcmd -ServerInstance $targetInstance -Database master -Query "EXEC sp_configure 'contained database authentication', 1; RECONFIGURE;" -ErrorAction Stop
            Write-Host "Contained database authentication enabled." -ForegroundColor Green
        } else {
            Write-Host "Contained database authentication already enabled." -ForegroundColor Gray
        }
    } else {
        Write-Host "Invoke-Sqlcmd not available (SqlServer module missing). Skipping containment enable step." -ForegroundColor Yellow
    }
} catch {
    Write-Host "Failed to verify/enable contained database authentication: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Create publish profile
$profilePath = Join-Path $tempDir "publish-profile.xml"
$targetConnection = "Data Source=$targetInstance;Initial Catalog=$DatabaseName;Integrated Security=True"
$profileContent = @"
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <IncludeCompositeObjects>True</IncludeCompositeObjects>
    <TargetDatabaseName>$DatabaseName</TargetDatabaseName>
    <DeployScriptFileName>deploy.sql</DeployScriptFileName>
    <TargetConnectionString>$targetConnection</TargetConnectionString>
    <ProfileVersionNumber>1</ProfileVersionNumber>
    <ExcludeUsers>True</ExcludeUsers>
    <ExcludeLogins>True</ExcludeLogins>
    <ExcludeDatabaseRoles>True</ExcludeDatabaseRoles>
    <IgnorePermissions>True</IgnorePermissions>
    <IgnoreUserSettingsObjects>True</IgnoreUserSettingsObjects>
    <IgnoreLoginSids>True</IgnoreLoginSids>
    <IgnoreRoleMembership>True</IgnoreRoleMembership>
    <CommandTimeout>0</CommandTimeout>
    <DoNotAlterDatabaseOptions>True</DoNotAlterDatabaseOptions>
  </PropertyGroup>
</Project>
"@

Set-Content -Path $profilePath -Value $profileContent -Encoding UTF8
Write-Host "Created publish profile: $profilePath" -ForegroundColor Green

# Publish dacpac
& $sqlpackagePath /Action:Publish `
    /SourceFile:$dacpacPath `
    /Profile:$profilePath `
    /DiagnosticsFile:"$tempDir\sqlpackage-diagnostics.log"

$publishExit = $LASTEXITCODE

# Final status
if ($publishExit -eq 0) {
    Write-Host "`nImport completed successfully!" -ForegroundColor Green
    Write-Host "Database: $DatabaseName" -ForegroundColor Green
    Write-Host "Instance: $targetInstance" -ForegroundColor Green
    
    # Archive the dacpac file
    try {
        $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
        $archiveName = "$DatabaseName-$timestamp.dacpac"
        $archivePath = Join-Path $tempDir $archiveName
        Move-Item -Path $dacpacPath -Destination $archivePath -Force
        Write-Host "Archived dacpac as $archivePath" -ForegroundColor Green
    } catch {
        Write-Host "Failed to archive dacpac: $($_.Exception.Message)" -ForegroundColor Yellow
    }
} else {
    Write-Host "`nImport failed on instance '$targetInstance' (exit code $publishExit)" -ForegroundColor Red
    Write-Host "LocalDB limitations: contained databases and certain security principals are not fully supported." -ForegroundColor Yellow
    Write-Host "Check diagnostics log: $tempDir\sqlpackage-diagnostics.log" -ForegroundColor Yellow
    exit $publishExit
}
