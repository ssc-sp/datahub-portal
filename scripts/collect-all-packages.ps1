param(
	[Parameter(Mandatory = $false)]
	[string]$RootPath = (Get-Location).Path,

	[Parameter(Mandatory = $false)]
	[switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-PackageReferenceNodes {
	param(
		[Parameter(Mandatory = $true)]
		[xml]$XmlDocument
	)

	if ($XmlDocument.DocumentElement.NamespaceURI) {
		$nsUri = $XmlDocument.DocumentElement.NamespaceURI
		$nsManager = New-Object System.Xml.XmlNamespaceManager($XmlDocument.NameTable)
		$nsManager.AddNamespace('msb', $nsUri)
		return ,@($XmlDocument.SelectNodes('//msb:PackageReference', $nsManager))
	}

	return ,@($XmlDocument.SelectNodes('//PackageReference'))
}

function Get-PackageReferenceName {
	param(
		[Parameter(Mandatory = $true)]
		[System.Xml.XmlNode]$Node
	)

	$includeValue = $Node.Attributes['Include']
	if ($null -ne $includeValue -and -not [string]::IsNullOrWhiteSpace($includeValue.Value)) {
		return $includeValue.Value.Trim()
	}

	$updateValue = $Node.Attributes['Update']
	if ($null -ne $updateValue -and -not [string]::IsNullOrWhiteSpace($updateValue.Value)) {
		return $updateValue.Value.Trim()
	}

	return $null
}

function Get-PackageVersionValue {
	param(
		[Parameter(Mandatory = $true)]
		[System.Xml.XmlNode]$Node
	)

	$versionAttr = $Node.Attributes['Version']
	if ($null -ne $versionAttr -and -not [string]::IsNullOrWhiteSpace($versionAttr.Value)) {
		return $versionAttr.Value.Trim()
	}

	foreach ($child in $Node.ChildNodes) {
		if ($child.NodeType -eq [System.Xml.XmlNodeType]::Element -and $child.LocalName -eq 'Version') {
			$inner = $child.InnerText
			if (-not [string]::IsNullOrWhiteSpace($inner)) {
				return $inner.Trim()
			}
		}
	}

	return $null
}

function Remove-PackageVersionFromNode {
	param(
		[Parameter(Mandatory = $true)]
		[System.Xml.XmlNode]$Node
	)

	$changed = $false
	$versionAttr = $Node.Attributes['Version']
	if ($null -ne $versionAttr) {
		[void]$Node.Attributes.Remove($versionAttr)
		$changed = $true
	}

	$versionChildren = @()
	foreach ($child in $Node.ChildNodes) {
		if ($child.NodeType -eq [System.Xml.XmlNodeType]::Element -and $child.LocalName -eq 'Version') {
			$versionChildren += $child
		}
	}

	foreach ($versionChild in $versionChildren) {
		[void]$Node.RemoveChild($versionChild)
		$changed = $true
	}

	return $changed
}

$resolvedRoot = (Resolve-Path -Path $RootPath).Path
$propsPath = Join-Path -Path $resolvedRoot -ChildPath 'Directory.Packages.props'

$csprojFiles = Get-ChildItem -Path $resolvedRoot -Filter '*.csproj' -Recurse -File |
	Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }

if ($csprojFiles.Count -eq 0) {
	Write-Warning "No .csproj files found under '$resolvedRoot'."
	exit 0
}

$packageMap = @{}
$conflicts = @{}
$projectChanges = @{}

foreach ($project in $csprojFiles) {
	$xml = New-Object System.Xml.XmlDocument
	$xml.PreserveWhitespace = $true
	$xml.Load($project.FullName)

	$nodes = Get-PackageReferenceNodes -XmlDocument $xml
	if ($nodes.Count -eq 0) {
		continue
	}

	$hasProjectChanges = $false

	foreach ($node in $nodes) {
		$packageName = Get-PackageReferenceName -Node $node
		if ([string]::IsNullOrWhiteSpace($packageName)) {
			continue
		}

		$version = Get-PackageVersionValue -Node $node
		if (-not [string]::IsNullOrWhiteSpace($version)) {
			if ($packageMap.ContainsKey($packageName)) {
				if ($packageMap[$packageName] -ne $version) {
					if (-not $conflicts.ContainsKey($packageName)) {
						$conflicts[$packageName] = New-Object System.Collections.Generic.HashSet[string]
						[void]$conflicts[$packageName].Add($packageMap[$packageName])
					}
					[void]$conflicts[$packageName].Add($version)
				}
			}
			else {
				$packageMap[$packageName] = $version
			}
		}

		$removed = Remove-PackageVersionFromNode -Node $node
		if ($removed) {
			$hasProjectChanges = $true
		}
	}

	if ($hasProjectChanges) {
		$projectChanges[$project.FullName] = $xml
	}
}

$packageCount = $packageMap.Count
$projectCount = $csprojFiles.Count
$changedProjectCount = $projectChanges.Count

Write-Host "Scanned $projectCount .csproj file(s)."
Write-Host "Found $packageCount package(s) with explicit versions."
Write-Host "$changedProjectCount project file(s) will be updated."
Write-Host "Directory packages file to generate: $propsPath"

if ($conflicts.Count -gt 0) {
	Write-Warning "Detected package version conflicts. The first encountered version will be written to Directory.Packages.props."
	foreach ($pkg in ($conflicts.Keys | Sort-Object)) {
		$versions = $conflicts[$pkg].ToArray() | Sort-Object
		Write-Warning ("  {0}: {1}" -f $pkg, ($versions -join ', '))
	}
}

if (-not $Force) {
	$answer = Read-Host 'Continue and apply these changes? (y/N)'
	if ($answer -notmatch '^(y|yes)$') {
		Write-Host 'Operation cancelled. No files were changed.'
		exit 0
	}
}

$settings = New-Object System.Xml.XmlWriterSettings
$settings.Indent = $true
$settings.OmitXmlDeclaration = $true
$settings.NewLineChars = "`r`n"
$settings.NewLineHandling = [System.Xml.NewLineHandling]::Replace

$stringBuilder = New-Object System.Text.StringBuilder
$stringWriter = New-Object System.IO.StringWriter($stringBuilder)
$xmlWriter = [System.Xml.XmlWriter]::Create($stringWriter, $settings)

$xmlWriter.WriteStartElement('Project')
$xmlWriter.WriteStartElement('PropertyGroup')
$xmlWriter.WriteElementString('ManagePackageVersionsCentrally', 'true')
$xmlWriter.WriteEndElement()

$xmlWriter.WriteStartElement('ItemGroup')
foreach ($pkg in ($packageMap.Keys | Sort-Object)) {
	$xmlWriter.WriteStartElement('PackageVersion')
	$xmlWriter.WriteAttributeString('Include', $pkg)
	$xmlWriter.WriteAttributeString('Version', $packageMap[$pkg])
	$xmlWriter.WriteEndElement()
}
$xmlWriter.WriteEndElement()
$xmlWriter.WriteEndElement()

$xmlWriter.Flush()
$xmlWriter.Close()
$stringWriter.Close()

$propsContent = $stringBuilder.ToString()
Set-Content -Path $propsPath -Value $propsContent -Encoding utf8

foreach ($projectPath in $projectChanges.Keys) {
	$projectXml = $projectChanges[$projectPath]
	$projectXml.Save($projectPath)
}

Write-Host "Generated: $propsPath"
Write-Host "Updated $changedProjectCount .csproj file(s)."
