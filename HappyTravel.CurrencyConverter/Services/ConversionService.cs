using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.CurrencyConverter.Infrastructure.Constants;
using Microsoft.AspNetCore.Mvc;
using static HappyTravel.CurrencyConverter.Infrastructure.Constants.Constants;

namespace HappyTravel.CurrencyConverter.Services
{
    public class ConversionService : IConversionService
    {
        public ConversionService(IRateService rateService)
        {
            _rateService = rateService;
        }


        public async ValueTask<Result<decimal, ProblemDetails>> Convert(string sourceCurrency, string targetCurrency, decimal value)
        {
            var (_, isFailure, results, error) = await Convert(sourceCurrency, targetCurrency, new List<decimal>(1) {value});
            if (isFailure)
                return Result.Failure<decimal, ProblemDetails>(error);

            return results.TryGetValue(value, out var result)
                ? Result.Ok<decimal, ProblemDetails>(result)
                : ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.NoQuoteFound, $"{sourceCurrency}{targetCurrency}"));
        }


        public async ValueTask<Result<Dictionary<decimal, decimal>, ProblemDetails>> Convert(string sourceCurrency, string targetCurrency, List<decimal> values)
        {
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


        private readonly IRateService _rateService;
    }
}