using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class UpdatingAchievementModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EventName",
                table: "TelemetryEvents",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-001",
                column: "ConcatenatedRules",
                value: "Utils.MatchUrl(\"\\\\/w\\\\/([0-9a-zA-Z]+)?\\\\/filelist$\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-003",
                column: "ConcatenatedRules",
                value: "Utils.MatchUrl(\"\\\\/resources$\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-008",
                column: "ConcatenatedRules",
                value: "Utils.MatchUrl(\"\\\\/profile$\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-000",
                column: "ConcatenatedRules",
                value: "Utils.OwnsAchievement(\"STR-001\", achivements)\nUtils.OwnsAchievement(\"STR-003\", achivements)\nUtils.OwnsAchievement(\"STR-004\", achivements)\nUtils.OwnsAchievement(\"STR-005\", achivements)\nUtils.OwnsAchievement(\"STR-006\", achivements)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EventName",
                table: "TelemetryEvents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-001",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_view_file_explorer\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-003",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_visit_resources\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "EXP-008",
                column: "ConcatenatedRules",
                value: "Utils.MatchMetric(\"user_view_profile\", currentMetric)");

            migrationBuilder.UpdateData(
                table: "Achivements",
                keyColumn: "Id",
                keyValue: "STR-000",
                column: "ConcatenatedRules",
                value: "Utils.OwnsAchievement(\"STR-001\", achivements)\nUtils.OwnsAchievement(\"STR-002\", achivements)\nUtils.OwnsAchievement(\"STR-003\", achivements)\nUtils.OwnsAchievement(\"STR-004\", achivements)\nUtils.OwnsAchievement(\"STR-005\", achivements)\nUtils.OwnsAchievement(\"STR-006\", achivements)");
        }
    }
}
