﻿// <auto-generated />
using System;
using HappyTravel.Tsutsujigasaki.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Tsutsujigasaki.Api.Migrations
{
    [DbContext(typeof(CurrencyConverterContext))]
    [Migration("20210501092915_RemoveRateCorrections")]
    partial class RemoveRateCorrections
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("HappyTravel.Tsutsujigasaki.Api.Data.CurrencyRate", b =>
                {
                    b.Property<string>("Source")
                        .HasColumnType("text");

                    b.Property<string>("Target")
                        .HasColumnType("text");

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("timestamp");

                    b.Property<decimal>("Rate")
                        .HasColumnType("numeric");

                    b.HasKey("Source", "Target", "ValidFrom");

                    b.ToTable("CurrencyRates");
                });

            modelBuilder.Entity("HappyTravel.Tsutsujigasaki.Api.Data.DefaultCurrencyRate", b =>
                {
                    b.Property<string>("Source")
                        .HasColumnType("text");

                    b.Property<string>("Target")
                        .HasColumnType("text");

                    b.Property<DateTime>("ValidFrom")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp")
                        .HasDefaultValueSql("current_timestamp(0)");

                    b.Property<decimal>("Rate")
                        .HasColumnType("numeric");

                    b.HasKey("Source", "Target", "ValidFrom");

                    b.ToTable("DefaultCurrencyRates");
                });
#pragma warning restore 612, 618
        }
    }
}
