@IWebHostEnvironment
Feature: Announcement carousel
The announcement carousel displays active announcement previews on the home page.

    Scenario: No active announcements are available
        Given there are no active announcement previews
        When the announcement carousel is rendered
        Then the announcement heading is not displayed
        And no announcement notices are displayed

    Scenario: At most three announcements are displayed in severity order
        Given the following active announcement previews
          | Preview                                 | Severity |
          | Information announcement\nInfo body   | 2        |
          | # Warning announcement\nWarning body | 1        |
          | Danger announcement\nDanger body     | 0        |
          | Extra announcement\nExtra body       | 2        |
        When the announcement carousel is rendered
        Then the announcement heading is displayed
        And 3 announcement notices are displayed
        And the announcement titles are displayed in this order
          | Title                    |
          | Danger announcement      |
          | Warning announcement     |
          | Information announcement |
        And the warning announcement body is displayed
        And each announcement has a read more link

    Scenario Outline: Announcement severity controls the notice role
        Given an active announcement preview with severity <severity>
        When the announcement carousel is rendered
        Then the announcement notice role is "<role>"

        Examples:
          | severity | role    |
          | 0        | danger  |
          | 1        | warning |
          | 2        | info    |
          | 10       | info    |

    Scenario: Image-only previews are not displayed as notices
        Given an active image-only announcement preview
        When the announcement carousel is rendered
        Then no announcement notices are displayed

    Scenario: French previews are requested for a French culture
        Given the current culture is French
        And there are no active announcement previews
        When the announcement carousel is rendered
        Then French announcement previews are requested
