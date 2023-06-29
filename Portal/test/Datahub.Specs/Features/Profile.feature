Feature: Profile
	The Profile should be accessible to all users.

@prof
Scenario: Profile page is accessible
	Given the user is on the profile page
	And click on the profile settings button
	Then the profile settings page is reached