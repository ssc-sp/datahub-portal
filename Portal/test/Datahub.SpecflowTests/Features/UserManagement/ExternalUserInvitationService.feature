@ExternalUserInvitationService
@IWebHostEnvironment
Feature: External User Invitation Service
    Managing workspace invitations for external users

    Background:
        Given a project "TEST" exists in the database
        And an external user with id 1 exists in the database
        And a portal user "inviter@test.com" exists as the inviter

    Scenario: A valid invitation token is reported as valid
        Given a valid workspace invitation exists for the external user
        When the invitation token validity is checked
        Then the token should be valid

    Scenario: An expired invitation token is reported as invalid
        Given an expired workspace invitation exists for the external user
        When the invitation token validity is checked
        Then the token should not be valid

    Scenario: An already accepted invitation token is reported as invalid
        Given a workspace invitation that was already accepted exists for the external user
        When the invitation token validity is checked
        Then the token should not be valid

    Scenario: An empty Guid invitation token is reported as invalid
        When the invitation token validity is checked with an empty Guid
        Then the token should not be valid

    Scenario: Creating an invitation stores it in the database
        When an invitation is created for the external user in project "TEST"
        Then the invitation should be stored in the database
        And the invitation should have a future expiry date

    Scenario: Creating an invitation sends a notification email
        When an invitation is created for the external user in project "TEST"
        Then a notification email should be sent

    Scenario: Creating an invitation for a non-existent project throws an error
        When an invitation is created for the external user in project "INVALID"
        Then an invalid operation exception should be thrown

    Scenario: Creating an invitation for a non-existent external user throws an error
        When an invitation is created for a non-existent external user in project "TEST"
        Then an invalid operation exception should be thrown

    Scenario: Cancelling an invitation sets its expiry to now
        Given a valid workspace invitation exists for the external user
        When the invitation is cancelled
        Then the invitation expiry should be set to approximately now

    Scenario: Cancelling a non-existent invitation returns null
        When a non-existent invitation is cancelled
        Then the result should be null

    Scenario: Resending an invitation cancels existing active invitations and creates a new one
        Given a valid workspace invitation exists for the external user
        When the invitation is resent to the external user in project "TEST"
        Then the original invitation should be expired
        And a new invitation should be created in the database

    Scenario: Completing an invitation with valid token and code assigns the workspace role
        Given a valid workspace invitation exists for the external user
        When the invitation is completed with the correct code and a new external subject
        Then the invitation should be marked as accepted
        And the external user should have the requested role in the project

    Scenario: Completing an invitation with an incorrect code returns false
        Given a valid workspace invitation exists for the external user
        When the invitation is completed with an incorrect code
        Then the completion should return false

    Scenario: Completing an expired invitation returns false
        Given an expired workspace invitation exists for the external user
        When the invitation is completed with the correct code and a new external subject
        Then the completion should return false

    Scenario: Completing an already-accepted invitation returns false
        Given a workspace invitation that was already accepted exists for the external user
        When the invitation is completed with the correct code and a new external subject
        Then the completion should return false
