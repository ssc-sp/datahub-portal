using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionNickname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AzureSubscriptions",
                newName: "Nickname");

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionName",
                table: "AzureSubscriptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionName",
                table: "AzureSubscriptions");

            migrationBuilder.RenameColumn(
                name: "Nickname",
                table: "AzureSubscriptions",
                newName: "Name");
        }
    }
}
