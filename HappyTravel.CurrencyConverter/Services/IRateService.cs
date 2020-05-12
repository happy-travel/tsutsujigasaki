using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.CurrencyConverter.Services
{
    public interface IRateService
    {
        public ValueTask<Result<decimal, ProblemDetails>> Get(Currencies sourceCurrency, Currencies targetCurrency);
    }
}
