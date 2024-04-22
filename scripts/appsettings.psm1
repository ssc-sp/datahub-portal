$kv_pattern = "@Microsoft\.KeyVault\(VaultName=(?<VaultName>[\w-]+);SecretName=(?<SecretName>[\w-]+)\)"

function Export-AppSettings(
    [Parameter(Mandatory = $true)]
    [string]$SourceFile,
    [Parameter(Mandatory=$true)]
    [ValidateSet("AppSettings", "Environment", "Terraform")]
    [string]$Target,
    [Parameter(Mandatory=$true)]
    [ValidateSet("test", "development","poc")]
    [string]$EnvironmentName,
    [Parameter(Mandatory=$true)]
    [string]$ProjectFolder
)
{
    #import module for keyvault
    Import-Module Az.KeyVault -Force -NoClobber
    #check if user is signed in on azure
    $context = Get-AzContext
    # get the path for the current script
    $srcPath = Split-Path -Parent $SourceFile
    # set . to srcPath if it is empty
    $srcPath = if ([string]::IsNullOrEmpty($srcPath)) { "." } else { $srcPath }
    Write-Host "Src path is $srcPath"
    if ($Target -eq "AppSettings") {
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

    $domain = "163oxygen.onmicrosoft.com"

    if ($null -eq $context) {
        connect-azaccount -Domain $domain
    } else {
        Write-Output "User $($context.Account.Id) is signed in."
    }
    # login user to azure
    Write-Output "Fetching secrets from keyvault"

    $resourcePrefix = "fsdh"
    $azureDevOpsOrganization = "DataSolutionsDonnees"
    $azureDevOpsProject = "FSDH%20SSC"
    $vaultName = "fsdh-static-test-akv"
    $azureDevopsRepository = "datahub-project-infrastructure-$EnvironmentName"

    $tenantId = Read-VaultSecret $vaultName "datahub-portal-tenant-id"
    $subscriptionId = Read-VaultSecret $vaultName "datahub-portal-subscription-id"
    $repositoryId = Read-VaultSecret $vaultName "datahub-infrastructure-repo-id"

    $datahubMssqlAdmin = Read-VaultSecret "fsdh-key-dev" "datahub-mssql-admin"
    $datahubMssqlPassword = Read-VaultSecret "fsdh-key-dev" "datahub-mssql-password"
    $sqlCreds = "User ID=$datahubMssqlAdmin;Password=$datahubMssqlPassword"
    if ($Target -eq "Terraform")
    {
        $sqlCreds = "Authentication=Active Directory Managed Identity"
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

    $sensitiveEntries = $flattenedObject.GetEnumerator() | Where-Object { $_.Value -like "*Password=*" }    
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

    $nonAkvSettings = $unflattenedObject | ConvertTo-Json -Depth 100

    if ($Target -eq "AppSettings") {
        Write-Output "Writing json object to $tgtFile"
        Write-Host "Sensitive entries"
        $nonAkvSettings | Out-File -FilePath $tgtFile
        Push-Location
        Set-Location $ProjectFolder
        try {       
            Write-Host "Setting user secrets from keyvault values" 
            foreach ($entry in $akvEntries)
            {
                $key = $entry.Name
                Write-Host "Setting user secret $key"
                $secretValue = Read-AllSecrets $entry.Value
                dotnet user-secrets set $key $secretValue
            }
            # Write-Host "Setting user secrets from sensitive values" 
            # foreach ($entry in $sensitiveEntries)
            # {
            #     $key = $entry.Name
                
            #     dotnet user-secrets set $key $secretValue
            # }
        } catch {
            Write-Error "Error setting user secrets"
        } finally {
            Pop-Location
        }

        Write-Output "Done"
	} elseif ($Target -eq "Environment") {
		Write-Output "Configuring environment variables"
		$nonAkvSettings | Out-File -FilePath $tgtFile
		foreach ($entry in $akvEntries)
		{
			$key = $entry.Name
            $secretValue = Read-AllSecrets $key
	        $envKey = $key -replace ":", "__"
            
            Write-Output "Setting environment variable $envKey"
            $env[$envKey] = $secretValue
		}
		foreach ($entry in $nonAkvSettings)
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

function Read-SecureString($secureString)
{
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureString)
    return [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

function Read-VaultSecret($vault, $secretId)
{
    try {
        return Read-SecureString((Get-AzKeyVaultSecret -VaultName $vault -Name $secretId).SecretValue)
	} catch {
		Write-Error "Error reading secret $secretId from vault $vault"
		return
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

Export-ModuleMember -Function Export-AppSettings, ConvertFrom-HashTable