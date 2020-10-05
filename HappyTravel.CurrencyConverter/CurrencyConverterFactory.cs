using System;
using System.Collections.Generic;
using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.Money.Enums;

namespace HappyTravel.CurrencyConverter
{
    public class CurrencyConverterFactory : ICurrencyConverterFactory
    {
        public CurrencyConverterFactory(ConversionBufferOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }


        public CurrencyConverterFactory(IEnumerable<BufferPair> bufferPairs)
        {
            _options = ConversionBufferOptionsFactory.Create(bufferPairs) ??
                throw new ArgumentNullException(nameof(bufferPairs));
        }


        public CurrencyConverterFactory(string optionsJson)
        {
            _options = ConversionBufferOptionsFactory.CreateFromString(optionsJson) ??
                throw new ArgumentNullException(nameof(optionsJson));
        }


        public CurrencyConverter Create(in decimal rate, Currencies sourceCurrency, Currencies targetCurrency)
        {
            CurrencyConverter.CheckPreconditions(in rate, sourceCurrency, targetCurrency);
            var bufferService = new ConversionBufferService(_options);

            return new CurrencyConverter(bufferService, in rate, sourceCurrency, targetCurrency);
        }
    
        
        private readonly ConversionBufferOptions _options;
    }
}
