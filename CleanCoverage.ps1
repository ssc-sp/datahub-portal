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

# Define the list of class name patterns to remove
$classPatternsToRemove = @(
    "Datahub.Functions.DirectFunctionExecutor*",
    "Datahub.Functions.FunctionExecutorHostBuilderExtensions",
    "Datahub.Functions.FunctionExecutorAutoStartup*",
    "Datahub.Functions.FunctionExecutorHostBuilderExtensions*",
    "Datahub.Functions.FunctionMetadataProviderAutoStartup*",
    "Datahub.Functions.GeneratedFunctionMetadataProvider",
    "Datahub.Functions.WorkerHostBuilderFunctionMetadataProviderExtension",
    "Datahub.Functions.WorkerExtensionStartupCodeExecutor"
)

# Find and remove the class nodes matching the patterns
foreach ($pattern in $classPatternsToRemove) {
    $classesToRemove = $coverageXml.coverage.packages.package.classes.class | Where-Object { $_.name -like $pattern }
    foreach ($class in $classesToRemove) {
        $class.ParentNode.RemoveChild($class) | Out-Null
    }
}

# Save the modified XML back to the file
$coverageXml.Save($coverageFilePath.FullName)

Write-Host "Removed generated code from coverage report."
