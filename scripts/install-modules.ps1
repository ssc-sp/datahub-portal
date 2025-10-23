#!/usr/bin/env pwsh

<#
.SYNOPSIS
Installs all required PowerShell modules for the DataHub Portal project.

.DESCRIPTION
This script imports the appsettings module and calls the Install-RequiredModules function
to download and install all necessary PowerShell modules for the DataHub Portal project.

.PARAMETER Scope
Specifies the installation scope. Valid values are 'CurrentUser' (default) and 'AllUsers'.

.PARAMETER Force
Forces installation even if modules already exist.

.EXAMPLE
./install-modules.ps1
Installs all required modules for the current user.

.EXAMPLE
./install-modules.ps1 -Scope AllUsers -Force
Forcefully installs all required modules for all users (requires admin privileges).
#>

param (
    [ValidateSet("CurrentUser", "AllUsers")]
    [string]$Scope = "CurrentUser",
    [switch]$Force
)

# Get the directory of this script
$ScriptDirectory = Split-Path -Parent $PSCommandPath

# Import the appsettings module
Import-Module "$ScriptDirectory/appsettings.psm1" -Force

# Display banner
Write-Host @"
╔══════════════════════════════════════════════════════════════════════════════╗
║                    DataHub Portal - Module Installer                         ║
║                                                                              ║
║  This script will install all required PowerShell modules for the           ║
║  DataHub Portal project, including Azure modules for Key Vault and          ║
║  Container Registry operations.                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan

# Call the Install-RequiredModules function
Install-RequiredModules -Scope $Scope -Force:$Force

Write-Host "`n🚀 You can now run other DataHub Portal scripts that depend on these modules." -ForegroundColor Green