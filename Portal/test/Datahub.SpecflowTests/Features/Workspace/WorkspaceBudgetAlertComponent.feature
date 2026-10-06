@IWebHostEnvironment
@toolbox
Feature: Workspace Budget Alert Component
A component displayed on the workspace dashboard to alert all users of current budget status.

    Scenario: The workspace is under 50% budget it should not render
        Given there is a workspace budget alert component with a percent budget of <percent>
        Then the alert should not be rendered

    Examples:
      | percent |
      | 0       |
      | 49.9    |

    Scenario: The workspace is above 50% it should render the alert
        Given there is a workspace budget alert component with a percent budget of <percent>
        Then the alert should be rendered with <percent> budget and the <role> notice role

    Examples:
      | percent | role    |
      | 50.00   | info    |
      | 50.01   | info    |
      | 74.99   | info    |
      | 75.00   | warning |
      | 75.01   | warning |
      | 89.99   | warning |
      | 90.00   | danger  |
      | 90.01   | danger  |
      | 99.99   | danger  |
      | 100     | danger  |
