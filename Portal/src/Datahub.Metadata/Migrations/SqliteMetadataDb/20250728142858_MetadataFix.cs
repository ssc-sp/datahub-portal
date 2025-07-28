using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Metadata.Migrations.SqliteMetadataDb
{
    /// <inheritdoc />
    public partial class MetadataFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalForms",
                columns: table => new
                {
                    ApprovalFormId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Department_NAME = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Sector_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    Sector_NAME = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Branch_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    Branch_NAME = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Division_NAME = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Section_NAME = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Name_NAME = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Phone_TXT = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Email_EMAIL = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Dataset_Title_TXT = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Type_Of_Data_TXT = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Misc_Compliant_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Authority_To_Release_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Private_Personal_Information_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Subject_To_Exceptions_Or_Eclusions_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Security_Compliant_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Can_Be_Released_For_Free_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Machine_Readable_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Non_Propietary_Format_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Localized_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Requires_Blanket_Approval_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Updated_On_Going_Basis_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Collection_Of_Datasets_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Approval_InSitu_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Approval_Other_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Approval_Other_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Confidentiality_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Privacy_Exemption_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Accessible_Format_FLAG = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalForms", x => x.ApprovalFormId);
                });

            migrationBuilder.CreateTable(
                name: "Keywords",
                columns: table => new
                {
                    KeywordId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    English_TXT = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    French_TXT = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Frequency = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keywords", x => x.KeywordId);
                });

            migrationBuilder.CreateTable(
                name: "MetadataVersions",
                columns: table => new
                {
                    MetadataVersionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Source_TXT = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Last_Update_DT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version_Info_TXT = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataVersions", x => x.MetadataVersionId);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.ProfileId);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Subject_TXT = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.SubjectId);
                });

            migrationBuilder.CreateTable(
                name: "SubSubjects",
                columns: table => new
                {
                    SubSubjectId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name_English_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Name_French_TXT = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubSubjects", x => x.SubSubjectId);
                });

            migrationBuilder.CreateTable(
                name: "FieldDefinitions",
                columns: table => new
                {
                    FieldDefinitionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MetadataVersionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Field_Name_TXT = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Sort_Order_NUM = table.Column<int>(type: "INTEGER", nullable: false),
                    Name_English_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Name_French_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    English_DESC = table.Column<string>(type: "TEXT", nullable: true),
                    French_DESC = table.Column<string>(type: "TEXT", nullable: true),
                    Required_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    MultiSelect_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Validators_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Custom_Field_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Default_Value_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    CascadeParentId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldDefinitions", x => x.FieldDefinitionId);
                    table.ForeignKey(
                        name: "FK_FieldDefinitions_MetadataVersions_MetadataVersionId",
                        column: x => x.MetadataVersionId,
                        principalTable: "MetadataVersions",
                        principalColumn: "MetadataVersionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObjectMetadata",
                columns: table => new
                {
                    ObjectMetadataId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MetadataVersionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ObjectId_TXT = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectMetadata", x => x.ObjectMetadataId);
                    table.ForeignKey(
                        name: "FK_ObjectMetadata_MetadataVersions_MetadataVersionId",
                        column: x => x.MetadataVersionId,
                        principalTable: "MetadataVersions",
                        principalColumn: "MetadataVersionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    SectionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name_English_TXT = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Name_French_TXT = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.SectionId);
                    table.ForeignKey(
                        name: "FK_Sections_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubSubjectSubject",
                columns: table => new
                {
                    SubSubjectsSubSubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectsSubjectId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubSubjectSubject", x => new { x.SubSubjectsSubSubjectId, x.SubjectsSubjectId });
                    table.ForeignKey(
                        name: "FK_SubSubjectSubject_SubSubjects_SubSubjectsSubSubjectId",
                        column: x => x.SubSubjectsSubSubjectId,
                        principalTable: "SubSubjects",
                        principalColumn: "SubSubjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubSubjectSubject_Subjects_SubjectsSubjectId",
                        column: x => x.SubjectsSubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldChoices",
                columns: table => new
                {
                    FieldChoiceId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FieldDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value_TXT = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Cascading_Value_TXT = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Label_English_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Label_French_TXT = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldChoices", x => x.FieldChoiceId);
                    table.ForeignKey(
                        name: "FK_FieldChoices_FieldDefinitions_FieldDefinitionId",
                        column: x => x.FieldDefinitionId,
                        principalTable: "FieldDefinitions",
                        principalColumn: "FieldDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogObjects",
                columns: table => new
                {
                    CatalogObjectId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectMetadataId = table.Column<long>(type: "INTEGER", nullable: false),
                    DataType = table.Column<byte>(type: "INTEGER", nullable: false),
                    Name_TXT = table.Column<string>(type: "TEXT", nullable: false),
                    Name_French_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Location_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityClass_TXT = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Unclassified"),
                    Contact_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Search_English_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Search_French_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Url_English_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Url_French_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<int>(type: "INTEGER", nullable: false),
                    Classification_Type = table.Column<byte>(type: "INTEGER", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogObjects", x => x.CatalogObjectId);
                    table.ForeignKey(
                        name: "FK_CatalogObjects_ObjectMetadata_ObjectMetadataId",
                        column: x => x.ObjectMetadataId,
                        principalTable: "ObjectMetadata",
                        principalColumn: "ObjectMetadataId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObjectFieldValues",
                columns: table => new
                {
                    ObjectMetadataId = table.Column<long>(type: "INTEGER", nullable: false),
                    FieldDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value_TXT = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectFieldValues", x => new { x.ObjectMetadataId, x.FieldDefinitionId });
                    table.ForeignKey(
                        name: "FK_ObjectFieldValues_FieldDefinitions_FieldDefinitionId",
                        column: x => x.FieldDefinitionId,
                        principalTable: "FieldDefinitions",
                        principalColumn: "FieldDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ObjectFieldValues_ObjectMetadata_ObjectMetadataId",
                        column: x => x.ObjectMetadataId,
                        principalTable: "ObjectMetadata",
                        principalColumn: "ObjectMetadataId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SectionFields",
                columns: table => new
                {
                    SectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Required_FLAG = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionFields", x => new { x.SectionId, x.FieldDefinitionId });
                    table.ForeignKey(
                        name: "FK_SectionFields_FieldDefinitions_FieldDefinitionId",
                        column: x => x.FieldDefinitionId,
                        principalTable: "FieldDefinitions",
                        principalColumn: "FieldDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SectionFields_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogObjects_GroupId",
                table: "CatalogObjects",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogObjects_ObjectMetadataId",
                table: "CatalogObjects",
                column: "ObjectMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldChoices_FieldDefinitionId",
                table: "FieldChoices",
                column: "FieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldDefinitions_Field_Name_TXT_MetadataVersionId",
                table: "FieldDefinitions",
                columns: new[] { "Field_Name_TXT", "MetadataVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldDefinitions_MetadataVersionId",
                table: "FieldDefinitions",
                column: "MetadataVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Keywords_English_TXT",
                table: "Keywords",
                column: "English_TXT",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Keywords_French_TXT",
                table: "Keywords",
                column: "French_TXT",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObjectFieldValues_FieldDefinitionId",
                table: "ObjectFieldValues",
                column: "FieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectMetadata_MetadataVersionId",
                table: "ObjectMetadata",
                column: "MetadataVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectMetadata_ObjectId_TXT",
                table: "ObjectMetadata",
                column: "ObjectId_TXT",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Name",
                table: "Profiles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionFields_FieldDefinitionId",
                table: "SectionFields",
                column: "FieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ProfileId",
                table: "Sections",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Subject_TXT",
                table: "Subjects",
                column: "Subject_TXT",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubSubjectSubject_SubjectsSubjectId",
                table: "SubSubjectSubject",
                column: "SubjectsSubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalForms");

            migrationBuilder.DropTable(
                name: "CatalogObjects");

            migrationBuilder.DropTable(
                name: "FieldChoices");

            migrationBuilder.DropTable(
                name: "Keywords");

            migrationBuilder.DropTable(
                name: "ObjectFieldValues");

            migrationBuilder.DropTable(
                name: "SectionFields");

            migrationBuilder.DropTable(
                name: "SubSubjectSubject");

            migrationBuilder.DropTable(
                name: "ObjectMetadata");

            migrationBuilder.DropTable(
                name: "FieldDefinitions");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "SubSubjects");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "MetadataVersions");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
