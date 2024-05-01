@resource-run-function
Feature: Resource Run Request
This is the feature that allows the user to request a run of a workspace definition in order to configure their cloud resources.

Scenario: User requests a run of a workspace definition with every required field
    Given a workspace definition with every required field
    When a resource run request processes the workspace definition
    Then the resource run request should parse the workspace definition without errors

Scenario: User requests a run of a workspace definition without every required field
    Given a workspace definition without every required field
    When a resource run request processes the workspace definition
    Then the resource run request should parse the workspace definition with errors

Scenario: User requests a run of a workspace definition without every field in `AppServiceConfiguration`
    Given a workspace definition with every required field
    And the workspace app configuration is null
    When a resource run request processes the workspace definition
    Then the resource run request should parse the workspace definition without errors