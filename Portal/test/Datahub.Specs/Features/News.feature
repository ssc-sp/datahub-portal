@ignore
Feature: News
	The News should be accessible to all users.

@news
Scenario: News page is accessible
	Given the user is on the News page
	#Then the workspace page should be without accessibility errors

@admnews
Scenario: Admin news page is accessible
	Given the user is on the News page as Admin
	Then the create news button is available
	Given the admin clicks the create button 
	Then the edit news page is loaded

