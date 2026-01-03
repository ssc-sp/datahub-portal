#!/bin/bash

# Define paths and passwords
CERT_DIR="/home/vscode/.aspnet/https"
CERT_PATH="$CERT_DIR/datahub-portal.pfx"
CERT_PASSWORD="devpassword"
PROJECT_PATH="Portal/src/Datahub.Portal/Datahub.Portal.csproj"

echo "🚀 Starting Post-Create Setup..."


if [ ! -f "$CERT_PATH" ]; then
    echo "🔒 Generating Development Certificate..."
    mkdir -p "$CERT_DIR"
    dotnet dev-certs https -ep "$CERT_PATH" -p "$CERT_PASSWORD"
else
    echo "✅ Certificate already exists."
fi


echo "📦 Checking Azure PowerShell (Az) Module..."
pwsh -c "
    \$ErrorActionPreference = 'Stop'
    if (-not (Get-Module -ListAvailable -Name Az)) { 
        Write-Host '   Installing Az Module (this may take a few minutes)...' -ForegroundColor Yellow
        Install-Module -Name Az -Scope CurrentUser -Repository PSGallery -Force -AllowClobber
    } else {
        Write-Host '   Az Module already installed.' -ForegroundColor Green
    }
"


echo "restore 📦 Restoring NuGet Packages..."
dotnet restore "$PROJECT_PATH"

echo "✨ Setup Complete!"