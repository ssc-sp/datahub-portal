@IWebHostEnvironment
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