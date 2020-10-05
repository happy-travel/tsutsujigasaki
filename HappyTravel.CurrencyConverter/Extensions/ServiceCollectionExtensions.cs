using System.Collections.Generic;
using HappyTravel.CurrencyConverter.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.CurrencyConverter.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCurrencyConversionFactory(this IServiceCollection services, string optionsJson)
        {
            services.AddTransient<ICurrencyConverterFactory>(provider => new CurrencyConverterFactory(optionsJson));

            return services;
        }
        
        
        public static IServiceCollection AddCurrencyConversionFactory(this IServiceCollection services, IEnumerable<BufferPair> bufferPairs)
        {
            services.AddTransient<ICurrencyConverterFactory>(provider => new CurrencyConverterFactory(bufferPairs));

            return services;
        }
        
        
        public static IServiceCollection AddCurrencyConversionFactory(this IServiceCollection services, ConversionBufferOptions options)
        {
            services.AddTransient<ICurrencyConverterFactory>(provider => new CurrencyConverterFactory(options));

            return services;
        }
    }
}
