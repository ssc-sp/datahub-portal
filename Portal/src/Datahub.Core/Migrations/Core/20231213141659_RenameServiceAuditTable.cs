using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class RenameServiceAuditTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Requests_Projects_Project_ID",
                table: "Project_Requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Project_Requests",
                table: "Project_Requests");

            migrationBuilder.RenameTable(
                name: "Project_Requests",
                newName: "ProjectRequestAudits");

            migrationBuilder.RenameIndex(
                name: "IX_Project_Requests_Project_ID",
                table: "ProjectRequestAudits",
                newName: "IX_ProjectRequestAudits_Project_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectRequestAudits",
                table: "ProjectRequestAudits",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequestAudits_Projects_Project_ID",
                table: "ProjectRequestAudits",
                column: "Project_ID",
                principalTable: "Projects",
                principalColumn: "Project_ID");

            // clear the table
            migrationBuilder.Sql("DELETE FROM ProjectRequestAudits", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequestAudits_Projects_Project_ID",
                table: "ProjectRequestAudits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectRequestAudits",
                table: "ProjectRequestAudits");

            migrationBuilder.RenameTable(
                name: "ProjectRequestAudits",
                newName: "Project_Requests");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectRequestAudits_Project_ID",
                table: "Project_Requests",
                newName: "IX_Project_Requests_Project_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Project_Requests",
                table: "Project_Requests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Requests_Projects_Project_ID",
                table: "Project_Requests",
                column: "Project_ID",
                principalTable: "Projects",
                principalColumn: "Project_ID");
        }
    }
}
