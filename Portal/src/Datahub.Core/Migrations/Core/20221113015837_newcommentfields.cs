using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core;

public partial class Newcommentfields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "Created_DT",
            table: "Project_Comments",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
            name: "Created_UserId",
            table: "Project_Comments",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "Last_Updated_DT",
            table: "Project_Comments",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
            name: "Last_Updated_UserId",
            table: "Project_Comments",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Created_DT",
            table: "Project_Comments");

        migrationBuilder.DropColumn(
            name: "Created_UserId",
            table: "Project_Comments");

        migrationBuilder.DropColumn(
            name: "Last_Updated_DT",
            table: "Project_Comments");

        migrationBuilder.DropColumn(
            name: "Last_Updated_UserId",
            table: "Project_Comments");
    }
}