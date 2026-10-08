@IWebHostEnvironment
Feature: Workspace project information
The workspace metadata page displays project information from Datahub_Project and allows administrators to update both descriptions.

    Scenario: Project information is displayed using the expected GCDS controls
        Given a workspace has project information
        When the workspace project information is rendered
        Then every workspace project information value is displayed
        And only the workspace project descriptions are editable
        And the project information save action is initially disabled

    Scenario: Both project descriptions are saved and synchronized to the catalog
        Given a workspace has project information
        When the workspace project information is rendered
        And both workspace project descriptions are changed
        And the workspace project information is saved
        Then both workspace project descriptions are persisted
        And the workspace catalog entry contains the updated descriptions
        And the project information success state is displayed

    Scenario: Both project descriptions are required
        Given a workspace has project information
        When the workspace project information is rendered
        And the English workspace project description is cleared
        And the workspace project information is saved
        Then the required project description error is displayed
        And the workspace project information is not persisted or synchronized

    Scenario: A nullable project budget is displayed as an empty read-only value
        Given a workspace has project information without a budget
        When the workspace project information is rendered
        Then the project budget value is empty and read-only

    Scenario Outline: Missing workspace project information displays the empty state
        Given <workspace_state>
        When the workspace project information is rendered
        Then the workspace project information empty state is displayed

        Examples:
          | workspace_state                         |
          | no matching workspace project exists   |
          | no workspace project acronym is supplied |

    Scenario: A catalog synchronization failure displays an error
        Given a workspace has project information and catalog synchronization will fail
        When the workspace project information is rendered
        And both workspace project descriptions are changed
        And the workspace project information is saved
        Then the project information failure state is displayed
