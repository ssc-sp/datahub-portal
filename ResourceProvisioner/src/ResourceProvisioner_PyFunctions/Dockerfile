# Use the official Azure Functions Python base image
# See https://hub.docker.com/r/microsoft/azure-functions-python
FROM mcr.microsoft.com/azure-functions/python:4-python3.12

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

# Copy the function app code to the container
COPY . /home/site/wwwroot

# Install any dependencies
RUN pip install -r /home/site/wwwroot/requirements.txt

