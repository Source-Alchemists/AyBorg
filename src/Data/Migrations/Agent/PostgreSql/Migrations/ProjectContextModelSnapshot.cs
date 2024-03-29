﻿// <auto-generated />
using System;
using AyBorg.Data.Agent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.PostgreSql.Migrations
{
    [DbContext(typeof(ProjectContext))]
    partial class ProjectContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AyBorg.Data.Agent.LinkRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ProjectRecordId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SourceId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TargetId")
                        .HasColumnType("uuid");

                    b.HasKey("DbId");

                    b.HasIndex("ProjectRecordId");

                    b.ToTable("AyBorgLinks");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.PluginMetaInfoRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AssemblyName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("AssemblyVersion")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("TypeName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("DbId");

                    b.ToTable("AyBorgStepPluginMetaInfo");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.ProjectMetaRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ApprovedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<Guid>("ProjectRecordId")
                        .HasColumnType("uuid");

                    b.Property<string>("ServiceUniqueName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("VersionIteration")
                        .HasColumnType("bigint");

                    b.Property<string>("VersionName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("DbId");

                    b.HasIndex("ProjectRecordId")
                        .IsUnique();

                    b.ToTable("AyBorgProjectMetas");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.ProjectRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("SettingsDbId")
                        .HasColumnType("uuid");

                    b.HasKey("DbId");

                    b.HasIndex("SettingsDbId");

                    b.ToTable("AyBorgProjects");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.ProjectSettingsRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("IsForceResultCommunicationEnabled")
                        .HasColumnType("boolean");

                    b.HasKey("DbId");

                    b.ToTable("AyBorgProjectSettings");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.StepPortRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Brand")
                        .HasColumnType("integer");

                    b.Property<int>("Direction")
                        .HasColumnType("integer");

                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<Guid>("StepRecordId")
                        .HasColumnType("uuid");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("DbId");

                    b.HasIndex("StepRecordId");

                    b.ToTable("AyBorgPorts");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.StepRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("MetaInfoDbId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ProjectRecordId")
                        .HasColumnType("uuid");

                    b.Property<int>("X")
                        .HasColumnType("integer");

                    b.Property<int>("Y")
                        .HasColumnType("integer");

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
