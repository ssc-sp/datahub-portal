@ignore
Feature: Res
	The resource page should be accessible to all users.

@res
Scenario: The resource page is accessible
	Given the user is on the resource page
	#Then the resource page should be without accessibility errors