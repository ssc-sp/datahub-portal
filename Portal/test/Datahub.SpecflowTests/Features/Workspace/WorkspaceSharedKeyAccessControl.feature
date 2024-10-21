@DbContext
Feature: Workspace Shared Key Access Control
  Control to toggle shared key access to a workspaces storage account

  Scenario: It should load the correct storage resource identifer
    Given a workspace in <subscription_id> with <acronym> and a storage resource named <storage_name> and resource group named <resource_group>
    When I load the storage resource id for acronym <acronym>
    Then the result should equal <storage_resource_id>
    
  Examples: 
    | subscription_id | acronym | storage_name | resource_group | storage_resource_id |
    | 1234-5678-9012-3456 | test | teststorage | testresourcegroup | /subscriptions/1234-5678-9012-3456/resourceGroups/testresourcegroup/providers/Microsoft.Storage/storageAccounts/teststorage |    