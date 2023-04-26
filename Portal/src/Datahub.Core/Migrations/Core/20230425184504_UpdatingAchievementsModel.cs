using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class UpdatingAchievementsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BackgroundImageUrl",
                table: "PortalUsers",
                newName: "DisplayName");

            migrationBuilder.AddColumn<string>(
                name: "BannerPictureUrl",
                table: "PortalUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "PortalUsers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-001",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_login\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-002",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_sent_invite\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-003",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_joined_project\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-004",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_left_project\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-005",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_daily_login\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-000",
                column: "ConcatenatedRules",
                value: "Utils.OwnsAchievement(\"EXP-001\", state)\nUtils.OwnsAchievement(\"EXP-002\", state)\nUtils.OwnsAchievement(\"EXP-003\", state)\nUtils.OwnsAchievement(\"EXP-004\", state)\nUtils.OwnsAchievement(\"EXP-005\", state)\nUtils.OwnsAchievement(\"EXP-006\", state)\nUtils.OwnsAchievement(\"EXP-007\", state)\nUtils.OwnsAchievement(\"EXP-008\", state)\nUtils.OwnsAchievement(\"EXP-009\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-001",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_view_file_explorer\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-002",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_click_databricks_link\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-003",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_visit_resources\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-004",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_view_project_not_member_of\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-005",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_view_project\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-006",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_click_recent_link\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-007",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_click_toggle_culture\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-008",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_view_profile\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-009",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_view_other_profile\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-000",
                column: "ConcatenatedRules",
                value: "Utils.OwnsAchievement(\"STR-001\", state)\nUtils.OwnsAchievement(\"STR-002\", state)\nUtils.OwnsAchievement(\"STR-003\", state)\nUtils.OwnsAchievement(\"STR-004\", state)\nUtils.OwnsAchievement(\"STR-005\", state)\nUtils.OwnsAchievement(\"STR-006\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-001",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_upload_file\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-002",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_share_file\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-003",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_share_file\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-004",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_delete_file\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-005",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_create_folder\", state)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-006",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_delete_folder\", state)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerPictureUrl",
                table: "PortalUsers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "PortalUsers");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "PortalUsers",
                newName: "BackgroundImageUrl");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-001",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_login\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-002",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_sent_invite\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-003",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_joined_project\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-004",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_left_project\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-005",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_daily_login\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-000",
                column: "ConcatenatedRules",
                value: "Utils.MetaAchievementPassed(\"EXP-001\", input1)\nUtils.MetaAchievementPassed(\"EXP-002\", input1)\nUtils.MetaAchievementPassed(\"EXP-003\", input1)\nUtils.MetaAchievementPassed(\"EXP-004\", input1)\nUtils.MetaAchievementPassed(\"EXP-005\", input1)\nUtils.MetaAchievementPassed(\"EXP-006\", input1)\nUtils.MetaAchievementPassed(\"EXP-007\", input1)\nUtils.MetaAchievementPassed(\"EXP-008\", input1)\nUtils.MetaAchievementPassed(\"EXP-009\", input1)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-001",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_view_file_explorer\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-002",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_click_databricks_link\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-003",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_visit_resources\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-004",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_view_project_not_member_of\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-005",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_view_project\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-006",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_click_recent_link\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-007",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_click_toggle_culture\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-008",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_view_profile\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-009",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_view_other_profile\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-000",
                column: "ConcatenatedRules",
                value: "Utils.MetaAchievementPassed(\"STR-001\", input1)\nUtils.MetaAchievementPassed(\"STR-002\", input1)\nUtils.MetaAchievementPassed(\"STR-003\", input1)\nUtils.MetaAchievementPassed(\"STR-004\", input1)\nUtils.MetaAchievementPassed(\"STR-005\", input1)\nUtils.MetaAchievementPassed(\"STR-006\", input1)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-001",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_upload_file\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-002",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_share_file\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-003",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_share_file\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-004",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_delete_file\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-005",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_create_folder\", EventMetrics) > 0");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-006",
                column: "ConcatenatedRules",
                value: "Utils.EventMetricExactCount(\"user_delete_folder\", EventMetrics) > 0");
        }
    }
}
