using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Metadata.Migrations
{
    public partial class UpdateCatalogStructureWithDataType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Branch_NUM",
                table: "CatalogObjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Contact_TXT",
                table: "CatalogObjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "DataType",
                table: "CatalogObjects",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "Sector_NUM",
                table: "CatalogObjects",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branch_NUM",
                table: "CatalogObjects");

            migrationBuilder.DropColumn(
                name: "Contact_TXT",
                table: "CatalogObjects");

            migrationBuilder.DropColumn(
                name: "DataType",
                table: "CatalogObjects");

            migrationBuilder.DropColumn(
                name: "Sector_NUM",
                table: "CatalogObjects");
        }
    }
}
