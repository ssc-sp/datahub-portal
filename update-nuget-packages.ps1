#This script will iterate through your csproj files and check if any of the packages need updating
#It will update the package version to the latest version available with a wild card for the build/patch
#Usage: .\update-nuget-packages.ps1
#Run dotnet restore after running this script.

$shouldExit = $false

Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object {
    if ($shouldExit) { break }
    $file = $_.FullName
    Write-Host "==== Checking $file ===="
    $output = dotnet list "$file" package --outdated 2>&1

    # Find the table header to start processing after it
    $start = $output | Select-String -Pattern 'Top-level Package' | Select-Object -First 1
    if ($start) {
        $startIndex = $output.IndexOf($start.Line) + 1
        $packageLines = $output[$startIndex..($output.Length - 1)]
    } else {
        $packageLines = @()
    }

    $foundOutdated = $false
    foreach ($line in $packageLines) {
        $trimmed = $line.Trim()
        if ($trimmed -eq "" -or $trimmed -like "*Package*" -or $trimmed -like "*Requested*" -or $trimmed -like "*Resolved*" -or $trimmed -like "*Latest*") {
            continue
        }
        if ($trimmed.StartsWith(">")) { $trimmed = $trimmed.Substring(1).Trim() }
        $parts = $trimmed -split '\s+'

        # Find all version numbers in the line
        $versions = [regex]::Matches($trimmed, '\d+\.\d+\.\d+(\*|)')
        if ($parts.Length -ge 4) {
            $pkg = $parts[0]
            $requested = $parts[1]
            $resolved = $parts[2]
            $latest = $parts[3]
            $current = $resolved

            # Only update if latest is a valid version and is newer than current
            if ($latest -match '^\d+\.\d+\.\d+$' -and $latest -ne $current) {
                $foundOutdated = $true
                if ($latest -match '^(\d+)\.(\d+)\.') {
                    $major = $matches[1]
                    $minor = $matches[2]
                    $newVersion = "$major.$minor.*"
                    Write-Host "  [DEBUG] latest: $latest, major: $major, minor: $minor, newversion: $newVersion"
                    Write-Host "Update $pkg in $file from $current to $newVersion ? (y/n or press Escape to exit): " -NoNewline
                    $key = [Console]::ReadKey($true)
                    Write-Host $key.KeyChar
                    if ($key.Key -eq 'Escape') {
                        Write-Host "`nExiting script."
                        $shouldExit = $true
                        break
                    } elseif ($key.KeyChar -eq 'y') {
                        $content = Get-Content $file -Raw
                        $pattern = "<PackageReference Include=`"$pkg`" Version=`"[^`"]*`""
                        $replacement = "<PackageReference Include=`"$pkg`" Version=`"$newVersion`""
                        $newContent = $content -replace $pattern, $replacement
                        if ($content -ne $newContent) {
                            Set-Content $file $newContent
                            Write-Host "  Updated $pkg from $current to $newVersion in $file"
                        } else {
                            Write-Host "  No change made for $pkg in $file"
                        }
                    } else {
                        Write-Host "  Skipped $pkg in $file"
                    }
                }
            }
        }
    }
    if (-not $foundOutdated) {
        Write-Host "No outdated packages found in $file."
    }
    Write-Host ""
}