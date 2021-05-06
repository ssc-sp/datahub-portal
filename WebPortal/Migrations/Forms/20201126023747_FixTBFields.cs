using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.ProjectForms.Migrations
{
    public partial class FixTBFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Intermediate_Outcome",
                table: "Tombstones");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Tombstone_ID",
            //    table: "Tombstones",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Activity_1_From_Immediate_Outcome_1_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Activity_1_From_Immediate_Outcome_2_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Activity_1_From_Immediate_Outcome_3_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Activity_1_From_Immediate_Outcome_4_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Activity_2_From_Immediate_Outcome_1_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Activity_2_From_Immediate_Outcome_2_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Activity_2_From_Immediate_Outcome_3_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Activity_2_From_Immediate_Outcome_4_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Immediate_Outcome_1_From_Outcome_1_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Immediate_Outcome_2_From_Outcome_1_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Immediate_Outcome_3_From_Outcome_2_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Immediate_Outcome_4_From_Outcome_2_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Output_1_From_Immediate_Outcome_1_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Output_1_From_Immediate_Outcome_2_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Output_1_From_Immediate_Outcome_3_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Output_1_From_Immediate_Outcome_4_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Output_2_From_Immediate_Outcome_1_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Output_2_From_Immediate_Outcome_2_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Output_2_From_Immediate_Outcome_3_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Output_2_From_Immediate_Outcome_4_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activity_1_From_Immediate_Outcome_1_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Activity_1_From_Immediate_Outcome_2_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Activity_1_From_Immediate_Outcome_3_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Activity_1_From_Immediate_Outcome_4_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Activity_2_From_Immediate_Outcome_1_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Activity_2_From_Immediate_Outcome_2_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Activity_2_From_Immediate_Outcome_3_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Activity_2_From_Immediate_Outcome_4_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Immediate_Outcome_1_From_Outcome_1_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Immediate_Outcome_2_From_Outcome_1_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Immediate_Outcome_3_From_Outcome_2_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Immediate_Outcome_4_From_Outcome_2_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Output_1_From_Immediate_Outcome_1_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Output_1_From_Immediate_Outcome_2_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Output_1_From_Immediate_Outcome_3_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Output_1_From_Immediate_Outcome_4_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Output_2_From_Immediate_Outcome_1_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Output_2_From_Immediate_Outcome_2_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Output_2_From_Immediate_Outcome_3_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Output_2_From_Immediate_Outcome_4_DESC",
                table: "Tombstones");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Tombstone_ID",
            //    table: "Tombstones",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Intermediate_Outcome",
                table: "Tombstones",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
