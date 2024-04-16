@RequiringResourceMessaging
Feature: Workspace Subscription Targeting
	This feature is to ensure that the workspace subscription targeting is working as expected. The limitations imposed by cloud providers are to be considered in the context of deploying the workspace.

Scenario: A workspace has a subscription id
	Given a workspace that has a subscription id
	When the workspace definition is requested
	Then the subscription id is included in the workspace definition
	
@ignore
Scenario: A new workspace needs to get the next subscription id
	Given a new workspace is created
	When the workspace definition is requested
	Then the subscription id is included in the workspace definition
	And the subscription id is the next available subscription id
	
