using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace Datahub.Core.Model.Achievements.Configuration;

internal class AchivementConfiguration : IEntityTypeConfiguration<Achievement>
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

    static IEnumerable<Achievement> GetAchievements()
    {
        return new List<Achievement>
        {
            #region `DHA` Datahub Achievements
            new Achievement
            (
                "DHA-001", "Collaboration Connoisseur", "Logged in to DataHub", 1, 
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserLogin}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "DHA-002", "Collaboration Commander", "Invite a user to your workspace", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserSentInvite}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "DHA-003",  "Workspace Warrior", "Join a workspace", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserJoinedProject}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "DHA-004", "Workspace Wanderlust", "Leave a workspace", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserLeftProject}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "DHA-005", "Consistent Contributor", "Login on multiple days", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserLoginMultipleDays}", EventMetrics) > 0"""
            ),
            #endregion
            
            #region `STR` Storage Explorer Achievements
            new Achievement
            (
                "STR-000", "Storage Savant", "Unlock all the 2.0 Storage Explorer achievements", 1,
                // rules
                """Utils.MetaAchievementPassed("STR-001", input1)""",
                """Utils.MetaAchievementPassed("STR-002", input1)""",
                """Utils.MetaAchievementPassed("STR-003", input1)""",
                """Utils.MetaAchievementPassed("STR-004", input1)""",
                """Utils.MetaAchievementPassed("STR-005", input1)""",
                """Utils.MetaAchievementPassed("STR-006", input1)"""
            ),
            new Achievement
            (
                "STR-001", "Unstoppable Uploader", "Upload a file using the workspace Storage Explorer", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserUploadFile}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "STR-002", "Storage Socialite", "Share a file using the workspace Storage Explorer", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserShareFile}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "STR-003", "File Fetcher", "Download a file using the workspace Storage Explorer", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserShareFile}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "STR-004", "Daredevil Deleter", "Delete a file from the workspace with the Storage Explorer", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserDeleteFile}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "STR-005", "Folder Fashionista", "Create a folder in the workspace's Storage Explorer", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserCreateFolder}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "STR-006", "Folder Farewell", "Delete a folder in the workspace's Storage Explorer", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserDeletedFolder}", EventMetrics) > 0"""
            ),
            #endregion

            #region `EXP` Exploration Achievements
            new Achievement
            (
                "EXP-000", "Explorer Extraordinaire", "Unlock all the 2.0 Exploration achievements", 1,
                // rules
                """Utils.MetaAchievementPassed("EXP-001", input1)""",
                """Utils.MetaAchievementPassed("EXP-002", input1)""",
                """Utils.MetaAchievementPassed("EXP-003", input1)""",
                """Utils.MetaAchievementPassed("EXP-004", input1)""",
                """Utils.MetaAchievementPassed("EXP-005", input1)""",
                """Utils.MetaAchievementPassed("EXP-006", input1)""",
                """Utils.MetaAchievementPassed("EXP-007", input1)""",
                """Utils.MetaAchievementPassed("EXP-008", input1)""",
                """Utils.MetaAchievementPassed("EXP-009", input1)"""
            ),
            new Achievement
            (
                "EXP-001", "Storage Safari", "Navigate to the Storage Explorer page of a workspace", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserViewFileExplorer}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "EXP-002", "Databricks Discovery", "Navigate to the Databricks page of a workspace", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserOpenDatabricks}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "EXP-003", "Resource Ranger", "View the resources section of DataHub", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserVisitResources}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "EXP-004", "Workspace Wanderer", "View a workspace you are not a member of", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserViewProjectNotMemberOf}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "EXP-005", "Workspace Wayfarer", "Visit one of your own workspaces", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserViewProject}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "EXP-006", "Link Legend", "Use a recent link to get to the same page again", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserRecentLink}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "EXP-007", "Prolific Polyglot", "Switch languages in the portal", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserToggleCulture}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "EXP-008", "Profile Peruser", "View your own profile page", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserViewProfile}", EventMetrics) > 0"""
            ),
            new Achievement
            (
                "EXP-009", "Profile Prowler", "View another person's profile", 1,
                // rules
                $"""Utils.EventMetricExactCount("{TelemetryEvents.UserViewOtherProfile}", EventMetrics) > 0"""
            )
            #endregion
        };
    }
}
