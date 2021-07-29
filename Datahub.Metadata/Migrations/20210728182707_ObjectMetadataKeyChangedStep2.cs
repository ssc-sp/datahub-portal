using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Metadata.Migrations
{
    public partial class ObjectMetadataKeyChangedStep2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectFieldValues_ObjectMetadata_TempObjectMetadataId",
                table: "ObjectFieldValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjectMetadata",
                table: "ObjectMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjectFieldValues",
                table: "ObjectFieldValues");

            migrationBuilder.DropColumn(
                name: "TempObjectMetadataId",
                table: "ObjectMetadata");

            migrationBuilder.DropColumn(
                name: "TempObjectMetadataId",
                table: "ObjectFieldValues");

            migrationBuilder.AddColumn<long>(
                name: "ObjectMetadataId",
                table: "ObjectMetadata",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "ObjectMetadataId",
                table: "ObjectFieldValues",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjectMetadata",
                table: "ObjectMetadata",
                column: "ObjectMetadataId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjectFieldValues",
                table: "ObjectFieldValues",
                columns: new[] { "ObjectMetadataId", "FieldDefinitionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectFieldValues_ObjectMetadata_ObjectMetadataId",
                table: "ObjectFieldValues",
                column: "ObjectMetadataId",
                principalTable: "ObjectMetadata",
                principalColumn: "ObjectMetadataId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectFieldValues_ObjectMetadata_ObjectMetadataId",
                table: "ObjectFieldValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjectMetadata",
                table: "ObjectMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjectFieldValues",
                table: "ObjectFieldValues");

            migrationBuilder.DropColumn(
                name: "ObjectMetadataId",
                table: "ObjectMetadata");

            migrationBuilder.DropColumn(
                name: "ObjectMetadataId",
                table: "ObjectFieldValues");

            migrationBuilder.AddColumn<int>(
                name: "TempObjectMetadataId",
                table: "ObjectMetadata",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "TempObjectMetadataId",
                table: "ObjectFieldValues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjectMetadata",
                table: "ObjectMetadata",
                column: "TempObjectMetadataId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjectFieldValues",
                table: "ObjectFieldValues",
                columns: new[] { "TempObjectMetadataId", "FieldDefinitionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectFieldValues_ObjectMetadata_TempObjectMetadataId",
                table: "ObjectFieldValues",
                column: "TempObjectMetadataId",
                principalTable: "ObjectMetadata",
                principalColumn: "TempObjectMetadataId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
