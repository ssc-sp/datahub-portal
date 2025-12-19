# Email Change Handling and Single Active Entry Guarantee

This document describes the lifecycle for user email changes, external-user invitations, deactivation, and the guarantees the system enforces to avoid multiple active entries for the same person.

## Key Entities

- `PortalUser`
  - Owns the canonical `Email` used by the portal for identity and notifications.
  - One-to-one relationship with `ExternalUser` or `EntraUser`.
  - Is the authoritative record for the user’s email, display/profile and activity.

- `ExternalUser`
  - Represents a non-portal identity that completes the external invitation flow.
  - Has a required `OID` (GCCF Object Identifier)
  - Tracks invitation attempts via `Invitations` (`ExternalUserInvite` collection).
  - Supports deactivation via `DeactivatedDate_DT` and `DeactivatedByUser`.

- `ExternalUserInvite`
  - Stores invitation tokens/codes, expiry, and acceptance timestamps that drive the onboarding of external users.

## Core Constraints

- `ExternalUser.OID` is unique (alternate key): only one `ExternalUser` can exist per OID.
- `ExternalUser.Id` is the primary key and is database-generated.
- `ExternalUser.DeactivatedByUser` uses `DeleteBehavior.NoAction` to preserve referential integrity without cascading deletes.
- Relationship: `ExternalUser` has many `ExternalUserInvite`, each linked via `User`.

These constraints ensure the system cannot have two active `ExternalUser` rows with the same OID.

## Email Change Scenarios

1. Email change within `PortalUser` (same person, email updated)
   - `PortalUser.Email` is updated directly.
   - `ExternalUser.OID` remains unchanged if identity provider principal stays the same.
   - No new `ExternalUser` record is created.
   - Any active invites remain associated with the same `ExternalUser`.
   - Result: Single active `ExternalUser`, single `PortalUser`, email updated.

2. Identity provider change causing a new OID (e.g., email change implying new external identity)
   - A new invitation is initiated for the user’s new identity.
   - Prior `ExternalUser` is deactivated: set `DeactivatedDate_DT` and `DeactivatedByUser`.
   - A new `ExternalUser` is created (or re-activated if previously deactivated) bound to the new OID.
   - `PortalUser` remains the owning record; its `Email` may update to reflect the new identity.
   - Result: Previous `ExternalUser` is inactive, new `ExternalUser` is active. Only one active `ExternalUser` exists.

3. User disabled and re-invited (re-onboarding)
   - When disabling:
     - Set `ExternalUser.DeactivatedDate_DT` and `ExternalUser.OID` is cleared or made non-valid (cannot be used to authenticate).
   - When re-inviting:
     - A fresh `ExternalUserInvite` is issued.
     - On acceptance, either:
       - Re-activate the existing `ExternalUser` by assigning the new OID and clearing `DeactivatedDate_DT`, or
       - Create a new `ExternalUser` with the new OID and keep the old one deactivated.
   - Because `OID` is unique, activation always results in only one `ExternalUser` having a valid, non-deactivated OID.
   - Result: Only one active `ExternalUser` per OID and per person.

## Operational Rules to Enforce Single Active Entry

- On deactivation:
  - Set `ExternalUser.DeactivatedDate_DT` and ensure the OID is not usable for authentication (either null or marked invalid).
- On invitation acceptance:
  - If an existing deactivated `ExternalUser` for the same `PortalUser` exists:
    - Prefer re-activating the same row by setting the fresh OID, clearing `DeactivatedDate_DT`.
    - Ensure the OID uniqueness constraint is satisfied (no other row holds the same OID).
  - If re-activation is not viable:
    - Create a new `ExternalUser` with the new OID and leave the old one deactivated.
- Before persisting a new or updated `ExternalUser`:
  - Validate there is no other active `ExternalUser` with the same OID.
  - Validate the owning `PortalUser` does not have more than one active external identity.

## Invitation Lifecycle

- Create invite (`ExternalUserInvite`) with token/code, expiry, `Request_DT`.
- Acceptance updates `InvitationTokenAccepted` or `InvitationCodeAccepted`.
- On acceptance:
  - Assign or update `ExternalUser.OID`.
  - Clear `ExternalUser.DeactivatedDate_DT`.
  - Maintain the unique OID index integrity to prevent duplicate active entries.

## Auditing and Traceability

- Use `DeactivatedByUser` to track who disabled the `ExternalUser`.
- Preserve `ExternalUserInvite` history to understand the onboarding journey.
- Consider adding soft-delete flags or status enums if operational reporting requires more granularity.

## Summary

- `PortalUser.Email` is the canonical email; updates here do not by themselves create new external identities.
- `ExternalUser.OID` is the unique external identity anchor; only one active `ExternalUser` can hold a given OID.
- Deactivation plus re-invitation flow guarantees a single active entry by either re-activating the prior record or creating a new one while keeping the old one inactive.
- The EF Core configuration (unique index on `OID`, controlled relationships, and explicit deactivation fields) enforces these guarantees at both the application and storage layers.