﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Noctus.Persistence.Contexts;

namespace Noctus.Persistence.Migrations
{
    [DbContext(typeof(NoctusDbContext))]
    [Migration("20210402135619_Initial009")]
    partial class Initial009
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.3")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Noctus.Domain.Entities.GenBucket", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<int>("CurrentStock")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastUpdateOn")
                        .HasColumnType("datetime2");

                    b.Property<int?>("LicenseKeyId")
                        .HasColumnType("int");

                    b.Property<string>("Ref")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("LicenseKeyId");

                    b.ToTable("AccountsGenBuckets");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.GenBucketConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LastUpdateOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("Ref")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("GenBucketConfigs");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.IdentifiersInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("IpAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastUpdateOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("MotherBoardSerialNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserDiscordId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("IdentifiersInfo");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.LicenseKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastUpdateOn")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("LicenseKeys");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.LicenseKeyEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<int>("Event")
                        .HasColumnType("int");

                    b.Property<int?>("IdentifiersInfoId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastUpdateOn")
                        .HasColumnType("datetime2");

                    b.Property<int?>("LicenseKeyId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("IdentifiersInfoId");

                    b.HasIndex("LicenseKeyId");

                    b.ToTable("LicenseKeyEvents");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.LicenseKeyFlag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("IdentifiersInfoId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastUpdateOn")
                        .HasColumnType("datetime2");

                    b.Property<int?>("LicenseKeyId")
                        .HasColumnType("int");

                    b.Property<int>("Reason")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("IdentifiersInfoId");

                    b.HasIndex("LicenseKeyId");

                    b.ToTable("LicenseKeyFlags");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.PipelineEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<int>("EventType")
                        .HasColumnType("int");

                    b.Property<int?>("IdentifiersInfoId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastUpdateOn")
                        .HasColumnType("datetime2");

                    b.Property<int?>("PipelineRunId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("IdentifiersInfoId");

                    b.HasIndex("PipelineRunId");

                    b.ToTable("PipelineEvents");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.PipelineRun", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AccountCountryCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<bool>("HasForwarding")
                        .HasColumnType("bit");

                    b.Property<int>("JobsNumber")
                        .HasColumnType("int");

                    b.Property<int>("JobsParallelism")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastUpdateOn")
                        .HasColumnType("datetime2");

                    b.Property<int?>("LicenseKeyId")
                        .HasColumnType("int");

                    b.Property<string>("PvaCountryCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("LicenseKeyId");

                    b.ToTable("PipelineRuns");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.GenBucket", b =>
                {
                    b.HasOne("Noctus.Domain.Entities.LicenseKey", null)
                        .WithMany("AccountsGenBuckets")
                        .HasForeignKey("LicenseKeyId");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.LicenseKeyEvent", b =>
                {
                    b.HasOne("Noctus.Domain.Entities.IdentifiersInfo", "IdentifiersInfo")
                        .WithMany()
                        .HasForeignKey("IdentifiersInfoId");

                    b.HasOne("Noctus.Domain.Entities.LicenseKey", null)
                        .WithMany("KeyEvents")
                        .HasForeignKey("LicenseKeyId");

                    b.Navigation("IdentifiersInfo");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.LicenseKeyFlag", b =>
                {
                    b.HasOne("Noctus.Domain.Entities.IdentifiersInfo", "IdentifiersInfo")
                        .WithMany()
                        .HasForeignKey("IdentifiersInfoId");

                    b.HasOne("Noctus.Domain.Entities.LicenseKey", null)
                        .WithMany("KeyFlags")
                        .HasForeignKey("LicenseKeyId");

                    b.Navigation("IdentifiersInfo");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.PipelineEvent", b =>
                {
                    b.HasOne("Noctus.Domain.Entities.IdentifiersInfo", "IdentifiersInfo")
                        .WithMany()
                        .HasForeignKey("IdentifiersInfoId");

                    b.HasOne("Noctus.Domain.Entities.PipelineRun", null)
                        .WithMany("Events")
                        .HasForeignKey("PipelineRunId");

                    b.Navigation("IdentifiersInfo");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.PipelineRun", b =>
                {
                    b.HasOne("Noctus.Domain.Entities.LicenseKey", null)
                        .WithMany("PipelineRuns")
                        .HasForeignKey("LicenseKeyId");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.LicenseKey", b =>
                {
                    b.Navigation("AccountsGenBuckets");

                    b.Navigation("KeyEvents");

                    b.Navigation("KeyFlags");

                    b.Navigation("PipelineRuns");
                });

            modelBuilder.Entity("Noctus.Domain.Entities.PipelineRun", b =>
                {
                    b.Navigation("Events");
                });
#pragma warning restore 612, 618
        }
    }
}
