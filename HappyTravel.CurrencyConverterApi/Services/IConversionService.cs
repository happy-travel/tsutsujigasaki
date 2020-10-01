using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.CurrencyConverterApi.Services
{
    public interface IConversionService
    {
        public ValueTask<Result<decimal, ProblemDetails>> Convert(Currencies sourceCurrency, Currencies targetCurrency, decimal value);
        public ValueTask<Result<Dictionary<decimal, decimal>, ProblemDetails>> Convert(Currencies sourceCurrency, Currencies targetCurrency, List<decimal> values);
    }
}
