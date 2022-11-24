using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class Fixeddatattypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Validation_Code",
                table: "ExternalPowerBiReports");

            migrationBuilder.DropColumn(
                name: "Validation_Salt",
                table: "ExternalPowerBiReports");

            migrationBuilder.AddColumn<byte[]>(
                name: "ValidationCode",
                table: "ExternalPowerBiReports",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ValidationSalt",
                table: "ExternalPowerBiReports",
                type: "varbinary(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidationCode",
                table: "ExternalPowerBiReports");

            migrationBuilder.DropColumn(
                name: "ValidationSalt",
                table: "ExternalPowerBiReports");

            migrationBuilder.AddColumn<string>(
                name: "Validation_Code",
                table: "ExternalPowerBiReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Validation_Salt",
                table: "ExternalPowerBiReports",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
