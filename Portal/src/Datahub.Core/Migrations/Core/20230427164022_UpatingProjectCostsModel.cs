using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class UpatingProjectCostsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_Current_Monthly_Costs");

            migrationBuilder.DropTable(
                name: "Project_MonthlyUsage");

            migrationBuilder.DropColumn(
                name: "Project_Acronym_CD",
                table: "Project_Costs");

            migrationBuilder.DropColumn(
                name: "Updated_DT",
                table: "Project_Costs");

            migrationBuilder.RenameColumn(
                name: "Usage_DT",
                table: "Project_Costs",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Cost_AMT",
                table: "Project_Costs",
                newName: "CadCost");

            migrationBuilder.AddColumn<string>(
                name: "CloudProvider",
                table: "Project_Costs",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "Project_Costs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloudProvider",
                table: "Project_Costs");

            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "Project_Costs");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Project_Costs",
                newName: "Usage_DT");

            migrationBuilder.RenameColumn(
                name: "CadCost",
                table: "Project_Costs",
                newName: "Cost_AMT");

            migrationBuilder.AddColumn<string>(
                name: "Project_Acronym_CD",
                table: "Project_Costs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated_DT",
                table: "Project_Costs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Project_Current_Monthly_Costs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectAcronym = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCostUSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Current_Monthly_Costs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Project_MonthlyUsage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ConsumeLastNotified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsumePercNotified = table.Column<int>(type: "int", nullable: true),
                    CurrentCost = table.Column<double>(type: "float", nullable: false),
                    CurrentCostPerDay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentCostPerService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentStorageUsage = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StorageLastNotified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StoragePercNotified = table.Column<int>(type: "int", nullable: true),
                    YesterdayCost = table.Column<double>(type: "float", nullable: false),
                    YesterdayCostPerService = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
    }
}
