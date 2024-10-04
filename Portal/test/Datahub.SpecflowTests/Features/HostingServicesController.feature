Feature: Hosting Services Endpoint
	The hosting services controller exposes an endpoint that echoes back the message that was sent to it. This feature file contains the tests for the echo endpoint.

	Scenario: Echo endpoint should return the same message that was sent
		Given I have a Hello World test message
		When I send a POST request to the echo endpoint
		Then the response should have a Hello, World! message