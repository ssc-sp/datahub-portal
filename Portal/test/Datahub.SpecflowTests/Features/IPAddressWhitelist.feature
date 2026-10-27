@IWebHostEnvironment
@toolbox
Feature: IP Address Whitelist
Allows a user to add an IP address to the whitelist of a cloud resource
    
    Scenario: The subscription id fetching the postgres database should use the workspace subscription id
        Given a workspace and an azure subscription id for an DatabaseIpWhitelistTable component
        When the workspace subscription id is retrieved
        Then the workspace subscription id should be the same as the azure subscription id

    Scenario: Firewall rules use an accessible GCDS table
        Given a rendered DatabaseIpWhitelistTable component
        When a sample firewall rule is displayed
        Then the firewall rules use a GCDS table
        And the firewall table has localized accessible columns
        And the firewall table supports filtering and sorting
        And the firewall rule values are displayed as plain text

    Scenario: Firewall rule actions require a selection
        Given a rendered DatabaseIpWhitelistTable component
        When a sample firewall rule is displayed
        Then the firewall rule actions are disabled
        When the sample firewall rule is selected
        Then the firewall rule actions are enabled
        When the selected firewall rule is edited
        Then the firewall rule editor is an inline GCDS form

    Scenario: Add firewall rules with an accessible inline form
        Given a rendered DatabaseIpWhitelistTable component
        When the add firewall rule form is opened
        Then the add firewall rule editor is an inline GCDS form

#    Scenario: Add my IP address to the whitelist
#        Given a current user with a name of <name>
#        And an IP address of <ip_address>
#        When the user adds their current IP to the whitelist
#        Then the <ip_address> should be in the whitelist
#        And the <name> associated with <ip_address>
#
#    Examples:
#      | name        | ip_address      |
#      | my-firewall | 123.123.123.123 |
#
#    Scenario: Names should conform to Azure Firewall naming conventions
#        Given a firewall <name>
#        When the name is cleaned up for compliance
#        Then the <name> should be <cleaned_name>
#
#    Examples:
#      | name        | cleaned_name |
#      | my-firewall | my-firewall  |
#      | my_firewall | my-firewall  |
#      | my.firewall | my-firewall  |
#      | my firewall | my-firewall  |
#
#    Scenario: Add my existing IP address to the whitelist
#        Given a current user with a name of <name>
#        And an IP address of <ip_address>
#        And the <ip_address> is already in the whitelist with <existing_name>
#        When the user adds their current IP to the whitelist
#        Then the <ip_address> should be in the whitelist with <name>
#
#    Examples:
#      | name | ip_address      | existing_name |
#      | test | 123.123.123.123 | my-firewall   |
