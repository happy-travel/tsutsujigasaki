﻿// <auto-generated />
using System;
using HappyTravel.CurrencyConverter.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.CurrencyConverter.Migrations
{
    [DbContext(typeof(CurrencyConverterContext))]
    [Migration("20200924140542_AddDefaultConfigurationsForRate")]
    partial class AddDefaultConfigurationsForRate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("HappyTravel.CurrencyConverter.Data.CurrencyRate", b =>
                {
                    b.Property<string>("Source")
                        .HasColumnType("text");

                    b.Property<string>("Target")
                        .HasColumnType("text");

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("timestamp");

                    b.Property<decimal>("Rate")
                        .HasColumnType("numeric");

                    b.Property<decimal>("RateCorrection")
                        .HasColumnType("numeric");

                    b.HasKey("Source", "Target", "ValidFrom");

                    b.ToTable("CurrencyRates");
                });

            modelBuilder.Entity("HappyTravel.CurrencyConverter.Data.DefaultCurrencyRate", b =>
                {
                    b.Property<int>("Source")
                        .HasColumnType("integer");

                    b.Property<int>("Target")
                        .HasColumnType("integer");

                    b.Property<decimal>("Rate")
                        .HasColumnType("numeric");

                    b.HasKey("Source", "Target");

                    b.ToTable("DefaultCurrencyRates");
                });
#pragma warning restore 612, 618
        }
    }
}
