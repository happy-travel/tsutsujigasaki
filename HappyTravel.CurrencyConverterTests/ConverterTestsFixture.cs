using System;
using System.Collections.Generic;
using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.Money.Enums;

namespace HappyTravel.CurrencyConverterTests
{
    public class ConverterTestsFixture : IDisposable
    {
        public ConverterTestsFixture()
        {
            ExceptionalPairs = new List<BufferPair>
            {
                new BufferPair
                {
                    BufferValue = decimal.Zero,
                    SourceCurrency = Currencies.USD,
                    TargetCurrency = Currencies.EUR
                },
                new BufferPair
                {
                    BufferValue = decimal.Zero,
                    SourceCurrency = Currencies.EUR,
                    TargetCurrency = Currencies.USD
                }
            };
        }


        public void Dispose()
        { }


        public List<BufferPair> ExceptionalPairs { get; }
    }
}
