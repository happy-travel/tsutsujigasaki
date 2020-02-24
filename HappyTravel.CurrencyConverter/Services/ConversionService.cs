using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.CurrencyConverter.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static HappyTravel.CurrencyConverter.Infrastructure.Constants.Constants;

namespace HappyTravel.CurrencyConverter.Services
{
    public class ConversionService : IConversionService
    {
        public ConversionService(ILoggerFactory loggerFactory, IRateService rateService)
        {
            _logger = loggerFactory.CreateLogger<ConversionService>();
            _rateService = rateService;
        }


        public async ValueTask<Result<decimal, ProblemDetails>> Convert(string sourceCurrency, string targetCurrency, decimal value)
        {
            var (_, isFailure, results, error) = await Convert(sourceCurrency, targetCurrency, new List<decimal>(1) {value});
            if (isFailure)
                return Result.Failure<decimal, ProblemDetails>(error);

            if (results.TryGetValue(value, out var result))
                return Result.Ok<decimal, ProblemDetails>(result);

            return ProblemDetailsBuilder.FailAndLogNoQuoteFound<decimal>(_logger, sourceCurrency + targetCurrency);
        }


        public async ValueTask<Result<Dictionary<decimal, decimal>, ProblemDetails>> Convert(string sourceCurrency, string targetCurrency, List<decimal> values)
        {
            if (string.IsNullOrWhiteSpace(sourceCurrency))
                return ProblemDetailsBuilder.FailAndLogArgumentNullOrEmpty<Dictionary<decimal, decimal>>(_logger, nameof(sourceCurrency));

            if (string.IsNullOrWhiteSpace(targetCurrency))
                return ProblemDetailsBuilder.FailAndLogArgumentNullOrEmpty<Dictionary<decimal, decimal>>(_logger, nameof(targetCurrency));

            if (values is null || !values.Any())
                return ProblemDetailsBuilder.FailAndLogArgumentNullOrEmpty<Dictionary<decimal, decimal>>(_logger, nameof(values));

            if (sourceCurrency.Equals(targetCurrency, StringComparison.InvariantCultureIgnoreCase))
                return Result.Ok<Dictionary<decimal, decimal>, ProblemDetails>(new Dictionary<decimal, decimal> {{values[0], values[0]}});

            var (_, isFailure, rate, error) = await _rateService.Get(sourceCurrency, targetCurrency);
            if (isFailure)
                return Result.Failure<Dictionary<decimal, decimal>, ProblemDetails>(error);

            var results = new Dictionary<decimal, decimal>(values.Count);
            foreach (var value in values)
            {
                var ceiled = Ceil(value * rate, targetCurrency);
                var isSane = IsSane(ceiled);
                if (isSane)
                    results.TryAdd(value, ceiled);
            }

            return Result.Ok<Dictionary<decimal, decimal>, ProblemDetails>(results);


            static decimal Ceil(decimal target, string toCurrency) 
                => Math.Round(target, SupportedCurrencies[toCurrency], MidpointRounding.AwayFromZero);


            static bool IsSane(decimal value) 
                => value > decimal.Zero;
        }


        private readonly ILogger<ConversionService> _logger;
        private readonly IRateService _rateService;
    }
}