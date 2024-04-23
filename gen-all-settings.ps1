$CurrentPath = Split-Path -Parent $MyInvocation.MyCommand.Path
#get full path from $CurrentPath

Import-Module $CurrentPath/scripts/appsettings.psm1 -Force

# Portal
Write-Host "`n** Exporting Portal settings`n"
Export-Settings -ProjectFolder "./Portal/src/Datahub.Portal" -SourceFile "./Portal/template.appsettings.dev.json" -Target AppSettings -EnvironmentName test
Export-Settings -SourceFile "./Portal/template.appsettings.dev.json" -Target Terraform -EnvironmentName development -TfFile "terraform/env/dev/portal-settings-dev.tf"

# Dotnet Function
Write-Host "`n** Exporting Dotnet Function settings`n"
Export-Settings -ProjectFolder "./ServerlessOperations/src/Datahub.Functions" -SourceFile "./ServerlessOperations/template.settings.dev.json" -Target Function -EnvironmentName test -TargetFile "local.settings.json"
