using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Portal.Migrations.Forms.DatahubProjectDB
{
    public partial class PublicDataFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublicDataFiles",
                columns: table => new
                {
                    PublicDataFile_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    File_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Filename_TXT = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FolderPath_TXT = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ProjectCode_CD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RequestingUserEmail_TXT = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestingUser_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestedDate_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedDate_DT = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicDataFiles", x => x.PublicDataFile_ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicDataFiles_File_ID",
                table: "PublicDataFiles",
                column: "File_ID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicDataFiles");
        }
    }
}
