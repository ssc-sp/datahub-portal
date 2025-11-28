@IWebHostEnvironment
@toolbox
Feature: Workspace CBR Budget Management Page
	The workspace CBR budget management page should show budget allocation settings for
	workspaces in the same CBR as the current workspace (including this workspace).


Scenario: The submit button should be initially disabled on the CBR settings page
	Given authorization as a CBR Owner for the CBR budget management page
	And a workspace CBR budget management page
	Then the submit button should be disabled

Scenario: The submit button should be enabled or disabled based on validating the CBR budget
	Given authorization as a CBR Owner for the CBR budget management page
	And a workspace CBR budget management page
	When the CBR budget row for the test workspace is edited
	And the CBR budget for the test workspace is changed to <budgetAmount>
	And the edited CBR budget for the test workspace is committed
	Then the submit button should be <submitEnabled>
	And the CBR budget validation error <errorShould> be shown

	Examples: 
	| budgetAmount | submitEnabled | errorShould |
	| 2000.00      | enabled       | should not  |
	| 9999999.99   | disabled      | should      |
