using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.ProjectForms.Migrations
{
    public partial class morefixesforalexTombstone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Ultimate_Outcome_1_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Ultimate_Outcome_2_DESC",
                table: "Tombstones");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Tombstone_ID",
            //    table: "Tombstones",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "TombstoneRisk_ID",
            //    table: "TombstoneRisks",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Risks_ID",
            //    table: "Risks",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "IndicatorRisk_ID",
            //    table: "IndicatorRisks",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "IndicatorAndResult_ID",
            //    table: "IndicatorAndResults",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Tombstone_ID",
                table: "Tombstones",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

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

            migrationBuilder.AddColumn<string>(
                name: "Ultimate_Outcome_1_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ultimate_Outcome_2_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "TombstoneRisk_ID",
                table: "TombstoneRisks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

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
