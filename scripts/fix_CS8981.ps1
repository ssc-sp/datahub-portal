#$files = ('C:\code\datahub-portal\Datahub.Core\Migrations\Core\20221012181943_clientengagements.cs','C:\code\datahub-portal\Datahub.Core\Migrations\Core\20221012181943_clientengagements.Designer.cs')
$files = gci -Recurse -Include *.cs
$TextInfo = (Get-Culture).TextInfo

foreach ($file in $files) {
    $isFixed = $false
    $newName = $file.FullName + ".old"
    $newLines = @()
    $cf = Get-Content $file.FullName
    foreach ($l in $cf)
    {
        #Write-Host "Processing $l"
        $fixed = $l        
        #(.*)\s+class\s+([a-z]+)\s+(.*)
        if ($l -cmatch '(.*)\s*class\s+([a-z]+)\s*(.*)')
        {
            $className = $Matches.2
            $capitalized = $TextInfo.ToTitleCase($className)
            Write-Host "Found pattern in ($file): '$l'"
            Write-Host "Class name is '$className' - capitalized '$capitalized'"
            $fixed = $l -replace $className,$capitalized            
            $isFixed = $true
        }
        $newLines += $fixed
    }
    if ($isFixed)
    {
        $newData = $newLines -join "`n"
        #Write-Host $newData
        Rename-Item $file $newName 
        $newData | Out-File $file.FullName
    }
}
#