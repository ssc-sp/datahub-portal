using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class AchievementRuleFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-001",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_login\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-002",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_sent_invite\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-003",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_joined_project\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-004",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_left_project\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "DHA-005",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_daily_login\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-000",
                column: "ConcatenatedRules",
                value: "Utils.OwnsAchievement(\"EXP-001\", achivements)\nUtils.OwnsAchievement(\"EXP-002\", achivements)\nUtils.OwnsAchievement(\"EXP-003\", achivements)\nUtils.OwnsAchievement(\"EXP-004\", achivements)\nUtils.OwnsAchievement(\"EXP-005\", achivements)\nUtils.OwnsAchievement(\"EXP-006\", achivements)\nUtils.OwnsAchievement(\"EXP-007\", achivements)\nUtils.OwnsAchievement(\"EXP-008\", achivements)\nUtils.OwnsAchievement(\"EXP-009\", achivements)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-001",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_view_file_explorer\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-002",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_click_databricks_link\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-003",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_visit_resources\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-004",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_view_project_not_member_of\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-005",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_view_project\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-006",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_click_recent_link\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-007",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_click_toggle_culture\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-008",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_view_profile\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-009",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_view_other_profile\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-000",
                column: "ConcatenatedRules",
                value: "Utils.OwnsAchievement(\"STR-001\", achivements)\nUtils.OwnsAchievement(\"STR-002\", achivements)\nUtils.OwnsAchievement(\"STR-003\", achivements)\nUtils.OwnsAchievement(\"STR-004\", achivements)\nUtils.OwnsAchievement(\"STR-005\", achivements)\nUtils.OwnsAchievement(\"STR-006\", achivements)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-001",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_upload_file\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-002",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_share_file\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-003",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_download_file\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-004",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_delete_file\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-005",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_create_folder\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-006",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_delete_folder\", currentMetric)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
