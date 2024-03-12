using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core;

public partial class Clientengagements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Client_Engagements",
            columns: table => new
            {
                Engagement_ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Engagement_Name = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                Engagement_Start_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                Requirements_Gathering_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Requirements_Gathering_ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Phase1_Development_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Phase1_Development_ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Phase1_Testing_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Phase1_Testing_ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Phase2_Development_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Phase2_Development_ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Phase2_Testing_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Phase2_Testing_ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Final_Updates_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Final_Release_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                Actual_Release_Date = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Client_Engagements", x => x.Engagement_ID);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Client_Engagements");
    }
}