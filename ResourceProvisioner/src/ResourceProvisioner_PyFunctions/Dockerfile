FROM python:3.12@sha256:a9e4190f02729f01e5b3719bd8b3ea0f8d9350dc17d01a1ce1ca6e3fbfcf7a99 AS build

COPY requirements.txt /

RUN pip install --upgrade pip
RUN pip install -r ./requirements.txt  --target="/home/.local/lib/python3.12/site-packages" --no-cache-dir --upgrade

FROM mcr.microsoft.com/azure-functions/python:4-python3.12@sha256:7e36a1ffbf85d88fe63b2b571867b551b46fb7690c600ec6df252485fddaa367

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    APPLICATIONINSIGHTS_CONNECTION_STRING= \
    DataHub_ENVNAME=dev \   
    AzureWebJobsStorage= \
    AzureWebJobsDashboard= \
    AzureClientId= \
    AzureClientSecret= \
    AzureStorageQueueConnectionString= \
    DatahubServiceBus= \
    DatahubMSSQLProjectConnectionString= \
    AzureTenantId= \
    AzureSubscriptionId= \
    AzureWebJobsFeatureFlags=EnableWorkerIndexing

COPY --from=build /home/.local/lib/python3.12/site-packages /home/.local/lib/python3.12/site-packages

COPY . /home/site/wwwroot
