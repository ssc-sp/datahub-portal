using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class Fixeddatattypesagain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidationCode",
                table: "ExternalPowerBiReports");

            migrationBuilder.AddColumn<string>(
                name: "Validation_Code",
                table: "ExternalPowerBiReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Validation_Code",
                table: "ExternalPowerBiReports");

            migrationBuilder.AddColumn<byte[]>(
                name: "ValidationCode",
                table: "ExternalPowerBiReports",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
