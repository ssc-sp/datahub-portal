FROM cgr.dev/chainguard/dotnet-sdk:latest AS build
WORKDIR /workspace
ENV DOTNET_CLI_HOME=/tmp

# copy source and ensure nonroot owns it
# Global repo files
COPY --chown=nonroot:nonroot ./stylecop.json                ./stylecop.json
COPY --chown=nonroot:nonroot ./.editorconfig                ./.editorconfig
COPY --chown=nonroot:nonroot ./global.json                  ./global.json
COPY --chown=nonroot:nonroot ./Analyzers.ruleset            ./Analyzers.ruleset
#COPY --chown=nonroot:nonroot ./Directory.Packages.props     ./Directory.Packages.props
COPY --chown=nonroot:nonroot ./nuget.config                 ./nuget.config

# Shared dependency files
COPY --chown=nonroot:nonroot ./Shared/src/Datahub.Markdown/ ./Shared/src/Datahub.Markdown/
COPY --chown=nonroot:nonroot ./Shared/src/Datahub.Shared/   ./Shared/src/Datahub.Shared/
COPY --chown=nonroot:nonroot ./Desktop/Desktop.SharedCode/  ./Desktop/Desktop.SharedCode/

# Portal dependency files
COPY --chown=nonroot:nonroot ./Portal/src/Datahub.Application/                ./Portal/src/Datahub.Application/
COPY --chown=nonroot:nonroot ./Portal/src/Datahub.CatalogSearch/              ./Portal/src/Datahub.CatalogSearch/
COPY --chown=nonroot:nonroot ./Portal/src/Datahub.Core/                       ./Portal/src/Datahub.Core/
COPY --chown=nonroot:nonroot ./Portal/src/Datahub.Infrastructure/             ./Portal/src/Datahub.Infrastructure/
COPY --chown=nonroot:nonroot ./Portal/src/Datahub.Infrastructure.Offline/     ./Portal/src/Datahub.Infrastructure.Offline/
COPY --chown=nonroot:nonroot ./Portal/src/Datahub.Metadata/                   ./Portal/src/Datahub.Metadata/
COPY --chown=nonroot:nonroot ./Portal/src/Datahub.Portal.Metadata/            ./Portal/src/Datahub.Portal.Metadata/
COPY --chown=nonroot:nonroot ./Shared/src/Datahub.Markdown/                   ./Shared/src/Datahub.Markdown/

# Portal files
COPY --chown=nonroot:nonroot ./Portal/src/Datahub.Portal/ ./Portal/src/Datahub.Portal/

RUN mkdir -p /workspace/publish

RUN dotnet restore Portal/src/Datahub.Portal/Datahub.Portal.csproj -r linux-x64
RUN dotnet publish Portal/src/Datahub.Portal/Datahub.Portal.csproj \
    -c Release -r linux-x64 --no-restore \
    --self-contained true \
    -p:PublishTrimmed=true \
    -p:PublishSingleFile=true \
    -o /workspace/publish

# production
FROM cgr.dev/chainguard/dotnet-runtime:latest
WORKDIR /app
COPY --from=build /workspace/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["./Datahub.Portal"]