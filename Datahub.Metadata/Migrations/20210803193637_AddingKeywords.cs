using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Metadata.Migrations
{
    public partial class AddingKeywords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Keywords",
                columns: table => new
                {
                    KeywordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    English_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    French_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keywords", x => x.KeywordId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Keywords_English_TXT",
                table: "Keywords",
                column: "English_TXT",
                unique: true,
                filter: "[English_TXT] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Keywords_French_TXT",
                table: "Keywords",
                column: "French_TXT",
                unique: true,
                filter: "[French_TXT] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Keywords");
        }
    }
}
