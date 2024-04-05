@WorkspaceManagement
Feature: ProjectUsageUpdater
	Tests for the ProjectUsageUpdater class, which includes simple storage, budget and costs updating and also the more complicated rollover feature

Scenario: When a project usage is updated and the update does not go into a new fiscal year, a rollover should not be triggered
	Given the first number is 50
	And the second number is 70
	When the two numbers are added
	Then the result should be 120
	
Scenario: When a project usage is updated and the update goes into a new fiscal year, a rollover should be triggered
	Given the first number is 50
	And the second number is 70
	When the two numbers are added
	Then the result should be 120
	And the rollover should be triggered
	
Scenario: When a project usage is updated and the update goes into a new fiscal year, but we are unable to determine the correct a rollover should be triggered