@WorkspaceManagement
Feature: WorkspaceBudget
	Tests for the workspace budget management service

Scenario: Querying budget amount for a workspace should return the right amount
	Given a workspace with a budget identifier of "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh-static-test-rg/providers/Microsoft.Consumption/budgets/fsdh-test-budget"
	And the budget amount is $1000
	When the budget amount is queried for the workspace
	Then the result should be the expected amount
	
Scenario: Setting a budget amount for a workspace should update the budget amount
	Given a workspace with a budget identifier of "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh-static-test-rg/providers/Microsoft.Consumption/budgets/fsdh-test-budget"
	And the budget amount is $1000
	When the budget amount is set to $500 for the workspace
	Then the result should be the expected amount
	
Scenario: Querying budget spent for a workspace should return the right amount
	Given a workspace with a budget identifier of "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh-static-test-rg/providers/Microsoft.Consumption/budgets/fsdh-test-budget"
	And the budget spent is less than $10
	When the budget spent is queried for the workspace
	Then the result should be less than the expected amount
	
Scenario: Updating budget spent for a workspace should update the budget spent
	Given a workspace with a budget identifier of "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh-static-test-rg/providers/Microsoft.Consumption/budgets/fsdh-test-budget"
	And an existing project credit record
	When the budget is updated for that workspace
	Then project credit record should be updated
	
	
		