@AccountPage
@toolbox
@IWebHostEnvironment
Feature: AccountPage
  The account page should render the viewed user profile and sections correctly.

  Scenario: User is on the account page
    Given the user is authenticated with <which> login
    And the user is on the account page
    Then the user should see their display name and email
    And the user should see their <which> login provider chip
    And the user should see their project
    Examples: 
        | which    |
        | GOC      |
        | external |
