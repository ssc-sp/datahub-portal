using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class AddingMonthlyUsageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Sector_Name",
                table: "Projects",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "Project_Status_Desc",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            //migrationBuilder.AddColumn<int>(
            //    name: "Project_Status",
            //    table: "Projects",
            //    type: "int",
            //    nullable: true);

            migrationBuilder.CreateTable(
                name: "Project_MonthlyUsage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentCost = table.Column<double>(type: "float", nullable: false),
                    CurrentCostPerService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YesterdayCost = table.Column<double>(type: "float", nullable: false),
                    YesterdayCostPerService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentStorageUsage = table.Column<long>(type: "bigint", nullable: false),
                    StorageLastNotified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StoragePercNotified = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_MonthlyUsage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_MonthlyUsage_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Project_MonthlyUsage_ProjectId",
                table: "Project_MonthlyUsage",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_MonthlyUsage");

            //migrationBuilder.DropColumn(
            //    name: "Project_Status",
            //    table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "Sector_Name",
                table: "Projects",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Project_Status_Desc",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
