@IWebHostEnvironment
Feature: WorkspaceUsers
    The Workspace Users page should be accessible and functional for all users.

    Scenario: Workspace Users page is accessible
        Given the user is on the workspace users page
        Then the workspace users page should be displayed

    Scenario: Add a new user to the workspace
        Given the user is on the workspace users page
        When the user adds a new user with email "newuser@example.com"
        Then the new user should be added to the workspace

    Scenario: Remove a user from the workspace
        Given the user is on the workspace users page
        When the user removes the user with email "user@example.com"
        Then the user should be removed from the workspace
    
    Scenario: Set Data Stuart role for a user
        Given the user is on the workspace users page
        When the user sets the Data Stuart role for the user with email "datastuart@example.com"
        And the user clicks the "Save" button
        Then the user with email "datastuart@example.com" should have the Data Stuart role

    Scenario: Remove Data Stuart role from a user
        Given the user is on the workspace users page
        When the user removes the Data Stuart role from the user with email "datastuart@example.com"
        And the user clicks the "Save" button
        Then the user with email "datastuart@example.com" should not have the Data Stuart role