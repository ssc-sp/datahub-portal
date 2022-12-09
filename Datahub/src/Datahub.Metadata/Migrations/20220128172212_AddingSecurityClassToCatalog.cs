using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Metadata.Migrations
{
    public partial class AddingSecurityClassToCatalog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location_TXT",
                table: "CatalogObjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityClass_TXT",
                table: "CatalogObjects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Unclassified");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location_TXT",
                table: "CatalogObjects");

            migrationBuilder.DropColumn(
                name: "SecurityClass_TXT",
                table: "CatalogObjects");
        }
    }
}
