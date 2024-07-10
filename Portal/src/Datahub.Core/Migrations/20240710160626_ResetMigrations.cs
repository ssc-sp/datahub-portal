using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class ResetMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achivements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    ConcatenatedRules = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achivements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AzureSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    SubscriptionId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    SubscriptionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzureSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogObjects",
                columns: table => new
                {
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    ObjectId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name_English = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Name_French = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Desc_English = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Desc_French = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogObjects", x => new { x.ObjectType, x.ObjectId });
                });

            migrationBuilder.CreateTable(
                name: "DBCodes",
                columns: table => new
                {
                    DBCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ClassWord_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClassWord_DEF = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBCodes", x => x.DBCode);
                });

            migrationBuilder.CreateTable(
                name: "DocumentationResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hits = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentationResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalPowerBiReports",
                columns: table => new
                {
                    ExternalPowerBiReport_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestingUser = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Is_Created = table.Column<bool>(type: "bit", nullable: false),
                    End_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Report_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Validation_Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidationSalt = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalPowerBiReports", x => x.ExternalPowerBiReport_ID);
                });

            migrationBuilder.CreateTable(
                name: "InfrastructureHealthChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Group = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ResourceType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    HealthCheckTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfrastructureHealthChecks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MiscStoredObjects",
                columns: table => new
                {
                    GeneratedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JsonContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MiscStoredObjects", x => x.GeneratedId);
                    table.UniqueConstraint("AK_MiscStoredObjects_TypeName_Id", x => new { x.TypeName, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "OnboardingApps",
                columns: table => new
                {
                    Application_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_Sector = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Client_Branch = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Client_Division = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Client_Contact_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Client_Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Additional_Contact_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Additional_Contact_Email_EMAIL = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Product_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Summary_Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Goal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Onboarding_Timeline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Engagement_Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Engagement_Category_Other = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data_Security_Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Questions_for_the_DataHub_Team = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotificationsSent = table.Column<bool>(type: "bit", nullable: false),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    ProjectCreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingApps", x => x.Application_ID);
                });

            migrationBuilder.CreateTable(
                name: "Organization_Levels",
                columns: table => new
                {
                    SectorAndBranchS_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Organization_ID = table.Column<int>(type: "int", nullable: false),
                    Full_Acronym_E = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Full_Acronym_F = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Org_Acronym_E = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Org_Acronym_F = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Org_Name_E = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Org_Name_F = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Org_Level = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    Superior_OrgId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Levels", x => x.SectorAndBranchS_ID);
                });

            migrationBuilder.CreateTable(
                name: "PortalUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GraphGuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstLoginDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BannerPictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Project_ApiUsers",
                columns: table => new
                {
                    ProjectApiUser_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Client_Name_TXT = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Project_Acronym_CD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email_Contact_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Expiration_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_ApiUsers", x => x.ProjectApiUser_ID);
                });

            migrationBuilder.CreateTable(
                name: "Project_Costs",
                columns: table => new
                {
                    ProjectCosts_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Project_ID = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CadCost = table.Column<double>(type: "float", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CloudProvider = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Costs", x => x.ProjectCosts_ID);
                });

            migrationBuilder.CreateTable(
                name: "Project_Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Project_Storage_Avgs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    AverageCapacity = table.Column<double>(type: "float", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CloudProvider = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Storage_Avgs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicDataFiles",
                columns: table => new
                {
                    PublicDataFile_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    File_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Filename_TXT = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FolderPath_TXT = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ProjectCode_CD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RequestingUser_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestedDate_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedDate_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedDate_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublicationDate_DT = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicDataFiles", x => x.PublicDataFile_ID);
                });

            migrationBuilder.CreateTable(
                name: "SelfRegistrationDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfRegistrationDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SharedDataFiles",
                columns: table => new
                {
                    SharedDataFile_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    File_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsOpenDataRequest_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Filename_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FolderPath_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectCode_CD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestingUser_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApprovingUser_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequestedDate_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedDate_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedDate_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublicationDate_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnpublishDate_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MetadataCompleted_FLAG = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedDataFiles", x => x.SharedDataFile_ID);
                });

            migrationBuilder.CreateTable(
                name: "SpatialObjectShares",
                columns: table => new
                {
                    GeoObjectShare_ID = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Json_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email_Contact_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ApprovalForm_ID = table.Column<int>(type: "int", nullable: false),
                    ShareStatus = table.Column<int>(type: "int", nullable: false),
                    Approval_Document_URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Publication_ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpatialObjectShares", x => x.GeoObjectShare_ID);
                });

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

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Project_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectorId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    Sector_Name = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Branch_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Division_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contact_List = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Project_Name_Fr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Project_Acronym_CD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Project_Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Project_Admin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Summary_Desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Summary_Desc_Fr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Goal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Initial_Meeting_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Number_Of_Users_Involved = table.Column<int>(type: "int", nullable: true),
                    Is_Private = table.Column<bool>(type: "bit", nullable: false),
                    Is_Featured = table.Column<bool>(type: "bit", nullable: false),
                    Data_Sensitivity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stage_Desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Status_Desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Status = table.Column<int>(type: "int", nullable: true),
                    Project_Phase = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GC_Docs_URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments_NT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Contact_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Next_Meeting_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DatahubAzureSubscriptionId = table.Column<int>(type: "int", nullable: false),
                    Databricks_URL = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    PowerBI_URL = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    WebForms_URL = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    DB_Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DB_Server = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DB_Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OnboardingApplicationId = table.Column<int>(type: "int", nullable: false),
                    MetadataAdded = table.Column<bool>(type: "bit", nullable: true),
                    WebAppEnabled = table.Column<bool>(type: "bit", nullable: true),
                    WebAppUrlRewritingEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OperationalWindow = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasCostRecovery = table.Column<bool>(type: "bit", nullable: false),
                    WebApp_URL = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Version = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    GitRepo_URL = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    HashedAPIToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Project_ID);
                    table.ForeignKey(
                        name: "FK_Projects_AzureSubscriptions_DatahubAzureSubscriptionId",
                        column: x => x.DatahubAzureSubscriptionId,
                        principalTable: "AzureSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projects_Organization_Levels_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Organization_Levels",
                        principalColumn: "SectorAndBranchS_ID");
                    table.ForeignKey(
                        name: "FK_Projects_Organization_Levels_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Organization_Levels",
                        principalColumn: "SectorAndBranchS_ID");
                    table.ForeignKey(
                        name: "FK_Projects_Organization_Levels_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Organization_Levels",
                        principalColumn: "SectorAndBranchS_ID");
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreviewEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreviewFr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyFr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ForceHidden = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcements_PortalUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Announcements_PortalUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TelemetryEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortalUserId = table.Column<int>(type: "int", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelemetryEvents_PortalUsers_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortalUserId = table.Column<int>(type: "int", nullable: false),
                    AchievementId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Count = table.Column<int>(type: "int", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achivements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achivements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAchievements_PortalUsers_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserInactivityNotifications",
                columns: table => new
                {
                    User_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    NotificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysBeforeLocked = table.Column<int>(type: "int", nullable: false),
                    DaysBeforeDeleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInactivityNotifications", x => x.User_ID);
                    table.ForeignKey(
                        name: "FK_UserInactivityNotifications_PortalUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRecentLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LinkType = table.Column<int>(type: "int", nullable: false),
                    PowerBIURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Variant = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatabricksURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AzureWebAppUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebFormsURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataProject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PBIReportId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PBIWorkspaceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExternalUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceArticleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceArticleTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRecentLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRecentLink_PortalUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    PortalUserId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AcceptedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    NotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    HideAchievements = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HideAlerts = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HiddenAlerts = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.PortalUserId);
                    table.ForeignKey(
                        name: "FK_UserSettings_PortalUsers_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OpenDataSharedFile",
                columns: table => new
                {
                    SharedDataFile_ID = table.Column<long>(type: "bigint", nullable: false),
                    ApprovalForm_ID = table.Column<int>(type: "int", nullable: true),
                    SignedApprovalForm_URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalFormRead_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    FileStorage_CD = table.Column<int>(type: "int", nullable: true),
                    UploadStatus_CD = table.Column<int>(type: "int", nullable: false),
                    UploadError_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileUrl_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalFormEdited_FLAG = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenDataSharedFile", x => x.SharedDataFile_ID);
                    table.ForeignKey(
                        name: "FK_OpenDataSharedFile_SharedDataFiles_SharedDataFile_ID",
                        column: x => x.SharedDataFile_ID,
                        principalTable: "SharedDataFiles",
                        principalColumn: "SharedDataFile_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Client_Engagements",
                columns: table => new
                {
                    Engagement_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Engagement_Name = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Engagment_Owners = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Engagment_Lead = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    Is_Engagement_Active = table.Column<bool>(type: "bit", nullable: false),
                    Engagement_Start_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Requirements_Gathering_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Phase1_Development_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Phase1_Testing_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Phase2_Development_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Phase2_Testing_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Final_Updates_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Final_Release_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Requirements_Gathering_ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Phase1_Development_ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Phase1_Testing_ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Phase2_Development_ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Phase2_Testing_ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Actual_Release_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client_Engagements", x => x.Engagement_ID);
                    table.ForeignKey(
                        name: "FK_Client_Engagements_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "OpenDataSubmissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProcessType = table.Column<int>(type: "int", nullable: false),
                    DatasetTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpenForAttachingFiles = table.Column<bool>(type: "bit", nullable: false),
                    RequestingUserId = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenDataSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenDataSubmissions_PortalUsers_RequestingUserId",
                        column: x => x.RequestingUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpenDataSubmissions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PowerBI_License_Requests",
                columns: table => new
                {
                    Request_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Premium_License_Flag = table.Column<bool>(type: "bit", nullable: false),
                    Contact_Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Contact_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: false),
                    Desktop_Usage_Flag = table.Column<bool>(type: "bit", nullable: false),
                    User_ID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerBI_License_Requests", x => x.Request_ID);
                    table.ForeignKey(
                        name: "FK_PowerBI_License_Requests_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PowerBi_Workspaces",
                columns: table => new
                {
                    Workspace_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Workspace_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sandbox_Flag = table.Column<bool>(type: "bit", nullable: false),
                    Project_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerBi_Workspaces", x => x.Workspace_ID);
                    table.ForeignKey(
                        name: "FK_PowerBi_Workspaces_Projects_Project_Id",
                        column: x => x.Project_Id,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "Project_Cloud_Storages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false, defaultValue: "Azure"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConnectionData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Cloud_Storages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Cloud_Storages_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Project_Comments",
                columns: table => new
                {
                    Comment_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Comment_Date_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment_NT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_DT = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Comments", x => x.Comment_ID);
                    table.ForeignKey(
                        name: "FK_Project_Comments_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "Project_Credits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRollover = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Current = table.Column<double>(type: "float", nullable: false),
                    BudgetCurrentSpent = table.Column<double>(type: "float", nullable: false),
                    CurrentPerService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentPerDay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YesterdayCredits = table.Column<double>(type: "float", nullable: false),
                    YesterdayPerService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastNotified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PercNotified = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Credits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Credits_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "Project_Pipeline_Links",
                columns: table => new
                {
                    Project_ID = table.Column<int>(type: "int", nullable: false),
                    Process_Nm = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Pipeline_Links", x => new { x.Project_ID, x.Process_Nm });
                    table.ForeignKey(
                        name: "FK_Project_Pipeline_Links_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Project_Repositories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    RepositoryUrl = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    HeadCommitId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Repositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Repositories_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "Project_Resources2",
                columns: table => new
                {
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JsonContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestedById = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<int>(type: "int", nullable: true),
                    InputJsonContent = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Resources2", x => x.ResourceId);
                    table.ForeignKey(
                        name: "FK_Project_Resources2_PortalUsers_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Project_Resources2_PortalUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Project_Resources2_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Project_Users",
                columns: table => new
                {
                    ProjectUser_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortalUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedPortalUserId = table.Column<int>(type: "int", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: true),
                    Project_ID = table.Column<int>(type: "int", nullable: false),
                    Approved_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    User_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDataApprover = table.Column<bool>(type: "bit", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    User_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApprovedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Users", x => x.ProjectUser_ID);
                    table.ForeignKey(
                        name: "FK_Project_Users_PortalUsers_ApprovedPortalUserId",
                        column: x => x.ApprovedPortalUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Project_Users_PortalUsers_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Project_Users_Project_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Project_Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Project_Users_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "Project_Users_Requests",
                columns: table => new
                {
                    ProjectUserRequest_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Requested_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Approved_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Users_Requests", x => x.ProjectUserRequest_ID);
                    table.ForeignKey(
                        name: "FK_Project_Users_Requests_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "Project_Whitelists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    AdminLastUpdated_ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminLastUpdated_UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AllowStorage = table.Column<bool>(type: "bit", nullable: false),
                    AllowDatabricks = table.Column<bool>(type: "bit", nullable: false),
                    AllowVMs = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Whitelists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Whitelists_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "ProjectCreationDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    InterestedFeatures = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCreationDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectCreationDetails_PortalUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectCreationDetails_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "ProjectInactivityNotifications",
                columns: table => new
                {
                    Project_ID = table.Column<int>(type: "int", nullable: false),
                    NotificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysBeforeDeletion = table.Column<int>(type: "int", nullable: false),
                    SentTo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInactivityNotifications", x => x.Project_ID);
                    table.ForeignKey(
                        name: "FK_ProjectInactivityNotifications_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "WebForms",
                columns: table => new
                {
                    WebForm_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description_DESC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebForms", x => x.WebForm_ID);
                    table.ForeignKey(
                        name: "FK_WebForms_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TbsOpenGovSubmissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    MetadataComplete = table.Column<bool>(type: "bit", nullable: false),
                    OpenGovCriteriaFormId = table.Column<int>(type: "int", nullable: true),
                    OpenGovCriteriaMetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LocalDQCheckStarted = table.Column<bool>(type: "bit", nullable: false),
                    LocalDQCheckPassed = table.Column<bool>(type: "bit", nullable: false),
                    InitialOpenGovSubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpenGovDQCheckPassed = table.Column<bool>(type: "bit", nullable: false),
                    ImsoApprovalRequestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImsoApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpenGovPublicationDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbsOpenGovSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TbsOpenGovSubmissions_OpenDataSubmissions_Id",
                        column: x => x.Id,
                        principalTable: "OpenDataSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PowerBI_License_User_Requests",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LicenseType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RequestID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerBI_License_User_Requests", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PowerBI_License_User_Requests_PowerBI_License_Requests_RequestID",
                        column: x => x.RequestID,
                        principalTable: "PowerBI_License_Requests",
                        principalColumn: "Request_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PowerBi_DataSets",
                columns: table => new
                {
                    DataSet_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataSet_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Workspace_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerBi_DataSets", x => x.DataSet_ID);
                    table.ForeignKey(
                        name: "FK_PowerBi_DataSets_PowerBi_Workspaces_Workspace_Id",
                        column: x => x.Workspace_Id,
                        principalTable: "PowerBi_Workspaces",
                        principalColumn: "Workspace_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PowerBi_Reports",
                columns: table => new
                {
                    Report_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Report_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Workspace_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InCatalog = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerBi_Reports", x => x.Report_ID);
                    table.ForeignKey(
                        name: "FK_PowerBi_Reports_PowerBi_Workspaces_Workspace_Id",
                        column: x => x.Workspace_Id,
                        principalTable: "PowerBi_Workspaces",
                        principalColumn: "Workspace_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpenDataPublishFiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<long>(type: "bigint", nullable: false),
                    FilePurpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectStorageId = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FolderPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContainerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadStatus = table.Column<int>(type: "int", nullable: false),
                    UploadMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenDataPublishFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenDataPublishFiles_OpenDataSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "OpenDataSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpenDataPublishFiles_Project_Cloud_Storages_ProjectStorageId",
                        column: x => x.ProjectStorageId,
                        principalTable: "Project_Cloud_Storages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    FieldID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Section_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Field_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description_DESC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Choices_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Extension_CD = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false, defaultValue: "NONE"),
                    Type_CD = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false, defaultValue: "Text"),
                    Max_Length_NUM = table.Column<int>(type: "int", nullable: true),
                    Notes_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mandatory_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Date_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WebForm_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.FieldID);
                    table.ForeignKey(
                        name: "FK_Fields_WebForms_WebForm_ID",
                        column: x => x.WebForm_ID,
                        principalTable: "WebForms",
                        principalColumn: "WebForm_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Achivements",
                columns: new[] { "Id", "ConcatenatedRules", "Description", "Name", "Points" },
                values: new object[,]
                {
                    { "DHA-001", "Utils.MatchMetric(\"user_login\", currentMetric)", "Logged in to DataHub", "Collaboration Connoisseur", 1 },
                    { "DHA-002", "Utils.MatchMetric(\"user_sent_invite\", currentMetric)", "Invite a user to your workspace", "Collaboration Commander", 1 },
                    { "DHA-003", "Utils.MatchMetric(\"user_joined_project\", currentMetric)", "Join a workspace", "Workspace Warrior", 1 },
                    { "DHA-004", "Utils.MatchMetric(\"user_left_project\", currentMetric)", "Leave a workspace", "Workspace Wanderlust", 1 },
                    { "DHA-005", "Utils.MatchMetric(\"user_daily_login\", currentMetric)", "Login on multiple days", "Consistent Contributor", 1 },
                    { "EXP-000", "Utils.OwnsAchievement(\"EXP-001\", achivements)\nUtils.OwnsAchievement(\"EXP-002\", achivements)\nUtils.OwnsAchievement(\"EXP-003\", achivements)\nUtils.OwnsAchievement(\"EXP-004\", achivements)\nUtils.OwnsAchievement(\"EXP-005\", achivements)\nUtils.OwnsAchievement(\"EXP-006\", achivements)\nUtils.OwnsAchievement(\"EXP-007\", achivements)\nUtils.OwnsAchievement(\"EXP-008\", achivements)\nUtils.OwnsAchievement(\"EXP-009\", achivements)", "Unlock all the 2.0 Exploration achievements", "Explorer Extraordinaire", 1 },
                    { "EXP-001", "Utils.MatchUrl(\"\\\\/w\\\\/([0-9a-zA-Z]+)?\\\\/filelist$\", currentMetric)", "Navigate to the Storage Explorer page of a workspace", "Storage Safari", 1 },
                    { "EXP-002", "Utils.MatchMetric(\"user_click_databricks_link\", currentMetric)", "Navigate to the Databricks page of a workspace", "Databricks Discovery", 1 },
                    { "EXP-003", "Utils.MatchUrl(\"\\\\/resources$\", currentMetric)", "View the resources section of DataHub", "Resource Ranger", 1 },
                    { "EXP-004", "Utils.MatchMetric(\"user_view_project_not_member_of\", currentMetric)", "View a workspace you are not a member of", "Workspace Wanderer", 1 },
                    { "EXP-005", "Utils.MatchMetric(\"user_view_project\", currentMetric)", "Visit one of your own workspaces", "Workspace Wayfarer", 1 },
                    { "EXP-006", "Utils.MatchMetric(\"user_click_recent_link\", currentMetric)", "Use a recent link to get to the same page again", "Link Legend", 1 },
                    { "EXP-007", "Utils.MatchMetric(\"user_click_toggle_culture\", currentMetric)", "Switch languages in the portal", "Prolific Polyglot", 1 },
                    { "EXP-008", "Utils.MatchUrl(\"\\\\/profile$\", currentMetric)", "View your own profile page", "Profile Peruser", 1 },
                    { "EXP-009", "Utils.MatchMetric(\"user_view_other_profile\", currentMetric)", "View another person's profile", "Profile Prowler", 1 },
                    { "STR-000", "Utils.OwnsAchievement(\"STR-001\", achivements)\nUtils.OwnsAchievement(\"STR-003\", achivements)\nUtils.OwnsAchievement(\"STR-004\", achivements)\nUtils.OwnsAchievement(\"STR-005\", achivements)\nUtils.OwnsAchievement(\"STR-006\", achivements)", "Unlock all the 2.0 Storage Explorer achievements", "Storage Savant", 1 },
                    { "STR-001", "Utils.MatchMetric(\"user_upload_file\", currentMetric)", "Upload a file using the workspace Storage Explorer", "Unstoppable Uploader", 1 },
                    { "STR-002", "Utils.MatchMetric(\"user_share_file\", currentMetric)", "Share a file using the workspace Storage Explorer", "Storage Socialite", 1 },
                    { "STR-003", "Utils.MatchMetric(\"user_download_file\", currentMetric)", "Download a file using the workspace Storage Explorer", "File Fetcher", 1 },
                    { "STR-004", "Utils.MatchMetric(\"user_delete_file\", currentMetric)", "Delete a file from the workspace with the Storage Explorer", "Daredevil Deleter", 1 },
                    { "STR-005", "Utils.MatchMetric(\"user_create_folder\", currentMetric)", "Create a folder in the workspace's Storage Explorer", "Folder Fashionista", 1 },
                    { "STR-006", "Utils.MatchMetric(\"user_delete_folder\", currentMetric)", "Delete a folder in the workspace's Storage Explorer", "Folder Farewell", 1 }
                });

            migrationBuilder.InsertData(
                table: "Project_Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Revoke the user's access to the project's private resources", "Remove User" },
                    { 2, "Head of the business unit and bears business responsibility for successful implementation and availability", "Workspace Lead" },
                    { 3, "Management authority within the project with direct supervision over the project resources and deliverables", "Admin" },
                    { 4, "Responsible for contributing to the overall project objectives and deliverables to ensure success", "Collaborator" },
                    { 5, "Able to view the workspace and its contents but not able to contribute or modify anything", "Guest" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CreatedById",
                table: "Announcements",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_UpdatedById",
                table: "Announcements",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Client_Engagements_Project_ID",
                table: "Client_Engagements",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_WebForm_ID",
                table: "Fields",
                column: "WebForm_ID");

            migrationBuilder.CreateIndex(
                name: "IX_OpenDataPublishFiles_ProjectStorageId",
                table: "OpenDataPublishFiles",
                column: "ProjectStorageId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenDataPublishFiles_SubmissionId",
                table: "OpenDataPublishFiles",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenDataSubmissions_ProjectId",
                table: "OpenDataSubmissions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenDataSubmissions_RequestingUserId",
                table: "OpenDataSubmissions",
                column: "RequestingUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenDataSubmissions_UniqueId",
                table: "OpenDataSubmissions",
                column: "UniqueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortalUsers_GraphGuid",
                table: "PortalUsers",
                column: "GraphGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PowerBi_DataSets_Workspace_Id",
                table: "PowerBi_DataSets",
                column: "Workspace_Id");

            migrationBuilder.CreateIndex(
                name: "IX_PowerBI_License_Requests_Project_ID",
                table: "PowerBI_License_Requests",
                column: "Project_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PowerBI_License_User_Requests_RequestID",
                table: "PowerBI_License_User_Requests",
                column: "RequestID");

            migrationBuilder.CreateIndex(
                name: "IX_PowerBi_Reports_Workspace_Id",
                table: "PowerBi_Reports",
                column: "Workspace_Id");

            migrationBuilder.CreateIndex(
                name: "IX_PowerBi_Workspaces_Project_Id",
                table: "PowerBi_Workspaces",
                column: "Project_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Cloud_Storages_ProjectId",
                table: "Project_Cloud_Storages",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Comments_Project_ID",
                table: "Project_Comments",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Costs_Project_ID_Date",
                table: "Project_Costs",
                columns: new[] { "Project_ID", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Project_Credits_ProjectId",
                table: "Project_Credits",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Project_Repositories_ProjectId",
                table: "Project_Repositories",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Resources2_ProjectId",
                table: "Project_Resources2",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Resources2_RequestedById",
                table: "Project_Resources2",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Resources2_UpdatedById",
                table: "Project_Resources2",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Storage_Avgs_ProjectId_Date",
                table: "Project_Storage_Avgs",
                columns: new[] { "ProjectId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_ApprovedPortalUserId",
                table: "Project_Users",
                column: "ApprovedPortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_PortalUserId",
                table: "Project_Users",
                column: "PortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_Project_ID",
                table: "Project_Users",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_RoleId",
                table: "Project_Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_Requests_Project_ID",
                table: "Project_Users_Requests",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Whitelists_ProjectId",
                table: "Project_Whitelists",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCreationDetails_CreatedById",
                table: "ProjectCreationDetails",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCreationDetails_ProjectId",
                table: "ProjectCreationDetails",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_BranchId",
                table: "Projects",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DatahubAzureSubscriptionId",
                table: "Projects",
                column: "DatahubAzureSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DivisionId",
                table: "Projects",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Project_Acronym_CD",
                table: "Projects",
                column: "Project_Acronym_CD",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SectorId",
                table: "Projects",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicDataFiles_File_ID",
                table: "PublicDataFiles",
                column: "File_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SharedDataFiles_File_ID",
                table: "SharedDataFiles",
                column: "File_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryEvents_PortalUserId",
                table: "TelemetryEvents",
                column: "PortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_PortalUserId",
                table: "UserAchievements",
                column: "PortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInactivityNotifications_UserId",
                table: "UserInactivityNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRecentLink_UserId",
                table: "UserRecentLink",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WebForms_Project_ID",
                table: "WebForms",
                column: "Project_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "CatalogObjects");

            migrationBuilder.DropTable(
                name: "Client_Engagements");

            migrationBuilder.DropTable(
                name: "DBCodes");

            migrationBuilder.DropTable(
                name: "DocumentationResources");

            migrationBuilder.DropTable(
                name: "ExternalPowerBiReports");

            migrationBuilder.DropTable(
                name: "Fields");

            migrationBuilder.DropTable(
                name: "InfrastructureHealthChecks");

            migrationBuilder.DropTable(
                name: "MiscStoredObjects");

            migrationBuilder.DropTable(
                name: "OnboardingApps");

            migrationBuilder.DropTable(
                name: "OpenDataPublishFiles");

            migrationBuilder.DropTable(
                name: "OpenDataSharedFile");

            migrationBuilder.DropTable(
                name: "PowerBi_DataSets");

            migrationBuilder.DropTable(
                name: "PowerBI_License_User_Requests");

            migrationBuilder.DropTable(
                name: "PowerBi_Reports");

            migrationBuilder.DropTable(
                name: "Project_ApiUsers");

            migrationBuilder.DropTable(
                name: "Project_Comments");

            migrationBuilder.DropTable(
                name: "Project_Costs");

            migrationBuilder.DropTable(
                name: "Project_Credits");

            migrationBuilder.DropTable(
                name: "Project_Pipeline_Links");

            migrationBuilder.DropTable(
                name: "Project_Repositories");

            migrationBuilder.DropTable(
                name: "Project_Resources2");

            migrationBuilder.DropTable(
                name: "Project_Storage_Avgs");

            migrationBuilder.DropTable(
                name: "Project_Users");

            migrationBuilder.DropTable(
                name: "Project_Users_Requests");

            migrationBuilder.DropTable(
                name: "Project_Whitelists");

            migrationBuilder.DropTable(
                name: "ProjectCreationDetails");

            migrationBuilder.DropTable(
                name: "ProjectInactivityNotifications");

            migrationBuilder.DropTable(
                name: "PublicDataFiles");

            migrationBuilder.DropTable(
                name: "SelfRegistrationDetails");

            migrationBuilder.DropTable(
                name: "SpatialObjectShares");

            migrationBuilder.DropTable(
                name: "SystemNotifications");

            migrationBuilder.DropTable(
                name: "TbsOpenGovSubmissions");

            migrationBuilder.DropTable(
                name: "TelemetryEvents");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "UserInactivityNotifications");

            migrationBuilder.DropTable(
                name: "UserRecentLink");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "WebForms");

            migrationBuilder.DropTable(
                name: "Project_Cloud_Storages");

            migrationBuilder.DropTable(
                name: "SharedDataFiles");

            migrationBuilder.DropTable(
                name: "PowerBI_License_Requests");

            migrationBuilder.DropTable(
                name: "PowerBi_Workspaces");

            migrationBuilder.DropTable(
                name: "Project_Roles");

            migrationBuilder.DropTable(
                name: "OpenDataSubmissions");

            migrationBuilder.DropTable(
                name: "Achivements");

            migrationBuilder.DropTable(
                name: "PortalUsers");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "AzureSubscriptions");

            migrationBuilder.DropTable(
                name: "Organization_Levels");
        }
    }
}
