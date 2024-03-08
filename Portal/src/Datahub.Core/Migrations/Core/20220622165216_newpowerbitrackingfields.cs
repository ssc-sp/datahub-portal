using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core;

public partial class Newpowerbitrackingfields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "End_Date",
            table: "ExternalPowerBiReports",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<bool>(
            name: "Is_Created",
            table: "ExternalPowerBiReports",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "End_Date",
            table: "ExternalPowerBiReports");

        migrationBuilder.DropColumn(
            name: "Is_Created",
            table: "ExternalPowerBiReports");
    }
}