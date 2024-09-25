# appsettings.Tests.ps1

# Load the module before running the tests
BeforeAll {
    # Import the module to be tested
    Import-Module "$PSScriptRoot/../appsettings.psm1" -Force

}

Describe "Read-SecureString Function Tests" {

    # Test case: SecureString conversion to plain string
    It "should convert a SecureString to a plain text string" {
        # Arrange: Create a SecureString
        $plainText = "SuperSecretTestString"
        $secureString = ConvertTo-SecureString $plainText -AsPlainText -Force

        # Act: Call the Read-SecureString function
        $result = Read-SecureString -secureString $secureString

        # Assert: The result should match the original plain text string
        $result | Should -Be $plainText
    }

    # Test case: Passing null or an invalid SecureString (edge case)
    It "should throw an error when null is passed as the secureString" {
        # Act and Assert: The function should throw an error when null is passed
        { Read-SecureString -secureString $null } | Should -Throw
    }
}


