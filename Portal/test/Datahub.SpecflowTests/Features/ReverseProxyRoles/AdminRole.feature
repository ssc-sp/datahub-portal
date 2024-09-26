Feature: Apply Datahub Admin role for proxy access
Validate ContextRequestHeaderTransform when user has Datahub Admin role

    Scenario: Get the role of a user in a workspace
        Given a user context with admin role <dhadmin>
        When ContextRequestHeaderTransform processes the request
        Then the status should be <status_code>

    Examples:
      | dhadmin | status_code |
      | true    | 0         |
      | false   | 403           |
