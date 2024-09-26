# appsettings.tests.ps1

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
        { Read-SecureString -secureString $null } | Should -Throw
    }
}


