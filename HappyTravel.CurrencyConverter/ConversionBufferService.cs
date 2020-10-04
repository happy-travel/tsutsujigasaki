using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.Money.Enums;

namespace HappyTravel.CurrencyConverter
{
    internal class ConversionBufferService
    {
        public ConversionBufferService(ConversionBufferOptions options) 
            => _options = options;


        public decimal GetBuffer(Currencies sourceCurrency, Currencies targetCurrency)
            => _options.ExceptionalPairs.TryGetValue((sourceCurrency, targetCurrency), out var buffer)
                ? buffer
                : _options.DefaultBuffer;


        private readonly ConversionBufferOptions _options;
        /*private static readonly Dictionary<(Currencies, Currencies), decimal> ExceptionalPairs = new Dictionary<(Currencies, Currencies), decimal>
        {
            {(Currencies.AED, Currencies.USD), decimal.Zero},
            {(Currencies.USD, Currencies.AED), decimal.Zero}
        };

        private const decimal DefaultBuffer = 0.005m;*/
    }
}