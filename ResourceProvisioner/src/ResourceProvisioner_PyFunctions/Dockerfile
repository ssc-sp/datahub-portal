FROM python:3.12@sha256:a9e4190f02729f01e5b3719bd8b3ea0f8d9350dc17d01a1ce1ca6e3fbfcf7a99 AS build

ENV POETRY_VIRTUALENVS_CREATE=false \
    POETRY_NO_INTERACTION=1

COPY pyproject.toml poetry.lock* /tmp/
WORKDIR /tmp

RUN pip install --upgrade pip poetry && \
    poetry install --only main --no-root --compile

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

COPY --from=build /usr/local/lib/python3.12/site-packages /home/.local/lib/python3.12/site-packages

COPY . /home/site/wwwroot
