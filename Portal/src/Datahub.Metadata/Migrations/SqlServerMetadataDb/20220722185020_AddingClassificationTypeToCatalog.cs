using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Metadata.Migrations
{
    public partial class AddingClassificationTypeToCatalog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Classification_Type",
                table: "CatalogObjects",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Classification_Type",
                table: "CatalogObjects");
        }
    }
}
