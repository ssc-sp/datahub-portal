using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations
{
    public partial class SectorBranchLookup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "Revised_Budget_NUM",
            //    table: "Budgets");

            //migrationBuilder.DropColumn(
            //    name: "Revised_Forecasts_NUM",
            //    table: "Budgets");

            //migrationBuilder.DropColumn(
            //    name: "Total_Capital_Forecast_NUM",
            //    table: "Budgets");

            //migrationBuilder.DropColumn(
            //    name: "Total_Forecasted_Expenditures_NUM",
            //    table: "Budgets");

            //migrationBuilder.DropColumn(
            //    name: "Total_GnC_Forecast_NUM",
            //    table: "Budgets");

            //migrationBuilder.DropColumn(
            //    name: "Total_OnM_Forecast_NUM",
            //    table: "Budgets");

            //migrationBuilder.DropColumn(
            //    name: "Total_Salary_Fte_NUM",
            //    table: "Budgets");

            //migrationBuilder.DropColumn(
            //    name: "Total_Salary__Amount_NUM",
            //    table: "Budgets");

            //migrationBuilder.AlterColumn<int>(
            //    name: "SectorBranch_ID",
            //    table: "SectorAndBranches",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Budget_ID",
            //    table: "Budgets",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateTable(
                name: "Sectors",
                columns: table => new
                {
                    SectorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectorNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectorNameFr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sectors", x => x.SectorId);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchNameFr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.BranchId);
                    table.ForeignKey(
                        name: "FK_Branches_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "SectorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_SectorId",
                table: "Branches",
                column: "SectorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Sectors");

            migrationBuilder.AlterColumn<int>(
                name: "SectorBranch_ID",
                table: "SectorAndBranches",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Budget_ID",
                table: "Budgets",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<double>(
                name: "Revised_Budget_NUM",
                table: "Budgets",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Revised_Forecasts_NUM",
                table: "Budgets",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Total_Capital_Forecast_NUM",
                table: "Budgets",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Total_Forecasted_Expenditures_NUM",
                table: "Budgets",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Total_GnC_Forecast_NUM",
                table: "Budgets",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Total_OnM_Forecast_NUM",
                table: "Budgets",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Total_Salary_Fte_NUM",
                table: "Budgets",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Total_Salary__Amount_NUM",
                table: "Budgets",
                type: "float",
                nullable: true);
        }
    }
}
