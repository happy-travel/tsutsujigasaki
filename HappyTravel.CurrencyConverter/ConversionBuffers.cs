using System.Collections.Generic;
using HappyTravel.Money.Enums;

namespace HappyTravel.CurrencyConverter
{
    internal static class ConversionBuffers
    {
        public static decimal GetBuffer(Currencies sourceCurrency, Currencies targetCurrency)
            => ExceptionalPairs.TryGetValue((sourceCurrency, targetCurrency), out var buffer)
                ? buffer
                : DefaultBuffer;


        private static readonly Dictionary<(Currencies, Currencies), decimal> ExceptionalPairs = new Dictionary<(Currencies, Currencies), decimal>
        {
            {(Currencies.AED, Currencies.USD), decimal.Zero},
            {(Currencies.USD, Currencies.AED), decimal.Zero}
        };

        private const decimal DefaultBuffer = 0.005m;
    }
}