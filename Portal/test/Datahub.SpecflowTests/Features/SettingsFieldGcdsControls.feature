@IWebHostEnvironment
Feature: Settings field GCDS controls
    Settings fields should use GCDS controls while preserving their existing behaviour

    Scenario: Workspace budgets use a GCDS input
        Given a disabled GCDS workspace budget control with a value of 125.50
        When the GCDS workspace budget is changed to 200.01
        Then the GCDS workspace budget control preserves its ID and disabled state
        And the workspace budget change is published as 200.01

    Scenario: Prevent auto delete uses GCDS checkboxes
        Given a GCDS prevent auto delete control
        When the GCDS prevent auto delete option is selected
        Then the prevent auto delete change is published as enabled

    Scenario: Settings actions use GCDS buttons
        Given the GCDS CBR and workspace deletion controls
        Then the CBR action is a secondary link to the workspace CBR settings
        And the workspace deletion action is a danger button with the expected target

    Scenario: Toolbox settings use GCDS controls
        Given GCDS PostgreSQL and Databricks configuration controls
        When available PostgreSQL and Databricks tiers are selected
        Then the toolbox configurations contain the selected tiers

    Scenario: Notification preferences use disabled GCDS radios
        Given the GCDS notification preference control
        Then all notifications are selected and the radio group is disabled
