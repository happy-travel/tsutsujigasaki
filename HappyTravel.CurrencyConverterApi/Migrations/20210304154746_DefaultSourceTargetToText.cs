using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Tsutsujigasaki.Api.Migrations
{
    public partial class DefaultSourceTargetToText : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Target",
                table: "DefaultCurrencyRates",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "DefaultCurrencyRates",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql("UPDATE \"DefaultCurrencyRates\" SET \"Source\" = 'NotSpecified' WHERE \"Source\" = '0';");
            migrationBuilder.Sql("UPDATE \"DefaultCurrencyRates\" SET \"Source\" = 'USD' WHERE \"Source\" = '1';");
            migrationBuilder.Sql("UPDATE \"DefaultCurrencyRates\" SET \"Source\" = 'EUR' WHERE \"Source\" = '2';");
            migrationBuilder.Sql("UPDATE \"DefaultCurrencyRates\" SET \"Source\" = 'AED' WHERE \"Source\" = '3';");
            migrationBuilder.Sql("UPDATE \"DefaultCurrencyRates\" SET \"Source\" = 'SAR' WHERE \"Source\" = '4';");

            migrationBuilder.Sql("UPDATE \"DefaultCurrencyRates\" SET \"Target\" = 'NotSpecified' WHERE \"Target\" = '0';");
            migrationBuilder.Sql("UPDATE \"DefaultCurrencyRates\" SET \"Target\" = 'USD' WHERE \"Target\" = '1';");
            migrationBuilder.Sql("UPDATE \"DefaultCurrencyRates\" SET \"Target\" = 'EUR' WHERE \"Target\" = '2';");
            migrationBuilder.Sql("UPDATE \"DefaultCurrencyRates\" SET \"Target\" = 'AED' WHERE \"Target\" = '3';");
            migrationBuilder.Sql("UPDATE \"DefaultCurrencyRates\" SET \"Target\" = 'SAR' WHERE \"Target\" = '4';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Target",
                table: "DefaultCurrencyRates",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Source",
                table: "DefaultCurrencyRates",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
