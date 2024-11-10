﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NotionAutomation.Db;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NotionAutomation.Migrations
{
    [DbContext(typeof(NotionDbContext))]
    [Migration("20241110120357_ChangedOnOrBeforeToDateCondition")]
    partial class ChangedOnOrBeforeToDateCondition
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                    b.Property<int>("DateCondition")
                        .HasColumnType("integer");

                    b.Property<int>("DayOffset")
                        .HasColumnType("integer");

                    b.Property<string>("EndingState")
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
