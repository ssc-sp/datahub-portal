Feature: Language

A short summary of the feature

@Translation
Scenario: Translation
	Given I am an authenticated user navigating on the home page
	When I click on my profile dropdown
	And click "Français"
	Then The page properly switches languages to french
