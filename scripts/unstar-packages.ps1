param(
	[Parameter(Mandatory = $false)]
	[string]$PropsPath = (Join-Path -Path (Get-Location).Path -ChildPath 'Directory.Packages.props'),

	[Parameter(Mandatory = $false)]
	[string]$NuGetSource = 'https://api.nuget.org/v3/index.json',

	[Parameter(Mandatory = $false)]
	[switch]$IncludePrerelease,

	[Parameter(Mandatory = $false)]
	[switch]$WhatIf
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-NuGetApiGet {
	param(
		[Parameter(Mandatory = $true)]
		[string]$Uri,

		[Parameter(Mandatory = $false)]
		[switch]$AllowNotFound
	)

	try {
		return Invoke-RestMethod -Uri $Uri -Method Get
	}
	catch {
		if ($AllowNotFound) {
			$statusCode = $null
			if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
				$statusCode = [int]$_.Exception.Response.StatusCode
			}

			if ($statusCode -eq 404) {
				return $null
			}
		}

		throw
	}
}

function Get-PropertyArray {
	param(
		[Parameter(Mandatory = $true)]
		[object]$Object,

		[Parameter(Mandatory = $true)]
		[string]$PropertyName
	)

	if ($null -eq $Object) {
		return ,@()
	}

	$property = $Object.PSObject.Properties[$PropertyName]
	if ($null -eq $property -or $null -eq $property.Value) {
		return ,@()
	}

	return ,@($property.Value)
}

function Get-PropertyValue {
	param(
		[Parameter(Mandatory = $true)]
		[object]$Object,

		[Parameter(Mandatory = $true)]
		[string]$PropertyName
	)

	if ($null -eq $Object) {
		return $null
	}

	$property = $Object.PSObject.Properties[$PropertyName]
	if ($null -eq $property) {
		return $null
	}

	return $property.Value
}

function Get-RegistrationBaseUrl {
	param(
		[Parameter(Mandatory = $true)]
		[string]$IndexUrl
	)

	$index = Invoke-NuGetApiGet -Uri $IndexUrl
	$resources = Get-PropertyArray -Object $index -PropertyName 'resources'

	$preferredTypes = @(
		'RegistrationsBaseUrl/3.6.0',
		'RegistrationsBaseUrl/3.4.0',
		'RegistrationsBaseUrl/3.0.0-rc',
		'RegistrationsBaseUrl'
	)

	foreach ($preferredType in $preferredTypes) {
		foreach ($resource in $resources) {
			$resourceType = $resource.'@type'
			if ($resourceType -is [array]) {
				if ($resourceType -contains $preferredType) {
					return $resource.'@id'.TrimEnd('/')
				}
			}
			elseif ([string]::Equals([string]$resourceType, $preferredType, [System.StringComparison]::OrdinalIgnoreCase)) {
				return $resource.'@id'.TrimEnd('/')
			}
		}
	}

	throw "Could not find a RegistrationsBaseUrl resource in '$IndexUrl'."
}

function Get-PackageVersionsFromRegistration {
	param(
		[Parameter(Mandatory = $true)]
		[string]$RegistrationBaseUrl,

		[Parameter(Mandatory = $true)]
		[string]$PackageId
	)

	$lowerPackageId = $PackageId.ToLowerInvariant()
	$indexUrl = "$RegistrationBaseUrl/$lowerPackageId/index.json"
	$registrationIndex = Invoke-NuGetApiGet -Uri $indexUrl -AllowNotFound
	if ($null -eq $registrationIndex) {
		return @()
	}

	$versions = @()
	foreach ($page in (Get-PropertyArray -Object $registrationIndex -PropertyName 'items')) {
		$pageItems = Get-PropertyArray -Object $page -PropertyName 'items'
		$pageId = Get-PropertyValue -Object $page -PropertyName '@id'
		if ($pageItems.Count -eq 0 -and -not [string]::IsNullOrWhiteSpace([string]$pageId)) {
			$pageDoc = Invoke-NuGetApiGet -Uri $pageId -AllowNotFound
			$pageItems = Get-PropertyArray -Object $pageDoc -PropertyName 'items'
		}

		foreach ($leaf in $pageItems) {
			$catalogEntry = Get-PropertyValue -Object $leaf -PropertyName 'catalogEntry'
			$version = Get-PropertyValue -Object $catalogEntry -PropertyName 'version'
			if (-not [string]::IsNullOrWhiteSpace($version)) {
				$versions += $version.Trim()
			}
		}
	}

	return $versions
}

function Get-VersionInfo {
	param(
		[Parameter(Mandatory = $true)]
		[string]$Version
	)

	$match = [regex]::Match($Version.Trim(), '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:\.(?<revision>\d+))?(?:-(?<prerelease>[^+]+))?(?:\+.*)?$')
	if (-not $match.Success) {
		return $null
	}

	return [pscustomobject]@{
		Original = $Version.Trim()
		Major = [int]$match.Groups['major'].Value
		Minor = [int]$match.Groups['minor'].Value
		Patch = [int]$match.Groups['patch'].Value
		Revision = if ($match.Groups['revision'].Success) { [int]$match.Groups['revision'].Value } else { 0 }
		PreRelease = if ($match.Groups['prerelease'].Success) { $match.Groups['prerelease'].Value } else { $null }
	}
}

