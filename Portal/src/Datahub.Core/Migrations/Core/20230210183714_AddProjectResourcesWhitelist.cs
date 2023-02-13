using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class AddProjectResourcesWhitelist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "Project_Resources_Whitelists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    AdminLastUpdatedID = table.Column<string>(name: "AdminLastUpdated_ID", type: "nvarchar(max)", nullable: true),
                    AdminLastUpdatedUserName = table.Column<string>(name: "AdminLastUpdated_UserName", type: "nvarchar(max)", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AllowStorage = table.Column<bool>(type: "bit", nullable: false),
                    AllowDatabricks = table.Column<bool>(type: "bit", nullable: false),
                    AllowVMs = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Resources_Whitelists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Resources_Whitelists_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Project_Resources_Whitelists_ProjectId",
                table: "Project_Resources_Whitelists",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_Resources_Whitelists");
        }
    }
}
