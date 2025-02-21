@IWebHostEnvironment
Feature: WorkspaceUsers
The Workspace Users page should list users, support changing user roles, and allow adding data steward role.
    
    Scenario: Users are listed on the page and some have data steward role
        Given the user is on the workspace users page
        Then user with email "<email>" should appear on the page
        And the user with email "<email>" <should> have the Data Steward role
        Examples: 
            | email             | should     |
            | wlead@example.com | should     |
            | admin@example.com | should not |
            | guest@example.com | should not |

    Scenario: Valid users should be listed on the page
        Given the user is on the workspace users page
        Then user with email "<email>" <shouldAppear> appear on the page
        Examples:
            | email               | shouldAppear |
            | guest@example.com   | should       |
            | admin@example.com   | should       |
            | wlead@example.com   | should       |
            | invalid@example.com | should not   |

    Scenario: Set Data Steward role for a user
        Given the user is on the workspace users page 
        When the user sets the Data Steward role for the user with email "admin@example.com"
        And the Save Changes button is visible
        And the user clicks the "Save" button
        Then the user with email "admin@example.com" should have the Data Steward role

    Scenario: Remove Data Steward role from a user
        Given the user is on the workspace users page
        When the user removes the Data Steward role from the user with email "wlead@example.com"
        And the Save Changes button is visible
        And the user clicks the "Save" button
        Then the user with email "wlead@example.com" should not have the Data Steward role

    Scenario: Data Steward checkbox should be disabled for guest users and enabled for others
        Given the user is on the workspace users page
        Then the Data Steward checkbox should be <enabled> for user "<email>"
        Examples: 
            | email             | enabled  |
            | wlead@example.com | enabled  |
            | admin@example.com | enabled  |
            | guest@example.com | disabled |


    Scenario: Data Steward should be removed and disabled when changing to a disallowed role, and should stay as-is when changing to allowed one
        Given the user is on the workspace users page
        And the user with email "<email>" <hasBefore> the Data Steward role
        And the Data Steward checkbox is <enabledBefore> for user "<email>"
        When the user updates the role of user with email "<email>" to <newRole>
        Then the user with email "<email>" <shouldAfter> have the Data Steward role
        And the Data Steward checkbox should be <enabledAfter> for user "<email>"
        Examples: 
            | email             | enabledBefore | hasBefore    | newRole      | shouldAfter | enabledAfter |
            | wlead@example.com | enabled       | has          | Guest        | should not  | disabled     |
            | wlead@example.com | enabled       | has          | Collaborator | should      | enabled      |
            | admin@example.com | enabled       | doesn't have | Guest        | should not  | disabled     |
            | admin@example.com | enabled       | doesn't have | Collaborator | should not  | enabled      |
            | guest@example.com | disabled      | doesn't have | Collaborator | should not  | enabled      |

    Scenario: Data Steward checkbox should be enabled after changing user role from Guest
        Given the user is on the workspace users page
        And the Data Steward checkbox is disabled for user "guest@example.com"
        When the user updates the role of user with email "guest@example.com" to Collaborator
        Then the Data Steward checkbox should be enabled for user "guest@example.com"