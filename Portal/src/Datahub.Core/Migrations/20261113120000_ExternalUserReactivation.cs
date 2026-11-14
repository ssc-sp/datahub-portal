using Datahub.Core.Model.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations;

[DbContext(typeof(SqlServerDatahubContext))]
[Migration("20261113120000_ExternalUserReactivation")]
public partial class ExternalUserReactivation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "ReactivationAccountExpiry",
            table: "WorkspaceInvitations",
            type: "datetimeoffset",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ReactivationAccountExpiry",
            table: "WorkspaceInvitations");
    }
}
