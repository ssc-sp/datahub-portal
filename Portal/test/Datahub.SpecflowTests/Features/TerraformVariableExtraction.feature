@terraform
Feature: Terraform Variable Extraction
Static class to extract specific variables from the output json of a terraform resource. Simply pass in the project with its resources
and call the method to extract the variable you want. The method will return either a null or the value of the variable you want.

    Background:
        Given a datahub project with the following resources
          | Resource Type                  | Json Content                                                                                                                                                                                                                                                                                                                                                                                                                               |
          | terraform:azure-databricks     | { "workspace_id": "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh_proj_die6_dev_rg/providers/Microsoft.Databricks/workspaces/fsdh-dbk-die6-dev", "workspace_url": "adb-4970741514517449.9.azuredatabricks.net", "workspace_name": "fsdh-dbk-die6-dev" }                                                                                                                                                           |
          | terraform:new-project-template | {  }                                                                                                                                                                                                                                                                                                                                                                                                                                       |
          | terraform:azure-storage-blob   | { "storage_account": "fsdhprojyt31dev", "container": "datahub", "storage_type": "blob", "resource_group_name": "fsdh_proj_yt31_dev_rg" }                                                                                                                                                                                                                                                                                                   |
          | terraform:azure-postgres       | { "postgres_id": "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh_proj_dr1_dev_rg/providers/Microsoft.DBforPostgreSQL/flexibleServers/fsdh-dr1-psql-dev", "postgres_dns": "fsdh-dr1-psql-dev.postgres.database.azure.com", "postgres_db_name": "fsdh", "postgres_secret_name_admin": "datahub-psql-admin", "postgres_secret_name_password": "datahub-psql-password", "postgres_server_name": "fsdh-dr1-psql-dev" } |

    Scenario: Extracting a Databricks Workspace Id
        When I call the method to extract the databricks workspace id
        Then I should get the following value
          | Name                  | Value                                                                                                                                                |
          | databricksWorkspaceId | /subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh_proj_die6_dev_rg/providers/Microsoft.Databricks/workspaces/fsdh-dbk-die6-dev |

    Scenario: Extracting a Databricks Workspace Url
        When I call the method to extract the databricks workspace url
        Then I should get the following value
          | Name          | Value                                      |
          | databricksUrl | https://adb-4970741514517449.9.azuredatabricks.net |

    Scenario: Extracting a Postgres Host
        When I call the method to extract the postgres host
        Then I should get the following value
          | Name         | Value                                         |
          | postgresHost | fsdh-dr1-psql-dev.postgres.database.azure.com |

    Scenario: Extracting a Postgres Database Name
        When I call the method to extract the postgres database name
        Then I should get the following value
          | Name                 | Value |
          | postgresDatabaseName | fsdh  |

    Scenario: Extracting a Postgres Admin Secret Name
        When I call the method to extract the postgres username secret name
        Then I should get the following value
        | Name                       | Value |
        | postgresUsernameSecretName | datahub-psql-admin   |

    Scenario: Extracting a Postgres Password Secret Name
        When I call the method to extract the postgres password secret name
        Then I should get the following value
        | Name                       | Value |
        | postgresPasswordSecretName | datahub-psql-password |
