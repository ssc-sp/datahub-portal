﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NRCan.Datahub.Metadata.Model;

namespace NRCan.Datahub.Metadata.Migrations
{
    [DbContext(typeof(MetadataDbContext))]
    [Migration("20210901180742_AddingKeywordSourceLengthModified")]
    partial class AddingKeywordSourceLengthModified
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.ApprovalForm", b =>
                {
                    b.Property<int>("ApprovalFormId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<bool>("Approval_InSitu_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Approval_Other_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("Approval_Other_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Authority_To_Release_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Can_Be_Released_For_Free_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Collection_Of_Datasets_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Copyright_Restrictions_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("Dataset_Title_TXT")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("Localized_Metadata_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Machine_Readable_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Non_Propietary_Format_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Not_Clasified_Or_Protected_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Private_Personal_Information_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Requires_Blanket_Approval_FLAG")
                        .HasColumnType("bit");

                    b.Property<bool>("Subject_To_Exceptions_Or_Eclusions_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("Type_Of_Data_TXT")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("nvarchar(16)");

                    b.Property<bool>("Updated_On_Going_Basis_FLAG")
                        .HasColumnType("bit");

                    b.HasKey("ApprovalFormId");

                    b.ToTable("ApprovalForms");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.FieldChoice", b =>
                {
                    b.Property<int>("FieldChoiceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("FieldDefinitionId")
                        .HasColumnType("int");

                    b.Property<string>("Label_English_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Label_French_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Value_TXT")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("FieldChoiceId");

                    b.HasIndex("FieldDefinitionId");

                    b.ToTable("FieldChoices");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.FieldDefinition", b =>
                {
                    b.Property<int>("FieldDefinitionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("English_DESC")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Field_Name_TXT")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("French_DESC")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MetadataVersionId")
                        .HasColumnType("int");

                    b.Property<bool>("MultiSelect_FLAG")
                        .HasColumnType("bit");

                    b.Property<string>("Name_English_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name_French_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Required_FLAG")
                        .HasColumnType("bit");

                    b.Property<int>("Sort_Order_NUM")
                        .HasColumnType("int");

                    b.Property<string>("Validators_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("FieldDefinitionId");

                    b.HasIndex("MetadataVersionId");

                    b.HasIndex("Field_Name_TXT", "MetadataVersionId")
                        .IsUnique();

                    b.ToTable("FieldDefinitions");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.Keyword", b =>
                {
                    b.Property<int>("KeywordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("English_TXT")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("French_TXT")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("Frequency")
                        .HasColumnType("int");

                    b.Property<string>("Source")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("KeywordId");

                    b.HasIndex("English_TXT")
                        .IsUnique()
                        .HasFilter("[English_TXT] IS NOT NULL");

                    b.HasIndex("French_TXT")
                        .IsUnique()
                        .HasFilter("[French_TXT] IS NOT NULL");

                    b.ToTable("Keywords");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.MetadataVersion", b =>
                {
                    b.Property<int>("MetadataVersionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<DateTime>("Last_Update_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Source_TXT")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("Version_Info_TXT")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("MetadataVersionId");

                    b.ToTable("MetadataVersions");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.ObjectFieldValue", b =>
                {
                    b.Property<long>("ObjectMetadataId")
                        .HasColumnType("bigint");

                    b.Property<int>("FieldDefinitionId")
                        .HasColumnType("int");

                    b.Property<string>("Value_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ObjectMetadataId", "FieldDefinitionId");

                    b.HasIndex("FieldDefinitionId");

                    b.ToTable("ObjectFieldValues");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.ObjectMetadata", b =>
                {
                    b.Property<long>("ObjectMetadataId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<int>("MetadataVersionId")
                        .HasColumnType("int");

                    b.Property<string>("ObjectId_TXT")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("ObjectMetadataId");

                    b.HasIndex("MetadataVersionId");

                    b.HasIndex("ObjectId_TXT")
                        .IsUnique();

                    b.ToTable("ObjectMetadata");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.FieldChoice", b =>
                {
                    b.HasOne("NRCan.Datahub.Metadata.Model.FieldDefinition", "FieldDefinition")
                        .WithMany("Choices")
                        .HasForeignKey("FieldDefinitionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FieldDefinition");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.FieldDefinition", b =>
                {
                    b.HasOne("NRCan.Datahub.Metadata.Model.MetadataVersion", "MetadataVersion")
                        .WithMany("Definitions")
                        .HasForeignKey("MetadataVersionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MetadataVersion");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.ObjectFieldValue", b =>
                {
                    b.HasOne("NRCan.Datahub.Metadata.Model.FieldDefinition", "FieldDefinition")
                        .WithMany("FieldValues")
                        .HasForeignKey("FieldDefinitionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("NRCan.Datahub.Metadata.Model.ObjectMetadata", "ObjectMetadata")
                        .WithMany("FieldValues")
                        .HasForeignKey("ObjectMetadataId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("FieldDefinition");

                    b.Navigation("ObjectMetadata");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.ObjectMetadata", b =>
                {
                    b.HasOne("NRCan.Datahub.Metadata.Model.MetadataVersion", "MetadataVersion")
                        .WithMany("Objects")
                        .HasForeignKey("MetadataVersionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MetadataVersion");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.FieldDefinition", b =>
                {
                    b.Navigation("Choices");

                    b.Navigation("FieldValues");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.MetadataVersion", b =>
                {
                    b.Navigation("Definitions");

                    b.Navigation("Objects");
                });

            modelBuilder.Entity("NRCan.Datahub.Metadata.Model.ObjectMetadata", b =>
                {
                    b.Navigation("FieldValues");
                });
#pragma warning restore 612, 618
        }
    }
}
