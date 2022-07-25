using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Metadata.Migrations
{
    public partial class AddBilingualUrlsToCatalog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Language",
                table: "CatalogObjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Url_English_TXT",
                table: "CatalogObjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url_French_TXT",
                table: "CatalogObjects",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "CatalogObjects");

            migrationBuilder.DropColumn(
                name: "Url_English_TXT",
                table: "CatalogObjects");

            migrationBuilder.DropColumn(
                name: "Url_French_TXT",
                table: "CatalogObjects");
        }
    }
}
