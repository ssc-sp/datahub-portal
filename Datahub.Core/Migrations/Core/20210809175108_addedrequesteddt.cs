using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Portal.Migrations.Forms.DatahubProjectDB
{
    public partial class addedrequesteddt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Requested_DT",
                table: "Project_Users_Requests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Requested_DT",
                table: "Project_Users_Requests");
        }
    }
}
