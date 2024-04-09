@infra-repository
Feature: Azure Devops Git with Access Token
	Access Azure Devops private git repository using access token on a service principal

Scenario: Service Principal can get access token
	Given service principal credentials are available
	When it requests an access token
	Then it should get a valid access token
	
Scenario: Service Principal can access Azure Devops Git
	Given service principal credentials are available
	And the cloned repository does not exist
	When it tries to clone Azure Devops Git repository
	Then the cloned repository should exist
