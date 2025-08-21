# Find all .json files excluding .fr.json
$i18nPath = Join-Path (Split-Path $MyInvocation.MyCommand.Path) 'i18n'
$files = Get-ChildItem -Path $i18nPath -Recurse -Filter *.json | Where-Object { $_.Name -notlike '*.fr.json' }

$keyFiles = @{}
foreach ($file in $files) {
	$lines = Get-Content $file.FullName
	$keys = @()
	$newLines = @()
	foreach ($line in $lines) {
		if ($line -match '"([^"\n]+)":\s*"([^"\n]*)"') {
			$key = $matches[1]
			$value = $matches[2]
			$keys += $key
			# Only keep lines where key != value
			if ($key -ne $value) {
				$newLines += $line
			}
		} else {
			$newLines += $line
		}
	}
	# Rename original file to .backup before overwriting
	$backupPath = "$($file.FullName).backup"
	if (-not (Test-Path $backupPath)) {
		Rename-Item -Path $file.FullName -NewName ($file.Name + ".backup")
		# Write filtered lines to original file name
		Set-Content -Path $file.FullName -Value $newLines
	} else {
		Write-Host "Backup file already exists: $backupPath" -ForegroundColor Magenta
	}
	foreach ($key in $keys) {
		if (-not $keyFiles.ContainsKey($key)) {
			$keyFiles[$key] = @()
		}
		$keyFiles[$key] += $file.FullName
	}
	$dupes = $keys | Group-Object | Where-Object { $_.Count -gt 1 }
	if ($dupes) {
		Write-Host "Duplicate keys found in $($file.FullName):" -ForegroundColor Yellow
		foreach ($d in $dupes) {
			Write-Host "  $($d.Name) ($($d.Count) times)" -ForegroundColor Red
			Write-Host "    Appears in: $($file.FullName)" -ForegroundColor Cyan
		}
	}
}
