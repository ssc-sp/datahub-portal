@WorkspaceManagement
Feature: WorkspaceStorage
	Tests for the workspace storage service

Scenario: Getting the storage capacity should return the correct value
	Given a workspace with a storage account id of "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh-static-test-rg/providers/Microsoft.Storage/storageAccounts/fsdhteststorageaccount"
	And the storage account has a capacity of above 0 bytes and below 9000000 bytes
	When the storage capacity is requested
	Then the result should be as expected
	
Scenario: Updating the storage capacity should return the correct value
	Given a workspace with a storage account id of "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh-static-test-rg/providers/Microsoft.Storage/storageAccounts/fsdhteststorageaccount"
	And a project exists in the database for the workspace
	And the storage account has a capacity of above 5000000 bytes and below 9000000 bytes
	When the workspace's storage capacity is updated
	Then the database should contain the updated capacity