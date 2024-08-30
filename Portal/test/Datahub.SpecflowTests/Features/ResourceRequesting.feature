Feature: Resource Requesting
Tests around the workspace resource requesting and its functionality for the user

    @queue
    Scenario: Request a resource to be provisioned for a workspace
        Given a workspace without a <resource_name> resource
        And a current user
        When a current user requests to run a <resource_name> for a workspace
        Then there should be a workspace <resource_name> resource created
        And there should be <number_of_messages> messages in resource messaging queue

    Examples:
      | resource_name      | number_of_messages |
      | NewProjectTemplate | 1                  |
      | AzureAppService    | 1                  |
      | AzureDatabricks    | 1                  |
      | AzureStorageBlob   | 1                  |
      | AzurePostgres      | 1                  |

    @queue
    Scenario: Request to run an update resource for a workspace
        Given a workspace without a <resource_name> resource
        And a current user
        When a current user requests to run a <resource_name> for a workspace
        Then there should not be a workspace <resource_name> resource created
        And there should be <number_of_messages> messages in resource messaging queue

    Examples:
      | resource_name  | number_of_messages |
      | VariableUpdate | 1                  |

    @queue
    Scenario: Request to run an unreleased workspace resource
        Given a workspace without a <resource_name> resource
        And a current user
        When a current user requests to run a <resource_name> for a workspace
        Then there should not be a workspace <resource_name> resource created
        And there should be 0 messages in resource messaging queue

    Examples:
      | resource_name       |
      | AzureArcGis         |
      | AzureVirtualMachine |
      | ContactUs           |