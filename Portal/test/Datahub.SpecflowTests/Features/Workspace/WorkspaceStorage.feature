@WorkspaceStorage
Feature: WorkspaceStorage
	Tests for the workspace storage service

@ignore
Scenario: Getting the storage capacity should return the correct value
	Given a workspace with a storage account
	And the storage account has a capacity of above 5000000 bytes and below 9000000 bytes
	When the storage capacity is requested
	Then the result should be as expected
	
Scenario: Getting the storage capacity of a non-existing workspace should return an error
	Given a workspace that does not exist
	When the storage capacity is requested
	Then an error should be returned
	
Scenario: Getting the storage capacity of a workspace with no storage account should return 0
	Given a workspace with no storage account
	When the storage capacity is requested
	Then the result should be zero
	
@ignore
Scenario: Updating the storage capacity should return the correct value
	Given a workspace with a storage account
	And the storage account has a capacity of above 5000000 bytes and below 9000000 bytes
	When the workspace's storage capacity is updated
	Then the database should contain the updated capacity

Scenario: Updating the storage capacity of a non-existing workspace should return an error
	Given a workspace that does not exist
	And the storage account has a capacity of above 0 bytes and below 0 bytes
	When the workspace's storage capacity is updated
	Then an error should be returned
	
@ignore
Scenario: Updating the storage capacity of a new workspace should work properly
	Given a new workspace
	And the storage account has a capacity of above 5000000 bytes and below 9000000 bytes
	When the workspace's storage capacity is updated
	Then the database should contain the updated capacity
	
Scenario: Updating the storage capacity of a workspace with no storage account should work properly
	Given a workspace with no storage account
	And the storage account has a capacity of above 0 bytes and below 0 bytes
	When the workspace's storage capacity is updated
	Then the database should contain the updated capacity