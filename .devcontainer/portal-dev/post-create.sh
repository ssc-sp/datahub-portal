#!/bin/bash

set -euo pipefail

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

sudo update-ca-certificates

echo "🔍 Testing TLS trust for package registries (NuGet / npm / PowerShell)..."

declare -A REGISTRIES=(
  ["NuGet"]="api.nuget.org"
  ["npm"]="registry.npmjs.org"
  ["PowerShellGallery"]="www.powershellgallery.com"
)

SSL_FAILED=0

for name in "${!REGISTRIES[@]}"; do
  host="${REGISTRIES[$name]}"
  echo "   → $name ($host)"

  VERIFY_OUTPUT=$(openssl s_client \
    -connect "${host}:443" \
    -servername "$host" \
    -CApath /etc/ssl/certs \
    </dev/null 2>&1 | grep "Verify return code")

  echo "     $VERIFY_OUTPUT"

  if ! echo "$VERIFY_OUTPUT" | grep -q "Verify return code: 0 (ok)"; then
    SSL_FAILED=1
  fi
done

if [ "$SSL_FAILED" -ne 0 ]; then
  echo ""
  echo "❌ TLS trust validation failed for one or more package registries."
  echo "   This strongly indicates SSL interception without the corporate root CA installed."
  echo "   NuGet / npm / PowerShell installs will be unreliable until trust is fixed."
  echo ""
  exit 1
fi

echo "✅ TLS trust looks good for NuGet, npm, and PowerShell Gallery."



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


echo "📦 Restoring NuGet Packages..."
dotnet restore "$PROJECT_PATH"

echo "✨ Setup Complete!"