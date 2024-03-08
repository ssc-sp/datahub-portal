using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class AddingTheProjectStorageCapacitiesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpenDataSharedFile_SharedDataFiles_SharedDataFile_ID",
                table: "OpenDataSharedFile");

            //migrationBuilder.DropTable(
            //    name: "Access_Requests");

            migrationBuilder.DropTable(
                name: "Registration_Requests");

            migrationBuilder.CreateTable(
                name: "Project_Storage_Capacities",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UsedCapacity = table.Column<double>(type: "float", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
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

            migrationBuilder.AddForeignKey(
                name: "FK_OpenDataSharedFile_SharedDataFiles_SharedDataFile_ID",
                table: "OpenDataSharedFile",
                column: "SharedDataFile_ID",
                principalTable: "SharedDataFiles",
                principalColumn: "SharedDataFile_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpenDataSharedFile_SharedDataFiles_SharedDataFile_ID",
                table: "OpenDataSharedFile");

            migrationBuilder.DropTable(
                name: "Project_Storage_Capacities");

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

            migrationBuilder.CreateTable(
                name: "Registration_Requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    LinkId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    ProjectAcronym = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registration_Requests", x => x.Id);
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
