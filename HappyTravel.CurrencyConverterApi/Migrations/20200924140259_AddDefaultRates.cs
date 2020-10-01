using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.CurrencyConverterApi.Migrations
{
    public partial class AddDefaultRates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RateCorrection",
                table: "CurrencyRates",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "DefaultCurrencyRates",
                columns: table => new
                {
                    Source = table.Column<int>(nullable: false),
                    Target = table.Column<int>(nullable: false),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultCurrencyRates", x => new { x.Source, x.Target });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefaultCurrencyRates");

            migrationBuilder.DropColumn(
                name: "RateCorrection",
                table: "CurrencyRates");
        }
    }
}
