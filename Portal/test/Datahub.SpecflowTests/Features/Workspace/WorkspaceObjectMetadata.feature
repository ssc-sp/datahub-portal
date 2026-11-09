@IWebHostEnvironment
Feature: Workspace GC Hosting metadata
The workspace metadata view presents the complete GC Hosting business record without allowing edits.

    Scenario: All GC Hosting business metadata is displayed using disabled GCDS controls
        Given a workspace has complete GC Hosting metadata
        When the workspace GC Hosting metadata is rendered
        Then every GC Hosting business metadata value is displayed
        And every GC Hosting metadata control is a disabled GCDS control

    Scenario: Optional project metadata can be empty
        Given a workspace has GC Hosting metadata without optional project details
        When the workspace GC Hosting metadata is rendered
        Then the optional project metadata values are empty and disabled

    Scenario: A workspace without GC Hosting metadata displays the empty state
        Given a workspace has no GC Hosting metadata
        When the workspace GC Hosting metadata is rendered
        Then the GC Hosting metadata empty state is displayed

    Scenario: An empty workspace acronym displays the empty state
        Given no workspace acronym is supplied for GC Hosting metadata
        When the workspace GC Hosting metadata is rendered
        Then the GC Hosting metadata empty state is displayed
