using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class Subnet_unicity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subnets_VNetId",
                table: "Subnets");

            migrationBuilder.CreateIndex(
                name: "IX_Subnets_VNetId_SubnetGroup",
                table: "Subnets",
                columns: new[] { "VNetId", "SubnetGroup" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subnets_VNetId_SubnetGroup",
                table: "Subnets");

            migrationBuilder.CreateIndex(
                name: "IX_Subnets_VNetId",
                table: "Subnets",
                column: "VNetId");
        }
    }
}
