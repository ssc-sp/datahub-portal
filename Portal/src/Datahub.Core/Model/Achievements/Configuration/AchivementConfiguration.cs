using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace Datahub.Core.Model.Achievements.Configuration;

public class AchivementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.HasData(GetAchievements());

        builder.ToTable("Achivements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
               .HasMaxLength(8);

        builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(128);

        builder.Property(e => e.Description)
               .IsRequired();

        builder.HasMany(e => e.UserAchievements)
               .WithOne(e => e.Achievement)
               .OnDelete(DeleteBehavior.Cascade);
    }

    public static IEnumerable<Achievement> GetAchievements()
    {
        return new List<Achievement>
        {
            #region `DHA` Datahub Achievements
            new Achievement
            (
                "DHA-001", "Collaboration Connoisseur", "Logged in to DataHub", 1, 
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserLogin}", state)"""
            ),
            new Achievement
            (
                "DHA-002", "Collaboration Commander", "Invite a user to your workspace", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserSentInvite}", state)"""
            ),
            new Achievement
            (
                "DHA-003",  "Workspace Warrior", "Join a workspace", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserJoinedProject}", state)"""
            ),
            new Achievement
            (
                "DHA-004", "Workspace Wanderlust", "Leave a workspace", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserLeftProject}", state)"""
            ),
            new Achievement
            (
                "DHA-005", "Consistent Contributor", "Login on multiple days", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserLoginMultipleDays}", state)"""
            ),
            #endregion
            
            #region `STR` Storage Explorer Achievements
            new Achievement
            (
                "STR-000", "Storage Savant", "Unlock all the 2.0 Storage Explorer achievements", 1,
                // rules
                """Utils.OwnsAchievement("STR-001", state)""",
                """Utils.OwnsAchievement("STR-002", state)""",
                """Utils.OwnsAchievement("STR-003", state)""",
                """Utils.OwnsAchievement("STR-004", state)""",
                """Utils.OwnsAchievement("STR-005", state)""",
                """Utils.OwnsAchievement("STR-006", state)"""
            ),
            new Achievement
            (
                "STR-001", "Unstoppable Uploader", "Upload a file using the workspace Storage Explorer", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserUploadFile}", state)"""
            ),
            new Achievement
            (
                "STR-002", "Storage Socialite", "Share a file using the workspace Storage Explorer", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserShareFile}", state)"""
            ),
            new Achievement
            (
                "STR-003", "File Fetcher", "Download a file using the workspace Storage Explorer", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserShareFile}", state)"""
            ),
            new Achievement
            (
                "STR-004", "Daredevil Deleter", "Delete a file from the workspace with the Storage Explorer", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserDeleteFile}", state)"""
            ),
            new Achievement
            (
                "STR-005", "Folder Fashionista", "Create a folder in the workspace's Storage Explorer", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserCreateFolder}", state)"""
            ),
            new Achievement
            (
                "STR-006", "Folder Farewell", "Delete a folder in the workspace's Storage Explorer", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserDeletedFolder}", state)"""
            ),
            #endregion

            #region `EXP` Exploration Achievements
            new Achievement
            (
                "EXP-000", "Explorer Extraordinaire", "Unlock all the 2.0 Exploration achievements", 1,
                // rules
                """Utils.OwnsAchievement("EXP-001", state)""",
                """Utils.OwnsAchievement("EXP-002", state)""",
                """Utils.OwnsAchievement("EXP-003", state)""",
                """Utils.OwnsAchievement("EXP-004", state)""",
                """Utils.OwnsAchievement("EXP-005", state)""",
                """Utils.OwnsAchievement("EXP-006", state)""",
                """Utils.OwnsAchievement("EXP-007", state)""",
                """Utils.OwnsAchievement("EXP-008", state)""",
                """Utils.OwnsAchievement("EXP-009", state)"""
            ),
            new Achievement
            (
                "EXP-001", "Storage Safari", "Navigate to the Storage Explorer page of a workspace", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserViewFileExplorer}", state)"""
            ),
            new Achievement
            (
                "EXP-002", "Databricks Discovery", "Navigate to the Databricks page of a workspace", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserOpenDatabricks}", state)"""
            ),
            new Achievement
            (
                "EXP-003", "Resource Ranger", "View the resources section of DataHub", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserVisitResources}", state)"""
            ),
            new Achievement
            (
                "EXP-004", "Workspace Wanderer", "View a workspace you are not a member of", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserViewProjectNotMemberOf}", state)"""
            ),
            new Achievement
            (
                "EXP-005", "Workspace Wayfarer", "Visit one of your own workspaces", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserViewProject}", state)"""
            ),
            new Achievement
            (
                "EXP-006", "Link Legend", "Use a recent link to get to the same page again", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserRecentLink}", state)"""
            ),
            new Achievement
            (
                "EXP-007", "Prolific Polyglot", "Switch languages in the portal", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserToggleCulture}", state)"""
            ),
            new Achievement
            (
                "EXP-008", "Profile Peruser", "View your own profile page", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserViewProfile}", state)"""
            ),
            new Achievement
            (
                "EXP-009", "Profile Prowler", "View another person's profile", 1,
                // rules
                $"""Utils.MatchMetric("{TelemetryEvents.UserViewOtherProfile}", state)"""
            )
            #endregion
        };
    }
}
