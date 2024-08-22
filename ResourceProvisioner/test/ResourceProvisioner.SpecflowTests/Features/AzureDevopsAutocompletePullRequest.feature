@azure-devops-pull-request
Feature: Azure DevOps Autocomplete Pull Request
This is the feature which merges the resource run changes with the Azure DevOps Git repository using the access token.

    Scenario: There is an existing pull request and it needs to be auto-completed
        Given a pull request id of 7
        And a workspace acronym "TEST1"
        When a pull request patch request is sent
        Then a successful response should be returned

#        When the pull request patch request is sent
#        Then the pull request should be created
