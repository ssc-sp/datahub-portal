@MockWorkspaceManagement
Feature: ProjectUsageUpdater
	Tests for the ProjectUsageUpdater class, which includes simple storage, budget and costs updating and also the more complicated rollover feature

Scenario: When a project usage is updated and the update does not go into a new fiscal year, a rollover should not be triggered
	Given a project usage update message	
	And an associated project credits record
	And the last update date is in the current fiscal year
	When the project usage is updated
	Then the rollover should not be triggered
	
Scenario: When a project usage is updated and the update goes into a new fiscal year, a rollover should be triggered
	Given a project usage update message
	And an associated project credits record
	And the last update date is in the previous fiscal year
	When the project usage is updated
	Then the rollover should be triggered
	And the project credits should be updated accordingly
	
Scenario: When a project usage is updated and the update goes into a new fiscal year, but we are unable to determine the correct costs, a rollover should not be triggered
	Given a project usage update message
	And an associated project credits record
	And the last update date is in the previous fiscal year
	And the difference between budget spent and cost captured is too large
	When the project usage is updated
	Then the rollover should not be triggered 
	
Scenario: When a project usage is queued, the blob download should work properly
	Given a project usage update message
	When the subscription costs are downloaded
	Then the blob download should work properly
	