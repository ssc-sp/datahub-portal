FROM mcr.microsoft.com/azure-functions/python:4-python3.11

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY . /home/site/wwwroot
COPY *.crt /usr/local/share/ca-certificates
RUN update-ca-certificates

RUN cd /home/site/wwwroot && pip install --target="./.python_packages/lib/site-packages" -r ./requirements.txt
