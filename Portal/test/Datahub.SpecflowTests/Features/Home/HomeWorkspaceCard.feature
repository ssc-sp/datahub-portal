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

    Scenario: A lone view workspace action is not rendered as a list
        Given a home workspace card has no additional actions
        When the home workspace card is rendered
        Then the view workspace action should not be in a list

    Scenario: Multiple workspace actions are rendered as a list
        Given a home workspace card has a completed storage action
        When the home workspace card is rendered
        Then the view workspace action should be in a list
