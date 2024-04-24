@RequiringResourceMessaging
Feature: Workspace Subscription Targeting
	This feature is to ensure that the workspace subscription targeting is working as expected. The limitations imposed by cloud providers are to be considered in the context of deploying the workspace.

Scenario: A workspace has a subscription id
	Given a workspace that has a subscription id
	When the workspace definition is requested
	Then the subscription id is included in the workspace definition
	
Scenario: A newly created workspace has a subscription id
	Given there is a subscription available
	And a new workspace is created
	When the workspace definition is requested
	Then the subscription id is included in the workspace definition
	
