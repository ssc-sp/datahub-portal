@WorkspaceCosts
Feature: WorkspaceCosts
	Testing systems related to getting and updating workspace costs
	
Scenario: Query daily costs for a workspace works correctly
	Given a workspace with known costs
	When I query the daily costs for the workspace
	Then I should receive the correct daily costs
	
Scenario: Query daily costs for a subscription works correctly
	Given a subscription with known costs
	When I query the daily costs for the subscription
	Then I should receive the correct daily costs
	
Scenario: Query total costs for a workspace works correctly
	Given a workspace with known costs
	When I query the total costs for the workspace
	Then I should receive the correct total costs

Scenario: Query total costs for a subscription works correctly
	Given a subscription with known costs
	When I query the total costs for the subscription
	Then I should receive the correct total costs
	
Scenario: Querying costs for incorrect workspace acronym should not work
	Given a non-existent workspace acronym
	When I query the daily costs for the workspace
	Then I should receive an error
	
Scenario: Querying costs with invalid date range should not work
	Given a workspace with known costs
	When I query the costs for the workspace with an invalid date range
	Then I should receive an error
	
@LargeResponse
Scenario: Querying costs with a large response does not cause duplicate costs
	Given a workspace with known costs
    When I query the costs for the workspace with a large response
    Then there should be no duplicate costs
	
Scenario: Update costs for old workspace works correctly
	Given a workspace with existing costs and credits
	When I update the costs for the workspace
	Then the costs and credits records should reflect the new costs
	
Scenario: Update costs for new workspace works correctly
	Given a workspace with no existing costs or credits
	When I update the costs for the workspace
	Then the costs and credits records should reflect the new costs
	
Scenario: Update costs replaces incorrect costs
	Given a workspace with existing costs and credits
	When I update the costs with existing cost but different enough values
	Then the relevant cost should be updated
	And the costs and credits records should reflect the new costs
	
Scenario: Update costs does not replace correct costs
	Given a workspace with existing costs and credits
	When I update the costs with existing cost but similar values
	Then the relevant cost should not be updated
	And the costs and credits records should reflect the new costs
	
Scenario: Updating costs with no new costs should not make any changes
	Given a workspace with existing costs and credits
	When I update the costs with no new costs
	Then the costs and credits records should not change
	
Scenario: Updating costs with invalid workspace acronym should not work
	Given a non-existent workspace acronym
	When I update the costs for the non-existent workspace
	Then I should receive an error
	
Scenario: Updating costs returns correct values when rollover is needed
	Given a workspace with existing costs and credits that need a rollover
	When I update the costs for the workspace
	Then a rollover needed is returned
	And the correct amount of costs is given
	
Scenario: No refresh needed for matching totals
	Given a workspace with total costs that match Azure totals
	When I verify if a refresh is needed
	Then no refresh should be needed
	
Scenario: Refresh needed for mismatching totals
	Given a workspace with total costs that do not match Azure totals
	When I verify if a refresh is needed
	Then a refresh should be needed
	
Scenario: Refresh works properly
	Given a workspace with no existing costs or credits
	When I refresh the costs for the workspace
	Then there should be costs for the whole fiscal year and the credits should be updated