using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.CurrencyConverterApi.Infrastructure;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyTravel.CurrencyConverterApi.Services
{
    public class ConversionService : IConversionService
    {
        public ConversionService(ILoggerFactory loggerFactory, IRateService rateService)
        {
            _logger = loggerFactory.CreateLogger<ConversionService>();
            _rateService = rateService;
        }


        public async ValueTask<Result<decimal, ProblemDetails>> Convert(Currencies sourceCurrency, Currencies targetCurrency, decimal value)
        {
            var (_, isFailure, results, error) = await Convert(sourceCurrency, targetCurrency, new List<decimal>(1) {value});
            if (isFailure)
                return Result.Failure<decimal, ProblemDetails>(error);

            if (results.TryGetValue(value, out var result))
                return Result.Ok<decimal, ProblemDetails>(result);

            return ProblemDetailsBuilder.FailAndLogNoQuoteFound<decimal>(_logger, sourceCurrency.ToString() + targetCurrency);
        }


        public async ValueTask<Result<Dictionary<decimal, decimal>, ProblemDetails>> Convert(Currencies sourceCurrency, Currencies targetCurrency, List<decimal> values)
        {
            if (sourceCurrency == Currencies.NotSpecified)
                return ProblemDetailsBuilder.FailAndLogArgumentNullOrEmpty<Dictionary<decimal, decimal>>(_logger, nameof(sourceCurrency));

            if (targetCurrency == Currencies.NotSpecified)
                return ProblemDetailsBuilder.FailAndLogArgumentNullOrEmpty<Dictionary<decimal, decimal>>(_logger, nameof(targetCurrency));

            if (values is null || !values.Any())
                return ProblemDetailsBuilder.FailAndLogArgumentNullOrEmpty<Dictionary<decimal, decimal>>(_logger, nameof(values));

            if (sourceCurrency == targetCurrency)
                return Result.Ok<Dictionary<decimal, decimal>, ProblemDetails>(values.ToDictionary(v => v, v => v));

            var (_, isFailure, rate, error) = await _rateService.Get(sourceCurrency, targetCurrency);
            if (isFailure)
                return Result.Failure<Dictionary<decimal, decimal>, ProblemDetails>(error);

            var results = new Dictionary<decimal, decimal>(values.Count);
            foreach (var value in values)
            {
                var ceiled = MoneyCeiler.Ceil(value * rate, targetCurrency);
                var isSane = IsSane(ceiled);
                if (isSane)
                    results.TryAdd(value, ceiled);
            }

            return Result.Ok<Dictionary<decimal, decimal>, ProblemDetails>(results);


            static bool IsSane(decimal value) 
                => value > decimal.Zero;
        }


        private readonly ILogger<ConversionService> _logger;
        private readonly IRateService _rateService;
    }
}