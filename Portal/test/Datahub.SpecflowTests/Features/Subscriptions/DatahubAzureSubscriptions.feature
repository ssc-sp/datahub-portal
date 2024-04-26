@AzureDatahubSubscription
Feature: Azure Subscription Service
This feature provides the ability to manage Azure subscriptions for the DataHub

    Scenario: List all subscriptions
        Given a datahub azure subscription service
        And at least one subscription exists
        When the list of subscriptions is requested
        Then the list of subscriptions is returned
        And the list of subscriptions contains at least one subscription
        
    Scenario: Add a valid Azure subscription
        Given a datahub azure subscription service
        When a new subscription is added
        Then the subscription is added to the list of subscriptions
        
    Scenario: Add an invalid Azure subscription
        Given a datahub azure subscription service
        When an invalid subscription is added
        Then an error is returned
        
    Scenario: Delete an existing subscription
        Given a datahub azure subscription service
        And there is a subscription with id "delete-me"
        When a subscription with id "delete-me" is deleted
        Then there should be no subscriptions with id "delete-me"
        
    Scenario: Delete a non-existing subscription
        Given a datahub azure subscription service
        When a non-existing subscription is deleted
        Then an error is returned

    Scenario: Get the next available suscription
        Given a datahub azure subscription service
        And at least one subscription exists
        When the next available subscription is requested
        Then the next available subscription is returned
        
    Scenario: Get the next available suscription when there are no subscriptions
        Given a datahub azure subscription service
        And there are no subscriptions
        When the next available subscription is requested
        Then an error is returned
