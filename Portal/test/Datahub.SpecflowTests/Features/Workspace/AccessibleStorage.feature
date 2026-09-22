@IWebHostEnvironment
Feature: Accessible storage configuration and selection
  Storage accounts and containers can be managed using labelled inline controls.

  Scenario: Loading the storage explorer does not move focus
    Given the storage explorer is ready for an administrator
    Then the storage explorer has not requested focus

  Scenario Outline: Inline storage forms request focus and restore their trigger
    Given the storage explorer is ready for an administrator
    When I open the storage explorer <action> form
    Then the storage explorer requests focus on "<heading>"
    And the active storage explorer form is outside the disclosure
    When I cancel the active storage explorer form
    Then the storage explorer requests focus on "<trigger>"
    And the storage focus module is imported once
    When the storage explorer renders again
    Then the storage focus request is not repeated
    Examples:
      | action | heading                       | trigger               |
      | add    | storage-configuration-heading | storage-add-button    |
      | edit   | storage-configuration-heading | storage-edit-button   |
      | remove | storage-removal-heading       | storage-remove-button |

  Scenario: Losing the JavaScript connection does not prevent opening a storage form
    Given the storage explorer is ready for an administrator
    And the storage focus JavaScript connection is disconnected
    When I open the storage explorer add form
    Then the storage explorer requests focus on "storage-configuration-heading"
    And the active storage explorer form is outside the disclosure

  Scenario Outline: Search is available only for Azure storage
    Given the storage explorer is ready with <provider> storage selected
    Then the storage search box is <visibility>
    Examples:
      | provider | visibility |
      | Azure    | shown      |
      | AWS      | hidden     |
      | GCP      | hidden     |

  Scenario: The storage selector starts as one compact disclosure
    Given the accessible storage selector is rendered for an administrator
    Then the storage disclosure starts collapsed
    And the storage disclosure summary displays the current selection
    And storage selection announcements are visually hidden
    And storage controls are inside the disclosure

  Scenario: The default FSDH storage does not display a container selector
    Given the accessible storage selector is rendered with default FSDH storage selected
    Then the default FSDH container selector is hidden

  Scenario: Select containers from distinct accounts with the same name
    Given the accessible storage selector is rendered for an administrator
    Then the storage selector displays the current provider account and container
    And the storage accounts with the same name remain distinct
    When I expand the storage disclosure
    When I select the second external storage account
    Then its first enabled container becomes selected
    And the storage disclosure remains expanded
    And the storage disclosure summary displays the current selection
    When I select another container in that storage account
    Then that container becomes selected
    And the storage disclosure remains expanded
    And the storage disclosure summary displays the current selection

  Scenario: Storage management does not collapse the selector
    Given the accessible storage selector is rendered for an administrator
    When I expand the storage disclosure
    And the inline storage configuration form is open
    Then the storage disclosure remains expanded
    And storage controls are disabled during the inline form

  Scenario: Disabled storage remains manageable without becoming active
    Given the accessible storage selector is rendered for an administrator
    When I select the disabled storage account
    Then the active storage container remains unchanged
    And storage management targets the disabled account

  Scenario Outline: Storage actions respect permissions and classification
    Given the accessible storage selector is rendered for a <role>
    And external storage is <availability>
    Then the add storage action is <add>
    And the edit storage action is <edit>
    Examples:
      | role          | availability | add    | edit   |
      | administrator | allowed      | shown  | shown  |
      | administrator | restricted   | hidden | shown  |
      | collaborator  | allowed      | hidden | hidden |

  Scenario: Provider changes reset the inline connection form
    Given the accessible new storage form is rendered
    Then the storage configuration is a labelled inline region
    And storage testing requires valid credentials
    When I choose AWS in the storage form
    Then the AWS credential inputs are shown with masked secrets
    When I choose GCP in the storage form
    Then the GCP credentials use a multiline input

  Scenario: Editing credentials requires connection verification again
    Given the accessible existing storage form is rendered
    Then the storage provider cannot be changed
    When I change the Azure storage credentials
    Then the connection must be tested again before saving

  Scenario: Cancelling storage edits discards the draft
    Given the accessible existing storage form is rendered
    When I change the storage friendly name
    And I cancel the storage form
    Then the original storage settings are unchanged
    And no storage save callback has run

  Scenario: Saving requires the classification confirmation
    Given the accessible existing storage form is rendered
    When I clear the unclassified data confirmation
    Then storage saving is disabled

  Scenario: A failed connection test reports an inline error
    Given the accessible existing storage form is rendered
    When I change the Azure storage credentials
    And I test the invalid storage connection
    Then the storage connection error appears inline

  Scenario: Failed persistence retains the form for retry
    Given the accessible existing storage form is rendered
    And storage persistence will fail
    When I save the storage form
    Then the storage persistence error appears inline
    And the storage form remains available for retry

  Scenario: Saving prevents duplicate submissions
    Given the accessible existing storage form is rendered
    And storage persistence is pending
    When I submit the storage form twice
    Then only one storage save callback runs

  Scenario: Removal requires explicit confirmation
    Given the accessible storage removal form is rendered
    Then the removal confirmation names the storage account
    When I cancel the storage removal
    Then no storage removal callback has run

  Scenario: Removal prevents duplicate submissions
    Given the accessible storage removal form is rendered
    And storage removal is pending
    When I confirm storage removal twice
    Then only one storage removal callback runs

  Scenario: Failed removal retains the inline confirmation
    Given the accessible storage removal form is rendered
    And storage removal will fail
    When I confirm storage removal
    Then the storage removal error appears inline
    And the storage removal can be retried

  Scenario Outline: Reloaded storage selects an enabled container
    Given storage containers were reloaded with the previous container <previous> and workspace default <default>
    Then the reloaded selection is <selection>
    Examples:
      | previous | default | selection |
      | enabled  | enabled | previous  |
      | removed  | enabled | default   |
      | disabled | enabled | default   |
      | removed  | removed | first     |
      | removed  | empty   | none      |
