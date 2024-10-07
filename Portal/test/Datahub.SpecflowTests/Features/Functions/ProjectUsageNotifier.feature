@ProjectUsageNotifier
Feature: Project Usage Notifier
    This serverless function is responsible for notifying users when their project usage exceeds a certain threshold and for updating the project resources to be deleted when it's exceeded 100% of the budget
    
    
    Scenario: A workspace delete should be required if the usage exceeds 100% of the budget
        Given a workspace with usage exceeding its budget
        When the notifier checks if a delete is required
        Then the result should be true
        
    Scenario: A workspace delete should not be required if the usage does not exceed 100% of the budget
        Given a workspace with usage not exceeding its budget
        When the notifier checks if a delete is required
        Then the result should be false
        
    Scenario: A workspace should set the resources to deleted if the usage exceeds 100% of the budget
        Given a workspace with usage exceeding its budget
        When the notifier verifies overbudget is deleted
        Then the resources should be set to deleted
        