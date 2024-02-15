@ignore
Feature: Tools
	The tools page should be accessible for admin logged users

@admintools
Scenario: Tools page for admins is accessible
	Given an admin is in the tools page
	And the sidebar contains the tools button
	Then a set of tools are available
	# Given the admin access is switch off
	# Then a set of tools are not available
