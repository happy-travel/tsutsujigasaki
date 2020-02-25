using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.CurrencyConverter.Migrations
{
    public partial class HashIndexOnCurrencyRates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRates_Source_Target",
                table: "CurrencyRates",
                columns: new[] { "Source", "Target" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CurrencyRates_Source_Target",
                table: "CurrencyRates");
        }
    }
}
