using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class AddProjectInactivityNotificationFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCostRecovery",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OperationalWindow",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectInactivityNotifications",
                columns: table => new
                {
                    Project_ID = table.Column<int>(type: "int", nullable: false),
                    NotificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysBeforeDeletion = table.Column<int>(type: "int", nullable: false),
                    SentTo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInactivityNotifications", x => x.Project_ID);
                    table.ForeignKey(
                        name: "FK_ProjectInactivityNotifications_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectInactivityNotifications");

            migrationBuilder.DropColumn(
                name: "HasCostRecovery",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OperationalWindow",
                table: "Projects");
        }
    }
}
