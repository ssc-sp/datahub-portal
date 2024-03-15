using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class UpdateWSSearchCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM CatalogObjects
                WHERE ObjectType = 1");

            migrationBuilder.Sql(@"
                INSERT INTO CatalogObjects(ObjectType, ObjectId, Name_English, Name_French, Desc_English, Desc_French)
                SELECT 1 as ObjectType, Project_Acronym_CD as ObjectId, Project_Name as Name_English, Project_Name_Fr as Name_French, Project_Summary_Desc as Desc_English, Project_Summary_Desc_Fr as Desc_French 
                FROM Projects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM CatalogObjects
                WHERE ObjectType = 1");
        }
    }
}
