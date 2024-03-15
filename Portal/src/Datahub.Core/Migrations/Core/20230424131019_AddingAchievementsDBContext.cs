using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class AddingAchievementsDBContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achivements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    ConcatenatedRules = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achivements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PortalUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GraphGuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FirstLoginDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BackgroundImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HideAchievements = table.Column<bool>(type: "bit", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortalUserId = table.Column<int>(type: "int", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelemetryEvents_PortalUsers_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortalUserId = table.Column<int>(type: "int", nullable: false),
                    AchievementId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Count = table.Column<int>(type: "int", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achivements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achivements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAchievements_PortalUsers_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Achivements",
                columns: new[] { "Id", "ConcatenatedRules", "Description", "Name", "Points" },
                values: new object[,]
                {
                    { "DHA-001", "Utils.EventMetricExactCount(\"user_login\", EventMetrics) > 0", "Logged in to DataHub", "Collaboration Connoisseur", 1 },
                    { "DHA-002", "Utils.EventMetricExactCount(\"user_sent_invite\", EventMetrics) > 0", "Invite a user to your workspace", "Collaboration Commander", 1 },
                    { "DHA-003", "Utils.EventMetricExactCount(\"user_joined_project\", EventMetrics) > 0", "Join a workspace", "Workspace Warrior", 1 },
                    { "DHA-004", "Utils.EventMetricExactCount(\"user_left_project\", EventMetrics) > 0", "Leave a workspace", "Workspace Wanderlust", 1 },
                    { "DHA-005", "Utils.EventMetricExactCount(\"user_daily_login\", EventMetrics) > 0", "Login on multiple days", "Consistent Contributor", 1 },
                    { "EXP-000", "Utils.MetaAchievementPassed(\"EXP-001\", input1)\nUtils.MetaAchievementPassed(\"EXP-002\", input1)\nUtils.MetaAchievementPassed(\"EXP-003\", input1)\nUtils.MetaAchievementPassed(\"EXP-004\", input1)\nUtils.MetaAchievementPassed(\"EXP-005\", input1)\nUtils.MetaAchievementPassed(\"EXP-006\", input1)\nUtils.MetaAchievementPassed(\"EXP-007\", input1)\nUtils.MetaAchievementPassed(\"EXP-008\", input1)\nUtils.MetaAchievementPassed(\"EXP-009\", input1)", "Unlock all the 2.0 Exploration achievements", "Explorer Extraordinaire", 1 },
                    { "EXP-001", "Utils.EventMetricExactCount(\"user_view_file_explorer\", EventMetrics) > 0", "Navigate to the Storage Explorer page of a workspace", "Storage Safari", 1 },
                    { "EXP-002", "Utils.EventMetricExactCount(\"user_click_databricks_link\", EventMetrics) > 0", "Navigate to the Databricks page of a workspace", "Databricks Discovery", 1 },
                    { "EXP-003", "Utils.EventMetricExactCount(\"user_visit_resources\", EventMetrics) > 0", "View the resources section of DataHub", "Resource Ranger", 1 },
                    { "EXP-004", "Utils.EventMetricExactCount(\"user_view_project_not_member_of\", EventMetrics) > 0", "View a workspace you are not a member of", "Workspace Wanderer", 1 },
                    { "EXP-005", "Utils.EventMetricExactCount(\"user_view_project\", EventMetrics) > 0", "Visit one of your own workspaces", "Workspace Wayfarer", 1 },
                    { "EXP-006", "Utils.EventMetricExactCount(\"user_click_recent_link\", EventMetrics) > 0", "Use a recent link to get to the same page again", "Link Legend", 1 },
                    { "EXP-007", "Utils.EventMetricExactCount(\"user_click_toggle_culture\", EventMetrics) > 0", "Switch languages in the portal", "Prolific Polyglot", 1 },
                    { "EXP-008", "Utils.EventMetricExactCount(\"user_view_profile\", EventMetrics) > 0", "View your own profile page", "Profile Peruser", 1 },
                    { "EXP-009", "Utils.EventMetricExactCount(\"user_view_other_profile\", EventMetrics) > 0", "View another person's profile", "Profile Prowler", 1 },
                    { "STR-000", "Utils.MetaAchievementPassed(\"STR-001\", input1)\nUtils.MetaAchievementPassed(\"STR-002\", input1)\nUtils.MetaAchievementPassed(\"STR-003\", input1)\nUtils.MetaAchievementPassed(\"STR-004\", input1)\nUtils.MetaAchievementPassed(\"STR-005\", input1)\nUtils.MetaAchievementPassed(\"STR-006\", input1)", "Unlock all the 2.0 Storage Explorer achievements", "Storage Savant", 1 },
                    { "STR-001", "Utils.EventMetricExactCount(\"user_upload_file\", EventMetrics) > 0", "Upload a file using the workspace Storage Explorer", "Unstoppable Uploader", 1 },
                    { "STR-002", "Utils.EventMetricExactCount(\"user_share_file\", EventMetrics) > 0", "Share a file using the workspace Storage Explorer", "Storage Socialite", 1 },
                    { "STR-003", "Utils.EventMetricExactCount(\"user_share_file\", EventMetrics) > 0", "Download a file using the workspace Storage Explorer", "File Fetcher", 1 },
                    { "STR-004", "Utils.EventMetricExactCount(\"user_delete_file\", EventMetrics) > 0", "Delete a file from the workspace with the Storage Explorer", "Daredevil Deleter", 1 },
                    { "STR-005", "Utils.EventMetricExactCount(\"user_create_folder\", EventMetrics) > 0", "Create a folder in the workspace's Storage Explorer", "Folder Fashionista", 1 },
                    { "STR-006", "Utils.EventMetricExactCount(\"user_delete_folder\", EventMetrics) > 0", "Delete a folder in the workspace's Storage Explorer", "Folder Farewell", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortalUsers_GraphGuid",
                table: "PortalUsers",
                column: "GraphGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryEvents_PortalUserId",
                table: "TelemetryEvents",
                column: "PortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_PortalUserId",
                table: "UserAchievements",
                column: "PortalUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelemetryEvents");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "Achivements");

            migrationBuilder.DropTable(
                name: "PortalUsers");
        }
    }
}
