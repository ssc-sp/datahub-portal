FROM cgr.dev/chainguard/dotnet-sdk:latest AS build
WORKDIR /workspace
ENV DOTNET_CLI_HOME=/tmp \
    DOTNET_CLI_TELEMETRY_OPTOUT=1

# Copy sources
USER root

# Global repo files
COPY stylecop.json                ./
COPY .editorconfig                ./
COPY global.json                  ./
COPY Analyzers.ruleset            ./
# COPY Directory.Packages.props   ./
COPY Directory.Build.props        ./
COPY nuget.config                 ./

# Shared dependency files
COPY Shared/src/Datahub.Markdown/ ./Shared/src/Datahub.Markdown/
COPY Shared/src/Datahub.Shared/   ./Shared/src/Datahub.Shared/
COPY Desktop/Desktop.SharedCode/  ./Desktop/Desktop.SharedCode/

# Portal dependency files
COPY Portal/src/Datahub.Application/            ./Portal/src/Datahub.Application/
COPY Portal/src/Datahub.CatalogSearch/          ./Portal/src/Datahub.CatalogSearch/
COPY Portal/src/Datahub.Core/                   ./Portal/src/Datahub.Core/
COPY Portal/src/Datahub.Infrastructure/         ./Portal/src/Datahub.Infrastructure/
COPY Portal/src/Datahub.Infrastructure.Offline/ ./Portal/src/Datahub.Infrastructure.Offline/
COPY Portal/src/Datahub.Metadata/               ./Portal/src/Datahub.Metadata/
COPY Portal/src/Datahub.Portal.Metadata/        ./Portal/src/Datahub.Portal.Metadata/

# Portal files
COPY Portal/src/Datahub.Portal/ ./Portal/src/Datahub.Portal/

# Fixes ownership
RUN chown -R nonroot:nonroot /workspace
USER nonroot

RUN mkdir -p /workspace/publish

RUN dotnet restore Portal/src/Datahub.Portal/Datahub.Portal.csproj --runtime linux-x64

RUN dotnet publish Portal/src/Datahub.Portal/Datahub.Portal.csproj \
    --runtime linux-x64 \
    --no-restore \
    --self-contained false \
    -p:UseAppHost=false \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -p:GenerateDocumentationFile=false \
    -c Release \
    -o /workspace/publish

# Production Run
FROM cgr.dev/chainguard/aspnet-runtime:latest
WORKDIR /app
COPY --from=build /workspace/publish ./
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_EnableDiagnostics=0 \
    ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Datahub.Portal.dll"]
