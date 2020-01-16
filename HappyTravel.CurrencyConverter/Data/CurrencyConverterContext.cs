using Microsoft.EntityFrameworkCore;

namespace HappyTravel.CurrencyConverter.Data
{
    public class CurrencyConverterContext : DbContext
    {
        public CurrencyConverterContext(DbContextOptions<CurrencyConverterContext> options) : base(options) {}
    }
}
