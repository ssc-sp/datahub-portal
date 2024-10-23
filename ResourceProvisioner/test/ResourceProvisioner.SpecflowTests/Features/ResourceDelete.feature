Feature: Resource Delete
This is the feature that will update the terraform files to delete the workspace resources

    Scenario: The repository service should invoke the DeleteTemplateAsync method when a template is set to be deleted
        Given a repository service with a stubbed CommitTerraformTemplate method
        And a template is set to be deleted
        When the ExecuteResourceRun method is invoked
        Then the DeleteTemplateAsync method should be invoked
        And the CopyTemplateAsync method should not be invoked
        And the result should be a successful RepositoryUpdateEvent

    Scenario: The terraform service should delete all the files that belong to the workspace resource
        Given a terraform service with a stubbed WriteDeletedFile method
        And a template name of <templateName>
        And a terraform workspace with acronym "ACRO"
        And a few files starting with the template name exist in the project path
        When the DeleteTemplateAsync method is invoked
        Then the files starting with the template name should be deleted
        And the WriteDeletedFile method should be invoked

    Examples:
      | templateName         |
      | azure-storage-blob   |
      | azure-databricks     |
      | azure-app-service    |
      | azure-postgres       |
      | new-project-template |

    Scenario: The terraform service should write a file with the deleted template name for the output status
        Given a terraform service
        And a template name of <templateName>
        And a project path of "testpath"
        When the WriteDeletedFile method is invoked
        Then a .tf file with the template name should be written to the project path
        And the file contents should have the module status variable of <templateName> and the value of "deleted"

    Examples:
      | templateName         |
      | azure-storage-blob   |
      | azure-databricks     |
      | azure-app-service    |
      | azure-postgres       |
      | new-project-template |