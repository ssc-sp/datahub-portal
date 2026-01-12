$kv_pattern = "@Microsoft\.KeyVault\(VaultName=(?<VaultName>[\w-]+);SecretName=(?<SecretName>[\w-]+)\)"

function Find-InfraRepo()
{
    $scriptDirectory = Split-Path -Parent $PSCommandPath
    Write-Host "The directory of this script is: $scriptDirectory"
    $parentFolder = Resolve-Path "$scriptDirectory/../../"
    Write-Host "Searching for infra repo in $parentFolder"
    $infra = "$parentFolder/datahub-infra"
    if (Test-Path -Path $infra)
    {
        Write-Host "Found infra repo at $infra"
        return $infra
    }
    return $null;
}

function Connect-FSDHAzure
{
    # Check if required modules are available and offer to install them
    $requiredModules = @("Az.KeyVault")
    $missingModules = @()
    
    foreach ($moduleName in $requiredModules) {
        if (-not (Get-Module -ListAvailable -Name $moduleName)) {
            $missingModules += $moduleName
        }
    }
    
    if ($missingModules.Count -gt 0) {
        Write-Warning "The following required modules are missing: $($missingModules -join ', ')"
        Write-Host "You can install all required modules by running: Install-RequiredModules" -ForegroundColor Yellow
        Write-Host "Or run the standalone script: ./scripts/install-modules.ps1" -ForegroundColor Yellow
        
        $response = Read-Host "Would you like to install the missing modules now? (y/N)"
        if ($response -eq 'y' -or $response -eq 'Y' -or $response -eq 'yes') {
            Install-RequiredModules
        } else {
            Write-Error "Cannot continue without required modules. Please install them and try again."
            return $false
        }
    }
    
    #import module for keyvault
    Import-Module Az.KeyVault -Force -NoClobber
    #check if user is signed in on azure
    $context = Get-AzContext
    $domain = "163oxygen.onmicrosoft.com"

    if ($null -eq $context) {
        Write-Output "Opening Azure session"
        Connect-AzAccount -Domain $domain
    } else {
        Write-Output "User $($context.Account.Id) is signed in."
    }
    
    return $true
}

