using System;
using System.Collections.Generic;
using HappyTravel.CurrencyConverter;
using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.Money.Enums;

namespace HappyTravel.CurrencyConverterApiTests
{
    public class ConversionServiceTestsFixture : IDisposable
    {
        public ConversionServiceTestsFixture()
        {
            Factory = new CurrencyConverterFactory(Pairs);
        }


        public void Dispose()
        { }


        public ICurrencyConverterFactory Factory { get; }


        private static readonly List<BufferPair> Pairs = new List<BufferPair>
        {
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.AED,
                TargetCurrency = Currencies.AED
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.AED,
                TargetCurrency = Currencies.EUR
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.AED,
                TargetCurrency = Currencies.SAR
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.AED,
                TargetCurrency = Currencies.USD
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.EUR,
                TargetCurrency = Currencies.AED
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.EUR,
                TargetCurrency = Currencies.EUR
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.EUR,
                TargetCurrency = Currencies.SAR
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.EUR,
                TargetCurrency = Currencies.USD
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.SAR,
                TargetCurrency = Currencies.AED
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.SAR,
                TargetCurrency = Currencies.EUR
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.SAR,
                TargetCurrency = Currencies.SAR
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.SAR,
                TargetCurrency = Currencies.USD
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.USD,
                TargetCurrency = Currencies.AED
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.USD,
                TargetCurrency = Currencies.EUR
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.USD,
                TargetCurrency = Currencies.SAR
            },
            new BufferPair
            {
                BufferValue = decimal.Zero,
                SourceCurrency = Currencies.USD,
                TargetCurrency = Currencies.USD
            }
        };
    }
}
