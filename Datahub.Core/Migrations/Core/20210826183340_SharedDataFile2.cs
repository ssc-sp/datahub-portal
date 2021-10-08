using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Shared.Migrations.Core
{
    public partial class SharedDataFile2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedDataFiles",
                columns: table => new
                {
                    SharedDataFile_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    File_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsOpenDataRequest_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Filename_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FolderPath_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectCode_CD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestingUser_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApprovingUser_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequestedDate_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedDate_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedDate_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublicationDate_DT = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedDataFiles", x => x.SharedDataFile_ID);
                });

            migrationBuilder.CreateTable(
                name: "OpenDataSharedFile",
                columns: table => new
                {
                    SharedDataFile_ID = table.Column<long>(type: "bigint", nullable: false),
                    ApprovalForm_ID = table.Column<int>(type: "int", nullable: true),
                    SignedApprovalForm_URL = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenDataSharedFile", x => x.SharedDataFile_ID);
                    table.ForeignKey(
                        name: "FK_OpenDataSharedFile_SharedDataFiles_SharedDataFile_ID",
                        column: x => x.SharedDataFile_ID,
                        principalTable: "SharedDataFiles",
                        principalColumn: "SharedDataFile_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedDataFiles_File_ID",
                table: "SharedDataFiles",
                column: "File_ID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenDataSharedFile");

            migrationBuilder.DropTable(
                name: "SharedDataFiles");
        }
    }
}
