@IWebHostEnvironment
Feature: News Carousel
	The news carousel should display the latest news items from the news feed.

@ignore
Scenario: Show padding on the carousel when it starts with an image
	Given there is a news carousel component with an image
	Then the carousel should not have padding on the x-axis
	And the carousel should not have padding on the y-axis

@ignore
Scenario: Show padding on the carousel when it does not start with an image
	Given there is a news carousel component without an image
	Then the carousel should have padding on the x-axis
	And the carousel should have padding on the y-axis
	
	
	
