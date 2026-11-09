@IWebHostEnvironment
@toolbox
Feature: CreateWorkspacePage

The create workspace page should show the workspace creation form to appropriate users, 
and validate new workspace parameters including CBR budget.

Scenario: Workspace creation form should be shown to CBR owner, with an error message for other users
	Given authorization as a <cbrOwner> for the workspace creation page
	And a workspace creation page
	Then the workspace creation form <formShould> be shown
	And the error message restricting workspace creation to CBR owners <errorShould> be shown 
	Examples: 
		| cbrOwner      | formShould | errorShould |
		| CBR Owner     | should     | should not  |
		| non-CBR owner | should not | should      |

Scenario: Workspace creation should be initially disabled (nothing entered in the form yet)
	Given authorization as a CBR Owner for the workspace creation page
	And a workspace creation page
	Then the workspace creation form should be invalid
	And the create workspace button should be disabled

Scenario: Workspace creation should be disabled when CBR is not selected
	Given authorization as a CBR Owner for the workspace creation page
	And a workspace creation page
	When the user enters a workspace title in the creation form
	Then the workspace creation form should be invalid
	And the create workspace button should be disabled

Scenario: Workspace creation should be enabled or disabled based on valid budget
	Given authorization as a CBR Owner for the workspace creation page
	And a workspace creation page
	When the user enters a workspace title in the creation form
	And the user selects a CBR from the dropdown in the workspace creation form
	And the user enters a budget of <budget> in the workspace creation form
	Then the workspace creation form should be <valid>
	And the create workspace button should be <createEnabled>
	Examples: 
		| budget    | valid   | createEnabled |
		| 1000.00   | valid   | enabled       |
		| 999999.99 | invalid | disabled      |

Scenario: After creating a workspace, that workspace should be in the database with the parent CBR ID set and the correct budget allocated.
	Given authorization as a CBR Owner for the workspace creation page
	And a workspace creation page
	When the user enters a workspace title in the creation form
	And the user selects a CBR from the dropdown in the workspace creation form
	And the user enters a budget of 2500.00 in the workspace creation form
	And the user clicks the create workspace button
	Then the workspace should be created with the correct parent CBR ID and budget
	And the navigation manager should be redirected to the created workspace
	And the saved feature interests should have 0 characters
Scenario: A malformed budget prevents continuing
    Given authorization as a CBR Owner for the workspace creation page
    And a workspace creation page
    When the user enters a workspace title in the creation form
    And the user selects a CBR from the dropdown in the workspace creation form
    And the user enters a malformed workspace budget
    Then the workspace creation form should be invalid

Scenario Outline: Feature interests enforce the combined storage limit
    Given authorization as a CBR Owner for the workspace creation page
    And a workspace creation page
    When the user enters a workspace title in the creation form
    And the user selects a CBR from the dropdown in the workspace creation form
    And the user enters a budget of 2500.00 in the workspace creation form
    And the user continues to feature interests
    And the user selects all feature interests
    And the user enters <count> characters for other feature interests
    Then feature interest overflow should be <overflow>
    Examples:
        | count | overflow |
        | 86    | hidden   |
        | 87    | shown    |

Scenario: Other text survives deselection and selections save in fixed order
    Given authorization as a CBR Owner for the workspace creation page
    And a workspace creation page
    When the user enters a workspace title in the creation form
    And the user selects a CBR from the dropdown in the workspace creation form
    And the user enters a budget of 2500.00 in the workspace creation form
    And the user continues to feature interests
    And the user selects all feature interests
    And the user enters 86 characters for other feature interests
    And the user deselects Other
    And the user selects all feature interests
    Then the other feature text should contain 86 characters
    When the user clicks the create workspace button
    Then the saved feature interests should have 128 characters

Scenario Outline: Identity validation prevents invalid titles and acronyms
    Given authorization as a CBR Owner for the workspace creation page
    And a workspace creation page
    When the user enters a title of <length> characters and acronym "<acronym>"
    Then the workspace creation form should be invalid
    Examples:
        | length | acronym  |
        | 0      | VALID    |
        | 101    | VALID    |
        | 10     |          |
        | 10     | A-B      |
        | 10     | ABCDEFGH |

Scenario: Valid boundary identity remains uppercase when returning to step one
    Given authorization as a CBR Owner for the workspace creation page
    And a workspace creation page
    When the user enters a title of 100 characters and acronym "abcdefg"
    Then the workspace creation form should be valid
    When the user goes back to workspace identity
    Then the workspace acronym should be "ABCDEFG"
