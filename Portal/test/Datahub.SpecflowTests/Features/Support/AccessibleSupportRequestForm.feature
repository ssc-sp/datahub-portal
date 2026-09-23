@IWebHostEnvironment
Feature: Accessible support request form
The support request wizard should use accessible GC Design System form controls.

    Scenario: The support request form is an accessible wizard
        Given the support request form is rendered
        Then the support request form is labelled by its heading
        And the support request stepper is hidden
        And the support request form actions use button semantics

    Scenario: Required details are validated
        Given the support request form is rendered
        When I advance to the support request details
        Then the support request form announces step 1 of 5
        When I try to review the support request without a description
        Then the support request details remain visible
        And the description displays a required error

    Scenario: Support request selections are reviewed and submitted
        Given the support request form is rendered
        When I advance to the support request details
        Then the support request form announces step 1 of 5
        When I complete the support request details
        And I advance to the support request review
        Then the support request review contains my selections
        When I return to the support request details
        Then my support request details are preserved
        When I advance to the support request review
        And I submit the support request
        Then the support request data is reported
        And the support request form is reset
