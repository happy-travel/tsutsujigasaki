using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Tsutsujigasaki.Api.Data;
using HappyTravel.Tsutsujigasaki.Api.Infrastructure;
using HappyTravel.Tsutsujigasaki.Api.Infrastructure.Constants;
using HappyTravel.Tsutsujigasaki.Api.Infrastructure.Logging;
using HappyTravel.Tsutsujigasaki.Api.Models;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HappyTravel.Tsutsujigasaki.Api.Services
{
    public class RateService : IRateService
    {
        public RateService(ILoggerFactory loggerFactory, IDoubleFlow cache, IHttpClientFactory clientFactory,
            IOptions<CurrencyLayerOptions> options, CurrencyConverterContext context)
        {
            _cache = cache;
            _clientFactory = clientFactory;
            _context = context;
            _logger = loggerFactory.CreateLogger<RateService>();
            _options = options.Value;
        }


        public async ValueTask<Result<decimal, ProblemDetails>> Get(Currencies sourceCurrency,
            Currencies targetCurrency)
        {
            if (sourceCurrency == Currencies.NotSpecified)
                return ProblemDetailsBuilder.FailAndLogArgumentNullOrEmpty<decimal>(_logger, nameof(sourceCurrency));

            if (targetCurrency == Currencies.NotSpecified)
                return ProblemDetailsBuilder.FailAndLogArgumentNullOrEmpty<decimal>(_logger, nameof(targetCurrency));

            if (sourceCurrency == targetCurrency)
                return Result.Success<decimal, ProblemDetails>(1);

            var cacheKey = _cache.BuildKey(nameof(RateService), nameof(Get), sourceCurrency.ToString(),
                targetCurrency.ToString());
            if (_cache.TryGetValue(cacheKey, out Result<decimal, ProblemDetails> result, GetTimeSpanToNextHour()))
                return result;

            result = await GetRates(sourceCurrency)
                .Bind(SplitCurrencyPair)
                .Bind(SetRates)
                .Bind(rates => GetRate(rates, sourceCurrency, targetCurrency));

            if (result.IsSuccess)
                await _cache.SetAsync(cacheKey, result, GetTimeSpanToNextHour());

            return result;
        }


        private async Task<Result<decimal, ProblemDetails>> GetRate(Dictionary<(string, string), decimal> rates,
            Currencies sourceCurrency, Currencies targetCurrency)
        {
            var defaultRate = await GetDefaultRate(sourceCurrency.ToString(), targetCurrency.ToString());
            if (defaultRate.HasValue)
                return defaultRate.Value;

            if (rates.TryGetValue((sourceCurrency.ToString(), targetCurrency.ToString()), out var rate))
                return Result.Success<decimal, ProblemDetails>(rate);

            var today = DateTime.Today;
            var storedRate = await _context.CurrencyRates
                .Where(r => r.Source == sourceCurrency.ToString() && r.Target == targetCurrency.ToString())
                .Where(r => today <= r.ValidFrom)
                .OrderByDescending(r => r.ValidFrom)
                .Select(r => r.Rate)
                .FirstOrDefaultAsync();

            return storedRate.Equals(default)
                ? ProblemDetailsBuilder.FailAndLogNoQuoteFound<decimal>(_logger,
                    sourceCurrency.ToString() + targetCurrency)
                : Result.Success<decimal, ProblemDetails>(storedRate);
        }


        private async Task<Result<Dictionary<string, decimal>, ProblemDetails>> GetRates(Currencies sourceCurrency)
        {
            var url = $"live?access_key={_options.ApiKey}&source={sourceCurrency}&currencies={GetSupportedCurrenciesString(sourceCurrency)}";
            try
            {
                using var client = _clientFactory.CreateClient(HttpClientNames.CurrencyLayer);
                using var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return ProblemDetailsBuilder.FailAndLogNetworkException<Dictionary<string, decimal>>(_logger,
                        response.StatusCode, response.ReasonPhrase);

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var streamReader = new StreamReader(stream);
                using var jsonTextReader = new JsonTextReader(streamReader);
                var serializer = new JsonSerializer();

                var result = serializer.Deserialize<CurrencyLayerResponse>(jsonTextReader);
                return result.IsSuccessful
                    ? Result.Success<Dictionary<string, decimal>, ProblemDetails>(result.Quotes)
                    : ProblemDetailsBuilder.Fail<Dictionary<string, decimal>>("Rate Service Exception",
                        result.Error.Message);
            }
            catch (Exception ex)
            {
                _logger.LogRateServiceException(ex);
                throw;
            }
        }


        private static string GetSupportedCurrenciesString(Currencies sourceCurrency)
        {
            var sourceCurrencyName = sourceCurrency.ToString();
            var currenciesToConvert = Enum.GetNames(typeof(Currencies))
                .Where(currency => currency != sourceCurrencyName);

            return string.Join(',', currenciesToConvert);
        }


        private static TimeSpan GetTimeSpanToNextHour()
        {
            var now = DateTime.UtcNow;
            var nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);

            return nextHour - now;
        }


        private async Task<Result<Dictionary<(string, string), decimal>, ProblemDetails>> SetRates(
            Dictionary<(string, string), decimal> rates)
        {
            var now = DateTime.UtcNow;

            var ratesToStore = new List<CurrencyRate>(rates.Count);
            foreach (var ((source, target), rate) in rates)
            {
                var defaultRate = await GetDefaultRate(source, target);

                ratesToStore.Add(new CurrencyRate
                {
                    Rate = defaultRate ?? rate,
                    RateCorrection = rate - defaultRate ?? 0,
                    Source = source,
                    Target = target,
                    ValidFrom = now
                });
            }

            _context.CurrencyRates.AddRange(ratesToStore);
            await _context.SaveChangesAsync();

            return Result.Success<Dictionary<(string, string), decimal>, ProblemDetails>(rates);
        }


        private Result<Dictionary<(string, string), decimal>, ProblemDetails> SplitCurrencyPair(
            Dictionary<string, decimal> rates)
        {
            if (rates is null || !rates.Any())
                return ProblemDetailsBuilder.FailAndLogArgumentNullOrEmpty<Dictionary<(string, string), decimal>>(
                    _logger, nameof(rates));

            var results = new Dictionary<(string, string), decimal>(rates.Count);
            foreach (var (token, value) in rates)
            {
                var source = token.Substring(0, SymbolLength);
                var target = token.Substring(3, SymbolLength);

                results.Add((source, target), value);
            }

            return Result.Success<Dictionary<(string, string), decimal>, ProblemDetails>(results);
        }

        private async ValueTask<decimal?> GetDefaultRate(string source, string target)
        {
            if (_defaultRates == null || !_defaultRates.Any())
                _defaultRates = await _context.DefaultCurrencyRates.ToDictionaryAsync(
                    r => (r.Source.ToString(), r.Target.ToString()), r => r.Rate);

            if (_defaultRates.TryGetValue((source, target), out var defaultRate))
                return defaultRate;

            return null;
        }


        private const int SymbolLength = 3;

        private static Dictionary<(string, string), decimal>? _defaultRates;
        private readonly IDoubleFlow _cache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly CurrencyConverterContext _context;
        private readonly CurrencyLayerOptions _options;
        private readonly ILogger<RateService> _logger;
    }
}