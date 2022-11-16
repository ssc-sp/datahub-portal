using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class Addedprojecttoengagements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Project_ID",
                table: "Client_Engagements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Client_Engagements_Project_ID",
                table: "Client_Engagements",
                column: "Project_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Client_Engagements_Projects_Project_ID",
                table: "Client_Engagements",
                column: "Project_ID",
                principalTable: "Projects",
                principalColumn: "Project_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Client_Engagements_Projects_Project_ID",
                table: "Client_Engagements");

            migrationBuilder.DropIndex(
                name: "IX_Client_Engagements_Project_ID",
                table: "Client_Engagements");

            migrationBuilder.DropColumn(
                name: "Project_ID",
                table: "Client_Engagements");
        }
    }
}
