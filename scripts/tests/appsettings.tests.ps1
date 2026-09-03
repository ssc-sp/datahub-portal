# appsettings.tests.ps1

Describe 'appsettings module' {
    BeforeAll {
        Import-Module "$PSScriptRoot/../appsettings.psm1" -Force

        # Fake Get-AzKeyVaultSecret global function to bypass an actual azure connection.
        function global:Get-AzKeyVaultSecret {
            param (
                [string] $VaultName,
                [string] $Name
            )
            # Return an object with a SecretValue that contains the SecureString
            return [PSCustomObject]@{
                SecretValue = ConvertTo-SecureString 'SuperSecretValue' -AsPlainText -Force
            }
        }
    }

    Describe 'Read-VaultSecret Function Tests' {
    It 'Should return the secret as a plain text string when valid vault and secretId are provided' {

        $result = Read-VaultSecret -vault 'TestVault' -secretId 'TestSecret'
        $result | Should -Be 'SuperSecretValue'
    }

    It "Throw an error when null or bad value is passed as the secureString" {
        { Read-VaultSecret -secretId 'TestSecret' } | Should -Throw
    }

}


    Describe "Read-SecureString Function Tests" {
        It "Should convert a SecureString to a plain text string" {
            
            $plainText = "SuperSecretTestString"
            $secureString = ConvertTo-SecureString $plainText -AsPlainText -Force
            $result = Read-SecureString -secureString $secureString

            $result | Should -Be $plainText
        }

        It "Throw an error when null or bad value is passed as the secureString" {
            { Read-SecureString -securestring $null } | Should -Throw
        }
    }

    Describe 'Export-Settings secret handling' {
        It 'Throws when a Key Vault secret cannot be resolved' {
            $tempRoot = Join-Path $TestDrive 'export-settings'
            $projectFolder = Join-Path $tempRoot 'project'
            $templatePath = Join-Path $tempRoot 'appsettings.template.json'

            New-Item -ItemType Directory -Path $projectFolder -Force | Out-Null
            [System.IO.File]::WriteAllText($templatePath, '{"TestSetting":"@Microsoft.KeyVault(VaultName=test-vault;SecretName=test-secret)"}')

            Mock Connect-FSDHAzure { $true }
            Mock Read-VaultSecret {
                param(
                    [string]$vault,
                    [string]$secretId
                )

                if ($secretId -eq 'test-secret') {
                    throw "Error reading secret $secretId from vault $vault - No such host is known."
                }

                return 'dummy'
            }

            { Export-Settings -SourceFile $templatePath -Target AppSettings -Environment dev -ProjectFolder $projectFolder } | Should -Throw
        }
    }
}


