# PowerShell Module Management for DataHub Portal

This directory contains PowerShell scripts and modules for managing the DataHub Portal project configuration and Azure resources.

## Required Modules

The DataHub Portal project requires several PowerShell modules to function properly:

- **Az.KeyVault** - For Azure Key Vault secret management
- **Az.ContainerRegistry** - For Azure Container Registry operations  
- **Az.Accounts** - For Azure authentication and account management (includes profile management)

## Installing Required Modules

### Option 1: Using the Install-RequiredModules Function

```powershell
# Import the module
Import-Module ./scripts/appsettings.psm1

# Install all required modules for current user
Install-RequiredModules

# Install for all users (requires admin privileges)
Install-RequiredModules -Scope AllUsers

# Force reinstall even if modules exist
Install-RequiredModules -Force
```

### Option 2: Using the Standalone Script

```powershell
# Basic installation
./scripts/install-modules.ps1

# Install for all users with force
./scripts/install-modules.ps1 -Scope AllUsers -Force
```

### Option 3: Automatic Installation

When you run any script that uses the `Export-Settings` function, it will automatically check for required modules and offer to install them if they're missing.

## Usage Examples

After installing the required modules, you can use the DataHub Portal scripts:

```powershell
# Generate all settings for development environment
./gen-all-settings.ps1 -Environment dev

# Export specific project settings
Import-Module ./scripts/appsettings.psm1
Export-Settings -ProjectFolder "./Portal/src/Datahub.Portal" -SourceFile "./Portal/template.settings.json" -Target AppSettings -Environment dev
```

## Module Functions

### Install-RequiredModules

Downloads and installs all required PowerShell modules for the DataHub Portal project.

**Parameters:**
- `Scope` - Installation scope: "CurrentUser" (default) or "AllUsers"
- `Force` - Forces installation even if modules already exist

**Example:**
```powershell
Install-RequiredModules -Scope CurrentUser -Force
```

### Export-Settings

Main function for exporting configuration settings from templates to various target formats.

**Parameters:**
- `SourceFile` - Path to the template settings file
- `Target` - Target format: "AppSettings", "Environment", "Terraform", or "Function"
- `Environment` - Target environment: "test", "dev", "int", or "poc"
- `ProjectFolder` - Project folder path (required for AppSettings and Function targets)
- `TfFile` - Terraform file path (required for Terraform target)
- `TargetFile` - Custom target file name (optional)

## Troubleshooting

### Execution Policy Issues

If you encounter execution policy errors, you may need to set the execution policy:

```powershell
# For current user
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# For all users (requires admin)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine
```

### Permission Issues

If you encounter permission issues when installing modules:

1. Try running PowerShell as Administrator
2. Use the `-Scope CurrentUser` parameter to install for current user only
3. Check if you have access to the PowerShell Gallery

### Azure Authentication

Some functions require Azure authentication. Make sure you're logged in:

```powershell
Connect-AzAccount -Domain "163oxygen.onmicrosoft.com"
```

## Contributing

When adding new PowerShell scripts that require additional modules:

1. Add the module to the `$requiredModules` array in the `Install-RequiredModules` function
2. Update this README with the new module information
3. Test the installation process on a clean environment