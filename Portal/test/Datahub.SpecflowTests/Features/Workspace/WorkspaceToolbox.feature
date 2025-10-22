@toolbox
Feature: WorkspaceToolbox
The workspace toolbox page should allow adding, configuring and removing tools properly while displaying
the correct information in the page.

# RENDER TOOLBOX

    Scenario: User is on the workspace toolbox page
        Given the user is on the workspace toolbox page
        Then the user should see the toolbox

    Scenario: User goes on the workspace toolbox page with invalid credits
        Given the workspace has <invalid> credits
        And the user is on the workspace toolbox page
        Then the user should see the toolbox

    Examples:
      | invalid  |
      | negative |
      | null     |

    Scenario: User goes on the workspace toolbox page with invalid budget
        Given the workspace has <invalid> budget
        And the user is on the workspace toolbox page
        Then the user should see the toolbox

    Examples:
      | invalid  |
      | negative |
      | null     |

    Scenario: User goes on the workspace toolbox page with existing tools in varying states
        Given the workspace has <existing-tool> in <state>
        And the user is on the workspace toolbox page
        Then the user should see the toolbox
        And the <existing-tool> should show the correct <state> in the toolbox

    Examples:
      | existing-tool        | state            |
      | new-project-template | completed        |
      | new-project-template | in-progress      |
      | new-project-template | create-requested |
      | new-project-template | delete-requested |
      | new-project-template | del-in-progress  |
      | new-project-template | failed           |
      | azure-storage-blob   | completed        |
      | azure-storage-blob   | in-progress      |
      | azure-storage-blob   | create-requested |
      | azure-storage-blob   | delete-requested |
      | azure-storage-blob   | del-in-progress  |
      | azure-storage-blob   | failed           |
      | azure-databricks     | completed        |
      | azure-databricks     | in-progress      |
      | azure-databricks     | create-requested |
      | azure-databricks     | delete-requested |
      | azure-databricks     | del-in-progress  |
      | azure-databricks     | failed           |
      | azure-app-service    | completed        |
      | azure-app-service    | in-progress      |
      | azure-app-service    | create-requested |
      | azure-app-service    | delete-requested |
      | azure-app-service    | del-in-progress  |         
      | azure-app-service    | failed           |
      | azure-postgres       | completed        |
      | azure-postgres       | in-progress      |
      | azure-postgres       | create-requested |
      | azure-postgres       | delete-requested |
      | azure-postgres       | del-in-progress  |
      | azure-postgres       | failed           |

      # SELECTION

    Scenario: User sees the appropriate tools in the existing tools
        Given the workspace has the <existing-tool> tool
        And the user is on the workspace toolbox page
        Then <existing-tool> should be in the Existing Tools section
        And <existing-tool> should not be in the Catalog section

    Examples:
      | existing-tool        |
      | new-project-template |
      | azure-storage-blob   |
      | azure-databricks     |
      | azure-app-service    |
      | azure-postgres       |

    Scenario: User sees the appropriate tools in the catalog
        Given the workspace does not have <catalog-tool>
        And the user is on the workspace toolbox page
        Then <catalog-tool> should be in the Catalog section
        And <catalog-tool> should not be in the Existing Tools section

    Examples:
      | catalog-tool         |
      | new-project-template |
      | azure-storage-blob   |
      | azure-databricks     |
      | azure-app-service    |
      | azure-postgres       |

    Scenario: User sees the appropriate tools in the summary after selecting them
        Given the workspace does not have <catalog-tool>
        And the workspace version is <version>
        And the user is on the workspace toolbox page
        When the user clicks the Add button for <catalog-tool>, if it is <available>
        Then <catalog-tool> should be in the Summary section as an added tool
        And there should be an underlying Add transaction for <catalog-tool>
        And the underlying Add transaction should have the correct <configuration-type> if the tool is <configurable>
        When the user clicks the Cancel button for <catalog-tool> in the Add section
        Then <catalog-tool> should not be in the Summary Add section
        And <catalog-tool> should be back in the Catalog section
        And there should be no underlying Add transaction for <catalog-tool>

    Examples:
	  | catalog-tool         | available | configurable | configuration-type                                                         | version |
	  | new-project-template | true      | false        | null                                                                       | v5.0.6  |
	  | azure-storage-blob   | true      | false        | null                                                                       | v5.0.6  |
	  | azure-databricks     | true      | false        | null                                                                       | v5.0.6  |
	  | azure-databricks     | true      | true         | Datahub.Shared.Entities.WorkspaceToolConfiguration.DatabricksConfiguration | v5.2.1  |
	  | azure-app-service    | true      | false        | null                                                                       | v5.0.6  |
	  | azure-postgres       | true      | true         | Datahub.Shared.Entities.WorkspaceToolConfiguration.PostgresConfiguration   | v5.0.6  |
	  | azure-arcgis         | false     | false        | null                                                                       | v5.0.6  |
	  | azure-api            | false     | false        | null                                                                       | v5.0.6  |

    Scenario: User sees the appropriate tools in the summary after removing them
        Given the workspace has the <existing-tool> tool
        And the user is on the workspace toolbox page
        When the user clicks the Remove button for <existing-tool>, if it is <removable>
        Then <existing-tool> should be in the Summary section as a removed tool
        And there should be an underlying Remove transaction for <existing-tool>
        When the user clicks the Cancel button for <existing-tool> in the Remove section
        Then <existing-tool> should not be in the Remove section of the Summary
        And should instead be back in the Existing Tools section
        And there should be no underlying Remove transaction for <existing-tool>

    Examples:
      | existing-tool        | removable |
      | new-project-template | false     |
      | azure-storage-blob   | false     |
      | azure-databricks     | false     |
      | azure-app-service    | true      |
      | azure-postgres       | true      |

    Scenario: User sees the appropriate tools in the summary after configuring them
        Given the workspace has the <existing-tool> tool
        And the workspace version is <version>
        And the <existing-tool> has an <existing-configuration> value for <configuration-parameter> (<db-name> in db)
        And the user is on the workspace toolbox page
        When the user clicks the Configure button for <existing-tool>, if it is <configurable>
        Then <existing-tool> should be in the Summary section as a configured tool
        And there should be an underlying Configure transaction for <existing-tool>
        And the underlying Configure transaction should have the correct <configuration-type> with the correct <configuration-parameter> and <existing-configuration>
        When the user clicks the Cancel button for <existing-tool> in the Configure section
        Then <existing-tool> should not be in the Configure section of the Summary
        And should instead be back in the Existing Tools section
        And there should be no underlying Configure transaction for <existing-tool>

    Examples:
      | existing-tool        | configurable | configuration-type                                                         | configuration-parameter | db-name                 | existing-configuration | version |
      | new-project-template | false        | null                                                                       | null                    | null                    | null                   | v5.0.6  |
      | azure-storage-blob   | false        | null                                                                       | null                    | null                    | null                   | v5.0.6  |
      | azure-databricks     | false        | null                                                                       | null                    | null                    | null                   | v5.0.6  |
      | azure-databricks     | true         | Datahub.Shared.Entities.WorkspaceToolConfiguration.DatabricksConfiguration | general_purpose_cluster | general_purpose_cluster | Standard_D4ds_v5       | v5.2.1  |
      | azure-app-service    | false        | null                                                                       | null                    | null                    | null                   | v5.0.6  |
      | azure-postgres       | true         | Datahub.Shared.Entities.WorkspaceToolConfiguration.PostgresConfiguration   | PSQL_SKU                | postgres_sku            | B_Standard_B1ms        | v5.0.6  |
      | azure-postgres       | true         | Datahub.Shared.Entities.WorkspaceToolConfiguration.PostgresConfiguration   | PSQL_SKU                | postgres_sku            | null                   | v5.0.6  |

    Scenario: User sees the appropriate dependencies in the summary after selecting a tool
        Given the workspace does not have <catalog-tool>
        And the user is on the workspace toolbox page
        When the user clicks the Add button for <catalog-tool>, if it is <available>
        Then <dependency-count> dependencies for <catalog-tool> should be in the Summary section as added tools
        And <catalog-tool> and its <dependency-count> dependencies should not be in the Catalog section as available tools
        When the user clicks the Cancel button for an <example-dependency> of <catalog-tool>
        Then <catalog-tool> and the one canceled dependency should not be in the Summary section
        And should instead be back in the Catalog section
        And any additional dependencies should still be in the Summary section

    Examples:
      | catalog-tool       | available | dependency-count | example-dependency   |
      | azure-storage-blob | true      | 1                | new-project-template |
      | azure-databricks   | true      | 2                | azure-storage-blob   |
      | azure-app-service  | true      | 2                | azure-storage-blob   |
      | azure-postgres     | true      | 1                | new-project-template |

    Scenario: User cannot proceed if there are no tools being added, removed, or configured
        Given the user is on the workspace toolbox page
        And there are no tools being added, removed, or configured
        When the user clicks the Next button
        Then the user should not be able to proceed

    Scenario: User can proceed if there are tools being added, removed or configured, and returning to the selection step
    maintains the selected tools
        Given the workspace has <tool> if it is not being added (<action>)
        Given the user is on the workspace toolbox page
        And they have done an <action> on a <tool>
        When the user clicks the Next button
        And the user clicks the Previous button
        Then the selected tool should still be selected

    Examples:
      | action     | tool                 |
      | added      | new-project-template |
      | removed    | azure-app-service    |
      | configured | azure-postgres       |

    Scenario: Users proceeding after having a configurable tool should go to configuration step, and proceeding with no configurable tools
    should go to the review step
        Given the workspace has <tool> if it is not being added (<action>)
        And the workspace version is <version>
        And the user is on the workspace toolbox page
        And they have done an <action> on a <tool>
        When the user clicks the Next button
        Then they should reach the <expected-step> step
        When the user clicks the Previous button
        Then they should be back on the selection step

    Examples:
      | action     | tool                 | expected-step | version |
      | added      | new-project-template | 2             | v5.0.6  |
      | removed    | azure-app-service    | 2             | v5.0.6  |
      | configured | azure-postgres       | 1             | v5.0.6  |
      | removed    | azure-postgres       | 2             | v5.0.6  |
      | added      | azure-postgres       | 1             | v5.0.6  |
      | added      | azure-databricks     | 2             | v5.0.6  |
      | configured | azure-databricks     | 1             | v5.2.1  |
      | added      | azure-databricks     | 1             | v5.2.1  |

    Scenario: Users click on the various information sheets for each tool
        Given the user is on the workspace toolbox page
        When the user clicks on the information sheet for <tool>
        Then the user should see the information sheet for <tool>

    Examples:
      | tool                 |
      | new-project-template |
      | azure-storage-blob   |
      | azure-databricks     |
      | azure-app-service    |
      | azure-postgres       |
      | azure-arcgis         |
      | azure-api            |

      # CONFIGURE AND REVIEW

    Scenario: Users see the correct configuration form for configurable tools
        Given the workspace has the <configurable-tool> tool
        And the <configurable-tool> has an <existing-configuration> value for <configuration-parameter> (<db-name> in db)
        And the user is on the workspace toolbox page
        When the user clicks the Configure button for <configurable-tool>, if it is <configurable>
        And the user clicks the Next button
        Then the user should see the configuration form for <configurable-tool> with <form-id>
        And the <example-form-input> should have <existing-configuration> as its value
        When the user sets <example-form-input> in the form to <new-configuration>
        Then the underlying Configure transaction should show the correct <existing-configuration> and <new-configuration> values for <configuration-parameter>
        When the user clicks the Next button
        Then the user should see review information for <configurable-tool> with the <existing-configuration> and <new-configuration>

    Examples:
      | configurable-tool | configurable | form-id                       | example-form-input  | existing-configuration | new-configuration | configuration-parameter | db-name      |
      | azure-postgres    | true         | postgres-configuration-form   | postgres-sku-select | B_Standard_B1ms        | B_Standard_B2s    | PSQL_SKU                | postgres_sku |
      | azure-postgres    | true         | postgres-configuration-form   | postgres-sku-select | null                   | B_Standard_B1ms   | PSQL_SKU                | postgres_sku |

    Scenario: Users enable or disable a boolean configuration option for configurable tools
        Given the workspace has the <configurable-tool> tool
        And the workspace version is <version>
        And the <configurable-tool> has a json configuration <config-name>
        And the user is on the workspace toolbox page
	    When the user clicks the Configure button for <configurable-tool>, if it is <configurable>
        And the user clicks the Next button
        Then the user should see the configuration form for <configurable-tool> with <form-id>
        And the <checkbox-id> checkbox should be <checked-before>
        When the user toggles the <checkbox-id> checkbox
        Then the underlying Configure transaction should show the correct <checked-before> and <checked-after> values for <configuration-parameter>
        When the user clicks the Next button
        Then the user should see review information for <configurable-tool> with boolean values <checked-before> and <checked-after>

    Examples: 
	    | configurable-tool | version | config-name           | configurable | form-id                       | checkbox-id                   | checked-before | checked-after | configuration-parameter |
	    | azure-databricks  | v5.2.1  | databricks-default    | true         | databricks-configuration-form | databricks-enable-ml-checkbox | false          | true          | enable_ml_cluster       |
	    | azure-databricks  | v5.2.1  | databricks-ml-default | true         | databricks-configuration-form | databricks-enable-ml-checkbox | true           | false         | enable_ml_cluster       |

      # SUBMISSION

    Scenario: Users see the correct submission process information in the UI, the appropriate changes are applied to the database and the request is correctly sent to the RP
        Given the workspace does not have <catalog-tool>
        And the workspace version is <version>
        And the user is on the workspace toolbox page
        When the user clicks the Add button for <catalog-tool>, if it is <available>
        And the user clicks the Next button
        And the user clicks the Next button again, if it is <configurable>
        Then at this stage, the generated workspace definition should be correct, with the correct <configuration> value
        When the user clicks the Complete button
        Then the user should see the request submission steps
        When the user waits for  2 sec
        Then the user should see the completed submission steps
        And the database should contain the corresponding changes
        And the request should have been properly sent to the resource provisioner
        And the user should be redirected to the workspace dashboard

    Examples:
	    | catalog-tool         | available | configurable | configuration    | version |
	    | new-project-template | true      | false        | null             | v5.0.6  |
	    | azure-storage-blob   | true      | false        | null             | v5.0.6  |
	    | azure-databricks     | true      | false        | null             | v5.0.6  |
	    | azure-app-service    | true      | false        | null             | v5.0.6  |
	    | azure-postgres       | true      | true         | B_Standard_B1ms  | v5.0.6  |
	    | azure-databricks     | true      | true         | Standard_D4ds_v5 | v5.2.1  |