# Running tests for Resource Provisioner

## Virtual Environment

Activate or create the vitual environment in `ResourceProvisioner`

### Create Virtual Environment 
``bash
python -m venv venv
``

### Activate Virtual Environment
Open a powershell console (or terminal in VS Code)

``bash
./venv/scripts/activate
``

### Directory

The tests should be executed from the `ResourceProvisioner_Functions_Tests` directory

## Install requirements

``bash
pip install -r requirements.txt
``

## Save requirements

``bash
pip freeze >requirements.txt
``

## Running Tests

### All Tests

`python .\function_app_test.py`

### Single Test 

`python .\function_app_test.py TestResourceProvisioner.<testname>`

`python .\function_app_test.py TestResourceProvisioner.test_azkeyvault_sync`
