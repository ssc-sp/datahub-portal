@AccountSettingsPage
@toolbox
@IWebHostEnvironment
Feature: AccountSettingsPage
  The account settings page should render the viewed user profile and sections correctly.

  Scenario: User is on the account settings page
    Given the user is authenticated with <which> login
    And the user is on the account settings page
    Then the user should see their display name and email
    Examples:
        | which    |
        | GOC      |
        | external |
