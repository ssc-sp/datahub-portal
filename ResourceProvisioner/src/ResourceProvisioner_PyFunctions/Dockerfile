FROM python:3.11 AS build

COPY requirements.txt /
RUN pip install --target /home/site/wwwroot -r /requirements.txt

FROM mcr.microsoft.com/azure-functions/python:4-python3.11

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
