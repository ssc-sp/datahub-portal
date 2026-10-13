@IWebHostEnvironment
Feature: Accessible web application control panel
Web application status and actions should be grouped separately from the page title.

    Scenario: A running configured application has GCDS controls
        Given a running configured web application is displayed
        Then the web application title contains no controls
        And the web application control panel displays the running status
        And the control panel uses the expected GCDS buttons
        When I open and cancel the web application configuration
        Then the control panel heading is the configuration return focus target

    Scenario: A stopped configured application offers start
        Given a stopped configured web application is displayed
        Then the web application control panel displays the stopped status
        And the control panel offers the GCDS start action

    Scenario: An unconfigured application offers only configuration
        Given an unconfigured provisioned web application is displayed
        Then the web application control panel only offers configuration

    Scenario: An unprovisioned application has no control panel
        Given an unprovisioned web application is displayed
        Then the web application control panel is not displayed
