@ignore
Feature: EnvironmentVariablesTable
As a user, I want to manage environment variables, so that I can configure my cloud resources correctly

    Scenario: Display environment variables
        Given I am on the Environment Variables page
        When I look at the Environment Variables table
        Then I should see all the environment variables

    Scenario: Filter environment variables
        Given I am on the Environment Variables page
        When I enter a filter string
        Then I should see only the environment variables that match the filter

    Scenario: Add a new environment variable
        Given I am on the Environment Variables page
        When I click on the Add button
        And I enter a key and a value for the new environment variable
        Then the new environment variable should be added to the table

    Scenario: Edit an existing environment variable
        Given I am on the Environment Variables page
        And an environment variable exists
        When I click on the Edit button for that environment variable
        And I change the key and/or value of the environment variable
        And I click on the Commit Edit button
        Then the changes should be saved

    Scenario: Delete an existing environment variable
        Given I am on the Environment Variables page
        And an environment variable exists
        When I click on the Delete button for that environment variable
        Then the environment variable should be removed from the table