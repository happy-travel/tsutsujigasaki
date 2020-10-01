using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.CurrencyConverterApi.Migrations
{
    public partial class AddDefaultConfigurationsForRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"INSERT INTO ""DefaultCurrencyRates"" (""Source"", ""Target"", ""Rate"")
            VALUES(1,3,3.668), (3,1,0.2726)";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}