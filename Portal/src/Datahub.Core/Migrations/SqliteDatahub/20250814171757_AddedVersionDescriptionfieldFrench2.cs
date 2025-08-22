using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class AddedVersionDescriptionfieldFrench2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "VersionDescriptionFr",
                table: "VersionTags",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

               }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropColumn(
                name: "VersionDescriptionFr",
                table: "VersionTags");

        }
    }
}
