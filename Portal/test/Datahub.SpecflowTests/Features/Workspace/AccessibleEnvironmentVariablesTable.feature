@IWebHostEnvironment
Feature: Accessible environment variables table
Environment variables should use the GC Design System table and preserve secure administration.

    Scenario: An administrator sees a sortable filtered table with masked values
        Given environment variables are displayed for an administrator
        Then a single GCDS environment variables table is displayed
        And the environment variable values are masked
        When I reveal the environment variable values
        Then available environment variable values are revealed
        And missing environment variable values use the localized fallback

    Scenario: A non-administrator sees a read-only table
        Given environment variables are displayed for a guest
        Then a single GCDS environment variables table is displayed
        And the environment variable values are masked
        And environment variable administration controls are not displayed

    Scenario: Invalid environment variables are rejected
        Given environment variables are displayed for an administrator
        When I try to add an invalid environment variable
        Then the environment variable validation errors are displayed
        And the environment variable is not saved

    Scenario: An administrator can add and edit environment variables
        Given environment variables are displayed for an administrator
        When I add a valid environment variable
        Then the environment variable table is refreshed
        And the restart warning is displayed
        When I edit an existing environment variable
        Then the environment variable key is immutable
        And the environment variable value is saved
