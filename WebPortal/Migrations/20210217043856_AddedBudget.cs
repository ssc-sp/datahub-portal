using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations
{
    public partial class AddedBudget : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<int>(
            //    name: "SectorBranch_ID",
            //    table: "SectorAndBranches",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Budget_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Division_NUM = table.Column<double>(type: "float", maxLength: 15, nullable: true),
                    SubLevel_TXT = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Departmental_Priorities_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sector_Priorities_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Key_Activity_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fund_NUM = table.Column<int>(type: "int", maxLength: 20, nullable: true),
                    Funding_Type_TXT = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Program_Activity_TXT = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Budget_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Anticipated_Transfers_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Revised_Budget_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Allocation_Percentage_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Indeterminate_Fte_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Indeterminate__Amount_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Determinate_Fte_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Determinate__Amount_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Planned_Staffing_Fte_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Planned_Staffing__Amount_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Total_Salary_Fte_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Total_Salary__Amount_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Information_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Information_Three_Year_Average_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Machine_and_Equipment_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Machine_and_Equipment_Three_Year_Average_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Professional_Seervices_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Professional_SeervicesThree_Year_Average_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Repairs_and_Maintenance_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Repairs_and_MaintenanceThree_Year_Average_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Rentals_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Rentals_Three_Year_Average_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Transportation_and_Communication_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Transportation_and_Communication_Three_Year_Average_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Utilities_Materials_and_Supplies_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Utilities_Materials_and_Supplies_Three_Year_Average_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Other_Payments_and_Ogd_Recoveries_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Other_Payments_and_Ogd_Recoveries_Three_Year_Average_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Total_OnM_Forecast_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Personnel_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    NonPersonnel_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Total_Capital_Forecast_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Grants_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Contributions_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Total_GnC_Forecast_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Total_Forecasted_Expenditures_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Adjustments_To_Forecast_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Attrition_Percentage_PCT = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Forecast_Adjustment_For_Salary_Attrition_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Forecast_Adjustment_For_Risk_Management_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Revised_Forecasts_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Percent_Of_Forecast_To_Budget_NUM = table.Column<double>(type: "float", maxLength: 20, nullable: true),
                    Comments_Notes_For_Financial_Information_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Involves_An_It_Or_Real_Property_Component_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Comments_Notes_For_NonFinancial_Information_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    SectorAndBranchSectorBranch_ID = table.Column<int>(type: "int", nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Budget_ID);
                    table.ForeignKey(
                        name: "FK_Budgets_SectorAndBranches_SectorAndBranchSectorBranch_ID",
                        column: x => x.SectorAndBranchSectorBranch_ID,
                        principalTable: "SectorAndBranches",
                        principalColumn: "SectorBranch_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_SectorAndBranchSectorBranch_ID",
                table: "Budgets",
                column: "SectorAndBranchSectorBranch_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Budgets");

            migrationBuilder.AlterColumn<int>(
                name: "SectorBranch_ID",
                table: "SectorAndBranches",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }
    }
}
