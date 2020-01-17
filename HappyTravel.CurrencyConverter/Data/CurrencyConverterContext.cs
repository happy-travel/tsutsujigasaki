using Microsoft.EntityFrameworkCore;

namespace HappyTravel.CurrencyConverter.Data
{
    #nullable enable
    public class CurrencyConverterContext : DbContext
    {
        public CurrencyConverterContext(DbContextOptions<CurrencyConverterContext> options) : base(options) {}


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CurrencyRate>(rate =>
            {
                rate.HasKey(r => new {SourceCurrency = r.Source, TargetCurrency = r.Target, r.ValidFrom});
                rate.Property(r => r.Rate).IsRequired();
                rate.Property(r => r.Source).IsRequired();
                rate.Property(r => r.Target).IsRequired();
                rate.Property(r => r.ValidFrom).IsRequired();
            });
        }


        public virtual DbSet<CurrencyRate>? CurrencyRates { get; set; }
    }
    #nullable restore
}
