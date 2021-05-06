using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.ProjectForms.Migrations
{
    public partial class InitialVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tombstones",
                columns: table => new
                {
                    Tombstone_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Program_Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Program_Official_Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Core_Responsbility_1_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Core_Responsbility_2_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Core_Responsbility_3_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Internal_Services_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Date_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Planned_Spending_AMTL = table.Column<decimal>(type: "Money", nullable: true),
                    Actual_Spending_AMTL = table.Column<decimal>(type: "Money", nullable: true),
                    Approval_By_Program_Offical_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Consultation_With_The_Head_Of_Evaluation_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Functional_SignOff_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Program_Inventory_Program_Description_URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Departmental_Result_1_CD = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Departmental_Result_2_CD = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Strategic_Priorities_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Strategic_Priorities_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Mandate_Letter_Commitment_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Mandate_Letter_Commitment_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Transfer_Payment_Programs_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Transfer_Payment_Programs_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Horizontal_Initiative_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Horizontal_Initiative_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Government_Of_Canada_Outcome_Areas_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Government_Of_Canada_Outcome_Areas_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Method_Of_Intervention_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Method_Of_Intervention_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Target_Group_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Target_Group_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Target_Group_3_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Target_Group_4_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Target_Group_5_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Government_Of_Canada_Activity_Tags_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Canadian_Classification_Of_Functions_Of_Government_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Ultimate_Outcome_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Ultimate_Outcome_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Intermediate_Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tombstones", x => x.Tombstone_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tombstones");
        }
    }
}
