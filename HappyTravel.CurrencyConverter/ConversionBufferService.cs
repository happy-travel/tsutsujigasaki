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
    }
}