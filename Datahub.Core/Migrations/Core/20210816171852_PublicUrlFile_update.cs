using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations.Forms.DatahubProjectDB
{
    public partial class PublicUrlFile_update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestingUserEmail_TXT",
                table: "PublicDataFiles");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublicationDate_DT",
                table: "PublicDataFiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedDate_DT",
                table: "PublicDataFiles",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicationDate_DT",
                table: "PublicDataFiles");

            migrationBuilder.DropColumn(
                name: "SubmittedDate_DT",
                table: "PublicDataFiles");

            migrationBuilder.AddColumn<string>(
                name: "RequestingUserEmail_TXT",
                table: "PublicDataFiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
