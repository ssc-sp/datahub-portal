@IWebHostEnvironment
Feature: RoleClaimTransformer
 Validates claims transformation for external and Entra users

Background:
  Given an authorization store with workspaces and roles

Scenario: External user receives workspace role claims
  Given an external user with name identifier "ext-123"
  When claims are transformed
  Then the user should have role "PRJ1-guest"
  And the user should have role "PRJ2-admin"

Scenario: Entra user receives workspace role claims and special roles
  Given an entra user with object id "entra-456" and email "user@example.com"
  When claims are transformed
  Then the user should have role "default"
  And the user should have role "PRJ1-collaborator"
  And the user should have role "PRJ2-admin"
