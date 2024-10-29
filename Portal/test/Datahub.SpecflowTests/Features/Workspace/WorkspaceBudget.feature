@ignore
@WorkspaceBudget
Feature: WorkspaceBudget
	Tests for the workspace budget management service

Scenario: Querying budget amount for a workspace should return the right amount
	Given a workspace with a budget
	And the budget amount is $1000
	When the budget amount is queried for the workspace
	Then the result should be the expected amount
	
Scenario: Querying budget amount for a non-existent workspace should return an error
	Given a non-existent workspace
	When the budget amount is queried
	Then the result should be an error
	
Scenario: Setting a budget amount for a workspace should update the budget amount
	Given a workspace with a budget
	And the budget amount is $1000
	When the budget amount is set to $500 for the workspace
	Then the result should be the expected amount
	
Scenario: Setting a budget amount for a non-existent workspace should return an error
	Given a non-existent workspace
	When the budget amount is set
	Then the result should be an error
	
Scenario: Querying budget spent for a workspace should return the right amount
	Given a workspace with a budget
	And the budget spent is less than $10
	When the budget spent is queried for the workspace
	Then the result should be less than the expected amount
	
Scenario: Querying budget spent for a non-existent workspace should return an error
	Given a non-existent workspace
	When the budget spent is queried
	Then the result should be an error
	
Scenario: Updating budget spent for a workspace should update the budget spent
	Given a workspace with a budget
	And an existing project credit record
	When the budget is updated for that workspace
	Then project credit record should be updated
	
Scenario: Updating budget spent for a non-existent workspace should return an error
	Given a non-existent workspace
	When the budget is updated
	Then the result should be an error
	
	
		