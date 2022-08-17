using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.Finance
{
    public partial class addedfundcentermodified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FundCenterModifiedEnglish",
                table: "HierarchyLevels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FundCenterModifiedFrench",
                table: "HierarchyLevels",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FundCenterModifiedEnglish",
                table: "HierarchyLevels");

            migrationBuilder.DropColumn(
                name: "FundCenterModifiedFrench",
                table: "HierarchyLevels");
        }
    }
}
