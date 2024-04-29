@ignore
Feature: WorkspaceWebAppPage
As a user I want to manage my web application so that I can configure it correctly

    Scenario: Start the web application
        Given I am on the Web Application page
        And the web application is not running
        When I click on the Start button
        Then the web application should start

    Scenario: Stop the web application
        Given I am on the Web Application page
        And the web application is running
        When I click on the Stop button
        Then the web application should stop

    Scenario: Restart the web application
        Given I am on the Web Application page
        And the web application is running
        When I click on the Restart button
        Then the web application should restart

    Scenario: Configure the web application
        Given I am on the Web Application page
        When I click on the Configure button
        Then the configuration dialog should be displayed

    Scenario: Display web application information
        Given I am on the Web Application page
        When I look at the Web Application Info table
        Then I should see all the information about the web application