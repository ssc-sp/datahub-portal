Feature: Terraform Template Readable
This is to create a user friendly string based on the Terraform template value

    Scenario: Terraform template string can be user friendly
        Given I have a Terraform template called "<template>"
        When I convert the Terraform template to a readable string in "<language>"
        Then the readable string should be "<readable>"

    Examples:
      | template             | readable                                    | language |
      | new-project-template | Workspace Resource Group                    | en       |
      | azure-storage-blob   | Azure Storage Blob                          | en       |
      | azure-databricks     | Azure Databricks                            | en       |
      | azure-app-service    | Azure App Service                           | en       |
      | azure-postgres       | Azure Postgres                              | en       |
      | new-project-template | Groupe de ressources de l'espace de travail | fr       |
      | azure-storage-blob   | Blob de stockage Azure                      | fr       |
      | azure-databricks     | Azure Databricks                            | fr       |
      | azure-app-service    | Service App Azure                           | fr       |
      | azure-postgres       | Azure Postgres                              | fr       |