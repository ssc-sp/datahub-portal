Feature: A11y
	The portal should be accessible to all users.

@a11y
Scenario: Home page accessible
	Given the user is authenticated
	And the user is on the home page
	Then there should be no accessibility errors