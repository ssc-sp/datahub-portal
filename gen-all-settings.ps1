$CurrentPath = Split-Path -Parent $MyInvocation.MyCommand.Path
#get full path from $CurrentPath

Import-Module $CurrentPath/scripts/appsettings.psm1 -Force

Export-AppSettings -SourceFile "./ResourceProvisioner/test/ResourceProvisioner.SpecflowTests/template.appsettings.test.json" -Target AppSettings -EnvironmentName test