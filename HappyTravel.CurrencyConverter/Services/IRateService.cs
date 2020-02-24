using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.CurrencyConverter.Services
{
    public interface IRateService
    {
        public ValueTask<Result<decimal, ProblemDetails>> Get(string sourceCurrency, string targetCurrency);
    }
}
