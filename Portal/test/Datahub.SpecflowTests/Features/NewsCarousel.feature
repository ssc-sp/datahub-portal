Feature: News Carousel
	The news carousel should display the latest news items from the news feed.


Scenario: Show padding on the carousel when it does not start with an image
	Given the news carousel is not starting with an image
	When the carousel is displayed
	Then the carousel should have padding on the x-axis
	And the carousel should have padding on the y-axis
	
