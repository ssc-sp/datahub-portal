Feature: Get Workspace Role Http Context Extension
Adds an extension method to fetch the role of a user in a workspace from the HttpContext

    Scenario: Get the role of a user in a workspace
        Given a user context with a claim of <workspace_role_suffix> and <workspace_acronym>
        When the user requests their role in the workspace <workspace_acronym>
        Then the role should be <role>

    Examples:
      | workspace_role_suffix | workspace_acronym | role         |
      | -admin                | test              | admin        |
      | -workspace-lead       | test              | workspace-lead         |
      | -collaborator         | test              | collaborator |
      | -guest                | test              | guest        |
      | -super-admin          | test              | null         |
      | -workspace-leader     | test              | null         |
      | -write                | test              | null         |
      | -read                 | test              | null         |
      | admin                 | test              | null         |
      | workspace-lead        | test              | null         |
      | collaborator          | test              | null         |
      | guest                 | test              | null         |
      | null                  | test              | null         |