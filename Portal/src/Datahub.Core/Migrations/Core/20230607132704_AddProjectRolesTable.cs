using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class AddProjectRolesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Requests_Project_Storage_Project_StorageId",
                table: "Project_Requests");

            migrationBuilder.DropTable(
                name: "Project_Resources");

            migrationBuilder.DropTable(
                name: "Project_Storage");

            migrationBuilder.DropTable(
                name: "Project_Storage_Capacities");

            migrationBuilder.DropIndex(
                name: "IX_Project_Requests_Project_StorageId",
                table: "Project_Requests");

            migrationBuilder.DropColumn(
                name: "Project_StorageId",
                table: "Project_Requests");

            migrationBuilder.CreateTable(
                name: "Project_Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Roles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Project_Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Revoke the user's access to the project's private resources", "Remove User" },
                    { 2, "Head of the business unit and bears business responsibility for successful implementation and availability", "Workspace Lead" },
                    { 3, "Management authority within the project with direct supervision over the project resources and deliverables", "Admin" },
                    { 4, "Responsible for contributing to the overall project objectives and deliverables to ensure success", "Collaborator" },
                    { 5, "Able to view the workspace and its contents but not able to contribute or modify anything", "Guest" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_Roles");

            migrationBuilder.AddColumn<Guid>(
                name: "Project_StorageId",
                table: "Project_Requests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Project_Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    Attributes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Param1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Param2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Param3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ResourceType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeRequested = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Resources_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "Project_Storage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Datahub_ProjectProject_ID = table.Column<int>(type: "int", nullable: true),
                    Storage_Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Storage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Storage_Projects_Datahub_ProjectProject_ID",
                        column: x => x.Datahub_ProjectProject_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "Project_Storage_Capacities",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotifiedAt50 = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotifiedAt75 = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsedCapacity = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Storage_Capacities", x => new { x.ProjectId, x.Type });
                    table.ForeignKey(
                        name: "FK_Project_Storage_Capacities_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Project_Requests_Project_StorageId",
                table: "Project_Requests",
                column: "Project_StorageId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Resources_Project_ID",
                table: "Project_Resources",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Storage_Datahub_ProjectProject_ID",
                table: "Project_Storage",
                column: "Datahub_ProjectProject_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Requests_Project_Storage_Project_StorageId",
                table: "Project_Requests",
                column: "Project_StorageId",
                principalTable: "Project_Storage",
                principalColumn: "Id");
        }
    }
}
