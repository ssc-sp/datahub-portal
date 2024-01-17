Feature: Resource Requesting
Tests around the workspace resource requesting and its functionality for the user

    @queue
    Scenario: Create a new workspace database
        Given a workspace without a database resource
        And a current user
        When a current user requests to create a workspace database
        Then there should be a workspace database resource created
        And there should be a queue message to create a workspace database
        
        
    @queue
    Scenario: Create a new workspace resource
        Given a workspace without a <resource_name> resource
        And a current user
        When a current user requests to create a workspace <resource_name>
        Then there should be a workspace <resource_name> resource created
        And there should be a queue message to create a workspace <resource_name>
        
        
        Examples:
        | resource_name |
        | TerraformTemplate.AzurePostgres |