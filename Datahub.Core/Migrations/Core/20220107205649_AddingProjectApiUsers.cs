using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class AddingProjectApiUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Project_ApiUsers",
                columns: table => new
                {
                    ProjectApiUser_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Project_Acronym_CD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email_Contact_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Expiration_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_ApiUsers", x => x.ProjectApiUser_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_ApiUsers");
        }
    }
}
