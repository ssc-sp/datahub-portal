using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.Finance
{
    public partial class summaryforecasttable2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Summaryorecasts",
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
                    table.PrimaryKey("PK_Summaryorecasts", x => x.Forecast_ID);
                    table.ForeignKey(
                        name: "FK_Summaryorecasts_FundCenters_FundCenter_ID",
                        column: x => x.FundCenter_ID,
                        principalTable: "FundCenters",
                        principalColumn: "FundCenter_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Summaryorecasts_FundCenter_ID",
                table: "Summaryorecasts",
                column: "FundCenter_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Summaryorecasts");
        }
    }
}
