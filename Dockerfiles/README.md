# Datahub Portal – Container Build Guide

This folder contains the Dockerfile(s) used to build and run various parts of the FSDH Application from a monorepo perspective. It’s written for local development and CI, using Chainguard/Wolfi .NET images and (optionally) JFrog Artifactory mirrors.

> **Repo layout expectation**: The Dockerfile assumes you run `docker build` from the **repo root** so that the `COPY` statements can pick the needed projects (Portal + shared libs). Keep `.` as the build context even though the Dockerfile lives in `Dockerfiles/`.

---

## TL;DR – Build & Run Example(Dockerfile.portal)

```bash
# From the repo root (keep "." as the context)
docker build \
  --no-cache \
  --pull \
  --platform linux/amd64 \
  -f Dockerfiles/Dockerfile.portal \
  -t datahub-portal:local \
  .

# Run (basic)
docker run --rm -p 8080:8080 \
  --platform linux/amd64 \
  -v $(pwd)/your.appsettings.json:/app/appsettings.json:ro \
  datahub-portal:local
```

---

## Prerequisites

* Docker 24+
* Network access to NuGet feeds used by the repo
* If using JFrog-hosted Chainguard mirrors, you’ll need an **Artifactory login** (see below)

> **Why not `COPY . .`?** This monorepo has many projects. We only copy the Portal and its referenced projects to keep the image (and build context) smaller and to avoid restoring/building unrelated code.

---

## Build Commands

### Standard build (recommended)

```bash
docker build \
  --no-cache \
  --pull \
  --platform linux/amd64 \
  -f Dockerfiles/Dockerfile.portal \
  -t datahub-portal:local \
  .
```

### Clean rebuild (ignore cache)

```bash
docker build \
  --no-cache \
  --pull \
  --platform linux/amd64 \
  -f Dockerfiles/Dockerfile.portal \
  -t datahub-portal:local \
  .
```

### Verbose BuildKit logs

```bash
DOCKER_BUILDKIT=1 docker build --progress=plain \
  --no-cache \
  --pull \
  --platform linux/amd64 \
  -f Dockerfiles/Dockerfile.portal \
  -t datahub-portal:local \
  .
```

> **Context** must be the repo root (`.`). If you run the build from a different directory, Docker won’t find the files the Dockerfile’s `COPY` lines expect.

---

## Running Locally

### Minimal run

```bash
docker run --rm -p 8080:8080 \
  --platform linux/amd64 \
  -v $(pwd)/your.appsettings.json:/app/appsettings.json:ro \
  datahub-portal:local
```

### With environment/config

The app reads configuration from environment variables and `appsettings.*.json`. appsettings.json values can be overwritten with environment variables. Examples:

```bash
docker run --rm -p 8080:8080 \
  --platform linux/amd64 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e CONNECTIONSTRINGS__DATAHUB_MSSQL_PROJECT="Server=tcp:<server>.database.windows.net,1433;Database=<db>;Authentication=Active Directory Default;Encrypt=True;" \
  datahub-portal:local
```

### Supplying an appsettings file (override)

```bash
# If you have a local file at ./Dockerfiles/your.appsettings.json
# it will end up at /app/appsettings.json inside the container

docker run --rm -p 8080:8080 \
  --platform linux/amd64 \
  -e ASPNETCORE_ENVIRONMENT=Local \
  -v $(pwd)/Dockerfiles/your.appsettings.json:/app/appsettings.json:ro \
  datahub-portal:local
```

---

## Using JFrog Artifactory (Chainguard mirrors)

The Dockerfile references Chainguard images via an internal Artifactory:

```
FROM artifacts-artefacts.devops.cloud-nuage.canada.ca/.../dotnet-sdk:9
FROM artifacts-artefacts.devops.cloud-nuage.canada.ca/.../aspnet-runtime:9
```

Authenticate once per machine:

```bash
# 1) Log in to the registry
#    Use your Artifactory username/API key or your SSO method per team docs
docker login artifacts-artefacts.devops.cloud-nuage.canada.ca

# 2) Build as usual (see commands above)
```

If login fails, confirm:

* You’re on VPN (if required)
* Your account has permission to pull from the repo paths
* Your Docker daemon can reach the registry (proxy/SSL issues)

---

## Configuration Cheat Sheet

* **DefaultAzureCredential** works if you mount your Azure CLI auth:

  ```bash
  -v $HOME/.azure:/root/.azure:ro
  ```

  and use connection strings like `Authentication=Active Directory Default;`.

* **Managed Identity** is typically for Azure hosting. For local containers, prefer `Active Directory Default` (CLI) or `Active Directory Service Principal` (AZURE_* env vars).

* **Disable DB migrations at startup** (if the app supports it):

  ```bash
  -e DATAHUB__DB__MIGRATE_ON_STARTUP=false
  ```

  (Adjust to the real key used by the app.)

---

## Troubleshooting

* **NuGet restore failures**: confirm `nuget.config` feeds are reachable from the build environment.
* **403/401 pulling base images**: you’re likely not logged into Artifactory or lack perms.
* **Antiforgery or Data Protection warnings**: in local dev these are usually safe. To persist keys across restarts, mount a host folder to `/home/nonroot/.aspnet/DataProtection-Keys`.
* **Az login not seen in container**: mount your Azure CLI folder or use service principal env vars.

---

## Make the image smaller

* We already build in a separate stage and copy only `/workspace/publish`.
* We turn off symbol/XML generation and disable the apphost to reduce size.
* Further reductions:

  * Strip unused content from `wwwroot` (fonts, maps, etc.) if allowable
  * Consider trimming/single-file **only** if you can test thoroughly (trimming can break reflection)

---

## FAQ

**Q: Why must I build from the repo root?**
A: The Dockerfile’s `COPY` statements use paths relative to the repo root to pull in only the projects the Portal needs.

**Q: Why Linux amd64?**
A: That’s what we deploy to and it avoids platform drift between dev machines.
