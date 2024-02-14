Feature: Home
	The portal home page should be accessible for logged users

@home
@ignore
Scenario: Home page accessible
	Given the user is authenticated
	Then the user is on the home page
	# Then there should be no accessibility errors