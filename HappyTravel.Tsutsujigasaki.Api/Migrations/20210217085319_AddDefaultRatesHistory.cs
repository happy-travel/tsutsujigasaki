using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Tsutsujigasaki.Api.Migrations
{
    public partial class AddDefaultRatesHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DefaultCurrencyRates",
                table: "DefaultCurrencyRates");

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "DefaultCurrencyRates",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "current_timestamp(0)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DefaultCurrencyRates",
                table: "DefaultCurrencyRates",
                columns: new[] { "Source", "Target", "ValidFrom" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DefaultCurrencyRates",
                table: "DefaultCurrencyRates");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "DefaultCurrencyRates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DefaultCurrencyRates",
                table: "DefaultCurrencyRates",
                columns: new[] { "Source", "Target" });
        }
    }
}
