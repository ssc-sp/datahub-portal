﻿using System;

namespace Datahub.Core.Model.Achievements;

public class TelemetryEvent
{
    public int Id { get; set; }
    public int PortalUserId { get; set; }
    public string EventName { get; set; }
    public DateTime EventDate { get; set; }

    #region Navigation props
    public virtual PortalUser PortalUser { get; set; }
    #endregion
}

public struct TelemetryEvents
{
    // EXP
    public const string UserLogin = "user_login";
    public const string UserLoginMultipleDays = "user_daily_login";
    public const string UserViewFileExplorer = "user_view_file_explorer";
    public const string UserOpenDatabricks = "user_click_databricks_link";
    public const string UserVisitResources = "user_visit_resources";
    public const string UserViewProject = "user_view_project";
    public const string UserViewProjectNotMemberOf = "user_view_project_not_member_of";
    public const string UserViewProfile = "user_view_profile";
    public const string UserViewOtherProfile = "user_view_other_profile";
    public const string UserRecentLink = "user_click_recent_link";
    public const string UserToggleCulture = "user_click_toggle_culture";

    // PRJ
    public const string UserSentInvite = "user_sent_invite";
    public const string UserJoinedProject = "user_joined_project";
    public const string UserLeftProject = "user_left_project";
    public const string UserUploadFile = "user_upload_file";
    public const string UserShareFile = "user_share_file";
    public const string UserDownloadFile = "user_download_file";
    public const string UserDeleteFile = "user_delete_file";
    public const string UserCreateFolder = "user_create_folder";
    public const string UserDeletedFolder = "user_delete_folder";
}
