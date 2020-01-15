using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.CurrencyConverter.Infrastructure.Constants;
using HappyTravel.CurrencyConverter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static HappyTravel.CurrencyConverter.Infrastructure.Constants.Constants;

namespace HappyTravel.CurrencyConverter.Services
{
    public class RateService : IRateService
    {
        public RateService(IDistributedFlow cache, IHttpClientFactory clientFactory, IOptions<CurrencyLayerOptions> options)
        {
            _cache = cache;
            _clientFactory = clientFactory;
            _options = options.Value;
        }


        public async Task<Result<decimal, ProblemDetails>> Get(string fromCurrency, string toCurrency)
            => (await GetRates(fromCurrency))
                .Bind(rates => GetRate(rates, fromCurrency, toCurrency));


        private Result<decimal, ProblemDetails> GetRate(Dictionary<string, decimal> quotes, string fromCurrency, string toCurrency)
        {
            if (quotes is null || !quotes.Any())
                return ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(quotes)));

            var currencyPair = $"{fromCurrency}{toCurrency}".ToUpperInvariant();
            if (quotes.TryGetValue(currencyPair, out var quote))
                return Result.Ok<decimal, ProblemDetails>(quote);

            return ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.NoQuoteFound, currencyPair));
        }


        private Task<Result<Dictionary<string, decimal>, ProblemDetails>> GetRates(string fromCurrency)
            => _cache.GetOrSetAsync(_cache.BuildKey(nameof(RateService), nameof(GetRates), fromCurrency),
                async () => await RequestRates(fromCurrency), GetTimeSpanToNextHour());


        private static string GetSupportedCurrenciesString(string fromCurrency)
        {
            var currenciesToConvert = SupportedCurrencies
                .Where(c => c.Key != fromCurrency)
                .Select(c => c.Key);

            return string.Join(',', currenciesToConvert);
        }


        private static TimeSpan GetTimeSpanToNextHour()
        {
            var now = DateTime.Now;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0).TimeOfDay;
        }


        private async Task<Result<Dictionary<string, decimal>, ProblemDetails>> RequestRates(string fromCurrency)
        {
            var url = $"live?access_key={_options.ApiKey}&source={fromCurrency}&currencies={GetSupportedCurrenciesString(fromCurrency)}";
            try
            {
                using var client = _clientFactory.CreateClient(HttpClientNames.CurrencyLayer);
                using var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return ProblemDetailsBuilder.Fail<Dictionary<string, decimal>>(ErrorMessages.NetworkError, response.StatusCode);

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var streamReader = new StreamReader(stream);
                using var jsonTextReader = new JsonTextReader(streamReader);
                var serializer = new JsonSerializer();

                var result = serializer.Deserialize<CurrencyLayerResponse>(jsonTextReader);
                return result!.IsSuccessful 
                    ? Result.Ok<Dictionary<string, decimal>, ProblemDetails>(result.Quotes) 
                    : ProblemDetailsBuilder.Fail<Dictionary<string, decimal>>(result.Error.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private readonly IDistributedFlow _cache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly CurrencyLayerOptions _options;
    }
}