// <auto-generated />
using System;
using Datahub.M365Forms.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Datahub.M365Forms.Migrations
{
    [DbContext(typeof(M365FormsDBContext))]
    [Migration("20220120143951_editedfields")]
    partial class Editedfields
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

                    b.Property<bool>("Business_Owner_Approval")
                        .HasColumnType("bit");

                    b.Property<bool>("Committee")
                        .HasColumnType("bit");

                    b.Property<string>("Composition")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description_of_Team")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<bool>("Event")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Expected_Lifespan_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("GCdocs_Hyperlink_URL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Information_and_Data_Security_Classification")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Last_Updated_UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("M365FormStatus")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name_of_Team")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("NotificationsSent")
                        .HasColumnType("bit");

                    b.Property<string>("Number")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Ongoing_Lifespan")
                        .HasColumnType("bit");

                    b.Property<bool>("Other")
                        .HasColumnType("bit");

                    b.Property<string>("Other_Txt")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Project_Or_Initiative")
                        .HasColumnType("bit");

                    b.Property<string>("Team_Function")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Team_Owner1")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Team_Owner2")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Team_Owner3")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Visibility")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Working_Group")
                        .HasColumnType("bit");

                    b.HasKey("Application_ID");

                    b.ToTable("M365FormsApplications");
                });
#pragma warning restore 612, 618
        }
    }
}
