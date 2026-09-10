@IWebHostEnvironment
Feature: Accessible add-user forms
The forms used to add workspace users should be part of the page and usable without a modal dialog.

    Scenario: The Entra user form is an accessible inline region
        Given the Entra add-user form is rendered
        Then the add-user form is not a dialog
        And the Entra add-user form is labelled by its heading
        And the add-user form actions use button semantics
        When the user cancels the add-user form
        Then the add-user form reports that it was cancelled

    Scenario: An Entra user's selected role is submitted
        Given the Entra add-user form is rendered
        When an Entra user is selected
        And the Entra user's role is changed to Collaborator
        And the Entra add-user form is submitted
        Then the Entra user is submitted as a Collaborator

    Scenario: The external user form is an accessible inline wizard
        Given the external add-user form is rendered
        Then the add-user form is not a dialog
        And the external add-user form is labelled by its heading
        And the external add-user form announces step 1 of 4
        And the add-user form actions use button semantics
        When the user cancels the add-user form
        Then the add-user form reports that it was cancelled
