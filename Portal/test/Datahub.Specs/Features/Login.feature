Feature: Login
	The login page should be accessible to everyone

@login
@ignore
Scenario: Login page accessible
	Given an unregistered user navigates to the login page
	Then the login page is accessible