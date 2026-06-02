FROM python:3.12@sha256:bd55e06128e2743d526738dd5cf5db924e49ead66d4fe2ad15f2aac8ad86aae9 AS build

COPY requirements.txt /

RUN pip install --upgrade pip
RUN pip install -r ./requirements.txt  --target="/home/.local/lib/python3.12/site-packages" --no-cache-dir --upgrade

FROM mcr.microsoft.com/azure-functions/python:4-python3.12@sha256:b4162b6bbcdd5724646b48ccd5a400715822faa234067ad2b525b20b8e54752e

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
