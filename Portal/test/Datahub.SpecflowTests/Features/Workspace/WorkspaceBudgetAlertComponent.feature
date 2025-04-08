@IWebHostEnvironment
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
        Then the alert should be rendered with <percent> budget and <class> class

    Examples:
      | percent | class                      |
      | 50.00   | mud-alert-outlined-info    |
      | 50.01   | mud-alert-outlined-info    |
      | 74.99   | mud-alert-outlined-info    |
      | 75.00   | mud-alert-outlined-warning |
      | 75.01   | mud-alert-outlined-warning |
      | 89.99   | mud-alert-outlined-warning |
      | 90.00   | mud-alert-outlined-error   |
      | 90.01   | mud-alert-outlined-error   |
      | 99.99   | mud-alert-outlined-error   |
      | 100     | mud-alert-outlined-error   |