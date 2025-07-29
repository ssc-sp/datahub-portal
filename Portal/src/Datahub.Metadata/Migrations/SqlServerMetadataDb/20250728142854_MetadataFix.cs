using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Metadata.Migrations.SqlServerMetadataDb
{
    /// <inheritdoc />
    public partial class MetadataFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branch_NUM",
                table: "CatalogObjects");

            migrationBuilder.DropColumn(
                name: "Sector_NUM",
                table: "CatalogObjects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Branch_NUM",
                table: "CatalogObjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sector_NUM",
                table: "CatalogObjects",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
