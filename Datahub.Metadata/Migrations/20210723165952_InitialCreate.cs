using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Metadata.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetadataVersions",
                columns: table => new
                {
                    MetadataVersionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Source_TXT = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Last_Update_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version_Info_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataVersions", x => x.MetadataVersionId);
                });

            migrationBuilder.CreateTable(
                name: "FieldDefinitions",
                columns: table => new
                {
                    FieldDefinitionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetadataVersionId = table.Column<int>(type: "int", nullable: false),
                    Field_Name_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Sort_Order_NUM = table.Column<int>(type: "int", nullable: false),
                    Name_English_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name_French_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    English_DESC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    French_DESC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Required_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    MultiSelect_FLAG = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldDefinitions", x => x.FieldDefinitionId);
                    table.ForeignKey(
                        name: "FK_FieldDefinitions_MetadataVersions_MetadataVersionId",
                        column: x => x.MetadataVersionId,
                        principalTable: "MetadataVersions",
                        principalColumn: "MetadataVersionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObjectMetadata",
                columns: table => new
                {
                    ObjectMetadataId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    MetadataVersionId = table.Column<int>(type: "int", nullable: false),
                    ObjectId_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectMetadata", x => x.ObjectMetadataId);
                    table.ForeignKey(
                        name: "FK_ObjectMetadata_MetadataVersions_MetadataVersionId",
                        column: x => x.MetadataVersionId,
                        principalTable: "MetadataVersions",
                        principalColumn: "MetadataVersionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldChoices",
                columns: table => new
                {
                    FieldChoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Value_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Label_English_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Label_French_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldChoices", x => x.FieldChoiceId);
                    table.ForeignKey(
                        name: "FK_FieldChoices_FieldDefinitions_FieldDefinitionId",
                        column: x => x.FieldDefinitionId,
                        principalTable: "FieldDefinitions",
                        principalColumn: "FieldDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObjectFieldValues",
                columns: table => new
                {
                    ObjectMetadataId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    FieldDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Value_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectFieldValues", x => new { x.ObjectMetadataId, x.FieldDefinitionId });
                    table.ForeignKey(
                        name: "FK_ObjectFieldValues_FieldDefinitions_FieldDefinitionId",
                        column: x => x.FieldDefinitionId,
                        principalTable: "FieldDefinitions",
                        principalColumn: "FieldDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ObjectFieldValues_ObjectMetadata_ObjectMetadataId",
                        column: x => x.ObjectMetadataId,
                        principalTable: "ObjectMetadata",
                        principalColumn: "ObjectMetadataId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldChoices_FieldDefinitionId",
                table: "FieldChoices",
                column: "FieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldDefinitions_Field_Name_TXT_MetadataVersionId",
                table: "FieldDefinitions",
                columns: new[] { "Field_Name_TXT", "MetadataVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldDefinitions_MetadataVersionId",
                table: "FieldDefinitions",
                column: "MetadataVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectFieldValues_FieldDefinitionId",
                table: "ObjectFieldValues",
                column: "FieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectMetadata_MetadataVersionId",
                table: "ObjectMetadata",
                column: "MetadataVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectMetadata_ObjectId_TXT",
                table: "ObjectMetadata",
                column: "ObjectId_TXT",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldChoices");

            migrationBuilder.DropTable(
                name: "ObjectFieldValues");

            migrationBuilder.DropTable(
                name: "FieldDefinitions");

            migrationBuilder.DropTable(
                name: "ObjectMetadata");

            migrationBuilder.DropTable(
                name: "MetadataVersions");
        }
    }
}
