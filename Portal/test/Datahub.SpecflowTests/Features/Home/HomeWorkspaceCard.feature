@IWebHostEnvironment
Feature: Home workspace card
The home workspace card displays a count of active workspace users.

    Scenario Outline: Inactive workspace users are excluded from the user count
        Given a home workspace card has one active user and one user with the <role> role
        When the home workspace card is rendered
        Then the workspace user count should be 1

        Examples:
          | role         |
          | Removed      |
          | DisabledUser |
