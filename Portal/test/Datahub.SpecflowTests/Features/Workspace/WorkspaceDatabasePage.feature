@ignore
Feature: Workspace Database Page
	The workspace database page should display the connection info and the IP whitelist.

Scenario: Hide the connection info by default
	Given I am visiting the workspace database page
	Then the page should not display the connection info

Scenario: Show the connection info when the user clicks the "Show Connection Info" button
	Given I am visiting the workspace database page
	When I click the "Show Connection Info" button
	Then the page should display the connection info

Scenario: Hide the connection info when the user clicks the "Hide Connection Info" button
	Given I previously clicked the "Show Connection Info" button
	When I click the "Hide Connection Info" button
	Then the page should not display the connection info