using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Shared.Migrations.Core
{
    public partial class TempSharingExpiration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate_DT",
                table: "SharedDataFiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MetadataCompleted_FLAG",
                table: "SharedDataFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpirationDate_DT",
                table: "SharedDataFiles");

            migrationBuilder.DropColumn(
                name: "MetadataCompleted_FLAG",
                table: "SharedDataFiles");
        }
    }
}
