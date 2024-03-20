using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class UpdateUserSearchCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM CatalogObjects
                WHERE ObjectType = 0");

            migrationBuilder.Sql(@"
                INSERT INTO CatalogObjects(ObjectType, ObjectId, Name_English, Name_French, Desc_English, Desc_French)
                SELECT 0 as ObjectType, GraphGuid as ObjectId, DisplayName as Name_English, DisplayName as Name_French, Email as Desc_English, Email as Desc_French 
                FROM PortalUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM CatalogObjects
                WHERE ObjectType = 0");
        }
    }
}
