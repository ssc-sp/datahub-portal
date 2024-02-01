Feature: IP Address Whitelist
	Allows a user to add an IP address to the whitelist of a cloud resource

Scenario: Add my IP address to the whitelist
	Given a current user with a name of <name>
	And an IP address of <ip_address>
	When the user adds their current IP to the whitelist
	Then the <ip_address> should be in the whitelist with <name>
	
	Examples:
		| ip_address  |
		| 192.168.2.1 |


Scenario: Conform to Azure Firewall naming conventions
	Given a current user
	And a firewall <name>
	When the user goes to create or update a firewall <name> to the whitelist
	Then the <name> should be cleaned up
	
	
