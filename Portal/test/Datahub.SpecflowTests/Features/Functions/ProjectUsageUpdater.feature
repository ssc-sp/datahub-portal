@ProjectUsage
Feature: ProjectUsageUpdater
Tests for the ProjectUsageUpdater class, which includes simple storage, budget and costs updating and also the more complicated rollover feature

    Scenario: When a project usage is updated and the update does not go into a new fiscal year, a rollover should not be triggered
        Given a project usage update message for a workspace that doesn't to be rolled over
        When the project usage is updated
        Then the rollover should not be triggered

    Scenario: When a project usage is updated and the update goes into a new fiscal year, a rollover should be triggered
        Given a project usage update message for a workspace that needs to be rolled over
        When the project usage is updated
        Then the rollover should be triggered