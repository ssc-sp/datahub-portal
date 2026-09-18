@StorageTier
@IWebHostEnvironment
Feature: Storage tier
The workspace storage interface displays file tiers and enforces tier-specific behavior.

    Scenario: Renaming an AWS file preserves its displayed class and updates the selection
        Given an AWS file explorer with two selected Standard files and a partial update failure
        When the explorer renames "a.csv" to "renamed.csv"
        Then the explorer should immediately display class "Standard" for "renamed.csv"
        And the explorer should select "renamed.csv"

    Scenario: A failed AWS rename keeps the original file selected
        Given an AWS file explorer with two selected Standard files and a partial update failure
        When the explorer cannot rename "a.csv" to "renamed.csv"
        Then the explorer should immediately display class "Standard" for "a.csv"
        And the explorer should select "a.csv"

    Scenario: AWS classes display as soon as the initial container loads
        Given an AWS file explorer with two selected Standard files and a partial update failure
        Then the explorer should immediately display class "Standard" for "a.csv"
        And the explorer should immediately display class "Standard" for "b.csv"
        And no storage class update should have been required

    Scenario: Switching containers refreshes AWS labels on the existing explorer
        Given an AWS file explorer with two selected Standard files and a partial update failure
        When the explorer loads another AWS container
        Then the explorer should immediately display class "Glacier Instant Retrieval" for "a.csv"
        And the explorer should immediately display class "Glacier Instant Retrieval" for "b.csv"
        And no storage class update should have been required

    Scenario: Opening a folder refreshes AWS labels for its full object paths
        Given an AWS file explorer with two selected Standard files and a partial update failure
        When the explorer opens a nested folder with different AWS classes
        Then the explorer should immediately display class "Standard - Infrequent Access" for "a.csv"
        And the explorer should immediately display class "One Zone - Infrequent Access" for "b.csv"
        And no storage class update should have been required

    Scenario Outline: AWS classes use readable names in file rows and properties
        Given a file item with tier "<Class>"
        When the file item is rendered
        Then the file item should display "(<Label>)"
        Given file properties with tier "<Class>"
        When the file properties are rendered
        Then the file properties should display storage tier "<Label>"

        Examples:
          | Class              | Label                        |
          | STANDARD           | Standard                     |
          | STANDARD_IA        | Standard - Infrequent Access |
          | ONEZONE_IA         | One Zone - Infrequent Access |
          | INTELLIGENT_TIERING | Intelligent-Tiering          |
          | GLACIER_IR         | Glacier Instant Retrieval    |
          | GLACIER            | Glacier Flexible Retrieval   |
          | DEEP_ARCHIVE       | Glacier Deep Archive         |
          | REDUCED_REDUNDANCY | Reduced Redundancy            |

    Scenario: Online AWS properties omit the archive notice
        Given file properties with tier "STANDARD"
        When the file properties are rendered
        Then the archive warning should be hidden

    Scenario Outline: AWS downloads with retrieval charges use the existing cost confirmation
        Given a storage heading in folder "nested/" with selected file "report.csv"
        And the heading uses AWS storage
        And the following file tiers
          | Path              | Tier    |
          | nested/report.csv | <Class> |
        And the archive warning is cancelled
        When the heading downloads the selected files
        Then the retrieval cost confirmation should be requested once
        And no selected file should be downloaded

        Examples:
          | Class       |
          | STANDARD_IA |
          | ONEZONE_IA  |
          | GLACIER_IR  |

    Scenario: Partial AWS updates reload actual classes in the file explorer
        Given an AWS file explorer with two selected Standard files and a partial update failure
        When the heading changes the tier to "STANDARD_IA"
        Then a failed tier change should be reported
        And the explorer should display the persisted AWS classes after the partial failure

    Scenario: Switching providers refreshes class options and clears the selected class
        Given a storage heading in folder "/" with selected file "report.csv"
        And the heading uses AWS storage
        When storage class options are refreshed
        Then the heading offers AWS classes without Azure or legacy destinations
        When the heading switches to an Azure container
        Then the heading offers Azure tiers with no selected AWS class

    Scenario Outline: Online AWS changes apply directly using canonical file paths
        Given a storage heading in folder "nested/" with selected file "report.csv"
        And the heading uses AWS storage
        When the heading changes the tier to "<Class>"
        Then tier "<Class>" should be persisted for path "nested/report.csv"
        And a successful tier change should be reported
        And no archive confirmation should be requested

        Examples:
          | Class        |
          | STANDARD_IA  |
          | GLACIER_IR   |
          | STANDARD     |
          | ONEZONE_IA   |
          | INTELLIGENT_TIERING |

    Scenario Outline: AWS archive changes use the existing archive confirmation
        Given a storage heading in folder "nested/" with selected file "report.csv"
        And the heading uses AWS storage
        And the archive warning is confirmed
        When the heading changes the tier to "<Class>"
        Then the archive warning should be requested once
        And tier "<Class>" should be persisted for path "nested/report.csv"

        Examples:
          | Class        |
          | GLACIER      |
          | DEEP_ARCHIVE |

    Scenario: Cancelling an AWS archive change does not persist
        Given a storage heading in folder "/" with selected file "report.csv"
        And the heading uses AWS storage
        And the archive warning is cancelled
        When the heading changes the tier to "GLACIER"
        Then no file tier should be changed
        And no storage tier change callback should be emitted

    Scenario: Read only storage cannot change class
        Given a storage heading in folder "/" with selected file "report.csv"
        And the heading uses AWS storage
        And the heading is read only
        When the heading changes the tier to "STANDARD_IA"
        Then no file tier should be changed

    Scenario: Guests cannot change storage class
        Given a storage heading in folder "/" with selected file "report.csv"
        And the heading uses AWS storage
        And the heading user is a guest
        When the heading changes the tier to "GLACIER_IR"
        Then no file tier should be changed

    Scenario Outline: AWS archive availability controls downloads
        Given a storage heading in folder "/" with selected file "report.csv"
        And the heading uses AWS storage
        And AWS archive availability is <State>
        When download availability is checked
        Then the selected file should be <Availability> for download

        Examples:
          | State           | Availability |
          | Available       | available    |
          | RestoreRequired | unavailable  |
          | Restoring       | unavailable  |

    Scenario: File properties refresh archive status explicitly
        Given file properties with tier "GLACIER"
        And AWS archive availability is RestoreRequired
        When the file properties are rendered
        Then AWS archive status should show "Restore this file through AWS tooling"
        Given AWS archive availability is Available
        When AWS archive status is refreshed
        Then AWS archive status should show "This file is available for download"

    Scenario: Nested file archive checks use the full object key
        Given a storage heading in folder "nested/" with selected file "report.csv"
        And the heading uses AWS storage
        And AWS archive availability is RestoreRequired
        When download availability is checked
        Then AWS archive status should use path "nested/report.csv"
        And the selected file should be unavailable for download

    Scenario: Archive status failures appear inline
        Given file properties with tier "GLACIER"
        And reading AWS archive status fails
        When the file properties are rendered
        Then AWS archive status should show "Unable to read AWS archive status. Please try again."

    Scenario: Duplicate AWS changes do not submit a second class change
        Given a storage heading in folder "/" with selected file "report.csv"
        And the heading uses AWS storage
        And the selected file update is delayed
        When the AWS class change begins
        And the heading changes the tier to "STANDARD_IA"
        And the heading changes the tier to "GLACIER"
        And the delayed update completes
        Then tier "STANDARD_IA" should be persisted for path "report.csv"
        And the storage tier change callback should receive "STANDARD_IA"

    Scenario: A thrown update failure does not prevent other selected files from updating
        Given a storage heading with selected files and a folder
        And the first selected file update throws
        When the heading changes the tier to "Cold"
        Then every selected file tier update should be attempted
        And a failed tier change should be reported
        And the heading should explain the 5 GB limit

    Scenario Outline: A file displays its storage tier
        Given a file item with tier "<Tier>"
        When the file item is rendered
        Then the file item should display "(<Tier>)"
        And the file item should display its name size and modified date

        Examples:
          | Tier    |
          | Hot     |
          | Cool    |
          | Cold    |
          | Archive |

    Scenario: A file without a tier omits the tier suffix
        Given a file item with no storage tier
        When the file item is rendered
        Then the file item should not display a storage tier suffix

    Scenario: A folder never displays a storage tier
        Given a folder item with tier "Archive"
        When the file item is rendered
        Then the file item should not display a storage tier suffix

    Scenario Outline: File properties display the current tier
        Given file properties with tier "<Tier>"
        When the file properties are rendered
        Then the file properties should display storage tier "<Tier>"
        And the archive warning should be <Warning>

        Examples:
          | Tier    | Warning |
          | Hot     | hidden  |
          | Cool    | hidden  |
          | Cold    | hidden  |
          | Archive | visible |
          | Unknown | hidden  |

    Scenario: File properties react to a tier change
        Given file properties with tier "Hot"
        When the file properties are rendered
        And the file properties tier changes to "Archive"
        Then the file properties should display storage tier "Archive"
        And the archive warning should be visible

    Scenario: A direct tier lookup detects a matching file
        Given the following file tiers
          | Path               | Tier    |
          | folder/hot.csv     | Hot     |
          | folder/archive.csv | Archive |
          | folder/cold.csv    | Cold    |
        When direct paths are checked for tier "Archive"
        Then the tier check should succeed
        And tier lookup should stop after "folder/archive.csv"

    Scenario: Tier matching is exact and case-sensitive
        Given the following file tiers
          | Path             | Tier |
          | folder/file.csv  | hot  |
        When direct paths are checked for tier "Hot"
        Then the tier check should fail

    Scenario: Metadata file objects are checked using their full paths
        Given metadata files in tiers
          | Path              | Tier |
          | nested/a.csv      | Hot  |
          | nested/deeper.csv | Cold |
        When metadata files are checked for tier "Cold"
        Then the tier check should succeed
        And storage tiers should be requested for the metadata file paths

    Scenario: Empty file collections do not query storage
        Given no files to check
        When direct paths are checked for tier "Archive"
        Then the tier check should fail
        And no storage tier should be requested

    Scenario Outline: Changing a selected file uses a canonical storage path
        Given a storage heading in folder "<Folder>" with selected file "report.csv"
        When the heading changes the tier to "Cool"
        Then tier "Cool" should be persisted for path "<Path>"
        And the storage tier change callback should receive "Cool"
        And a successful tier change should be reported

        Examples:
          | Folder  | Path              |
          | /       | report.csv        |
          | nested/ | nested/report.csv |

    Scenario: Bulk tier changes ignore selected folders
        Given a storage heading with selected files and a folder
        When the heading changes the tier to "Cold"
        Then only the selected files should be changed to tier "Cold"

    Scenario: A failed bulk tier change is reported after every file is attempted
        Given a storage heading with selected files and a failed tier update
        When the heading changes the tier to "Cold"
        Then every selected file tier update should be attempted
        And a failed tier change should be reported
        And the storage tier change callback should receive "Cold"

    Scenario: Cancelling the archive warning prevents a tier change
        Given a storage heading in folder "/" with selected file "report.csv"
        And the archive warning is cancelled
        When the heading changes the tier to "Archive"
        Then no file tier should be changed
        And no storage tier change callback should be emitted

    Scenario: Confirming the archive warning permits a tier change
        Given a storage heading in folder "/" with selected file "report.csv"
        And the archive warning is confirmed
        When the heading changes the tier to "Archive"
        Then the archive warning should be requested once
        And tier "Archive" should be persisted for path "report.csv"
