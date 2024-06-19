param (
    [Parameter(Mandatory=$true)]
    [ValidateSet("AddMigration", "RemoveMigration")]
    [string]$Action,
    [string]$MigrationName
)

if ($Action -eq "AddMigration") {
    if (-not $MigrationName) {
        Write-Host "Error: MigrationName parameter is mandatory for AddMigration action."
        exit 1
    }
}
# install dotnet tool install --global dotnet-ef if not installed
$dotnetEf = Get-Command dotnet-ef -ErrorAction SilentlyContinue
if (-not $dotnetEf) {
	Write-Host "dotnet-ef is not installed. Installing..."
	Start-Process -FilePath "dotnet" -ArgumentList "tool install --global dotnet-ef" -NoNewWindow -PassThru -Wait
}

# Run the dotnet build command and capture the exit code
$buildResult = Start-Process -FilePath "dotnet" -ArgumentList "build" -NoNewWindow -PassThru -Wait -

Write-Host "Build completed"

# Check the exit code
if ($buildResult.ExitCode -gt 0) {
    Write-Host "Build failed cannot continue. Error code is $($buildResult.ExitCode)"
    Exit 1
}

if ($Action -eq "AddMigration") {

    Write-Host "Adding migration with name: $MigrationName"
    Start-Process -FilePath "dotnet" -ArgumentList "ef migrations add $MigrationName --context SqlServerDatahunterDbContext --configuration MIGRATION -v" -NoNewWindow -PassThru -Wait
    Start-Process -FilePath "dotnet" -ArgumentList "ef migrations add $MigrationName --context SqliteDatahunterDbContext --configuration MIGRATION -v" -NoNewWindow -PassThru -Wait

    # Add your logic for AddMigration here using $MigrationName
}
elseif ($Action -eq "RemoveMigration") {

    Write-Host "Removing migration"
    Start-Process -FilePath "dotnet" -ArgumentList "ef migrations remove --context SqlServerDatahunterDbContext --configuration MIGRATION -v --force" -NoNewWindow -PassThru -Wait
    Start-Process -FilePath "dotnet" -ArgumentList "ef migrations remove --context SqliteDatahunterDbContext --configuration MIGRATION -v --force" -NoNewWindow -PassThru -Wait

    # Add your logic for RemoveMigration here
}
else {
    Write-Host "Error: Invalid action specified. Please use 'AddMigration' or 'RemoveMigration'."
    exit 1
}

 