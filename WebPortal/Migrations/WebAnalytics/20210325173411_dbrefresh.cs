using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations.WebAnalytics
{
    public partial class dbrefresh : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebAnalytics",
                columns: table => new
                {
                    WebAnalytics_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Owner = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    URL = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Measurement_Original = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Pageview_Measurement = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Download_Measurement = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Clickout_Measurement = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Clickout_URL = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Pageview = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Downloads = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OutboundLink = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Indicator = table.Column<int>(type: "int", nullable: false),
                    Lang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HTTP_Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebAnalytics", x => x.WebAnalytics_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebAnalytics");
        }
    }
}
