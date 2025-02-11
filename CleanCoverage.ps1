param (
    [string]$coverageFilePathPattern
)

# Find the actual coverage file path
$coverageFilePath = Get-ChildItem -Path $coverageFilePathPattern -File | Select-Object -First 1

if (-Not $coverageFilePath) {
    Write-Host "Coverage file not found: $coverageFilePathPattern"
    exit 1
}

# Read the entire file as a single string
[xml]$coverageXml = Get-Content $coverageFilePath.FullName -Raw

# Find and remove the class nodes for DirectFunctionExecutor
$classesToRemove = $coverageXml.coverage.packages.package.classes.class | Where-Object { $_.name -like "Datahub.Functions.DirectFunctionExecutor*" }

foreach ($class in $classesToRemove) {
    $class.ParentNode.RemoveChild($class) | Out-Null
}

# Save the modified XML back to the file
$coverageXml.Save($coverageFilePath.FullName)

Write-Host "Removed generated code from coverage report."
