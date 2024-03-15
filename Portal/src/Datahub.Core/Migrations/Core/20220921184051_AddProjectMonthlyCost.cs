using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core;

public partial class AddProjectMonthlyCost : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Project_Current_Monthly_Costs",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                TotalCostUSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ProjectAcronym = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Project_Current_Monthly_Costs", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Project_Current_Monthly_Costs");
    }
}