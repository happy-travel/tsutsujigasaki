using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.CurrencyConverter.Services
{
    public interface IConversionService
    {
        public ValueTask<Result<decimal, ProblemDetails>> Convert(string fromCurrency, string toCurrency, decimal value);
        public ValueTask<Result<Dictionary<decimal, decimal>, ProblemDetails>> Convert(string fromCurrency, string toCurrency, List<decimal> values);
    }
}
