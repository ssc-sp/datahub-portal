using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class OpenDataPublishing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpenDataSubmissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProcessType = table.Column<int>(type: "int", nullable: false),
                    DatasetTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpenForAttachingFiles = table.Column<bool>(type: "bit", nullable: false),
                    RequestingUserId = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenDataSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenDataSubmissions_PortalUsers_RequestingUserId",
                        column: x => x.RequestingUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpenDataSubmissions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpenDataPublishFiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<long>(type: "bigint", nullable: false),
                    FilePurpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectStorageId = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FolderPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadStatus = table.Column<int>(type: "int", nullable: false),
                    UploadMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenDataPublishFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenDataPublishFiles_OpenDataSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "OpenDataSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpenDataPublishFiles_Project_Cloud_Storages_ProjectStorageId",
                        column: x => x.ProjectStorageId,
                        principalTable: "Project_Cloud_Storages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TbsOpenGovSubmissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    MetadataComplete = table.Column<bool>(type: "bit", nullable: false),
                    OpenGovCriteriaFormId = table.Column<int>(type: "int", nullable: true),
                    OpenGovCriteriaMetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LocalDQCheckStarted = table.Column<bool>(type: "bit", nullable: false),
                    LocalDQCheckPassed = table.Column<bool>(type: "bit", nullable: false),
                    InitialOpenGovSubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpenGovDQCheckPassed = table.Column<bool>(type: "bit", nullable: false),
                    ImsoApprovalRequestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImsoApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpenGovPublicationDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbsOpenGovSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TbsOpenGovSubmissions_OpenDataSubmissions_Id",
                        column: x => x.Id,
                        principalTable: "OpenDataSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpenDataPublishFiles_ProjectStorageId",
                table: "OpenDataPublishFiles",
                column: "ProjectStorageId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenDataPublishFiles_SubmissionId",
                table: "OpenDataPublishFiles",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenDataSubmissions_ProjectId",
                table: "OpenDataSubmissions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenDataSubmissions_RequestingUserId",
                table: "OpenDataSubmissions",
                column: "RequestingUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenDataSubmissions_UniqueId",
                table: "OpenDataSubmissions",
                column: "UniqueId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenDataPublishFiles");

            migrationBuilder.DropTable(
                name: "TbsOpenGovSubmissions");

            migrationBuilder.DropTable(
                name: "OpenDataSubmissions");
        }
    }
}
