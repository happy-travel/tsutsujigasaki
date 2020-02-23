using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.CurrencyConverter.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrencyRates",
                columns: table => new
                {
                    Source = table.Column<string>(nullable: false),
                    Target = table.Column<string>(nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyRates", x => new { x.Source, x.Target, x.ValidFrom });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyRates");
        }
    }
}
