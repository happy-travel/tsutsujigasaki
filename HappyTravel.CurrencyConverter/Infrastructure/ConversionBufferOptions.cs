using System.Collections.Generic;
using HappyTravel.Money.Enums;

namespace HappyTravel.CurrencyConverter.Infrastructure
{
    public class ConversionBufferOptions
    {
        internal ConversionBufferOptions(Dictionary<(Currencies, Currencies), decimal> exceptionalPairs, decimal? defaultBuffer = null)
        {
            ExceptionalPairs = exceptionalPairs;
            if (defaultBuffer.HasValue)
                DefaultBuffer = defaultBuffer.Value;
        }


        public decimal DefaultBuffer { get; } = 0.005m;
        public Dictionary<(Currencies, Currencies), decimal> ExceptionalPairs { get; }
    }
}
