﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Server.EFcore.Contexts;

#nullable disable

namespace Server.Migrations
{
    [DbContext(typeof(GameDbContext))]
    [Migration("20231214124019_AddNickname")]
    partial class AddNickname
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Server.EFcore.Models.CharacterData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BaseCharacterCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EXP")
                        .HasColumnType("int");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("Server.EFcore.Models.CompletedAchievement", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AchievementCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("GameWorldId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("GameWorldId");

                    b.HasIndex("AchievementCode", "GameWorldId")
                        .IsUnique();

                    b.ToTable("CompletedAchievement");
                });

            modelBuilder.Entity("Server.EFcore.Models.GameAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccountLevel")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LoginId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginPassword")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PlayerName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UID")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("LoginId")
                        .IsUnique();

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Server.EFcore.Models.GameWorld", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("GameWorlds");
                });

            modelBuilder.Entity("Server.EFcore.Models.Item", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BaseItemCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("GameAccountId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("GameAccountId");

                    b.HasIndex("OwnerId");

                    b.ToTable("Items");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Item");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Server.EFcore.Models.EquipItem", b =>
                {
                    b.HasBaseType("Server.EFcore.Models.Item");

                    b.Property<int>("EXP")
                        .HasColumnType("int");

                    b.HasDiscriminator().HasValue("EquipItem");
                });

            modelBuilder.Entity("Server.EFcore.Models.CharacterData", b =>
                {
                    b.HasOne("Server.EFcore.Models.GameAccount", "Owner")
                        .WithMany("CharacterDatas")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Server.EFcore.Models.CompletedAchievement", b =>
                {
                    b.HasOne("Server.EFcore.Models.GameWorld", "GameWorld")
                        .WithMany("Achievements")
                        .HasForeignKey("GameWorldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GameWorld");
                });

            modelBuilder.Entity("Server.EFcore.Models.GameWorld", b =>
                {
                    b.HasOne("Server.EFcore.Models.GameAccount", "Owner")
                        .WithOne("GameWorld")
                        .HasForeignKey("Server.EFcore.Models.GameWorld", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Server.EFcore.Models.Item", b =>
                {
                    b.HasOne("Server.EFcore.Models.GameAccount", null)
                        .WithMany("Inventory")
                        .HasForeignKey("GameAccountId");

                    b.HasOne("Server.EFcore.Models.GameAccount", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Server.EFcore.Models.GameAccount", b =>
                {
                    b.Navigation("CharacterDatas");

                    b.Navigation("GameWorld")
                        .IsRequired();

                    b.Navigation("Inventory");
                });

            modelBuilder.Entity("Server.EFcore.Models.GameWorld", b =>
                {
                    b.Navigation("Achievements");
                });
#pragma warning restore 612, 618
        }
    }
}
