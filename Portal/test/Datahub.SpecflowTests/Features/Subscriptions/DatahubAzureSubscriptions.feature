@AzureDatahubSubscription
Feature: Azure Subscription Service
This feature provides the ability to manage Azure subscriptions for the DataHub

    Scenario: List all subscriptions
        Given a datahub azure subscription service
        And at least one subscription exists
        When the list of subscriptions is requested
        Then the list of subscriptions is returned
        And the list of subscriptions contains at least one subscription