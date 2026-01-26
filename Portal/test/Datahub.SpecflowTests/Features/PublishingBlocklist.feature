@IWebHostEnvironment
Feature: Publishing Blocklist
Allows administrators to manage the blocklist for Open Government publishing
by adding, editing, and deleting blocklist entries.

    Scenario: Add a new blocklist entry with valid data
        Given a publishing blocklist service with no existing entries
        When a new blocklist entry is added with department "Fisheries and Oceans Canada" and email domain "@dfo-mpo.gc.ca"
        Then the blocklist should contain 1 entry
        And the blocklist entry should have department name "Fisheries and Oceans Canada"
        And the blocklist entry should have email hostname "@dfo-mpo.gc.ca"
        And the blocklist entry status should be Active

    Scenario: Check if a user is blocked by email domain
        Given a publishing blocklist service with an entry for email domain "@blocked.gc.ca"
        When checking if email domain "@blocked.gc.ca" is blocked
        Then the user should be blocked

    Scenario: Check if a user is not blocked by email domain
        Given a publishing blocklist service with an entry for email domain "@blocked.gc.ca"
        When checking if email domain "@allowed.gc.ca" is blocked
        Then the user should not be blocked

    Scenario: Update an existing blocklist entry
        Given a publishing blocklist service with an entry for email domain "@original.gc.ca"
        When the blocklist entry is updated with department "Updated Department" and email domain "@updated.gc.ca"
        Then the blocklist entry should have department name "Updated Department"
        And the blocklist entry should have email hostname "@updated.gc.ca"

    Scenario: Delete a blocklist entry (soft delete)
        Given a publishing blocklist service with an entry for email domain "@todelete.gc.ca"
        When the blocklist entry is deleted
        Then the blocklist entry status should be Deleted
        And the blocklist entry should have a removal date
        And the user should not be blocked when checking email domain "@todelete.gc.ca"

    Scenario: Email hostname should be converted to lowercase
        Given a publishing blocklist service with no existing entries
        When a new blocklist entry is added with department "Test Department" and email domain "@TEST.GC.CA"
        Then the blocklist entry should have email hostname "@test.gc.ca"

    Scenario: Only active entries are returned
        Given a publishing blocklist service with 2 active entries and 1 deleted entry
        When retrieving active blocklist entries
        Then the active blocklist entries should contain 2 entries
