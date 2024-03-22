using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core;

public partial class PowerBiEmbededData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ExternalPowerBiReports",
            columns: table => new
            {
                ExternalPowerBiReport_ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Report_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExternalPowerBiReports", x => x.ExternalPowerBiReport_ID);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ExternalPowerBiReports");

        migrationBuilder.AddCheckConstraint(
            name: "CHK_DB_Type",
            table: "Projects",
            sql: "DB_Type in ('SQL Server', 'Postgres')");
    }
}