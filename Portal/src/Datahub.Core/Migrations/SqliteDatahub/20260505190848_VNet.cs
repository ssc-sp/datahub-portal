using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class VNet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VNets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VNetId = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    VNetName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SubscriptionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VNets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VNets_AzureSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "AzureSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subnets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubnetName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AddressPrefix = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    SubnetGroup = table.Column<int>(type: "INTEGER", nullable: false),
                    VNetId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subnets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subnets_VNets_VNetId",
                        column: x => x.VNetId,
                        principalTable: "VNets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceSubnets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubnetId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceSubnets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspaceSubnets_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkspaceSubnets_Subnets_SubnetId",
                        column: x => x.SubnetId,
                        principalTable: "Subnets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subnets_VNetId",
                table: "Subnets",
                column: "VNetId");

            migrationBuilder.CreateIndex(
                name: "IX_VNets_SubscriptionId",
                table: "VNets",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceSubnets_ProjectId_SubnetId",
                table: "WorkspaceSubnets",
                columns: new[] { "ProjectId", "SubnetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceSubnets_SubnetId",
                table: "WorkspaceSubnets",
                column: "SubnetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkspaceSubnets");

            migrationBuilder.DropTable(
                name: "Subnets");

            migrationBuilder.DropTable(
                name: "VNets");
        }
    }
}
