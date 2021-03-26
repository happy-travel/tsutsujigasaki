using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Tsutsujigasaki.Api.Services
{
    public interface IRateService
    {
        public ValueTask<Result<decimal, ProblemDetails>> Get(Currencies sourceCurrency, Currencies targetCurrency);
    }
}
