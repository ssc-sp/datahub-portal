@IWebHostEnvironment
Feature: File metadata editor
The file metadata editor lets workspace collaborators view and maintain custom file metadata.

    Scenario: Mandatory metadata is hidden while custom metadata is displayed
        Given the file has the following metadata
          | Key       | Value |
          | createdby | user  |
          | fileid    | 123   |
          | custom    | value |
        When the file metadata editor is rendered for editing
        Then the metadata editor should display "custom"
        And the metadata editor should display "value"
        And the metadata editor should not display "createdby"
        And the metadata editor should not display "fileid"

    Scenario: A file without custom metadata shows an empty message
        Given the file has the following metadata
          | Key           | Value |
          | createdby     | user  |
          | uploadBatchId | batch |
        When the file metadata editor is rendered for editing
        Then the metadata editor should display "No custom metadata fields"

    Scenario: A metadata entry can be added and removed
        Given the file has the following metadata
          | Key      | Value |
          | existing | value |
        When the file metadata editor is rendered for editing
        And the user adds a metadata entry
        Then the metadata editor should have two editable entries
        When the user removes the first metadata entry
        Then the metadata editor should have one editable entry

    Scenario: Duplicate metadata keys cannot be saved
        Given the file has the following metadata
          | Key  | Value  |
          | Name | first  |
          | name | second |
        When the file metadata editor is rendered for editing
        And the user saves the metadata
        Then the metadata editor should display "Duplicate keys are not allowed."
        And the metadata should not be saved

    Scenario: Metadata is persisted successfully
        Given the file has the following metadata
          | Key       | Value |
          | createdby | user  |
          | custom    | value |
        When the file metadata editor is rendered for editing
        And the user saves the metadata
        Then the file metadata should be saved
          | Key       | Value |
          | createdby | user  |
          | custom    | value |
        And the metadata editor should leave edit mode

    Scenario: A storage failure is displayed when saving
        Given the file has the following metadata
          | Key    | Value |
          | custom | value |
        And saving metadata fails with "save failed"
        When the file metadata editor is rendered for editing
        And the user saves the metadata
        Then the metadata editor should display "save failed"
        And the metadata editor should remain in edit mode

    Scenario: Cancelling reloads metadata and discards changes
        Given the file has the following metadata
          | Key    | Value    |
          | custom | original |
        When the file metadata editor is rendered for editing
        And the user changes the first metadata value to "changed"
        And the user cancels metadata editing
        Then the metadata editor should display "original"
        And the metadata editor should not display "changed"
        And the metadata should have been loaded 2 times

    Scenario: Unsupported metadata operations display an error
        Given metadata operations are unsupported
        When the file metadata editor is rendered for editing
        Then the metadata editor should display "This storage container does not support metadata operations."
