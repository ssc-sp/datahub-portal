using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Metadata.Migrations
{
    public partial class UpdateCatalogStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogKeywords");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "CatalogObjects");

            migrationBuilder.DropColumn(
                name: "Keywords_English_TXT",
                table: "CatalogObjects");

            migrationBuilder.DropColumn(
                name: "Keywords_French_TXT",
                table: "CatalogObjects");

            migrationBuilder.DropColumn(
                name: "SectorId",
                table: "CatalogObjects");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "CatalogObjects");

            migrationBuilder.AlterColumn<string>(
                name: "Name_TXT",
                table: "CatalogObjects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<string>(
                name: "Search_English_TXT",
                table: "CatalogObjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Search_French_TXT",
                table: "CatalogObjects",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Search_English_TXT",
                table: "CatalogObjects");

            migrationBuilder.DropColumn(
                name: "Search_French_TXT",
                table: "CatalogObjects");

            migrationBuilder.AlterColumn<string>(
                name: "Name_TXT",
                table: "CatalogObjects",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "CatalogObjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Keywords_English_TXT",
                table: "CatalogObjects",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Keywords_French_TXT",
                table: "CatalogObjects",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectorId",
                table: "CatalogObjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "CatalogObjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CatalogKeywords",
                columns: table => new
                {
                    CatalogKeywordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Keyword_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogKeywords", x => x.CatalogKeywordId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogKeywords_Keyword_TXT",
                table: "CatalogKeywords",
                column: "Keyword_TXT",
                unique: true);
        }
    }
}
