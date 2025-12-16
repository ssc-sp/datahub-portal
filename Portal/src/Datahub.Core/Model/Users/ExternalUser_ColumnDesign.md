# ExternalUser Table Schema and Design Rationale

## Overview

The `ExternalUser` entity represents external (non-portal) users who are linked to portal users within the Datahub system. This table tracks identity information, login activity, deactivation status, and relationships to portal users and invitation requests.

## Column Specifications

| Column Name | Data Type | Nullable | Required | Primary Key | Purpose | Design Rationale |
|---|---|---|---|---|---|---|
| `Id` | int | No | Yes | ? | Unique identifier for the external user record | Standard primary key for entity identification and database relationships |
| `OID` | string | No | Yes | | GCCF object identifier for external identity | Required field that uniquely identifies the user in the Government of Canada Cloud Federation. Blank OID indicates incomplete invitation flow or deactivation. This is the primary identity attribute for authentication and authorization |
| `FirstLogin_DT` | DateTimeOffset? | Yes | No | | Timestamp of first login | Nullable field to track user activation. Allows identifying when a user first engaged with the portal. Useful for onboarding analytics and user lifecycle management. Null until user completes first login |
| `LastLogin_DT` | DateTimeOffset? | Yes | No | | Timestamp of most recent login | Nullable field to track user activity. Enables user inactivity detection, dormant account identification, and engagement metrics. Updated on each login event |
| `DeactivatedDate_DT` | DateTimeOffset? | Yes | No | | Timestamp when external user was deactivated | Nullable field to track deactivation events. Enables soft-deletion approach and audit trail for access revocation. Null indicates active user status |
| `DeactivatedByUser` | PortalUser? | Yes | No | | Navigation property to deactivating portal user | Nullable navigation property that references the `PortalUser` who performed the deactivation. Enables audit logging and accountability. Supports identifying who initiated access revocation |
| `Invitations` | ICollection<ExternalUserInvite> | No | Yes | | Collection of related invitation requests | Relationship collection tracking all invitation events for this external user. Used for invitation history, resend capabilities, and understanding user onboarding process. Initialized as empty list by default |
| `PortalUserId` | int | No | Yes | FK | Foreign key to owning PortalUser | Required foreign key establishing the required relationship between `ExternalUser` and `PortalUser`. Every external user must be linked to exactly one portal user |
| `PortalUser` | PortalUser | No | Yes | | Navigation to owning PortalUser | Required navigation property establishing the relationship to the owner. Marked with `required` keyword and initialized with null-forgiving operator to satisfy EF Core constraints |

## Design Patterns

### Nullable DateTimeOffset Fields
All temporal tracking fields (`FirstLogin_DT`, `LastLogin_DT`, `DeactivatedDate_DT`) are nullable (`DateTimeOffset?`) to represent:
- **Not yet occurred**: Null indicates the event hasn't happened
- **Soft deletion**: `DeactivatedDate_DT` being non-null indicates a deactivated user without hard deletion
- **User lifecycle states**: Allows tracking complete user journey from invitation through activation to deactivation

### Identity Design
- **OID as primary identity**: The GCCF Object ID is the authoritative external identity
- **Blank OID strategy**: Indicates incomplete invitation or deactivated state without removing the record
- **Immutable linkage**: Foreign key `PortalUserId` creates required relationship to a `PortalUser`

### Audit Trail
- **DeactivatedByUser**: Establishes accountability by tracking which portal user initiated deactivation
- **Timestamp fields**: Enable temporal queries for compliance and analytics

### Relationship Management
- **One-to-Many with PortalUser**: Each external user belongs to exactly one portal user
- **One-to-Many with ExternalUserInvite**: Tracks all invitation attempts and resends for the user
- **Collection initialization**: `Invitations` collection is initialized with empty list to prevent null reference exceptions

## Usage Scenarios

1. **User Authentication**: Validate login using OID and track `LastLogin_DT`
2. **User Activation**: Set `FirstLogin_DT` on first successful login
3. **Inactivity Detection**: Query `LastLogin_DT` to identify dormant accounts
4. **Access Revocation**: Set `DeactivatedDate_DT` and reference `DeactivatedByUser` for audit
5. **Invitation Management**: Query `Invitations` collection for resend or history
6. **Portal User Association**: Use `PortalUser` navigation for permission context and metadata

---

# PortalUser Table Schema and Design Rationale

## Overview

The `PortalUser` entity serves as the central identity hub within the Datahub portal. It aggregates user profile information, activity tracking, and relationships to external identities, achievements, telemetry data, and user-specific settings. Every user in the system—whether external or Entra-based—must have a corresponding `PortalUser` record.

## Column Specifications

