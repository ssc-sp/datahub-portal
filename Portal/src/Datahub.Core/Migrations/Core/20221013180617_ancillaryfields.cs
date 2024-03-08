using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core;

public partial class Ancillaryfields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "Created_DT",
            table: "Client_Engagements",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
            name: "Created_UserId",
            table: "Client_Engagements",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "Last_Updated_DT",
            table: "Client_Engagements",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
            name: "Last_Updated_UserId",
            table: "Client_Engagements",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "Timestamp",
            table: "Client_Engagements",
            type: "rowversion",
            rowVersion: true,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Created_DT",
            table: "Client_Engagements");

        migrationBuilder.DropColumn(
            name: "Created_UserId",
            table: "Client_Engagements");

        migrationBuilder.DropColumn(
            name: "Last_Updated_DT",
            table: "Client_Engagements");

        migrationBuilder.DropColumn(
            name: "Last_Updated_UserId",
            table: "Client_Engagements");

        migrationBuilder.DropColumn(
            name: "Timestamp",
            table: "Client_Engagements");
    }
}