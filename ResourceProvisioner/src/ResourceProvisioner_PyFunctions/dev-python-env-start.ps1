echo "Starting Python environment setup"

# Check if the virtual environment is already activated and deactivate it
if ($env:VIRTUAL_ENV -ne $null) {
    echo "Deactivating the existing virtual environment"
    deactivate
}


# Check if the .venv directory exists and remove it
if (Test-Path .venv) {
    echo "Removing the existing virtual environment"
    Remove-Item -Path .venv -Recurse -Force -ErrorAction SilentlyContinue
}

# Create a virtual environment and install the required packages
echo "Creating a new virtual environment"
python -m venv .venv

# Activate the virtual environment
echo "Activating the virtual environment"
.venv\Scripts\activate

# Install the required packages
echo "Installing the required packages"
pip3 install -r requirements.txt

echo "Python environment setup complete"