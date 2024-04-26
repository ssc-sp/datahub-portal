param (    
    [ValidateSet("test", "dev","int","poc")]
    [string]$Environment = "dev")
$ErrorActionPreference = "Stop"
$CurrentPath = Split-Path -Parent $MyInvocation.MyCommand.Path
#get full path from $CurrentPath

Import-Module $CurrentPath/scripts/appsettings.psm1 -Force

# Portal
Write-Host "`n** Exporting Portal settings`n"
Export-Settings -ProjectFolder "./Portal/src/Datahub.Portal" -SourceFile "./Portal/template.appsettings.json" -Target AppSettings -Environment $Environment
Export-Settings -SourceFile "./Portal/template.appsettings.json" -Target Terraform -Environment $Environment -TfFile "terraform/env/$Environment/portal-settings-$Environment.tf"

# Dotnet Function
Write-Host "`n** Exporting Dotnet Function settings`n"
Export-Settings -ProjectFolder "./ServerlessOperations/src/Datahub.Functions" -SourceFile "./ServerlessOperations/template.settings.json" -Target Function -Environment dev -TargetFile "local.settings.json"
Export-Settings -ProjectFolder "./ServerlessOperations/test/Datahub.Functions.UnitTests" -SourceFile "./ServerlessOperations/template.settings.json" -Target Appsettings -Environment dev -TargetFile "appsettings.Test.json"
Export-Settings -SourceFile "./ServerlessOperations/template.settings.json" -Target Terraform -Environment $Environment -TfFile "terraform/env/$Environment/dotnetfunc-settings-$Environment.tf"

# Resource Provisioner
Write-Host "`n** Exporting Resource Provisioner settings`n"
Export-Settings -ProjectFolder "./ResourceProvisioner/src/ResourceProvisioner.Functions" -SourceFile "./ResourceProvisioner/template.settings.json" -Target Function -Environment dev -TargetFile "local.settings.json"
Export-Settings -ProjectFolder "./ResourceProvisioner/test/ResourceProvisioner.Application.IntegrationTests" -SourceFile "./ResourceProvisioner/template.settings.json" -Target Appsettings -Environment dev -TargetFile "appsettings.test.json"
Export-Settings -ProjectFolder "./ResourceProvisioner/test/ResourceProvisioner.Application.UnitTests" -SourceFile "./ResourceProvisioner/template.settings.json" -Target Appsettings -Environment dev -TargetFile "appsettings.test.json"
Export-Settings -ProjectFolder "./ResourceProvisioner/test/ResourceProvisioner.Domain.UnitTests" -SourceFile "./ResourceProvisioner/template.settings.json" -Target Appsettings -Environment dev -TargetFile "appsettings.test.json"
Export-Settings -ProjectFolder "./ResourceProvisioner/test/ResourceProvisioner.Infrastructure.UnitTests" -SourceFile "./ResourceProvisioner/template.settings.json" -Target Appsettings -Environment dev -TargetFile "appsettings.test.json"
Export-Settings -ProjectFolder "./ResourceProvisioner/test/ResourceProvisioner.SpecflowTests" -SourceFile "./ResourceProvisioner/template.settings.json" -Target Appsettings -Environment dev -TargetFile "appsettings.test.json"
Export-Settings -SourceFile "./ResourceProvisioner/template.settings.json" -Target Terraform -Environment $Environment -TfFile "terraform/env/$Environment/rp-settings-$Environment.tf"
