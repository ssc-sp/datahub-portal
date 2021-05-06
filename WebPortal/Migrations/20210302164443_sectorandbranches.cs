using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations
{
    public partial class sectorandbranches : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<int>(
            //    name: "SectorId",
            //    table: "Sectors",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "SectorAndBranchForSectorId",
                table: "Sectors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            //migrationBuilder.AlterColumn<int>(
            //    name: "SectorBranch_ID",
            //    table: "SectorAndBranches",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "SectorAndBranches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectorId",
                table: "SectorAndBranches",
                type: "int",
                nullable: true);

            //migrationBuilder.AlterColumn<int>(
            //    name: "Budget_ID",
            //    table: "Budgets",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "BranchId",
            //    table: "Branches",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "SectorAndBranchForBranchId",
                table: "Branches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SectorAndBranches_BranchId",
                table: "SectorAndBranches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SectorAndBranches_SectorId",
                table: "SectorAndBranches",
                column: "SectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_SectorAndBranches_Branches_BranchId",
                table: "SectorAndBranches",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SectorAndBranches_Sectors_SectorId",
                table: "SectorAndBranches",
                column: "SectorId",
                principalTable: "Sectors",
                principalColumn: "SectorId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SectorAndBranches_Branches_BranchId",
                table: "SectorAndBranches");

            migrationBuilder.DropForeignKey(
                name: "FK_SectorAndBranches_Sectors_SectorId",
                table: "SectorAndBranches");

            migrationBuilder.DropIndex(
                name: "IX_SectorAndBranches_BranchId",
                table: "SectorAndBranches");

            migrationBuilder.DropIndex(
                name: "IX_SectorAndBranches_SectorId",
                table: "SectorAndBranches");

            migrationBuilder.DropColumn(
                name: "SectorAndBranchForSectorId",
                table: "Sectors");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "SectorAndBranches");

            migrationBuilder.DropColumn(
                name: "SectorId",
                table: "SectorAndBranches");

            migrationBuilder.DropColumn(
                name: "SectorAndBranchForBranchId",
                table: "Branches");

            migrationBuilder.AlterColumn<int>(
                name: "SectorId",
                table: "Sectors",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "SectorBranch_ID",
                table: "SectorAndBranches",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Budget_ID",
                table: "Budgets",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "BranchId",
                table: "Branches",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }
    }
}
