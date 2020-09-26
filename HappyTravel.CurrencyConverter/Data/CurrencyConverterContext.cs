using System;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.CurrencyConverter.Data
{
#nullable enable
    public class CurrencyConverterContext : DbContext
    {
        public CurrencyConverterContext(DbContextOptions<CurrencyConverterContext> options) : base(options) {}

        
        [Obsolete("This constructor is for testing purposes only. Comment to crate a migration.")]
        protected CurrencyConverterContext() { }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CurrencyRate>(rate =>
            {
                rate.HasKey(r => new {SourceCurrency = r.Source, TargetCurrency = r.Target, r.ValidFrom});
                rate.Property(r => r.Rate)
                    .HasColumnType("numeric")
                    .IsRequired();
                rate.Property(r => r.RateCorrection)
                    .HasColumnType("numeric")
                    .IsRequired();
                rate.Property(r => r.Source).IsRequired();
                rate.Property(r => r.Target).IsRequired();
                rate.Property(r => r.ValidFrom)
                    .HasColumnType("timestamp")
                    .IsRequired();
            });

            builder.Entity<DefaultCurrencyRate>(rate =>
            {
                rate.HasKey(r => new {SourceCurrency = r.Source, TargetCurrency = r.Target});
                rate.Property(r => r.Rate)
                    .HasColumnType("numeric")
                    .IsRequired();
                rate.Property(r => r.Source).IsRequired();
                rate.Property(r => r.Target).IsRequired();
            });
        }


        public virtual DbSet<DefaultCurrencyRate> DefaultCurrencyRates { get; set; } = null!;
        public virtual DbSet<CurrencyRate> CurrencyRates { get; set; } = null!;
    }
#nullable restore
}