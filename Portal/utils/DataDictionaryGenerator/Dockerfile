# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:8.0@sha256:a9cb56dff4de2b15faa1597ed12fbff1679c06b6e29a5678e08c0669216117d5 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:ebecce75fb42c4c14db108f88054d20a93b0b9e6dfbacad56c8f744c342cf9ef AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Portal/utils/DataDictionaryGenerator/DataDictionaryGenerator.csproj", "Portal/utils/DataDictionaryGenerator/"]
RUN dotnet restore "./Portal/utils/DataDictionaryGenerator/DataDictionaryGenerator.csproj"
COPY . .
WORKDIR "/src/Portal/utils/DataDictionaryGenerator"
RUN dotnet build "./DataDictionaryGenerator.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DataDictionaryGenerator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataDictionaryGenerator.dll"]