function Export-Settings(
    [Parameter(Mandatory = $true)]
    [string]$SourceFile,
    [Parameter(Mandatory=$true)]
    [ValidateSet("AppSettings", "Environment", "Terraform", "Function")]
    [string]$Target,
    [Parameter(Mandatory=$true)]
    [ValidateSet("test", "dev","int","poc")]
    [string]$Environment,
    [string]$ProjectFolder,
    [string]$TfFile = $null,
    [string]$TargetFile = $null
)
{
    if (-not (Connect-FSDHAzure)) {
        return
    }

    # get the path for the current script
    $srcPath = Split-Path -Parent $SourceFile
    # set . to srcPath if it is empty
    $srcPath = if ([string]::IsNullOrEmpty($srcPath)) { "." } else { $srcPath }
    Write-Host "Src path is $srcPath"
    if ($Target -eq "AppSettings" -or $Target -eq "Function") {
        # check if project folder is set
        if ([string]::IsNullOrEmpty($ProjectFolder)) {
            Write-Error "Project folder is required for AppSettings target"
            return
        }
        Write-Host "Initializing user secrets"
        Push-Location
        Set-Location $ProjectFolder
        try {
			dotnet user-secrets init
		} catch {
			Write-Host "User secrets already initialized"
		} finally {
			Pop-Location
		}
	} 

    # login user to azure
    Write-Output "Fetching secrets from keyvault"

    $resourcePrefix = "fsdh"
    $resourcePrefixAlphanumeric = $resourcePrefix -replace "[^a-zA-Z0-9]", ""
    $azureDevOpsOrganization = "DataSolutionsDonnees"
    $azureDevOpsProject = "FSDH%20SSC"
    $vaultName = "fsdh-static-test-akv"
    $azureDevopsRepository = "datahub-project-infrastructure-$Environment"

    $tenantId = Read-VaultSecret $vaultName "datahub-portal-tenant-id"
    $subscriptionId = Read-VaultSecret $vaultName "datahub-portal-subscription-id"
    $repositoryId = Read-VaultSecret $vaultName "datahub-infrastructure-repo-id"

    $datahubMssqlAdmin = Read-VaultSecret "fsdh-key-dev" "datahub-mssql-admin"
    $datahubMssqlPassword = Read-VaultSecret "fsdh-key-dev" "datahub-mssql-password"
    $sqlCreds = "User ID=$datahubMssqlAdmin;Password=$datahubMssqlPassword"
    $infraRepo = $null
    if ($Target -eq "Terraform")
    {
        $sqlCreds = "Authentication=Active Directory Managed Identity"
        $infraRepo = Find-InfraRepo
        if ($null -eq $infraRepo)
        {
            Write-Error "Infra repo not found"
            return
        }
        Write-Host "Infra repo found in $infraRepo"
        #make sure TF file is set
        if ($null -eq $TfFile)
        {
            Write-Error "TfFile is required for Terraform target"
            return
        }
        $tfFilePath = Join-Path $infraRepo $TfFile
        if (-not (Test-Path $infraRepo))
        {
            Write-Error "Folder $infraRepo not found"
            return
        }
    }
    #$srcFolder = Get-Path -Parent $SourceFile
    if (-not (Test-Path $SourceFile))
    {
        $SourceFile = Join-Path $PSScriptRoot $SourceFile
        if (-not (Test-Path $SourceFile))
		{
			Write-Error "File $SourceFile not found"
			return
		}
    }
    $template = Get-Content $SourceFile
    # remove template from filename
    $tgtFile = $SourceFile.replace("template.", "") 

    $jsonObject = $ExecutionContext.InvokeCommand.ExpandString($template)

    $object = $jsonObject | ConvertFrom-Json
    $flattenedObject = ConvertTo-HashTable -Object $object

    $sensitiveEntries = $flattenedObject.GetEnumerator() | Where-Object { $_.Value -like "*Password=*" -or $_.Key -like "*ConnectionStrings*" -or $_.Value -like "*Server=*"}     
    $sensitiveKeys = $sensitiveEntries | ForEach-Object { $_.Name }
    #Write-Host "Flattened object"
    #$flattenedObject | Format-Table

    $akvEntries = $flattenedObject.GetEnumerator() | Where-Object { $_.Value -like "*@Microsoft.KeyVault*" }
    $akvKeys = $akvEntries | ForEach-Object { $_.Name }
    $otherEntries = $flattenedObject.GetEnumerator() | Where-Object { $akvKeys -notcontains $_.Name -and $sensitiveKeys -notcontains $_.Name}

    #Write-Host "AKV entries"
    #$akvEntries | Format-Table -HideTableHeaders

    #Write-Host "non AKV entries"
    #$otherEntries | Format-Table -HideTableHeaders

    $hashtable = @{}
    $otherEntries | ForEach-Object { $hashtable[$_.Name] = $_.Value }

    $unflattenedObject = ConvertFrom-HashTable -Hashtable $hashtable

    $nonAkvJson = $unflattenedObject | ConvertTo-Json -Depth 100

    if ($Target -eq "AppSettings") {
        #Write-Host "Target file is $tgtFile $($null -eq $TargetFile) - TargetFile is $TargetFile - fname = $([System.IO.Path]::GetFileName($tgtFile))"        
        $fName = if ($TargetFile) { $TargetFile } else { [System.IO.Path]::GetFileName($tgtFile) }
        $tgtFile = Join-Path $ProjectFolder $fName
        Write-Output "Writing json object to $tgtFile"
        Write-Host "Processing Sensitive entries"
        $nonAkvJson | Out-File -FilePath $tgtFile
        Push-Location
        Set-Location $ProjectFolder
        try {       
            Write-Host "Setting user secrets from keyvault values" 
            foreach ($entry in $akvEntries)
            {
                $key = $entry.Name
                Write-Host "Setting user secret $key"
                try {
                    $secretValue = Read-AllSecrets $entry.Value
                    if ($null -eq $secretValue -or $secretValue -eq "") {
                        Write-Warning "Secret value for $key is null or empty, skipping..."
                        continue
                    }
                    $result = dotnet user-secrets set $key $secretValue 2>&1
                    if ($LASTEXITCODE -ne 0) {
                        Write-Warning "Failed to set user secret for $key. dotnet user-secrets returned exit code $LASTEXITCODE"
                        Write-Host "Output: $result" -ForegroundColor Yellow
                    }
                } catch {
                    Write-Warning "Error processing secret $key`: $($_.Exception.Message)"
                }
            }

        } catch {
            Write-Error "Error setting user secrets: $($_.Exception.Message)"
            Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
            Write-Host "Project folder: $ProjectFolder" -ForegroundColor Yellow
        } finally {
            Pop-Location
        }

        Write-Output "Done"
	} elseif ($Target -eq "Function") {
        $fName = if ($TargetFile) { $TargetFile } else { [System.IO.Path]::GetFileName($tgtFile) }
        $tgtFile = Join-Path $ProjectFolder $fName
        Write-Output "Writing json object to $tgtFile"

		Write-Output "Configuring function settings"

        $functionValues = @{}
		foreach ($entry in $otherEntries)
		{
			$key = $entry.Name
	        $envKey = $key -replace ":", "__"
            Write-Output "Setting function variable $envKey"
            $functionValues[$envKey] = $entry.Value
		}

        $valuesJson = $functionValues | ConvertTo-Json -Depth 100        
        $functionSettings = "{`n  `"IsEncrypted`": false,`n  `"Values`":   $valuesJson `n}"
        $functionSettings | Out-File -FilePath $tgtFile

        Push-Location
        Set-Location $ProjectFolder
        try {       
            Write-Host "Setting user secrets from keyvault values" 
            foreach ($entry in $akvEntries)
            {
                $key = $entry.Name
                Write-Host "Setting user secret $key"
                try {
                    $secretValue = Read-AllSecrets $entry.Value
                    if ($null -eq $secretValue -or $secretValue -eq "") {
                        Write-Warning "Secret value for $key is null or empty, skipping..."
                        continue
                    }
                    $result = dotnet user-secrets set $key $secretValue 2>&1
                    if ($LASTEXITCODE -ne 0) {
                        Write-Warning "Failed to set user secret for $key. dotnet user-secrets returned exit code $LASTEXITCODE"
                        Write-Host "Output: $result" -ForegroundColor Yellow
                    }
                } catch {
                    Write-Warning "Error processing secret $key`: $($_.Exception.Message)"
                }
            }

        } catch {
            Write-Error "Error setting user secrets: $($_.Exception.Message)"
            Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
            Write-Host "Project folder: $ProjectFolder" -ForegroundColor Yellow
        } finally {
            Pop-Location
        }


		Write-Output "Done"	        
	} elseif ($Target -eq "Environment") {
		Write-Output "Configuring environment variables"		
		foreach ($entry in $akvEntries)
		{
			$key = $entry.Name
            if ($sensitiveKeys -contains $key)
            {
                Write-Output "Skipping sensitive key $key"
                continue
            }
            $secretValue = Read-AllSecrets $key
	        $envKey = $key -replace ":", "__"
            
            Write-Output "Setting environment variable $envKey"
            $env[$envKey] = $secretValue
		}
		foreach ($entry in $otherEntries)
		{
			$key = $entry.Name
	        $envKey = $key -replace ":", "__"
            Write-Output "Setting environment variable $envKey"
            $env[$envKey] = $entry.Value
		}
		foreach ($entry in $sensitiveEntries)
		{
			$key = $entry.Name
	        $envKey = $key -replace ":", "__"
            Write-Output "Setting environment variable $envKey"
            $env[$envKey] = $entry.Value
		}

		Write-Output "Done"
	} elseif ($Target -eq "Terraform") {
		Write-Output "Generating Terraform output"
        $padding = 50
        #take the tf output file without extension
        $varName = [System.IO.Path]::GetFileNameWithoutExtension($TfFile) -replace "-", "_"
        $aspEnv = if ($Environment -eq "dev") { "Development" } else { $Environment}
        $header = @"
variable "$varName" {
    description = "Generated settings from $SourceFile"
    type        = map(string)
    default     = {
      "ASPNETCORE_DETAILEDERRORS"                        = "false"
      "ASPNETCORE_ENVIRONMENT"                           = "$aspEnv"
      "WEBSITE_RUN_FROM_PACKAGE"                         = var.app_deploy_as_package    
"@

        $footer = "    }`n}"
        $tfOutput = ""
        
		foreach ($entry in $akvEntries)
		{
			$key = $entry.Name
            if ($sensitiveKeys -contains $key)
            {
                Write-Output "Skipping sensitive key $key"
                continue
            }        
	        $envKey = $key -replace ":", "__"
            #pad spaces to $envKey
            $envKey = "`"$envKey`"".PadRight($padding)
            Write-Host "Setting akv $envKey"
            $tfOutput += "      $envKey = `"$($entry.Value)`"" + "`n"
		}
		foreach ($entry in $otherEntries)
		{
			$key = $entry.Name
	        $envKey = $key -replace ":", "__"
            $envKey = "`"$envKey`"".PadRight($padding)
            Write-Host "Setting $envKey"
            $tfOutput += "      $envKey = `"$($entry.Value)`"" + "`n"
		}
		foreach ($entry in $sensitiveEntries)
		{
			$key = $entry.Name
	        $envKey = $key -replace ":", "__"
            $envKey = "`"$envKey`"".PadRight($padding)
            $secretValue = Read-AllSecrets $entry.Value
            Write-Host "Setting sensitive entry $envKey"
            $tfOutput += "      $envKey = `"$($secretValue)`"" + "`n"
		}
        # sort all tfOutput lines
        $sortedTfOutput = $tfOutput -split "`n" | Sort-Object | Out-String
        $tfOutput = $header + $sortedTfOutput
        $tfOutput += $footer
        Write-Host "Writing to $tfFilePath"
        #Write-Host $tfOutput
        $tfOutput | Out-File -FilePath $tfFilePath
		Write-Host "Done"
	}
}

function Read-AllSecrets($value)
{
    $vaultName = $null
    $secretName = $null
    $patterns = 0
    while ($value -match $kv_pattern) {
        $vaultName = $Matches.VaultName
        $secretName = $Matches.SecretName
        Write-Host "Reading secret $secretName from vault $vaultName"                
        $secretValue = Read-VaultSecret $vaultName $secretName
        # replace $kv_pattern with secret value
        $value = $value.Replace($Matches[0], $secretValue)
        #$value = $value -replace $kv_pattern, $secretValue
        #Write-Host "Value is $value"
        $patterns++
    }
    # if ($patterns -eq 0) {
    #     Write-Host "No keyvault pattern found in $value"
    # }
    return $value
}

function Read-SecureString {
    param (
        [Parameter(Mandatory = $true)]
        [System.Security.SecureString]$SecureString
    )

    try {
        # Convert SecureString to plain text , linux safe
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureString)
        return [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($BSTR)
    }
    catch {
        Write-Error "An error occurred while converting the SecureString: $_"
    }
    finally {
        if ($BSTR) {
            # Ensure the allocated memory is freed even if there's an error
            [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
        }
    }
}



function Read-VaultSecret {
    param (
        [Parameter(Mandatory = $true)]
        [string]$vault,

        [Parameter(Mandatory = $true)]
        [string]$secretId
    )

    try {
        $secret = Get-AzKeyVaultSecret -VaultName $vault -Name $secretId

        if (-not $secret.SecretValue) {
            throw "The secret value retrieved from the vault is null or empty."
        }

        return Read-SecureString($secret.SecretValue)
    }
    catch {
        Write-Error "Error reading secret $secretId from vault $vault - $_"
    }
}


function ConvertTo-HashTable {
    param(
        [Parameter(Mandatory=$true)]
        [PSCustomObject]$Object,
        [string]$Parent = $null
    )

    $result = @{}

    $Object.PSObject.Properties | ForEach-Object {
        $key = if ($Parent) { "$($Parent):$($_.Name)" } else { $_.Name }
        $value = $_.Value

        if ($value -is [PSCustomObject]) {
            $result += ConvertTo-HashTable -Object $value -Parent $key
        } else {
            $result[$key] = $value
        }
    }

    return $result
}

function ConvertFrom-HashTable {
    param(
        [Parameter(Mandatory=$true)]
        [Hashtable]$Hashtable,
        [string]$Separator = ':'                
    )

    $result = New-Object PSObject

    foreach ($entry in $Hashtable.GetEnumerator()) {
        $keys = $entry.Key -split $Separator
        $value = $entry.Value
        #Write-Host "Key is $keys - value is $value"
        $currentObject = $result

        for ($i = 0; $i -lt $keys.Length; $i++) {
            $key = $keys[$i]

            if ($i -eq $keys.Length - 1) {
                $currentObject | Add-Member -Type NoteProperty -Name $key -Value $value
            } else {
                if ($null -eq $currentObject.PSObject.Properties.Name -or (-not $currentObject.PSObject.Properties.Name.Contains($key))) {
                    $currentObject | Add-Member -Type NoteProperty -Name $key -Value (New-Object PSObject)
                }
              
                #Write-Host "Setting current object to $currentObject"
                $currentObject = $currentObject.PSObject.Properties[$key].Value
            }
        }
    }

    return $result
}

function Install-RequiredModules
{
    <#
    .SYNOPSIS
    Downloads and installs all required PowerShell modules for the DataHub Portal project.
    
    .DESCRIPTION
    This function installs the commonly used PowerShell modules across the DataHub Portal project:
    - Az.KeyVault: For Azure Key Vault operations
    - Az.ContainerRegistry: For Azure Container Registry operations
    - Az.Accounts: For Azure authentication
    - Az.Profile: For Azure context management
    
    .PARAMETER Scope
    Specifies the installation scope. Valid values are 'CurrentUser' (default) and 'AllUsers'.
    
    .PARAMETER Force
    Forces installation even if modules already exist.
    
    .EXAMPLE
    Install-RequiredModules
    Installs all required modules for the current user.
    
    .EXAMPLE
    Install-RequiredModules -Scope AllUsers -Force
    Forcefully installs all required modules for all users.
    #>
    
    param (
        [ValidateSet("CurrentUser", "AllUsers")]
        [string]$Scope = "CurrentUser",
        [switch]$Force
    )
    
    # List of required modules for the DataHub Portal project
    $requiredModules = @(
        @{
            Name = "Az.KeyVault"
            Description = "Azure Key Vault module for secret management"
        },
        @{
            Name = "Az.ContainerRegistry" 
            Description = "Azure Container Registry module"
        },
        @{
            Name = "Az.Accounts"
            Description = "Azure authentication and account management (includes profile management)"
        }
    )
    
    Write-Host "Installing required PowerShell modules for DataHub Portal..." -ForegroundColor Green
    Write-Host "Installation scope: $Scope" -ForegroundColor Yellow
    
    foreach ($module in $requiredModules) {
        $moduleName = $module.Name
        $description = $module.Description
        
        Write-Host "`nProcessing module: $moduleName" -ForegroundColor Cyan
        Write-Host "Description: $description" -ForegroundColor Gray
        
        try {
            # Check if module is already installed
            $installedModule = Get-Module -ListAvailable -Name $moduleName
            
            if ($installedModule -and -not $Force) {
                Write-Host "✓ Module $moduleName is already installed (Version: $($installedModule[0].Version))" -ForegroundColor Green
                continue
            }
            
            if ($Force -and $installedModule) {
                Write-Host "⚠ Forcing reinstallation of $moduleName..." -ForegroundColor Yellow
            }
            
            Write-Host "📦 Installing $moduleName..." -ForegroundColor Blue
            
            # Install the module
            Install-Module -Name $moduleName -Force:$Force -Scope $Scope -AllowClobber
            
            # Verify installation
            $verifyModule = Get-Module -ListAvailable -Name $moduleName
            if ($verifyModule) {
                Write-Host "✓ Successfully installed $moduleName (Version: $($verifyModule[0].Version))" -ForegroundColor Green
            } else {
                Write-Warning "⚠ Installation of $moduleName may have failed - module not found after installation"
            }
        }
        catch {
            Write-Error "❌ Failed to install module $moduleName`: $($_.Exception.Message)"
            Write-Host "You may need to run PowerShell as Administrator or check your execution policy." -ForegroundColor Red
        }
    }
    
    Write-Host "`n🎉 Module installation process completed!" -ForegroundColor Green
    Write-Host "You can now use the DataHub Portal PowerShell scripts." -ForegroundColor Yellow
    
    # Display summary
    Write-Host "`n📋 Installation Summary:" -ForegroundColor Cyan
    foreach ($module in $requiredModules) {
        $installedModule = Get-Module -ListAvailable -Name $module.Name
        if ($installedModule) {
            Write-Host "✓ $($module.Name) - Version $($installedModule[0].Version)" -ForegroundColor Green
        } else {
            Write-Host "❌ $($module.Name) - Not installed" -ForegroundColor Red
        }
    }
}

Export-ModuleMember -Function Export-Settings, ConvertFrom-HashTable, Find-InfraRepo, Read-SecureString, Read-VaultSecret, Install-RequiredModules, Connect-FSDHAzure
