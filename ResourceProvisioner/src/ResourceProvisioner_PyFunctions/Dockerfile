FROM python:3.12@sha256:e0823b6251241a8a42a8a525003af9e8991ff3da0097a57884127c5913f9cb58 AS build

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
