@IWebHostEnvironment
Feature: DatahubAuthView
    Validates the DatahubAuthView for both external and Entra users

    Scenario: Workspace lead accesses a WorkspaceLead AuthLevel DatahubAuthView they have access to
        Given a workspace lead for workspace ABC
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceLead
        When the lead views the component
        Then they should be able to view it

    Scenario: Contributor tries to access a WorkspaceLead AuthLevel DatahubAuthView they should not have access to
        Given a contributor for workspace ABC
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceLead
        When the contributor views the component
        Then they should not be able to view it

    Scenario: External user tries to access a WorkspaceLead AuthLevel DatahubAuthView they should not have access to
        Given an external user for workspace ABC
        And a DatahubAuthView for workspace ABC and AuthLevel WorkspaceLead
        When the external user views the component
        Then they should not be able to view it