function Compare-VersionInfo {
	param(
		[Parameter(Mandatory = $true)]
		[object]$Left,

		[Parameter(Mandatory = $true)]
		[object]$Right
	)

	if ($Left.Major -ne $Right.Major) { return [Math]::Sign($Left.Major - $Right.Major) }
	if ($Left.Minor -ne $Right.Minor) { return [Math]::Sign($Left.Minor - $Right.Minor) }
	if ($Left.Patch -ne $Right.Patch) { return [Math]::Sign($Left.Patch - $Right.Patch) }
	if ($Left.Revision -ne $Right.Revision) { return [Math]::Sign($Left.Revision - $Right.Revision) }

	$leftHasPre = -not [string]::IsNullOrWhiteSpace($Left.PreRelease)
	$rightHasPre = -not [string]::IsNullOrWhiteSpace($Right.PreRelease)

	if ($leftHasPre -and -not $rightHasPre) { return -1 }
	if (-not $leftHasPre -and $rightHasPre) { return 1 }
	if (-not $leftHasPre -and -not $rightHasPre) { return 0 }

	$preCompare = [string]::Compare($Left.PreRelease, $Right.PreRelease, [System.StringComparison]::OrdinalIgnoreCase)
	if ($preCompare -gt 0) { return 1 }
	if ($preCompare -lt 0) { return -1 }
	return 0
}

function Get-LatestPatchVersion {
	param(
		[Parameter(Mandatory = $true)]
		[string]$RegistrationBaseUrl,

		[Parameter(Mandatory = $true)]
		[string]$PackageId,

		[Parameter(Mandatory = $true)]
		[int]$Major,

		[Parameter(Mandatory = $true)]
		[int]$Minor,

		[Parameter(Mandatory = $true)]
		[bool]$AllowPrerelease
	)

	$candidates = @()
	$versions = Get-PackageVersionsFromRegistration -RegistrationBaseUrl $RegistrationBaseUrl -PackageId $PackageId
	foreach ($version in $versions) {
		$info = Get-VersionInfo -Version $version
		if ($null -eq $info) {
			continue
		}

		if ($info.Major -ne $Major -or $info.Minor -ne $Minor) {
			continue
		}

		if (-not $AllowPrerelease -and -not [string]::IsNullOrWhiteSpace($info.PreRelease)) {
			continue
		}

		$candidates += $info
	}

	if ($candidates.Count -eq 0) {
		return $null
	}

	$latest = $candidates[0]
	for ($i = 1; $i -lt $candidates.Count; $i++) {
		if ((Compare-VersionInfo -Left $candidates[$i] -Right $latest) -gt 0) {
			$latest = $candidates[$i]
		}
	}

	return $latest.Original
}

if (-not (Test-Path -Path $PropsPath)) {
	throw "Could not find '$PropsPath'."
}

$resolvedPropsPath = (Resolve-Path -Path $PropsPath).Path
$registrationBaseUrl = Get-RegistrationBaseUrl -IndexUrl $NuGetSource

Write-Host "Using NuGet index: $NuGetSource"
Write-Host "Using registration base: $registrationBaseUrl"
Write-Host "Reading: $resolvedPropsPath"

$xml = New-Object System.Xml.XmlDocument
$xml.PreserveWhitespace = $true
$xml.Load($resolvedPropsPath)

$namespaceManager = $null
$packageNodes = $null
if ($xml.DocumentElement.NamespaceURI) {
	$namespaceManager = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
	$namespaceManager.AddNamespace('msb', $xml.DocumentElement.NamespaceURI)
	$packageNodes = @($xml.SelectNodes('//msb:PackageVersion', $namespaceManager))
}
else {
	$packageNodes = @($xml.SelectNodes('//PackageVersion'))
}

$pattern = '^(?<major>\d+)\.(?<minor>\d+)\.\*$'
$cache = @{}
$updates = @()

foreach ($node in $packageNodes) {
	$includeAttr = $node.Attributes['Include']
	$versionAttr = $node.Attributes['Version']
	if ($null -eq $includeAttr -or $null -eq $versionAttr) {
		continue
	}

	$packageId = $includeAttr.Value.Trim()
	$versionValue = $versionAttr.Value.Trim()
	$match = [regex]::Match($versionValue, $pattern)
	if (-not $match.Success) {
		continue
	}

	$major = [int]$match.Groups['major'].Value
	$minor = [int]$match.Groups['minor'].Value
	$cacheKey = "$packageId|$major|$minor|$IncludePrerelease"

	if (-not $cache.ContainsKey($cacheKey)) {
		$latestPatch = Get-LatestPatchVersion -RegistrationBaseUrl $registrationBaseUrl -PackageId $packageId -Major $major -Minor $minor -AllowPrerelease:$IncludePrerelease
		$cache[$cacheKey] = $latestPatch
	}

	$newVersion = $cache[$cacheKey]
	if ([string]::IsNullOrWhiteSpace($newVersion)) {
		Write-Warning "No matching version found for $packageId with major.minor $major.$minor from source '$NuGetSource'."
		continue
	}

	if ($newVersion -ne $versionValue) {
		$versionAttr.Value = $newVersion
		$updates += [pscustomobject]@{
			Package = $packageId
			OldVersion = $versionValue
			NewVersion = $newVersion
		}
	}
}

if ($updates.Count -eq 0) {
	Write-Host 'No wildcard patch versions required updates.'
	exit 0
}

Write-Host "Resolved $($updates.Count) wildcard patch version(s):"
foreach ($entry in $updates | Sort-Object -Property Package) {
	Write-Host " - $($entry.Package): $($entry.OldVersion) -> $($entry.NewVersion)"
}

if ($WhatIf) {
	Write-Host 'WhatIf set: no file changes were written.'
	exit 0
}

$xml.Save($resolvedPropsPath)
Write-Host "Updated '$resolvedPropsPath'."
