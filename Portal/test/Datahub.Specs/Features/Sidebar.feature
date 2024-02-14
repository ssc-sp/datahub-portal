@ignore
Feature: Sidebar
	The Sidebar should match the role

@sidebar
Scenario: Sidebar panel matching role
	Given the user is on any page
	Then the sidebar matches pixel by pixel