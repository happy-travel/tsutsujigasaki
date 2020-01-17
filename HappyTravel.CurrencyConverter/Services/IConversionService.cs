using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.CurrencyConverter.Services
{
    public interface IConversionService
    {
        public ValueTask<Result<decimal, ProblemDetails>> Convert(string sourceCurrency, string targetCurrency, decimal value);
        public ValueTask<Result<Dictionary<decimal, decimal>, ProblemDetails>> Convert(string sourceCurrency, string targetCurrency, List<decimal> values);
    }
}
