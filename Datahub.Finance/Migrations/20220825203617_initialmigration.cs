using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Finance.Migrations
{
    public partial class initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FiscalYears",
                columns: table => new
                {
                    YearId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalYears", x => x.YearId);
                });

            migrationBuilder.CreateTable(
                name: "HierarchyLevels",
                columns: table => new
                {
                    HierarchyLevelID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FCCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ParentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FundCenterNameEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FundCenterNameFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FundCenterModifiedEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FundCenterModifiedFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HierarchyLevels", x => x.HierarchyLevelID);
                });

            migrationBuilder.CreateTable(
                name: "FundCenters",
                columns: table => new
                {
                    FundCenter_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYearYearId = table.Column<int>(type: "int", nullable: false),
                    SectorHierarchyLevelID = table.Column<int>(type: "int", nullable: false),
                    BranchHierarchyLevelID = table.Column<int>(type: "int", nullable: false),
                    DivisionHierarchyLevelID = table.Column<int>(type: "int", nullable: false),
                    AttritionRate = table.Column<double>(type: "float", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundCenters", x => x.FundCenter_ID);
                    table.ForeignKey(
                        name: "FK_FundCenters_FiscalYears_FiscalYearYearId",
                        column: x => x.FiscalYearYearId,
                        principalTable: "FiscalYears",
                        principalColumn: "YearId");
                    table.ForeignKey(
                        name: "FK_FundCenters_HierarchyLevels_BranchHierarchyLevelID",
                        column: x => x.BranchHierarchyLevelID,
                        principalTable: "HierarchyLevels",
                        principalColumn: "HierarchyLevelID");
                    table.ForeignKey(
                        name: "FK_FundCenters_HierarchyLevels_DivisionHierarchyLevelID",
                        column: x => x.DivisionHierarchyLevelID,
                        principalTable: "HierarchyLevels",
                        principalColumn: "HierarchyLevelID");
                    table.ForeignKey(
                        name: "FK_FundCenters_HierarchyLevels_SectorHierarchyLevelID",
                        column: x => x.SectorHierarchyLevelID,
                        principalTable: "HierarchyLevels",
                        principalColumn: "HierarchyLevelID");
                });

            migrationBuilder.CreateTable(
                name: "Forecasts",
                columns: table => new
                {
                    Forecast_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FundCenter_ID = table.Column<int>(type: "int", nullable: true),
                    Employee_Planned_Staffing = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Employee_Last_Name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Employee_First_Name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Is_Indeterminate = table.Column<bool>(type: "bit", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Fund = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Start_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FTE = table.Column<double>(type: "float", nullable: true),
                    Salary = table.Column<double>(type: "float", nullable: true),
                    Incremental_Replacement = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Location_Of_Hiring = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Potential_Hiring_Process = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forecasts", x => x.Forecast_ID);
                    table.ForeignKey(
                        name: "FK_Forecasts_FundCenters_FundCenter_ID",
                        column: x => x.FundCenter_ID,
                        principalTable: "FundCenters",
                        principalColumn: "FundCenter_ID");
                });

            migrationBuilder.CreateTable(
                name: "SummaryForecasts",
                columns: table => new
                {
                    Forecast_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FundCenter_ID = table.Column<int>(type: "int", nullable: true),
                    Fund = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Key_Activity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Key_Driver = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Budget = table.Column<double>(type: "float", nullable: true),
                    SFT_Forecast = table.Column<double>(type: "float", nullable: true),
                    THC = table.Column<double>(type: "float", nullable: true),
                    Other_OnM = table.Column<double>(type: "float", nullable: true),
                    Personel = table.Column<double>(type: "float", nullable: true),
                    Non_Personel = table.Column<double>(type: "float", nullable: true),
                    Grants = table.Column<double>(type: "float", nullable: true),
                    Contribution = table.Column<double>(type: "float", nullable: true),
                    Total_Forecast = table.Column<double>(type: "float", nullable: true),
                    AdditionalNotes = table.Column<double>(type: "float", nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummaryForecasts", x => x.Forecast_ID);
                    table.ForeignKey(
                        name: "FK_SummaryForecasts_FundCenters_FundCenter_ID",
                        column: x => x.FundCenter_ID,
                        principalTable: "FundCenters",
                        principalColumn: "FundCenter_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Forecasts_FundCenter_ID",
                table: "Forecasts",
                column: "FundCenter_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FundCenters_BranchHierarchyLevelID",
                table: "FundCenters",
                column: "BranchHierarchyLevelID");

            migrationBuilder.CreateIndex(
                name: "IX_FundCenters_DivisionHierarchyLevelID",
                table: "FundCenters",
                column: "DivisionHierarchyLevelID");

            migrationBuilder.CreateIndex(
                name: "IX_FundCenters_FiscalYearYearId",
                table: "FundCenters",
                column: "FiscalYearYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FundCenters_SectorHierarchyLevelID",
                table: "FundCenters",
                column: "SectorHierarchyLevelID");

            migrationBuilder.CreateIndex(
                name: "IX_SummaryForecasts_FundCenter_ID",
                table: "SummaryForecasts",
                column: "FundCenter_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Forecasts");

            migrationBuilder.DropTable(
                name: "SummaryForecasts");

            migrationBuilder.DropTable(
                name: "FundCenters");

            migrationBuilder.DropTable(
                name: "FiscalYears");

            migrationBuilder.DropTable(
                name: "HierarchyLevels");
        }
    }
}
