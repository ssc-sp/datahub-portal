# Deactivate the current virtual environment
deactivate

# Delete the virtual environment directory
Remove-Item -Recurse -Force venv/

# Create a new virtual environment
python -m venv venv

# Activate the new virtual environment
./venv/Scripts/activate

# Install the required packages
pip install -r ./src/ResourceProvisioner_PyFunctions/requirements.txt
