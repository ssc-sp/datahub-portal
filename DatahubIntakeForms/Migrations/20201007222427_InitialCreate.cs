using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Portal.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 200, nullable: false),
                    AzureTags = table.Column<string>(maxLength: 200, nullable: false),
                    Sector = table.Column<string>(maxLength: 200, nullable: false),
                    Branch = table.Column<string>(maxLength: 200, nullable: false),
                    ContactEmail = table.Column<string>(maxLength: 200, nullable: false),
                    ContactName = table.Column<string>(maxLength: 200, nullable: false),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
