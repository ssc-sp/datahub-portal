@IWebHostEnvironment
Feature: Database information table
    The database information table protects workspace connection credentials.

    Scenario: Hide connection information after it has been displayed
        Given a database information table with visible connection information
        When the database connection information is hidden
        Then the database connection placeholders should be displayed
        And the database connection values should not be displayed
