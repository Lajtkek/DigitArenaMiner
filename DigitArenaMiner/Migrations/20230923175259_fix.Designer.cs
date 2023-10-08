﻿// <auto-generated />
using DigitArenaBot;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DigitArenaMiner.Migrations
{
    [DbContext(typeof(DefaultDatabaseContext))]
    [Migration("20230923175259_fix")]
    partial class fix
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DigitArenaBot.Models.ArchivedMessages", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("ArchivedMessages");
                });

            modelBuilder.Entity("DigitArenaBot.Models.MessageReactionCount", b =>
                {
                    b.Property<decimal>("IdMessage")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("EmoteIdentifier")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<int>("Count")
                        .HasColumnType("integer");

                    b.Property<decimal>("IdSender")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("IdMessage", "EmoteIdentifier");

                    b.HasIndex("IdSender", "EmoteIdentifier");

                    b.ToTable("MessageReactionCounts");
                });
#pragma warning restore 612, 618
        }
    }
}
