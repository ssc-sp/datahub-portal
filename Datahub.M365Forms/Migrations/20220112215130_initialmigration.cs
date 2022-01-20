using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.M365Forms.Migrations
{
    public partial class initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "M365FormsApplications",
                columns: table => new
                {
                    Application_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name_of_Team = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Description_of_Team = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_Purpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_Function = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Composition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Information_and_Data_Security_Classification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Visibility = table.Column<bool>(type: "bit", nullable: false),
                    GCdocs_Hyperlink_URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expected_Lifespan_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Expected_Lifespan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Business_Owner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_Owner1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_Owner2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_Owner3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Business_Owner_Approval = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_M365FormsApplications", x => x.Application_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "M365FormsApplications");
        }
    }
}
