Feature: Azure Devops Git with Access Token
	Access Azure Devops private git repository using access token on a service principal

@infra-sp
Scenario: Service Principal can get access token
	Given a service principal
	When it requests an access token
	Then it should get a valid access token
