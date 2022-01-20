﻿// <auto-generated />
using System;
using Datahub.Portal.Data.M365Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Datahub.M365Forms.Migrations
{
    [DbContext(typeof(M365FormsDBContext))]
    [Migration("20220112215130_initialmigration")]
    partial class initialmigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Datahub.Portal.Data.M365FormsApplication", b =>
                {
                    b.Property<int>("Application_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Application_ID"), 1L, 1);

                    b.Property<string>("Business_Owner")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Business_Owner_Approval")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Composition")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description_of_Team")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Expected_Lifespan")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Expected_Lifespan_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("GCdocs_Hyperlink_URL")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Information_and_Data_Security_Classification")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Last_Updated_UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name_of_Team")
                        .IsRequired()
                        .HasMaxLength(35)
                        .HasColumnType("nvarchar(35)");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Team_Function")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Team_Owner1")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Team_Owner2")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Team_Owner3")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Team_Purpose")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<bool>("Visibility")
                        .HasColumnType("bit");

                    b.HasKey("Application_ID");

                    b.ToTable("M365FormsApplications");
                });
#pragma warning restore 612, 618
        }
    }
}
