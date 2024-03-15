using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class NewProjectStatusField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Project_Status",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpenDataSharedFile_SharedDataFiles_SharedDataFile_ID",
                table: "OpenDataSharedFile");

            migrationBuilder.DropColumn(
                name: "Project_Status",
                table: "Projects");

            migrationBuilder.CreateTable(
                name: "Access_Requests",
                columns: table => new
                {
                    RequestID = table.Column<int>(name: "Request_ID", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompletionDT = table.Column<DateTime>(name: "Completion_DT", type: "datetime2", nullable: true),
                    Databricks = table.Column<bool>(type: "bit", nullable: false),
                    PowerBI = table.Column<bool>(type: "bit", nullable: false),
                    ProjectID = table.Column<int>(name: "Project_ID", type: "int", nullable: true),
                    RequestDT = table.Column<DateTime>(name: "Request_DT", type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    UserID = table.Column<string>(name: "User_ID", type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UserName = table.Column<string>(name: "User_Name", type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WebForms = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Access_Requests", x => x.RequestID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Access_Requests_Project_ID",
                table: "Access_Requests",
                column: "Project_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_OpenDataSharedFile_SharedDataFiles_SharedDataFile_ID",
                table: "OpenDataSharedFile",
                column: "SharedDataFile_ID",
                principalTable: "SharedDataFiles",
                principalColumn: "SharedDataFile_ID");
        }
    }
}