| Column Name | Data Type | Nullable | Required | Primary Key | Purpose | Design Rationale |
|---|---|---|---|---|---|---|
| `Id` | int | No | Yes | ? | Unique identifier for the portal user record | Standard primary key for entity identification. Used as foreign key by related entities (ExternalUser, EntraUser, UserAchievement, etc.) |
| `ExternalUser` | ExternalUser? | Yes | No | | Navigation to associated external user | Nullable navigation allowing users to optionally be linked to external identity systems. Part of identity flexibility pattern |
| `EntraUser` | EntraUser? | Yes | No | | Navigation to associated Entra user | Nullable navigation allowing users to optionally be linked to Microsoft Entra identity. Part of identity flexibility pattern |
| `Email` | string | No | Yes | | User's email address | Required field serving as a human-readable identifier and primary contact. Used for notifications, password resets, and user lookup |
| `DisplayName` | string? | Yes | No | | User's display name | Optional field for user-friendly name presentation in UI. Nullable to allow systems to generate names dynamically |
| `FirstLoginDateTime` | DateTime? | Yes | No | | Timestamp of first login | Nullable field tracking account activation. Enables identifying newly activated accounts and user lifecycle analytics |
| `LastLoginDateTime` | DateTime? | Yes | No | | Timestamp of most recent login | Nullable field for activity tracking. Used for user engagement metrics and inactivity detection across the entire portal |
| `BannerPictureUrl` | string? | Yes | No | | URL to user's banner picture | Optional field for profile customization. Enables user personalization without storing large binary data |
| `ProfilePictureUrl` | string? | Yes | No | | URL to user's profile picture | Optional field for profile customization. Enables user identification in lists and profiles without storing binary data |
| `InactivityNotifications` | List<UserInactivityNotifications>? | Yes | No | | Collection of inactivity notifications sent to user | Nullable collection tracking notifications sent during inactivity periods. Enables audit trail for notification events |
| `Achievements` | ICollection<UserAchievement> | No | Yes | | Collection of user's earned achievements | Relationship collection tracking gamification milestones. Initialized as empty list to prevent null exceptions |
| `TelemetryEvents` | ICollection<TelemetryEvent> | No | Yes | | Collection of telemetry events performed by user | Relationship collection tracking user actions for analytics and behavior insights. Initialized as empty list |
| `RecentLinks` | ICollection<UserRecentLink> | No | Yes | | Collection of user's recently accessed links | Relationship collection enabling quick navigation to frequently used resources. Initialized as empty list |
| `UserRoles` | ICollection<UserRoleLinks> | No | Yes | | Collection of user's role assignments | Relationship collection managing authorization roles. Initialized as empty list |
| `UserSettings` | UserSettings? | Yes | No | | Navigation to user's settings | Optional one-to-one navigation to user preferences and configuration. Nullable if user hasn't customized settings |
| `OpenDataSubmissions` | ICollection<OpenDataSubmission>? | Yes | No | | Collection of Open Data submissions by user | Nullable collection tracking user contributions to open data initiatives. Enables null when not applicable |
| `Timestamp` | byte[]? | Yes | No | | Concurrency control timestamp | Optional byte array used by Entity Framework Core for optimistic concurrency control. Automatically managed by database |

## Design Patterns

### Identity Flexibility
- **Multiple identity sources**: A `PortalUser` can be linked to either an `ExternalUser` OR an `EntraUser` (enforced by validation)
- **Validation logic**: The `Validate` method ensures every portal user has at least one external identity
- **Decoupled identity**: Email is the primary human-readable identifier, independent of authentication source

### Activity Tracking
- **Dual timestamp fields**: Both `FirstLoginDateTime` and `LastLoginDateTime` are nullable to track user lifecycle
- **Portal-level tracking**: Unlike `ExternalUser` which tracks external-system-specific logins, `PortalUser` tracks overall portal engagement
- **DateTime vs DateTimeOffset**: Uses `DateTime` (UTC presumed) rather than `DateTimeOffset` for simplified activity tracking

### User Enrichment
- **Profile customization**: Picture URLs enable visual identification without storing binary data
- **Display name flexibility**: Optional display name allows customization or dynamic generation
- **Nullable optional fields**: Most fields are nullable to support progressive data completion and optional user customization

### Relationship Management
- **Collections initialization**: All relationship collections are initialized as empty lists to prevent null reference exceptions and enable LINQ queries
- **Concurrency control**: Timestamp field supports optimistic concurrency control in multi-user scenarios
- **User settings hierarchy**: One-to-one relationship with `UserSettings` for organizing additional user configuration

### Utility Functions
- **GetUserAchievements()**: Returns achievements ordered by ID and unlock date for chronological presentation
- **GetUnEarnedAchievements()**: Enables achievement progress tracking and gamification suggestions

## Relationship Architecture

```
PortalUser (1) <---> (1) ExternalUser
           ?         
           ?? Deactivation audit trail (ExternalUser.DeactivatedByUser)

PortalUser (1) <---> (1) EntraUser

PortalUser (1) <---> (?) UserAchievement
           (1) <---> (?) TelemetryEvent
           (1) <---> (?) UserRecentLink
           (1) <---> (?) UserRoleLinks
           (1) <---> (?) UserInactivityNotifications

PortalUser (1) <---> (1) UserSettings

PortalUser (1) <---> (?) OpenDataSubmission
```

## Identity Strategy

The system supports two authentication pathways:

1. **External Identity (GCCF)**
   - Users authenticated via Government of Canada Cloud Federation
   - Linked through `ExternalUser` entity
   - Tracks invitation lifecycle, external login activity

2. **Entra Identity (Microsoft)**
   - Users authenticated via Microsoft Entra (Azure AD)
   - Linked through `EntraUser` entity
   - Leverages enterprise directory integration

Every `PortalUser` must have at least one identity source, enforced by validation logic.

## Usage Scenarios

1. **User Registration**: Create `PortalUser` with email, optionally linked to identity source
2. **Activity Tracking**: Update `LastLoginDateTime` on each portal login
3. **Achievement System**: Query `Achievements` collection for user progress
4. **Role-Based Access**: Check `UserRoles` collection for authorization decisions
5. **User Enrichment**: Lazy-load `UserSettings` for personalization
6. **Telemetry Analysis**: Query `TelemetryEvents` for user behavior analytics
7. **Audit Deactivation**: Track deactivation through `ExternalUser.DeactivatedByUser` reference
8. **Inactivity Handling**: Query `LastLoginDateTime` and trigger `InactivityNotifications`
