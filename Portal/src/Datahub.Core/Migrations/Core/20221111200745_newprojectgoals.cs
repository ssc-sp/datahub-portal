using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core;

public partial class Newprojectgoals : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {

        migrationBuilder.AddColumn<string>(
            name: "Project_Goal",
            table: "Projects",
            type: "nvarchar(max)",
            nullable: true);


    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "MetadataAdded",
            table: "Projects");

        migrationBuilder.DropColumn(
            name: "Project_Goal",
            table: "Projects");

        migrationBuilder.DropColumn(
            name: "Engagment_Lead",
            table: "Client_Engagements");

        migrationBuilder.DropColumn(
            name: "Engagment_Owners",
            table: "Client_Engagements");

        migrationBuilder.CreateTable(
            name: "Access_Requests",
            columns: table => new
            {
                Request_ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Project_ID = table.Column<int>(type: "int", nullable: true),
                Completion_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                Databricks = table.Column<bool>(type: "bit", nullable: false),
                PowerBI = table.Column<bool>(type: "bit", nullable: false),
                Request_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                User_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                User_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                WebForms = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Access_Requests", x => x.Request_ID);
                table.ForeignKey(
                    name: "FK_Access_Requests_Projects_Project_ID",
                    column: x => x.Project_ID,
                    principalTable: "Projects",
                    principalColumn: "Project_ID");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Access_Requests_Project_ID",
            table: "Access_Requests",
            column: "Project_ID");
    }
}