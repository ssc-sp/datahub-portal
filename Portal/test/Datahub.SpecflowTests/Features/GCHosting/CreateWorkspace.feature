Feature: API Create Workspace
Tests around the workspace resource requesting and its functionality for the user

@queue
Scenario: GC Hosting creates a workspace using the API
	Given a request with <json_body>
	Then the response should have a <response_code> status code

Examples:
	| json_body                            | response_code |
	| 13acc5c5-6971-477f-bd45-25bcf95da884 | 1             |
	| 287376a8-678d-4c3e-869a-ff61c33ed41d | 1             |
	| AzureDatabricks                      | 1             |
	| AzureStorageBlob                     | 1             |
	| AzurePostgres                        | 1             |