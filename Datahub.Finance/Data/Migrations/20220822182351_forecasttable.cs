using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.Finance
{
    public partial class forecasttable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Forecasts",
                columns: table => new
                {
                    Forecast_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FundCenter_ID = table.Column<int>(type: "int", nullable: true),
                    Employee_Planned_Staffing = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Employee_Last_Name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Employee_First_Name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Is_Indeterminate = table.Column<bool>(type: "bit", nullable: false),
                    Fund = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Start_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FTE = table.Column<double>(type: "float", nullable: true),
                    Salary = table.Column<double>(type: "float", nullable: true),
                    Incremental_Replacement = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Location_Of_Hiring = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Potential_Hiring_Process = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_Forecasts_FundCenter_ID",
                table: "Forecasts",
                column: "FundCenter_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Forecasts");
        }
    }
}
