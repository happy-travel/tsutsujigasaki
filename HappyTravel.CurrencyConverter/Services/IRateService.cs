using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.CurrencyConverter.Services
{
    public interface IRateService
    {
        public Task<Result<decimal, ProblemDetails>> Get(string fromCurrency, string toCurrency);
    }
}
