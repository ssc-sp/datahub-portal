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

# Activate the virtual environment
Write-Host "Activating the virtual environment"
.venv\Scripts\activate

# Install the required packages
Write-Host "Installing the required packages"
pip3 install -r requirements.txt

Write-Host "Python environment setup complete"