Feature: Hosting Services Integration
	To integrate with hosting services, we have an endpoint that currently echoes the message that was sent to it. We also have a test page that can be used to test the endpoint.

	Scenario: Echo endpoint should return the same message that was sent
		Given I have a Hello World test message
		When I send a POST request to the echo endpoint
		Then the response should have a Hello, World! message

	@ignore
	Scenario: Test page should create a simulated Hosting Services call to the echo endpoint
		Given I complete the test page form
		When I submit the form
		Then the page should display the successful response from the endpoint