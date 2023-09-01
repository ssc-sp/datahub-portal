using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class UpdateRepoSearchCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"  
                INSERT INTO CatalogObjects(ObjectType, ObjectId, Name_English, Name_French, Desc_English, Desc_French, Location)
                SELECT 2 as ObjectType, p.Project_Acronym_CD as ObjectId, p.Project_Name as Name_English, p.Project_Name_Fr as Name_French, pr.Provider as Desc_English, pr.Provider as Desc_French, pr.RepositoryUrl as Location
                FROM Project_Repositories as pr
                JOIN Projects as p on pr.ProjectId = p.Project_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
