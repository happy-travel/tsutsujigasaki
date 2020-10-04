using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.CurrencyConverter;
using HappyTravel.CurrencyConverterApi.Infrastructure;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyTravel.CurrencyConverterApi.Services
{
    public class ConversionService : IConversionService
    {
        public ConversionService(ILoggerFactory loggerFactory, IRateService rateService, ICurrencyConverterFactory converterFactory)
        {
            _converterFactory = converterFactory;
            _logger = loggerFactory.CreateLogger<ConversionService>();
            _rateService = rateService;
        }


        public async ValueTask<Result<MoneyAmount, ProblemDetails>> Convert(Currencies sourceCurrency, Currencies targetCurrency, decimal value)
        {
            var (_, isFailure, results, error) = await Convert(sourceCurrency, targetCurrency, new List<decimal>(1) {value});
            if (isFailure)
                return Result.Failure<MoneyAmount, ProblemDetails>(error);

            if (results.TryGetValue(new MoneyAmount(value, targetCurrency), out var result))
                return Result.Success<MoneyAmount, ProblemDetails>(result);

            return ProblemDetailsBuilder.FailAndLogNoQuoteFound<MoneyAmount>(_logger, sourceCurrency.ToString() + targetCurrency);
        }


        public async ValueTask<Result<Dictionary<MoneyAmount, MoneyAmount>, ProblemDetails>> Convert(Currencies sourceCurrency, Currencies targetCurrency, List<decimal> values)
        {
            try
            {
                var (_, isFailure, rate, error) = await _rateService.Get(sourceCurrency, targetCurrency);
                if (isFailure)
                    return Result.Failure<Dictionary<MoneyAmount, MoneyAmount>, ProblemDetails>(error);

                var converter = _converterFactory.Create(in rate, sourceCurrency, targetCurrency);
                var results = converter.Convert(values);
                
                return Result.Success<Dictionary<MoneyAmount, MoneyAmount>, ProblemDetails>(results);

            }
            catch (Exception ex)
            {
                return ProblemDetailsBuilder.Build(ex.Message, ex.Message);
            }
        }


        private readonly ICurrencyConverterFactory _converterFactory;
        private readonly ILogger<ConversionService> _logger;
        private readonly IRateService _rateService;
    }
}