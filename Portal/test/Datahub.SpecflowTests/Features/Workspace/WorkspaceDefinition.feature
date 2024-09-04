Feature: Workspace Definition
    
    The JSON representation that defines a workspace, including its name, resources, and users among other things.

    @workspace-definition    
    Scenario: Exists resource status
        Given a workspace with a new-project-template resource that exists
        When the workspace definition is requested
        Then the new-project-template resource should be in the created status
        And the workspace definition should have a status of created for new-project-template
        