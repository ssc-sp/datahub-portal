#!/usr/bin/env pwsh
Write-Host "Starting Python environment setup"

# Check if the virtual environment is already activated and deactivate it
if ($env:VIRTUAL_ENV -ne $null) {
    Write-Host "Deactivating the existing virtual environment"
    deactivate
}

# Check if the .venv directory exists and remove it
if (Test-Path .venv) {
    Write-Host "Removing the existing virtual environment"
    Remove-Item -Path .venv -Recurse -Force -ErrorAction SilentlyContinue
}

# Create or reuse a Poetry virtual environment and install the required packages
Write-Host "Creating or updating the Poetry environment"
poetry env use python3.12
if (-not $?) { Write-Host "Failed to configure Poetry environment"; exit 1 }

Write-Host "Installing the required packages"
poetry install
if (-not $?) { Write-Host "Failed to install required packages"; exit 1 }

Write-Host "Python environment setup complete"
Write-Host "Validating the setup by running the function app"
poetry run python .\function_app.py
# Capture the Python error code
$pythonExitCode = $LASTEXITCODE
Write-Host "Python script exited with code: $pythonExitCode"

if ($pythonExitCode -ne 0) {
    Write-Host "An error occurred while running the Python script."
    exit $pythonExitCode
}