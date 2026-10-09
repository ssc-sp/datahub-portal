@IWebHostEnvironment
Feature: Accessible add-user forms
The forms used to add workspace users should be part of the page and usable without a modal dialog.

    Scenario: The Entra user form is an accessible inline region
        Given the Entra add-user form is rendered
        Then the add-user form is not a dialog
        And the Entra add-user form is labelled by its heading
        And the form uses GCDS inputs and buttons
        And the add-user form actions use button semantics
        When the user cancels the add-user form
        Then the add-user form reports that it was cancelled

    Scenario: An Entra user's selected role is submitted
        Given the Entra add-user form is rendered
        When an Entra user is selected
        Then the pending Entra user is displayed as an accessible list item
        When the Entra user's role is changed to Collaborator
        And the Entra add-user form is submitted
        Then the Entra user is submitted as a Collaborator

    Scenario: The external user form is an accessible inline wizard
        Given the external add-user form is rendered
        Then the add-user form is not a dialog
        And the external add-user form is labelled by its heading
        And the external add-user form announces step 1 of 4
        And the form uses GCDS inputs and buttons
        And the add-user form actions use button semantics
        When the user cancels the add-user form
        Then the add-user form reports that it was cancelled

    Scenario: The external user role uses the accessible select
        Given the external add-user form is rendered
        When a valid external email address is entered
        And the external add-user form advances to user details
        Then the external role is selected with a GCDS select
        And the external account expiry uses a GCDS date input

    Scenario Outline: The external user expiry must be in the future
        Given the external add-user form is rendered
        When a valid external email address is entered
        And the external add-user form advances to user details
        And valid external user details are entered with an expiry <expiry>
        Then the external expiry validation is <validation>

        Examples:
            | expiry   | validation |
            | today    | rejected   |
            | past     | rejected   |
            | tomorrow | accepted   |

    Scenario: An existing external user's expiry can be corrected while inviting them
        Given the external add-user form is rendered
        And an existing external user has an expired account
        When the existing external user's email address is entered
        And the external add-user form advances to user details
        Then the existing external user's expiry is editable
        When valid external user details are entered with an expiry tomorrow
        And the external invitation is completed
        Then the existing external user's future expiry is persisted

    Scenario Outline: External user dialogs reject expiry dates that are not in the future
        Given the <dialog> external user expiry dialog is rendered
        When the account expiry is changed to <expiry> and saved
        Then the external user expiry change is rejected

        Examples:
            | dialog | expiry |
            | manage | blank  |
            | manage | past   |
            | manage | today  |
            | extend | blank  |
            | extend | past   |
            | extend | today  |

    Scenario Outline: External user dialogs save future expiry dates
        Given the <dialog> external user expiry dialog is rendered
        When the account expiry is changed to tomorrow and saved
        Then the external user's future expiry is persisted by the dialog

        Examples:
            | dialog |
            | manage |
            | extend |
