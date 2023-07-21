﻿// <auto-generated />
using System;
using AyBorg.Data.Agent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.SqlLite.Migrations.Device
{
    [DbContext(typeof(DeviceContext))]
    [Migration("20230719104954_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.0");

            modelBuilder.Entity("AyBorg.Data.Agent.DevicePortRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("Brand")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("DeviceRecordId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Direction")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("DbId");

                    b.HasIndex("DeviceRecordId");

                    b.ToTable("AyBorgDevicePorts");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.DeviceRecord", b =>
                {
                    b.Property<Guid>("DbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("MetaInfoDbId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ProviderMetaInfoDbId")
                        .HasColumnType("TEXT");

                    b.HasKey("DbId");

                    b.HasIndex("MetaInfoDbId");

                    b.HasIndex("ProviderMetaInfoDbId");

                    b.ToTable("AyBorgDevices");
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

                    b.ToTable("AyBorgDevicePluginMetaInfo");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.DevicePortRecord", b =>
                {
                    b.HasOne("AyBorg.Data.Agent.DeviceRecord", "DeviceRecord")
                        .WithMany("Ports")
                        .HasForeignKey("DeviceRecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DeviceRecord");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.DeviceRecord", b =>
                {
                    b.HasOne("AyBorg.Data.Agent.PluginMetaInfoRecord", "MetaInfo")
                        .WithMany()
                        .HasForeignKey("MetaInfoDbId");

                    b.HasOne("AyBorg.Data.Agent.PluginMetaInfoRecord", "ProviderMetaInfo")
                        .WithMany()
                        .HasForeignKey("ProviderMetaInfoDbId");

                    b.Navigation("MetaInfo");

                    b.Navigation("ProviderMetaInfo");
                });

            modelBuilder.Entity("AyBorg.Data.Agent.DeviceRecord", b =>
                {
                    b.Navigation("Ports");
                });
#pragma warning restore 612, 618
        }
    }
}