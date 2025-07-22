param(
    [string]$PackageName,
    [string]$NewVersion
)

Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object {
    $file = $_.FullName
    $content = Get-Content $file -Raw
    $pattern = "<PackageReference Include=`"$PackageName`" Version=`"[^`"]*`""
    $replacement = "<PackageReference Include=`"$PackageName`" Version=`"$NewVersion`""
    $newContent = $content -replace $pattern, $replacement
    if ($content -ne $newContent) {
        Set-Content $file $newContent
        Write-Host "Updated $PackageName to $NewVersion in $file"
    }
}