@ignore
Feature: Learn Page
	The Learn Page should display a list of available documents. 

Scenario: Display the featured documents on load
	Given I am on the Learn Page
	When the page loads
	Then I should see a list of featured documents

Scenario: Display a document when clicked
	Given I am on the Learn Page
	When I click on Read More for a document
	Then I should see the document

Scenario: Display media when it is used in a document
	Given I am on the Learn Page
	When I access on a document
	Then I should see the media used in the document loaded from Blob Storage