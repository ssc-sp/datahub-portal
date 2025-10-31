using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class CBR_Cleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK from GCHostingWorkspaceDetails to Projects (legacy1:1 using the same Id)
            migrationBuilder.DropForeignKey(
                name: "FK_GCHostingWorkspaceDetails_Projects_Id",
                table: "GCHostingWorkspaceDetails");

            // Drop FK from Projects -> GCHostingWorkspaceDetails so we can rebuild the table
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_GCHostingWorkspaceDetails_ParentGCHostingBudgetId",
                table: "Projects");

            // New column unrelated to the identity change
            migrationBuilder.AddColumn<bool>(
                name: "AnnouncementCreated",
                table: "VersionTags",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Rebuild GCHostingWorkspaceDetails with Id as IDENTITY; preserve data and Id values
            migrationBuilder.Sql(@"
BEGIN TRAN;

-- Create a temp table with the same schema but with IDENTITY(1,1) on Id
CREATE TABLE [dbo].[GCHostingWorkspaceDetails_tmp](
 [Id] INT IDENTITY(1,1) NOT NULL,
 [GcHostingId] NVARCHAR(MAX) NOT NULL,
 [LeadFirstName] NVARCHAR(200) NOT NULL,
 [LeadLastName] NVARCHAR(200) NOT NULL,
 [DepartmentName] NVARCHAR(200) NOT NULL,
 [LeadEmail] NVARCHAR(200) NOT NULL,
 [FinancialAuthorityFirstName] NVARCHAR(200) NOT NULL,
 [FinancialAuthorityLastName] NVARCHAR(200) NOT NULL,
 [FinancialAuthorityCommitmentIsRef] NVARCHAR(50) NOT NULL,
 [FinancialAuthorityCommitmentIsOrg] NVARCHAR(50) NOT NULL,
 [FinancialAuthorityEmail] NVARCHAR(200) NOT NULL,
 [WorkspaceBudget] DECIMAL(18,4) NOT NULL,
 [WorkspaceName] NVARCHAR(200) NOT NULL,
 [WorkspaceDescription] NVARCHAR(MAX) NOT NULL,
 [Subject] NVARCHAR(MAX) NOT NULL,
 [Keywords] NVARCHAR(MAX) NOT NULL,
 [RetentionPeriodYears] INT NOT NULL,
 [RetentionPeriodStartDate] DATETIME2 NOT NULL,
 [RetentionValue] NVARCHAR(MAX) NOT NULL,
 [GeneratesInfoBusinessValue] BIT NOT NULL,
 [SecurityClassification] NVARCHAR(MAX) NOT NULL,
 [ProjectTitle] NVARCHAR(200) NULL,
 [ProjectDescription] NVARCHAR(MAX) NULL,
 [CBRName] NVARCHAR(MAX) NOT NULL,
 [CBRID] NVARCHAR(MAX) NOT NULL,
 CONSTRAINT [PK_GCHostingWorkspaceDetails_tmp] PRIMARY KEY CLUSTERED ([Id])
);

-- Preserve existing Id values
SET IDENTITY_INSERT [dbo].[GCHostingWorkspaceDetails_tmp] ON;
INSERT INTO [dbo].[GCHostingWorkspaceDetails_tmp] (
 [Id], [GcHostingId], [LeadFirstName], [LeadLastName], [DepartmentName], [LeadEmail],
 [FinancialAuthorityFirstName], [FinancialAuthorityLastName], [FinancialAuthorityCommitmentIsRef], [FinancialAuthorityCommitmentIsOrg], [FinancialAuthorityEmail],
 [WorkspaceBudget], [WorkspaceName], [WorkspaceDescription], [Subject], [Keywords], [RetentionPeriodYears], [RetentionPeriodStartDate], [RetentionValue],
 [GeneratesInfoBusinessValue], [SecurityClassification], [ProjectTitle], [ProjectDescription], [CBRName], [CBRID]
)
SELECT 
 [Id], [GcHostingId], [LeadFirstName], [LeadLastName], [DepartmentName], [LeadEmail],
 [FinancialAuthorityFirstName], [FinancialAuthorityLastName], [FinancialAuthorityCommitmentIsRef], [FinancialAuthorityCommitmentIsOrg], [FinancialAuthorityEmail],
 [WorkspaceBudget], [WorkspaceName], [WorkspaceDescription], [Subject], [Keywords], [RetentionPeriodYears], [RetentionPeriodStartDate], [RetentionValue],
 [GeneratesInfoBusinessValue], [SecurityClassification], [ProjectTitle], [ProjectDescription], [CBRName], [CBRID]
FROM [dbo].[GCHostingWorkspaceDetails];
SET IDENTITY_INSERT [dbo].[GCHostingWorkspaceDetails_tmp] OFF;

-- Replace original table
DROP TABLE [dbo].[GCHostingWorkspaceDetails];
EXEC sp_rename 'dbo.GCHostingWorkspaceDetails_tmp', 'GCHostingWorkspaceDetails';
EXEC sp_rename 'dbo.PK_GCHostingWorkspaceDetails_tmp', 'PK_GCHostingWorkspaceDetails';

-- Ensure identity seed is set to MAX(Id)
DECLARE @maxId INT = (SELECT ISNULL(MAX([Id]),0) FROM [dbo].[GCHostingWorkspaceDetails]);
DBCC CHECKIDENT ('dbo.GCHostingWorkspaceDetails', RESEED, @maxId);

COMMIT;
");

            // Recreate FK from Projects -> GCHostingWorkspaceDetails(Id)
            migrationBuilder.AddForeignKey(
                name: "FK_Projects_GCHostingWorkspaceDetails_ParentGCHostingBudgetId",
                table: "Projects",
                column: "ParentGCHostingBudgetId",
                principalTable: "GCHostingWorkspaceDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FK from Projects -> GCHostingWorkspaceDetails before rebuilding back to non-identity
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_GCHostingWorkspaceDetails_ParentGCHostingBudgetId",
                table: "Projects");

            // Remove the extra column
            migrationBuilder.DropColumn(
                name: "AnnouncementCreated",
                table: "VersionTags");

            // Rebuild GCHostingWorkspaceDetails with Id as non-identity and restore legacy FK to Projects(Id)
            migrationBuilder.Sql(@"
BEGIN TRAN;

-- Create a temp table with Id NOT IDENTITY
CREATE TABLE [dbo].[GCHostingWorkspaceDetails_tmp](
 [Id] INT NOT NULL,
 [GcHostingId] NVARCHAR(MAX) NOT NULL,
 [LeadFirstName] NVARCHAR(200) NOT NULL,
 [LeadLastName] NVARCHAR(200) NOT NULL,
 [DepartmentName] NVARCHAR(200) NOT NULL,
 [LeadEmail] NVARCHAR(200) NOT NULL,
 [FinancialAuthorityFirstName] NVARCHAR(200) NOT NULL,
 [FinancialAuthorityLastName] NVARCHAR(200) NOT NULL,
 [FinancialAuthorityCommitmentIsRef] NVARCHAR(50) NOT NULL,
 [FinancialAuthorityCommitmentIsOrg] NVARCHAR(50) NOT NULL,
 [FinancialAuthorityEmail] NVARCHAR(200) NOT NULL,
 [WorkspaceBudget] DECIMAL(18,4) NOT NULL,
 [WorkspaceName] NVARCHAR(200) NOT NULL,
 [WorkspaceDescription] NVARCHAR(MAX) NOT NULL,
 [Subject] NVARCHAR(MAX) NOT NULL,
 [Keywords] NVARCHAR(MAX) NOT NULL,
 [RetentionPeriodYears] INT NOT NULL,
 [RetentionPeriodStartDate] DATETIME2 NOT NULL,
 [RetentionValue] NVARCHAR(MAX) NOT NULL,
 [GeneratesInfoBusinessValue] BIT NOT NULL,
 [SecurityClassification] NVARCHAR(MAX) NOT NULL,
 [ProjectTitle] NVARCHAR(200) NULL,
 [ProjectDescription] NVARCHAR(MAX) NULL,
 [CBRName] NVARCHAR(MAX) NOT NULL,
 [CBRID] NVARCHAR(MAX) NOT NULL,
 CONSTRAINT [PK_GCHostingWorkspaceDetails_tmp] PRIMARY KEY CLUSTERED ([Id])
);

INSERT INTO [dbo].[GCHostingWorkspaceDetails_tmp] (
 [Id], [GcHostingId], [LeadFirstName], [LeadLastName], [DepartmentName], [LeadEmail],
 [FinancialAuthorityFirstName], [FinancialAuthorityLastName], [FinancialAuthorityCommitmentIsRef], [FinancialAuthorityCommitmentIsOrg], [FinancialAuthorityEmail],
 [WorkspaceBudget], [WorkspaceName], [WorkspaceDescription], [Subject], [Keywords], [RetentionPeriodYears], [RetentionPeriodStartDate], [RetentionValue],
 [GeneratesInfoBusinessValue], [SecurityClassification], [ProjectTitle], [ProjectDescription], [CBRName], [CBRID]
)
SELECT 
 [Id], [GcHostingId], [LeadFirstName], [LeadLastName], [DepartmentName], [LeadEmail],
 [FinancialAuthorityFirstName], [FinancialAuthorityLastName], [FinancialAuthorityCommitmentIsRef], [FinancialAuthorityCommitmentIsOrg], [FinancialAuthorityEmail],
 [WorkspaceBudget], [WorkspaceName], [WorkspaceDescription], [Subject], [Keywords], [RetentionPeriodYears], [RetentionPeriodStartDate], [RetentionValue],
 [GeneratesInfoBusinessValue], [SecurityClassification], [ProjectTitle], [ProjectDescription], [CBRName], [CBRID]
FROM [dbo].[GCHostingWorkspaceDetails];

DROP TABLE [dbo].[GCHostingWorkspaceDetails];
EXEC sp_rename 'dbo.GCHostingWorkspaceDetails_tmp', 'GCHostingWorkspaceDetails';
EXEC sp_rename 'dbo.PK_GCHostingWorkspaceDetails_tmp', 'PK_GCHostingWorkspaceDetails';

COMMIT;
");

            // Restore legacy FK from GCHostingWorkspaceDetails(Id) -> Projects(Project_ID)
            migrationBuilder.AddForeignKey(
                name: "FK_GCHostingWorkspaceDetails_Projects_Id",
                table: "GCHostingWorkspaceDetails",
                column: "Id",
                principalTable: "Projects",
                principalColumn: "Project_ID");

            // Restore FK from Projects -> GCHostingWorkspaceDetails(Id)
            migrationBuilder.AddForeignKey(
                name: "FK_Projects_GCHostingWorkspaceDetails_ParentGCHostingBudgetId",
                table: "Projects",
                column: "ParentGCHostingBudgetId",
                principalTable: "GCHostingWorkspaceDetails",
                principalColumn: "Id");
        }
    }
}
