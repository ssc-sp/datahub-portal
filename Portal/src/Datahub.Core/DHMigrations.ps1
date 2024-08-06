param (
    [Parameter(Mandatory=$true)]
    [ValidateSet("AddMigration", "RemoveMigration","Script")]
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
else
{
    Start-Process -FilePath "dotnet" -ArgumentList "tool update --global dotnet-ef" -NoNewWindow -PassThru -Wait
}

# Define the timeout duration in milliseconds (120 seconds * 1000)
$timeoutDuration = 120 * 1000
# Run the dotnet build command and capture the exit code
$buildProcess = Start-Process -FilePath "dotnet" -ArgumentList "build" -NoNewWindow -PassThru
$buildProcess.WaitForExit($timeoutDuration)

# Check the exit code
if (($buildProcess.ExitCode -gt 0) -or (-not $buildProcess.HasExited)) {
    $buildProcess.Kill()
    Write-Host "Build failed cannot continue. Error code is $($buildProcess.ExitCode)"
    Exit 1
}
Write-Host "Build completed"

if ($Action -eq "AddMigration") {

    Write-Host "Adding migration with name: $MigrationName"

    Start-Process -FilePath "dotnet" -ArgumentList "ef migrations add $MigrationName --context SqlServerDatahubContext --configuration MIGRATION -v" -NoNewWindow -PassThru -Wait
    Start-Process -FilePath "dotnet" -ArgumentList "ef migrations add $MigrationName --context SqliteDatahubContext --configuration MIGRATION -v" -NoNewWindow -PassThru -Wait

    # Add your logic for AddMigration here using $MigrationName
}
elseif ($Action -eq "Script") {

    Write-Host "Generating script for migration"
    Start-Process -FilePath "dotnet" -ArgumentList "ef migrations script --context SqlServerDatahubContext --configuration MIGRATION -v" -NoNewWindow -PassThru -Wait
    #Start-Process -FilePath "dotnet" -ArgumentList "ef migrations remove --context SqliteDatahubContext --configuration MIGRATION -v --force" -NoNewWindow -PassThru -Wait

    # Add your logic for RemoveMigration here
}
elseif ($Action -eq "RemoveMigration") {

    Write-Host "Removing migration"
    Start-Process -FilePath "dotnet" -ArgumentList "ef migrations remove --context SqlServerDatahubContext --configuration MIGRATION -v --force" -NoNewWindow -PassThru -Wait
    Start-Process -FilePath "dotnet" -ArgumentList "ef migrations remove --context SqliteDatahubContext --configuration MIGRATION -v --force" -NoNewWindow -PassThru -Wait

    # Add your logic for RemoveMigration here
}
else {
    Write-Host "Error: Invalid action specified. Please use 'AddMigration' or 'RemoveMigration'."
    exit 1
}

 