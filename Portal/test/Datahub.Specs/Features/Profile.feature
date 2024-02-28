@ignore
Feature: Profile
	The Profile should be accessible to all users.

@prof
Scenario: Profile page is accessible
	Given the user navigates to the profile page
	Then the profile page is reached
	
Scenario: Profile settings page is accessible
	Given the user is on the profile page
	And click on the profile settings button
	Then the profile settings page is reached
	
Scenario: Profile appearance settings page is accessible
	Given the user is on the profile page
	And click on the profile settings button
	And click on the appearance settings button
	Then the profile appearance settings page is reached
	
Scenario: Profile notification settings page is accessible
	Given the user is on the profile page
	And click on the profile settings button
	And click on the notification settings button
	Then the profile notification settings page is reached
	
Scenario: Profile achievements page is accessible
	Given the user is on the profile page
	And click on the view all achievements button
	Then the profile achievements page is reached