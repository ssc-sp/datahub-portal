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

# Create a virtual environment and install the required packages
Write-Host "Creating a new virtual environment"
py -3.11 -m venv .venv
if (-not $?) { Write-Host "Failed to create virtual environment"; exit 1 }

# Activate the virtual environment
Write-Host "Activating the virtual environment"
. .venv\Scripts\Activate.ps1
if (-not $?) { Write-Host "Failed to activate virtual environment"; exit 1 }

# Install the required packages
Write-Host "Installing the required packages"
pip3 install -r requirements.txt
if (-not $?) { Write-Host "Failed to install required packages"; exit 1 }

Write-Host "Python environment setup complete"
Write-Host "Validating the setup by running the function app"
python .\function_app.py
# Capture the Python error code
$pythonExitCode = $LASTEXITCODE
Write-Host "Python script exited with code: $pythonExitCode"

if ($pythonExitCode -ne 0) {
    Write-Host "An error occurred while running the Python script."
    exit $pythonExitCode
}