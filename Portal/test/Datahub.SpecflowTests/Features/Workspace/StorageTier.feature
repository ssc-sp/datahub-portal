@StorageTier
@IWebHostEnvironment
Feature: Storage tier
The workspace storage interface displays file tiers and enforces tier-specific behavior.

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
