﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NotionAutomation.Db;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NotionAutomation.Migrations
{
    [DbContext(typeof(NotionDbContext))]
    partial class NotionDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0-rc.2.24474.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("NotionAutomation.Objects.NotionDatabaseRule", b =>
                {
                    b.Property<Guid>("RuleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DatabaseId")
                        .HasColumnType("uuid");

                    b.Property<int>("DayOffset")
                        .HasColumnType("integer");

                    b.Property<string>("EndingState")
                        .HasColumnType("text");

                    b.Property<string>("OnDay")
                        .HasColumnType("text");

                    b.Property<string>("StartingState")
                        .HasColumnType("text");

                    b.HasKey("RuleId");

                    b.ToTable("NotionDatabaseRules");
                });
#pragma warning restore 612, 618
        }
    }
}
