using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class newTombstoneFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDateOfPipApprovalLocked",
                table: "Tombstones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGBALocked",
                table: "Tombstones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGCInfoBaseProgramTagsLocked",
                table: "Tombstones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLatestUpdateInformationLocked",
                table: "Tombstones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProgramInformationLocked",
                table: "Tombstones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSectorProgramTagsLocked",
                table: "Tombstones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSpendingLocked",
                table: "Tombstones",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDateOfPipApprovalLocked",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "IsGBALocked",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "IsGCInfoBaseProgramTagsLocked",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "IsLatestUpdateInformationLocked",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "IsProgramInformationLocked",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "IsSectorProgramTagsLocked",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "IsSpendingLocked",
                table: "Tombstones");
        }
    }
}
