param (    
    [ValidateSet("test", "dev", "int", "poc")]
    [string[]]$Environment = @("dev"),
    [switch]$SkipAppSettings,
    [switch]$SkipTests
)

#temporary until we figure out a solution to integrate TF settings
$SkipTerraform = $true
$ErrorActionPreference = "Stop"
$CurrentPath = Split-Path -Parent $MyInvocation.MyCommand.Path
#get full path from $CurrentPath

Import-Module $CurrentPath/scripts/appsettings.psm1 -Force

foreach ($env in $Environment) {
    # Portal
    Write-Host "`n** Exporting Portal settings in $env`n"
    if (-not $SkipAppSettings) {
        Export-Settings -ProjectFolder "./Portal/src/Datahub.Portal" -SourceFile "./Portal/template.settings.json" -Target AppSettings -Environment $env -TargetFile "appsettings.json"
    }
    if (-not $SkipTests) {
        Export-Settings -ProjectFolder "./Portal/test/Datahub.SpecflowTests" -SourceFile "./Portal/template.settings.json" -Target AppSettings -Environment dev -TargetFile "appsettings.test.json"
    }
    if (-not $SkipTerraform) {
        Export-Settings -SourceFile "./Portal/template.settings.json" -Target Terraform -Environment $env -TfFile "terraform/env/$env/portal-settings-$env.tf"
    }

    # Dotnet Function
    Write-Host "`n** Exporting Dotnet Function settings in $env`n"
    if (-not $SkipAppSettings) {
        Export-Settings -ProjectFolder "./ServerlessOperations/src/Datahub.Functions" -SourceFile "./ServerlessOperations/template.settings.json" -Target Function -Environment dev -TargetFile "local.settings.json"
    }
    if (-not $SkipTests) {
        Export-Settings -ProjectFolder "./ServerlessOperations/test/Datahub.Functions.UnitTests" -SourceFile "./ServerlessOperations/template.settings.json" -Target Appsettings -Environment dev -TargetFile "appsettings.Test.json"
    }
    if (-not $SkipTerraform) {
        Export-Settings -SourceFile "./ServerlessOperations/template.settings.json" -Target Terraform -Environment $env -TfFile "terraform/env/$env/dotnetfunc-settings-$env.tf"
    }

    # Resource Provisioner
    Write-Host "`n** Exporting Resource Provisioner settings in $env`n"
    if (-not $SkipAppSettings) {
        Export-Settings -ProjectFolder "./ResourceProvisioner/src/ResourceProvisioner.Functions" -SourceFile "./ResourceProvisioner/template.settings.json" -Target Function -Environment dev -TargetFile "local.settings.json"
    }
    if (-not $SkipTests) {
        Export-Settings -ProjectFolder "./ResourceProvisioner/test/ResourceProvisioner.Application.IntegrationTests" -SourceFile "./ResourceProvisioner/template.settings.json" -Target Appsettings -Environment dev -TargetFile "appsettings.test.json"
        Export-Settings -ProjectFolder "./ResourceProvisioner/test/ResourceProvisioner.Application.UnitTests" -SourceFile "./ResourceProvisioner/template.settings.json" -Target Appsettings -Environment dev -TargetFile "appsettings.test.json"
        Export-Settings -ProjectFolder "./ResourceProvisioner/test/ResourceProvisioner.Domain.UnitTests" -SourceFile "./ResourceProvisioner/template.settings.json" -Target Appsettings -Environment dev -TargetFile "appsettings.test.json"
        Export-Settings -ProjectFolder "./ResourceProvisioner/test/ResourceProvisioner.Infrastructure.UnitTests" -SourceFile "./ResourceProvisioner/template.settings.json" -Target Appsettings -Environment dev -TargetFile "appsettings.test.json"
        Export-Settings -ProjectFolder "./ResourceProvisioner/test/ResourceProvisioner.SpecflowTests" -SourceFile "./ResourceProvisioner/template.settings.json" -Target Appsettings -Environment dev -TargetFile "appsettings.test.json"
    }
    if (-not $SkipTerraform) {
        Export-Settings -SourceFile "./ResourceProvisioner/template.settings.json" -Target Terraform -Environment $env -TfFile "terraform/env/$env/rp-settings-$env.tf"
    }
}
