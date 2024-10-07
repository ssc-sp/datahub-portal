@ignore
Feature: Support Request
	The Support Request Page allows users to submit a support request to the support team. The support team will receive the request and respond to the user via email. The user can also view their support request history.

Scenario: Display the previous support requests on load
	Given I have submitted support requests in the past
	When I navigate to the Support Request Page
	Then I should see a list of all my previous support requests and their status

Scenario: Submit a support request
	Given I am on the Support Request Page
	When I fill out the support request form and submit it
	Then I should receive a confirmation message that my request has been submitted
	And the support team should receive an email with my request
	And the support request should be added to Azure DevOps

Scenario: Handle old support requests that use <b> tags in issue parsing
	Given I have an old support request with <b> tags in the description
	Then it should parse the description properly

Scenario: Handle new support requests that use <strong> tags in issue parsing   
	Given I create a new workitem that uses <strong> tags in the description    
	Then it should parse the description properly