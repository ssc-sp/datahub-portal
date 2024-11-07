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
        And the resource messaging service should be notified
        
    Scenario: A workspace should not update anything if all the resources are already deleted
        Given a workspace with usage exceeding its budget
        And the resources are already deleted
        When the notifier verifies overbudget is deleted
        Then the resource messaging service should not be notified
        
    Scenario: A workspace that is over budget but has prevent auto delete set to true should not be deleted
        Given a workspace with usage exceeding its budget
        And the workspace has prevent auto delete set to true
        When the notifier verifies overbudget is deleted
        Then the resources should not be set to deleted
        And the resource messaging service should not be notified
        
    Scenario: A workspace that is over budget should email the admin users when deleting a resource
        Given a workspace with usage exceeding its budget
        And there is a workspace lead and 5 admin users
        When the notifier verifies overbudget is deleted
        Then the 5 admin users and workspace lead should be emailed
