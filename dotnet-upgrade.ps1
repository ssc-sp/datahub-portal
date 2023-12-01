Param(
    [Parameter(Mandatory=$false)]
    [string]$Directory = ".",

    [Parameter(Mandatory=$false)]
    [switch]$WhatIf
)

# write me a powershell script that iterates through all csproj files and upgrades dotnet 7 to dotnet 8
# Get all csproj files in the current directory and subdirectories
$csprojFiles = Get-ChildItem $Directory -Recurse -Filter *.csproj

#Only if $WhatIf is not present
# Loop through each csproj file and replace dotnet 7 with dotnet 8
if (-not $WhatIf)
{
	Write-Host "Replacing dotnet 7 with dotnet 8"
	foreach ($file in $csprojFiles) {
		Write-Host "Processing $file"
		(Get-Content $file.FullName) | ForEach-Object {
			$_ -replace 'net7.0', 'net8.0'
		} | Set-Content $file.FullName
	}
}

$patternList = @(
	"(Microsoft\..*)", "(System\..*)"
)

function Replace-LastNumberWithStar {
    param (
        [string]$Version
    )

    $versionParts = $Version.Split('.')
    $versionParts[-1] = "*"
    $newVersion = $versionParts -join '.'
    return $newVersion
}

# Loop through each csproj file and replace the specified PackageReference with the new version
foreach ($file in $csprojFiles) {
	Write-Host "Processing $file"
	$updatedCsProj =	(Get-Content $file.FullName) | ForEach-Object {
			$line = $_
			ForEach ($pattern in $patternList)
			{
				if ($line -match "PackageReference Include=`"$pattern`" Version=`"(7\..*)`"") {
					Write-Host "Found 7.x package: $($Matches[1]) - $($Matches[2]) - searching for 8.x version on nuget"
					$latestVersion = (Find-Package -Name $Matches[1] -AllVersions | Where-Object { $_.Version -like "8.*" } | Sort-Object -Property Version -Descending | Select-Object -First 1).Version
					$tgtVersion = Replace-LastNumberWithStar($latestVersion)
					Write-Host "Found 8.x package: $latestVersion - Target is $tgtVersion"
					return $line -replace "Version=`".*`"", "Version=`"$tgtVersion`""
				}
			}
			return $line
	}
	if (!$WhatIf)
	{
		#rename original csproj to .backup
		Rename-Item $file.FullName "$($file.FullName).backup"
		#write new csproj
		$updatedCsProj | Set-Content $file.FullName
	}
	# Set-Content $file.FullName
	
}
