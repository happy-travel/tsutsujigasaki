using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.CurrencyConverterApi.Services
{
    public interface IConversionService
    {
        public ValueTask<Result<MoneyAmount, ProblemDetails>> Convert(Currencies sourceCurrency, Currencies targetCurrency, decimal value);
        public ValueTask<Result<Dictionary<MoneyAmount, MoneyAmount>, ProblemDetails>> Convert(Currencies sourceCurrency, Currencies targetCurrency, List<decimal> values);
    }
}
