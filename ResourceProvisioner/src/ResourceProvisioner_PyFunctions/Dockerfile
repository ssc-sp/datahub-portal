FROM python:3.11 AS build

COPY requirements.txt /
RUN pip install --target="/home/site/wwwroot/.python_packages/lib/site-packages" -r ./requirements.txt --upgrade --no-cache-dir

FROM mcr.microsoft.com/azure-functions/python:4-python3.11@sha256:29c9068316cccc8abba71f24e8f71d873d6f553b1a47c84b34e2a6d8cf76184f

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

COPY --from=build /home/site/wwwroot /home/site/wwwroot

COPY . /home/site/wwwroot