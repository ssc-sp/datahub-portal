using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class DeletedQuestionnaireTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDataSteward",
                table: "Project_Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Project_Delete_Questionnaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsWorkspaceNotRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsDataMigrated = table.Column<bool>(type: "bit", nullable: false),
                    IsDataNotSubjectToLitigation = table.Column<bool>(type: "bit", nullable: false),
                    DoesDataNotHaveArchivalValue = table.Column<bool>(type: "bit", nullable: false),
                    IsDeletionConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    DeletedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Delete_Questionnaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Delete_Questionnaires_PortalUsers_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Project_Delete_Questionnaires_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Revoke the user's access to the workspace");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Head of the workspace and bears business responsibility for success of the workspace");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Management authority within the workspace with direct supervision over the cloud resourcing and users");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Responsible for contributing to the overall workspace objectives and deliverables");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Delete_Questionnaires_DeletedById",
                table: "Project_Delete_Questionnaires",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Delete_Questionnaires_Project_ID",
                table: "Project_Delete_Questionnaires",
                column: "Project_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_Delete_Questionnaires");

            migrationBuilder.DropColumn(
                name: "IsDataSteward",
                table: "Project_Users");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Revoke the user's access to the project's private resources");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Head of the business unit and bears business responsibility for successful implementation and availability");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Management authority within the project with direct supervision over the project resources and deliverables");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Responsible for contributing to the overall project objectives and deliverables to ensure success");
        }
    }
}
