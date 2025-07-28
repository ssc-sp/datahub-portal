using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Metadata.Migrations
{
    public partial class AddingObjectCatalogDataModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogKeywords",
                columns: table => new
                {
                    CatalogKeywordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Keyword_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Language = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogKeywords", x => x.CatalogKeywordId);
                });

            migrationBuilder.CreateTable(
                name: "CatalogObjects",
                columns: table => new
                {
                    CatalogObjectId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObjectMetadataId = table.Column<long>(type: "bigint", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    SectorId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Name_TXT = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Keywords_English_TXT = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Keywords_French_TXT = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogObjects", x => x.CatalogObjectId);
                    table.ForeignKey(
                        name: "FK_CatalogObjects_ObjectMetadata_ObjectMetadataId",
                        column: x => x.ObjectMetadataId,
                        principalTable: "ObjectMetadata",
                        principalColumn: "ObjectMetadataId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogObjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogKeywords_Keyword_TXT",
                table: "CatalogKeywords",
                column: "Keyword_TXT",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogObjects_ObjectMetadataId",
                table: "CatalogObjects",
                column: "ObjectMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogObjects_SubjectId",
                table: "CatalogObjects",
                column: "SubjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogKeywords");

            migrationBuilder.DropTable(
                name: "CatalogObjects");
        }
    }
}
