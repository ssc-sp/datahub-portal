using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.ProjectForms.Migrations
{
    public partial class modelchanges2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AlterColumn<string>(
                name: "Risk_Drivers_TXT",
                table: "Risks",
                type: "nvarchar(max)",
                maxLength: 7500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 7500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Risk_Description_TXT",
                table: "Risks",
                type: "nvarchar(max)",
                maxLength: 7500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 7500,
                oldNullable: true);

           

            migrationBuilder.AddColumn<string>(
                name: "Future_Mitigation_Activities_TXT2",
                table: "Risks",
                type: "nvarchar(max)",
                maxLength: 7500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Future_Mitigation_Activities_TXT3",
                table: "Risks",
                type: "nvarchar(max)",
                maxLength: 7500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Future_Mitigation_Activities_TXT4",
                table: "Risks",
                type: "nvarchar(max)",
                maxLength: 7500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Future_Mitigation_Activities_TXT5",
                table: "Risks",
                type: "nvarchar(max)",
                maxLength: 7500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ongoing_Monitoring_Activities_TXT2",
                table: "Risks",
                type: "nvarchar(max)",
                maxLength: 7500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ongoing_Monitoring_Activities_TXT3",
                table: "Risks",
                type: "nvarchar(max)",
                maxLength: 7500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ongoing_Monitoring_Activities_TXT4",
                table: "Risks",
                type: "nvarchar(max)",
                maxLength: 7500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ongoing_Monitoring_Activities_TXT5",
                table: "Risks",
                type: "nvarchar(max)",
                maxLength: 7500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Risk_Category",
                table: "Risks",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Risk_Title",
                table: "Risks",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

           
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Future_Mitigation_Activities_TXT2",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "Future_Mitigation_Activities_TXT3",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "Future_Mitigation_Activities_TXT4",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "Future_Mitigation_Activities_TXT5",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "Ongoing_Monitoring_Activities_TXT2",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "Ongoing_Monitoring_Activities_TXT3",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "Ongoing_Monitoring_Activities_TXT4",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "Ongoing_Monitoring_Activities_TXT5",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "Risk_Category",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "Risk_Title",
                table: "Risks");

            migrationBuilder.AlterColumn<int>(
                name: "Tombstone_ID",
                table: "Tombstones",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "TombstoneRisk_ID",
                table: "TombstoneRisks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "Risk_Drivers_TXT",
                table: "Risks",
                type: "nvarchar(max)",
                maxLength: 7500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 7500);

            migrationBuilder.AlterColumn<string>(
                name: "Risk_Description_TXT",
                table: "Risks",
                type: "nvarchar(max)",
                maxLength: 7500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 7500);

            migrationBuilder.AlterColumn<int>(
                name: "Risks_ID",
                table: "Risks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "IndicatorRisk_ID",
                table: "IndicatorRisks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "IndicatorAndResult_ID",
                table: "IndicatorAndResults",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }
    }
}
