@IWebHostEnvironment
Feature: Accessible web app configuration form
The web app configuration should be part of the page and use GC Design System controls.

    Scenario: The form is an accessible inline region
        Given the web app configuration form has an existing private configuration
        Then the web app configuration is a labelled inline region
        And the web app configuration uses GCDS controls
        And the existing web app token is masked

    Scenario: Required custom configuration is validated
        Given the web app configuration form is empty
        When I select a private web app repository
        And I save the web app configuration
        Then the web app configuration is not submitted
        And the required web app configuration errors are displayed

    Scenario: The existing private token is retained
        Given the web app configuration form has an existing private configuration
        When I save the web app configuration
        Then the existing private web app configuration is submitted
        And the original web app configuration is unchanged

    Scenario: Embedded repository credentials are extracted
        Given the web app configuration form is empty
        When I enter a web app repository URL containing a credential
        And I enter the web app compose path
        And I save the web app configuration
        Then the sanitized private web app configuration is submitted

    Scenario: Cancelling does not change the configuration
        Given the web app configuration form has an existing private configuration
        When I edit and cancel the web app configuration
        Then the web app configuration reports cancellation
        And the original web app configuration is unchanged
