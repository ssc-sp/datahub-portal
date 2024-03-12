using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class AddProjectCreditsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConsumeLastNotified",
                table: "Project_MonthlyUsage",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsumePercNotified",
                table: "Project_MonthlyUsage",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Project_Credits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Current = table.Column<double>(type: "float", nullable: false),
                    CurrentPerService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentPerDay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YesterdayCredits = table.Column<double>(type: "float", nullable: false),
                    YesterdayPerService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastNotified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PercNotified = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Credits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Credits_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Project_Credits_ProjectId",
                table: "Project_Credits",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_Credits");

            migrationBuilder.DropColumn(
                name: "ConsumeLastNotified",
                table: "Project_MonthlyUsage");

            migrationBuilder.DropColumn(
                name: "ConsumePercNotified",
                table: "Project_MonthlyUsage");
        }
    }
}
