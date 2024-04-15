Feature: Resource Run Request
    This is the feature that allows the user to request a run of a workspace definition in order to configure their cloud resources.
    
Scenario: User requests a run of a workspace definition without every field in `AppServiceConfiguration`
    Given the user has a workspace definition
    When the user requests a run of the workspace definition
    Then the resource run request should parse the workspace definition without errors