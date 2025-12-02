@IWebHostEnvironment
Feature: API Create Workspace
Tests around the workspace resource requesting and its functionality for the user

@queue
Scenario: GC Hosting creates a workspace using the API
	Given a request with <json_body>
	Then the response should have a <response_code> status code and <response_json> json
	And the created project metadata <should> be filled in

Examples:
	| json_body                            | response_code | should     | response_json                                                |
	| 13acc5c5-6971-477f-bd45-25bcf95da884 | 400           | should not | ''                                                           |
	| 287376a8-678d-4c3e-869a-ff61c33ed41d | 400           | should not | ''                                                           |
	| 78e092fc-8da2-42bc-acb6-c5be1955062a | 400           | should not | ''                                                           |
	| a8f21f62-ee89-4ca0-982f-cdbee50a0402 | 400           | should not | ''                                                           |
	| c21b1c50-fdfa-4267-b578-e9442eec5412 | 400           | should not | ''                                                           |
	| d6358d5a-46de-436f-b1b5-91ee824be02c | 400           | should not | ''                                                           |
	| valid1                               | 200           | should     | '{"Acronym":"TEST","ResourceGroup":"fsdh_proj_test_dev_rg"}' |
