@IWebHostEnvironment
@WorkspaceExternalUsersPage
Feature: Workspace External Users Page
    Workspace leads can manage expired invitations for external users.

    Scenario: An expired invitation offers delete and resend actions
        Given an expired invitation is shown on the external workspace users page
        Then the delete invitation action should be shown
        And the resend invitation action should be shown

    Scenario: An active invitation cannot be deleted
        Given an active invitation is shown on the external workspace users page
        Then the delete invitation action should not be shown
        And the resend invitation action should be shown

    Scenario: Cancelling deletion leaves the expired invitation untouched
        Given an expired invitation is shown on the external workspace users page
        When the workspace lead chooses to delete the invitation
        And the workspace lead cancels invitation deletion
        Then the invitation should remain on the external workspace users page

    Scenario: Confirming deletion removes only the invitation
        Given an expired invitation is shown on the external workspace users page
        When the workspace lead chooses to delete the invitation
        And the workspace lead confirms invitation deletion
        Then the invitation should be removed from the external workspace users page
        And the external user profile should remain in the database
        And invitation deletion success feedback should be shown

    Scenario: A refused deletion keeps the invitation visible
        Given an expired invitation is shown and the deletion service will refuse it
        When the workspace lead chooses to delete the invitation
        And the workspace lead confirms invitation deletion
        Then invitation deletion failure feedback should be shown
        And the invitation should remain on the external workspace users page
