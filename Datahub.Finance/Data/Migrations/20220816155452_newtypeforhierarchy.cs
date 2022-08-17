using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.Finance
{
    public partial class newtypeforhierarchy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branch",
                table: "FundCenters");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "FundCenters");

            migrationBuilder.DropColumn(
                name: "Sector",
                table: "FundCenters");

            migrationBuilder.AddColumn<int>(
                name: "BranchHierarchyLevelID",
                table: "FundCenters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DivisionHierarchyLevelID",
                table: "FundCenters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SectorHierarchyLevelID",
                table: "FundCenters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FundCenters_BranchHierarchyLevelID",
                table: "FundCenters",
                column: "BranchHierarchyLevelID");

            migrationBuilder.CreateIndex(
                name: "IX_FundCenters_DivisionHierarchyLevelID",
                table: "FundCenters",
                column: "DivisionHierarchyLevelID");

            migrationBuilder.CreateIndex(
                name: "IX_FundCenters_SectorHierarchyLevelID",
                table: "FundCenters",
                column: "SectorHierarchyLevelID");

            migrationBuilder.AddForeignKey(
                name: "FK_FundCenters_HierarchyLevels_BranchHierarchyLevelID",
                table: "FundCenters",
                column: "BranchHierarchyLevelID",
                principalTable: "HierarchyLevels",
                principalColumn: "HierarchyLevelID",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_FundCenters_HierarchyLevels_DivisionHierarchyLevelID",
                table: "FundCenters",
                column: "DivisionHierarchyLevelID",
                principalTable: "HierarchyLevels",
                principalColumn: "HierarchyLevelID",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_FundCenters_HierarchyLevels_SectorHierarchyLevelID",
                table: "FundCenters",
                column: "SectorHierarchyLevelID",
                principalTable: "HierarchyLevels",
                principalColumn: "HierarchyLevelID",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundCenters_HierarchyLevels_BranchHierarchyLevelID",
                table: "FundCenters");

            migrationBuilder.DropForeignKey(
                name: "FK_FundCenters_HierarchyLevels_DivisionHierarchyLevelID",
                table: "FundCenters");

            migrationBuilder.DropForeignKey(
                name: "FK_FundCenters_HierarchyLevels_SectorHierarchyLevelID",
                table: "FundCenters");

            migrationBuilder.DropIndex(
                name: "IX_FundCenters_BranchHierarchyLevelID",
                table: "FundCenters");

            migrationBuilder.DropIndex(
                name: "IX_FundCenters_DivisionHierarchyLevelID",
                table: "FundCenters");

            migrationBuilder.DropIndex(
                name: "IX_FundCenters_SectorHierarchyLevelID",
                table: "FundCenters");

            migrationBuilder.DropColumn(
                name: "BranchHierarchyLevelID",
                table: "FundCenters");

            migrationBuilder.DropColumn(
                name: "DivisionHierarchyLevelID",
                table: "FundCenters");

            migrationBuilder.DropColumn(
                name: "SectorHierarchyLevelID",
                table: "FundCenters");

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "FundCenters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "FundCenters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "FundCenters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
