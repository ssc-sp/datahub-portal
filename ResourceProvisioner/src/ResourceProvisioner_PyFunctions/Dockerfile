FROM python:3.12@sha256:d824f747699b6a0f1ebbee9289018c1e484c808e0d928a27be5f3ed7c5f15e4b AS build

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
