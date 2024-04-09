Feature: Workspace Settings Page
    The workspace settings page should display the workspace budget and allow for deletion of the workspace.

    Scenario: Workspace lead inputs negative value for workspace budget
        When a workspace lead inputs a value for the workspace budget
        And the value input is negative
        Then this should not be allowed
        And the user should be alerted to this being erroneous