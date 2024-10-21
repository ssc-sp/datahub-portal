@ProjectUsage
Feature: ProjectUsageScheduler
    Tests for scheduling of project usage updates and project capacity updates
    
    Scenario: When scheduling happens, the correct number of messages get scheduled
        Given workspaces that need to be updated
        When the scheduler runs
        Then the correct number of messages get scheduled
        
    Scenario: When scheduling happens, if no workspaces need to be updated, no messages get scheduled
        Given no workspaces that need to be updated
        When the scheduler runs
        Then the correct number of messages get scheduled