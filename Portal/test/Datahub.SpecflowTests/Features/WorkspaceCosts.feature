@WorkspaceManagement
Feature: WorkspaceCosts
	Testing systems related to getting and updating workspace costs

Scenario: Querying subscription costs should return right amount
	Given a workspace with a subscription cost of at most 100
	When the subscription cost is mock queried
	Then the result should not exceed the expected value
	
Scenario: Querying subscription costs with an invalid date range should return an error
	Given a workspace with a subscription cost of at most 100
	When the subscription cost is mock queried with an invalid date range
	Then an error should be returned
	
Scenario: Grouping costs by source should work properly
	Given a list of mock costs
	When the costs are grouped by source
	Then the result should have the expected count and total of source costs
	
Scenario: Grouping costs by date should work properly
	Given a list of mock costs
	When the costs are grouped by date
	Then the result should have the expected count and total of daily costs
	
Scenario: Filtering the current fiscal year should work properly
	Given a list of mock costs
	When the costs are filtered by the current fiscal year
	Then the result should have the expected count and total of fiscal year costs
	
Scenario: Filtering the last fiscal year should work properly
	Given a list of mock costs
	When the costs are filtered by the last fiscal year
	Then the result should have the expected count and total of last fiscal year costs
	
Scenario: Filtering mock costs for a specific date range should work properly
	Given a list of mock costs
	When the costs are filtered by a specific date range
	Then the result should have the expected count and total of costs in the date range
	
Scenario: Filtering mock costs by workspace should work properly
	Given a list of mock costs
	When the costs are filtered by a specific workspace
	Then the result should have the expected count and total of costs in the workspace

Scenario: Updating workspace costs should work properly
	Given a workspace, with populated costs and credits
	When the costs are updated
	Then the costs table and credits table should be updated accordingly and correctly