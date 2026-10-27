@IWebHostEnvironment
Feature: Accessible support request history
The support request history should use an accessible GC Design System table.

    Scenario: Loading support request history is announced
        Given the support request history is loading
        Then the support request history loading state is announced
        And the support request history table is hidden

    Scenario: Previous support requests use an accessible table
        Given the support request history is loaded
        Then the support request history uses a GCDS table
        And the support request history has an accessible caption
        And the support request history has six localized columns
        And the support request ID is the row header
        And the support request fields use localized plain text
        And the support request history is paginated by ten rows
