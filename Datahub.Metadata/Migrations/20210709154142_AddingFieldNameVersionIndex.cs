using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Metadata.Migrations
{
    public partial class AddingFieldNameVersionIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FieldDefinition_FieldName",
                table: "FieldDefinition");

            migrationBuilder.CreateIndex(
                name: "IX_FieldDefinition_FieldName_VersionId",
                table: "FieldDefinition",
                columns: new[] { "FieldName", "VersionId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FieldDefinition_FieldName_VersionId",
                table: "FieldDefinition");

            migrationBuilder.CreateIndex(
                name: "IX_FieldDefinition_FieldName",
                table: "FieldDefinition",
                column: "FieldName",
                unique: true);
        }
    }
}
