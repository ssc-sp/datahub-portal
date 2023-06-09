using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class RoleAndPortalUserIdForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE Project_Users
SET PortalUserId = (
    SELECT Id
    FROM PortalUsers
    WHERE PortalUsers.GraphGuid = Project_Users.User_ID
)
WHERE PortalUserId IS NULL


UPDATE Project_Users
SET ApprovedPortalUserId = (
    SELECT Id
    FROM PortalUsers
    WHERE PortalUsers.GraphGuid = Project_Users.ApprovedUser
)
WHERE ApprovedPortalUserId IS NULL


UPDATE Project_Users
SET RoleId =
        CASE
            WHEN IsAdmin = 1 AND IsDataApprover = 1 THEN 2  -- Workspace Lead
            WHEN IsAdmin = 1 AND IsDataApprover = 0 THEN 3  -- Admin 
            WHEN IsAdmin = 0 AND IsDataApprover = 0 THEN 4  -- Collaborator
            ELSE 5                                          -- Guest
            END
WHERE RoleId IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE Project_Users
SET
    User_ID =(
        SELECT GraphGuid
        FROM PortalUsers
        WHERE PortalUsers.Id = Project_Users.PortalUserId
    )
WHERE User_ID IS NULL;


UPDATE Project_Users
SET
    ApprovedUser =(
        SELECT GraphGuid
        FROM PortalUsers
        WHERE PortalUsers.Id = Project_Users.ApprovedPortalUserId
    )
WHERE ApprovedUser IS NULL;


UPDATE Project_Users
SET
    IsAdmin = CASE
                  WHEN RoleId = 2 THEN 1
                  WHEN RoleId = 3 THEN 1
                  WHEN RoleId = 4 THEN 0
                  ELSE 0
        END,
    IsDataApprover = CASE
                         WHEN RoleId = 2 THEN 1
                         WHEN RoleId = 3 THEN 0
                         WHEN RoleId = 4 THEN 0
                         ELSE 0
        END
WHERE RoleId IS NOT NULL;");
        }
    }
}
