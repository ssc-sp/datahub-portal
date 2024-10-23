Feature: Resource Delete
    This is the feature that will update the terraform files to delete the workspace resources
    
    Scenario: The repository service should invoke the DeleteTemplateAsync method when a template is set to be deleted
        Given a repository service with a stubbed CommitTerraformTemplate method
        And a template is set to be deleted
        When the ExecuteResourceRun method is invoked
        Then the DeleteTemplateAsync method should be invoked
        And the CopyTemplateAsync method should not be invoked
        And the result should be a successful RepositoryUpdateEvent
