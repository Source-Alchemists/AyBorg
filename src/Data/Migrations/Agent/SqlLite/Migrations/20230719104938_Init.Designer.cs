﻿// <auto-generated />
using System;
using AyBorg.Data.Agent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.SqlLite.Migrations
{
    [DbContext(typeof(ProjectContext))]
    [Migration("20230719104938_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.0");

            modelBuilder.Entity("AyBorg.Data.Agent.LinkRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ProjectRecordId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SourceId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TargetId")
                        .HasColumnType("TEXT");

                    b.HasKey("DbId");

                    b.HasIndex("ProjectRecordId");

                    b.ToTable("AyBorgLinks");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.PluginMetaInfoRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AssemblyName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AssemblyVersion")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("TypeName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("DbId");

                    b.ToTable("AyBorgStepPluginMetaInfo");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.ProjectMetaRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ApprovedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ProjectRecordId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ServiceUniqueName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("State")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("TEXT");

                    b.Property<long>("VersionIteration")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VersionName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("DbId");

                    b.HasIndex("ProjectRecordId")
                        .IsUnique();

                    b.ToTable("AyBorgProjectMetas");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.ProjectRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SettingsDbId")
                        .HasColumnType("TEXT");

                    b.HasKey("DbId");

                    b.HasIndex("SettingsDbId");

                    b.ToTable("AyBorgProjects");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.ProjectSettingsRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsForceResultCommunicationEnabled")
                        .HasColumnType("INTEGER");

                    b.HasKey("DbId");

                    b.ToTable("AyBorgProjectSettings");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.StepPortRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("Brand")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Direction")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("StepRecordId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("DbId");

                    b.HasIndex("StepRecordId");

                    b.ToTable("AyBorgPorts");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.StepRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("MetaInfoDbId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ProjectRecordId")
                        .HasColumnType("TEXT");

                    b.Property<int>("X")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Y")
                        .HasColumnType("INTEGER");

                    b.HasKey("DbId");

                    b.HasIndex("MetaInfoDbId");

                    b.HasIndex("ProjectRecordId");

                    b.ToTable("AyBorgSteps");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.LinkRecord", b =>
                {
                    b.HasOne("AyBorg.Data.Agent.ProjectRecord", "ProjectRecord")
                        .WithMany("Links")
                        .HasForeignKey("ProjectRecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProjectRecord");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.ProjectMetaRecord", b =>
                {
                    b.HasOne("AyBorg.Data.Agent.ProjectRecord", null)
                        .WithOne("Meta")
                        .HasForeignKey("AyBorg.Data.Agent.ProjectMetaRecord", "ProjectRecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AyBorg.Data.Agent.ProjectRecord", b =>
                {
                    b.HasOne("AyBorg.Data.Agent.ProjectSettingsRecord", "Settings")
                        .WithMany()
                        .HasForeignKey("SettingsDbId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Settings");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.StepPortRecord", b =>
                {
                    b.HasOne("AyBorg.Data.Agent.StepRecord", "StepRecord")
                        .WithMany("Ports")
                        .HasForeignKey("StepRecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("StepRecord");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.StepRecord", b =>
                {
                    b.HasOne("AyBorg.Data.Agent.PluginMetaInfoRecord", "MetaInfo")
                        .WithMany()
                        .HasForeignKey("MetaInfoDbId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AyBorg.Data.Agent.ProjectRecord", "ProjectRecord")
                        .WithMany("Steps")
                        .HasForeignKey("ProjectRecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MetaInfo");

                    b.Navigation("ProjectRecord");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.ProjectRecord", b =>
                {
                    b.Navigation("Links");

                    b.Navigation("Meta")
                        .IsRequired();

                    b.Navigation("Steps");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.StepRecord", b =>
                {
                    b.Navigation("Ports");
                });
#pragma warning restore 612, 618
        }
    }
}
