using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Tsutsujigasaki.Api.Migrations
{
    public partial class RemoveRateCorrections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RateCorrection",
                table: "CurrencyRates");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RateCorrection",
                table: "CurrencyRates",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
