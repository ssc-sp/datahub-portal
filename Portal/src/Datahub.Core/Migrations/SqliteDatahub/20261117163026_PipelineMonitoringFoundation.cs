using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class PipelineMonitoringFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PipelineObservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Organization = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Environment = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    DefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ObservedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    RunId = table.Column<long>(type: "INTEGER", nullable: true),
                    SourceChangedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PayloadJson = table.Column<string>(type: "TEXT", nullable: false),
                    PayloadHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineObservations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PipelineObservationWorkspaces",
                columns: table => new
                {
                    ObservationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Acronym = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineObservationWorkspaces", x => new { x.ObservationId, x.Acronym });
                    table.ForeignKey(
                        name: "FK_PipelineObservationWorkspaces_PipelineObservations_ObservationId",
                        column: x => x.ObservationId,
                        principalTable: "PipelineObservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PipelineObservations_Organization_ProjectId_Environment_DefinitionId_Kind_ObservedAtUtc",
                table: "PipelineObservations",
                columns: new[] { "Organization", "ProjectId", "Environment", "DefinitionId", "Kind", "ObservedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PipelineObservations_Organization_ProjectId_Environment_DefinitionId_RunId_ObservedAtUtc",
                table: "PipelineObservations",
                columns: new[] { "Organization", "ProjectId", "Environment", "DefinitionId", "RunId", "ObservedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PipelineObservationWorkspaces_Acronym_ObservationId",
                table: "PipelineObservationWorkspaces",
                columns: new[] { "Acronym", "ObservationId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PipelineObservationWorkspaces");

            migrationBuilder.DropTable(
                name: "PipelineObservations");
        }
    }
}
