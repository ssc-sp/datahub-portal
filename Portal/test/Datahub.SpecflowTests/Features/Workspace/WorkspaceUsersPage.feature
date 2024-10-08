@IWebHostEnvironment
Feature: WorkspaceUsers
    The Workspace Users page should allow adding data steward role.
    
    Scenario: User is listed on the page
        Given the user is on the workspace users page
        Then user with email "dataSteward@example.com" should appear on the page

    Scenario: Set Data Steward role for a user
        Given the user is on the workspace users page 
        When the user sets the Data Steward role for the user with email "dataSteward@example.com"
        And the user clicks the "Save" button
        Then the user with email "dataSteward@example.com" should have the Data Steward role

    Scenario: Remove Data Steward role from a user
        Given the user is on the workspace users page
        When the user removes the Data Steward role from the user with email "dataSteward@example.com"
        And the user clicks the "Save" button
        Then the user with email "dataSteward@example.com" should not have the Data Steward role