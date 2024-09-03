﻿@WorkspaceResourceGroups
Feature: WorkspaceResourceGroups
    Tests for the workspace resource groups service
    
Scenario: Getting the resource group of a workspace with RG stored in new-project-template should return the correct value
    Given a workspace with a new-project-template
    When the resource group is requested
    Then the result should be the expected resource group
    
Scenario: Getting the resource group of a workspace with RG stored in blob-storage should return the correct value and should assign the RG to the new-project-template
    Given a workspace with an empty new-project-template and a blob-storage
    When the resource group is requested
    Then the result should be the expected resource group
    And the new-project-template should be assigned the resource group
    
Scenario: Getting the resource group of a workspace with RG stored nowhere should return the correct value, make a query to ARM and assign the RG to the new-project-template
    Given a workspace with RG not stored in DB
    When the resource group is requested
    Then the result should be the expected resource group
    And the new-project-template should be assigned the resource group
   
Scenario: Getting the resource group of an invalid workspace should throw an exception
    Given an invalid workspace
    When the resource group is requested
    Then an exception should be thrown
    
Scenario: Getting the resource groups of a subscription should return the correct values
    Given a subscription with resource groups
    When the subscription resource groups are requested
    Then the result should be the expected resource group
    
Scenario: Getting the resource groups of an invalid subscription should throw an exception
    Given an invalid subscription
    When the subscription resource groups are requested
    Then an exception should be thrown
    
Scenario: Getting all the resource groups should work properly
    Given a list of projects with resource groups
    When all the resource groups are requested
    Then the result should be the expected resource groups