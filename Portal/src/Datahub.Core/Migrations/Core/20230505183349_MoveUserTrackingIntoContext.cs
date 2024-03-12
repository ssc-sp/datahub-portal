using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class MoveUserTrackingIntoContext : Migration
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

            migrationBuilder.CreateTable(
                name: "UserRecentLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LinkType = table.Column<int>(type: "int", nullable: false),
                    PowerBIURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Variant = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatabricksURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebFormsURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataProject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PBIReportId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PBIWorkspaceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    accessedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExternalUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceArticleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceArticleTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRecentLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRecentLink_PortalUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcceptedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserId);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_UserRecentLink_UserId",
                table: "UserRecentLink",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRecentLink");

            migrationBuilder.DropTable(
                name: "UserSettings");

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
