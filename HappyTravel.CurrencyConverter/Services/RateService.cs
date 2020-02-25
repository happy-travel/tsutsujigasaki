using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.CurrencyConverter.Data;
using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.CurrencyConverter.Infrastructure.Constants;
using HappyTravel.CurrencyConverter.Infrastructure.Logging;
using HappyTravel.CurrencyConverter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static HappyTravel.CurrencyConverter.Infrastructure.Constants.Constants;

namespace HappyTravel.CurrencyConverter.Services
{
    public class RateService : IRateService
    {
        public RateService(ILoggerFactory loggerFactory, IDoubleFlow cache, IHttpClientFactory clientFactory, IOptions<CurrencyLayerOptions> options, CurrencyConverterContext context)
        {
            _cache = cache;
            _clientFactory = clientFactory;
            _context = context;
            _logger = loggerFactory.CreateLogger<RateService>();
            _options = options.Value;
        }


        public async ValueTask<Result<decimal, ProblemDetails>> Get(string sourceCurrency, string targetCurrency)
        {
            if (string.IsNullOrWhiteSpace(sourceCurrency))
                return ProblemDetailsBuilder.FailAndLogArgumentNullOrEmpty<decimal>(_logger, nameof(sourceCurrency));

            if (string.IsNullOrWhiteSpace(targetCurrency))
                return ProblemDetailsBuilder.FailAndLogArgumentNullOrEmpty<decimal>(_logger, nameof(targetCurrency));

            if (sourceCurrency.Equals(targetCurrency, StringComparison.InvariantCultureIgnoreCase))
                return Result.Ok<decimal, ProblemDetails>(1);

            return await _cache.GetOrSetAsync(_cache.BuildKey(nameof(RateService), nameof(Get), sourceCurrency, targetCurrency), async () => await GetRates(sourceCurrency)
                    .Bind(SplitCurrencyPair)
                    .Bind(SetRates)
                    .Bind(rates => GetRate(rates, sourceCurrency, targetCurrency)),
                GetTimeSpanToNextHour());
        }


        private async Task<Result<decimal, ProblemDetails>> GetRate(Dictionary<(string, string), decimal> rates, string sourceCurrency, string targetCurrency)
        {
            if (rates.TryGetValue((sourceCurrency, targetCurrency), out var rate))
                return Result.Ok<decimal, ProblemDetails>(rate);

            var today = DateTime.Today;
            var storedRate = await _context.CurrencyRates
                .Where(r => r.Source == sourceCurrency && r.Target == targetCurrency)
                .Where(r => today <= r.ValidFrom)
                .OrderByDescending(r => r.ValidFrom)
                .Select(r => r.Rate)
                .FirstOrDefaultAsync();

            return storedRate.Equals(default) 
                ? ProblemDetailsBuilder.FailAndLogNoQuoteFound<decimal>(_logger, sourceCurrency + targetCurrency) 
                : Result.Ok<decimal, ProblemDetails>(storedRate);
        }


        private async Task<Result<Dictionary<string, decimal>, ProblemDetails>> GetRates(string sourceCurrency)
        {
            var url = $"live?access_key={_options.ApiKey}&source={sourceCurrency}&currencies={GetSupportedCurrenciesString(sourceCurrency)}";
            try
            {
                using var client = _clientFactory.CreateClient(HttpClientNames.CurrencyLayer);
                using var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return ProblemDetailsBuilder.FailAndLogNetworkException<Dictionary<string, decimal>>(_logger, response.StatusCode, response.ReasonPhrase);

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var streamReader = new StreamReader(stream);
                using var jsonTextReader = new JsonTextReader(streamReader);
                var serializer = new JsonSerializer();

                var result = serializer.Deserialize<CurrencyLayerResponse>(jsonTextReader);
                return result.IsSuccessful 
                    ? Result.Ok<Dictionary<string, decimal>, ProblemDetails>(result.Quotes) 
                    : ProblemDetailsBuilder.Fail<Dictionary<string, decimal>>("Rate Service Exception", result.Error.Message);
            }
            catch (Exception ex)
            {
                _logger.LogRateServiceException(ex);
                throw;
            }
        }


        private static string GetSupportedCurrenciesString(string sourceCurrency)
        {
            var currenciesToConvert = SupportedCurrencies
                .Where(c => c.Key != sourceCurrency)
                .Select(c => c.Key);

            return string.Join(',', currenciesToConvert);
        }


        private static TimeSpan GetTimeSpanToNextHour()
        {
            var now = DateTime.UtcNow;
            var nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);

            return nextHour - now;
        }


        private async Task<Result<Dictionary<(string, string), decimal>, ProblemDetails>> SetRates(Dictionary<(string, string), decimal> rates)
        {
            var now = DateTime.UtcNow;

            var ratesToStore = new List<CurrencyRate>(rates.Count);
            foreach (var ((source, target), rate) in rates)
                ratesToStore.Add(new CurrencyRate
                {
                    Rate = rate,
                    Source = source,
                    Target = target,
                    ValidFrom = now
                });

            _context.CurrencyRates.AddRange(ratesToStore);
            await _context.SaveChangesAsync();

            return Result.Ok<Dictionary<(string, string), decimal>, ProblemDetails>(rates);
        }


        private Result<Dictionary<(string, string), decimal>, ProblemDetails> SplitCurrencyPair(Dictionary<string, decimal> rates)
        {
            if (rates is null || !rates.Any())
                return ProblemDetailsBuilder.FailAndLogArgumentNullOrEmpty<Dictionary<(string, string), decimal>>(_logger, nameof(rates));

            var results = new Dictionary<(string, string), decimal>(rates.Count);
            foreach (var (token, value) in rates)
            {
                var source = token.Substring(0, SymbolLength);
                var target = token.Substring(3, SymbolLength);

                results.Add((source, target), value);
            }

            return Result.Ok<Dictionary<(string, string), decimal>, ProblemDetails>(results);
        }


        private const int SymbolLength = 3;

        private readonly IDoubleFlow _cache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly CurrencyConverterContext _context;
        private readonly CurrencyLayerOptions _options;
        private readonly ILogger<RateService> _logger;
    }
}