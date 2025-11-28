@IWebHostEnvironment
Feature: Support Access to Workspaces
	To protect the privacy of users, the system should only allow the admin team access to workspaces on request. This will allow the admin team to provide support to users who are experiencing issues with their workspace. The admin team should be able to access the workspace for a limited period of time to diagnose and resolve the issue. This will help to ensure that the admin team only accesses workspaces when necessary and that users are aware of when their workspace is being accessed.

	Scenario: Admin team should not have access to workspace by default
		Given the user has created a workspace
		When the user has not requested support for the workspace
		Then the admin team should not have access to the workspace

	Scenario: User request support for a workspace
		Given the user has created a workspace
		When the user requests support for the workspace
		Then the admin team should have access to the workspace

	Scenario: User revokes access to workspace
		Given the user has requested support for a workspace
		When the user revokes access to the workspace
		Then the admin team should not have access to the workspace