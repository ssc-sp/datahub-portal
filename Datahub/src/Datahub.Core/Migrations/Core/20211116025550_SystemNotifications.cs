using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core
{
    public partial class SystemNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemNotifications",
                columns: table => new
                {
                    Notification_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceivingUser_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Generated_TS = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Read_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    NotificationTextEn_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotificationTextFr_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionLink_URL = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ActionLink_Key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemNotifications", x => x.Notification_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemNotifications");
        }
    }
}
