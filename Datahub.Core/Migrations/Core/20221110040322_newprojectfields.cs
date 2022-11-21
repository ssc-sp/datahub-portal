using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class Newprojectfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Last_Updated_UserId",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectorId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_BranchId",
                table: "Projects",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DivisionId",
                table: "Projects",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SectorId",
                table: "Projects",
                column: "SectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Organization_Levels_BranchId",
                table: "Projects",
                column: "BranchId",
                principalTable: "Organization_Levels",
                principalColumn: "SectorAndBranchS_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Organization_Levels_DivisionId",
                table: "Projects",
                column: "DivisionId",
                principalTable: "Organization_Levels",
                principalColumn: "SectorAndBranchS_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Organization_Levels_SectorId",
                table: "Projects",
                column: "SectorId",
                principalTable: "Organization_Levels",
                principalColumn: "SectorAndBranchS_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Organization_Levels_BranchId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Organization_Levels_DivisionId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Organization_Levels_SectorId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_BranchId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_DivisionId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_SectorId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Last_Updated_UserId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SectorId",
                table: "Projects");
        }
    }
}
