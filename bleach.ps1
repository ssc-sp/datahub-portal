param (
    [switch]$WhatIf
)

Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object {
    $projectDir = $_.DirectoryName
    $binDir = Join-Path -Path $projectDir -ChildPath "bin"
    $objDir = Join-Path -Path $projectDir -ChildPath "obj"

    if (Test-Path $binDir) {
        Remove-Item -Recurse -Force $binDir -WhatIf:$WhatIf
        if ($WhatIf) {
            Write-Output "Would delete $binDir"
        } else {
            Write-Output "Deleted $binDir"
        }
    }

    if (Test-Path $objDir) {
        Remove-Item -Recurse -Force $objDir -WhatIf:$WhatIf
        if ($WhatIf) {
            Write-Output "Would delete $objDir"
        } else {
            Write-Output "Deleted $objDir"
        }
    }
}