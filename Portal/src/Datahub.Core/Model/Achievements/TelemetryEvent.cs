using Datahub.Core.Model.Users;

namespace Datahub.Core.Model.Achievements;

/// <summary>
/// Represents a telemetry event record, capturing user interactions and system events.
/// </summary>
public class TelemetryEvent
{
    /// <summary>
    /// Gets or sets the unique identifier for the telemetry event.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the portal user associated with this event.
    /// </summary>
    public int PortalUserId { get; set; }

    /// <summary>
    /// Gets or sets the name of the event.
    /// </summary>
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the event occurred.
    /// </summary>
    public DateTime EventDate { get; set; }

    #region Navigation props

    /// <summary>
    /// Gets or sets the navigation property for the portal user associated with this event.
    /// </summary>
    public virtual PortalUser PortalUser { get; set; } = null!;
    #endregion
}

/// <summary>
/// Defines a collection of constant strings representing telemetry event names.
/// These constants are used to categorize different types of user activities and system events.
/// </summary>
public struct TelemetryEvents
{
    /// <summary>
    /// Event for user login.
    /// </summary>
    public const string UserLogin = "user_login";

    /// <summary>
    /// Event for user login on multiple consecutive days.
    /// </summary>
    public const string UserLoginMultipleDays = "user_daily_login";

    /// <summary>
    /// Event for user viewing the file explorer.
    /// </summary>
    public const string UserViewFileExplorer = "user_view_file_explorer";

    /// <summary>
    /// Event for user clicking a Databricks link.
    /// </summary>
    public const string UserOpenDatabricks = "user_click_databricks_link";

    /// <summary>
    /// Event for user clicking an Azure Web App link.
    /// </summary>
    public const string UserOpenAzureWebApp = "user_click_azure_web_app_link";

    /// <summary>
    /// Event for user visiting the resources page.
    /// </summary>
    public const string UserVisitResources = "user_visit_resources";

    /// <summary>
    /// Event for user viewing a workspace.
    /// </summary>
    public const string UserViewProject = "user_view_project";

    /// <summary>
    /// Event for user viewing a workspace they are not a member of.
    /// </summary>
    public const string UserViewProjectNotMemberOf = "user_view_project_not_member_of";

    /// <summary>
    /// Event for user viewing their own profile.
    /// </summary>
    public const string UserViewProfile = "user_view_profile";

    /// <summary>
    /// Event for user viewing another user's profile.
    /// </summary>
    public const string UserViewOtherProfile = "user_view_other_profile";

    /// <summary>
    /// Event for user clicking a recent link.
    /// </summary>
    public const string UserRecentLink = "user_click_recent_link";

    /// <summary>
    /// Event for user toggling the culture/language.
    /// </summary>
    public const string UserToggleCulture = "user_click_toggle_culture";

    /// <summary>
    /// Event for a generic user button click.
    /// </summary>
    public const string UserClickButton = "user_click_button";

    // Workspaces

    /// <summary>
    /// Event for user sending an invitation to a workspace.
    /// </summary>
    public const string UserSentInvite = "user_sent_invite";

    /// <summary>
    /// Event for user joining a workspace.
    /// </summary>
    public const string UserJoinedProject = "user_joined_project";

    /// <summary>
    /// Event for user leaving a workspace.
    /// </summary>
    public const string UserLeftProject = "user_left_project";

    /// <summary>
    /// Event for user uploading a file.
    /// </summary>
    public const string UserUploadFile = "user_upload_file";

    /// <summary>
    /// Event for user sharing a file.
    /// </summary>
    public const string UserShareFile = "user_share_file";

    /// <summary>
    /// Event for user downloading a file.
    /// </summary>
    public const string UserDownloadFile = "user_download_file";

    /// <summary>
    /// Event for user deleting a file.
    /// </summary>
    public const string UserDeleteFile = "user_delete_file";

    /// <summary>
    /// Event for user creating a folder.
    /// </summary>
    public const string UserCreateFolder = "user_create_folder";

    /// <summary>
    /// Event for user deleting a folder.
    /// </summary>
    public const string UserDeletedFolder = "user_delete_folder";

    // AUDITING

    /// <summary>
    /// Event for user toggling the 'AllowSharedKeyAccess' setting for storage.
    /// </summary>
    public const string UserToggleStorageAllowSharedKeyAccess = "user_toggle_storage_allow_shared_key_access";
}
