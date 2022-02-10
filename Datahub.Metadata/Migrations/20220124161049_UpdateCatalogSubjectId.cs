using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Metadata.Migrations
{
    public partial class UpdateCatalogSubjectId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogObjects_Subjects_SubjectId",
                table: "CatalogObjects");

            migrationBuilder.DropIndex(
                name: "IX_CatalogObjects_SubjectId",
                table: "CatalogObjects");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CatalogObjects_SubjectId",
                table: "CatalogObjects",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogObjects_Subjects_SubjectId",
                table: "CatalogObjects",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
