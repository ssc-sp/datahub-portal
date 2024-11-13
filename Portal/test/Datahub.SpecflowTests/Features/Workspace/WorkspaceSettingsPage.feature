@IWebHostEnvironment
Feature: Workspace Settings Page
    The workspace settings page should display settings for the workspace

    Scenario: The workspace settings page should have a prevent auto delete setting
        Given a workspace settings page
        When the prevent auto delete is toggled
        And the workspace settings page changes are saved
        Then the workspace should have the prevent delete setting enabled
        
    Scenario: The workspace settings page should trigger a terraform run on budget change
        Given a workspace settings page
        When the budget is changed to 200.01
        And the workspace settings page changes are saved
        Then the workspace should trigger a terraform run
        And the workspace should have a budget of 200.01
        
    Scenario: If there are no changes the workspace settings page should not trigger a terraform run
        Given a workspace settings page
        When the workspace settings page changes are saved
        Then the workspace should not trigger a terraform run
        And the audit track data event should not be logged
        
    Scenario: The workspace settings page should show a save changes button if there are changes
        Given a workspace settings page
        When the prevent auto delete is toggled
        Then the workspace settings page should show a save changes button
