using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class RequestStorageLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Project_StorageId",
                table: "Project_Requests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Project_Requests_Project_StorageId",
                table: "Project_Requests",
                column: "Project_StorageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Requests_Project_Storage_Project_StorageId",
                table: "Project_Requests",
                column: "Project_StorageId",
                principalTable: "Project_Storage",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Requests_Project_Storage_Project_StorageId",
                table: "Project_Requests");

            migrationBuilder.DropIndex(
                name: "IX_Project_Requests_Project_StorageId",
                table: "Project_Requests");

            migrationBuilder.DropColumn(
                name: "Project_StorageId",
                table: "Project_Requests");
        }
    }
}
