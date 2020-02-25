﻿// <auto-generated />
using System;
using HappyTravel.CurrencyConverter.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.CurrencyConverter.Migrations
{
    [DbContext(typeof(CurrencyConverterContext))]
    partial class CurrencyConverterContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.2")
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

                    b.HasKey("Source", "Target", "ValidFrom");

                    b.HasIndex("Source", "Target")
                        .HasAnnotation("Npgsql:IndexMethod", "hash");

                    b.ToTable("CurrencyRates");
                });
#pragma warning restore 612, 618
        }
    }
}
