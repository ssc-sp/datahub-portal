using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class DeletedQuestionnaireTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Project_Delete_Questionnaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsWorkspaceNotRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDataMigrated = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDataNotSubjectToLitigation = table.Column<bool>(type: "INTEGER", nullable: false),
                    DoesDataNotHaveArchivalValue = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeletionConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Project_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    DeletedById = table.Column<int>(type: "INTEGER", nullable: true)
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
        }
    }
}
