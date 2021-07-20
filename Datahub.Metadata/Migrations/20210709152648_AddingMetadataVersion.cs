using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Metadata.Migrations
{
    public partial class AddingMetadataVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Source",
                table: "FieldDefinition",
                newName: "VersionId");

            migrationBuilder.CreateTable(
                name: "MetadataVersion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Source = table.Column<int>(type: "int", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VersionData = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataVersion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldDefinition_VersionId",
                table: "FieldDefinition",
                column: "VersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_FieldDefinition_MetadataVersion_VersionId",
                table: "FieldDefinition",
                column: "VersionId",
                principalTable: "MetadataVersion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FieldDefinition_MetadataVersion_VersionId",
                table: "FieldDefinition");

            migrationBuilder.DropTable(
                name: "MetadataVersion");

            migrationBuilder.DropIndex(
                name: "IX_FieldDefinition_VersionId",
                table: "FieldDefinition");

            migrationBuilder.RenameColumn(
                name: "VersionId",
                table: "FieldDefinition",
                newName: "Source");
        }
    }
}
