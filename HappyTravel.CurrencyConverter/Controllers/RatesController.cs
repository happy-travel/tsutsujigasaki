using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.CurrencyConverter.Infrastructure.Constants;
using HappyTravel.CurrencyConverter.Services;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.CurrencyConverter.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class RatesController : ControllerBase
    {
        public RatesController(IRateService rateService)
        {
            _rateService = rateService;
        }


        [ProducesResponseType(typeof(decimal), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [HttpGet("{fromCurrency}/{toCurrency}")]
        public async Task<IActionResult> Convert([FromRoute] string fromCurrency, [FromRoute] string toCurrency)
        {
            if (string.IsNullOrWhiteSpace(fromCurrency))
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(fromCurrency))));

            if (string.IsNullOrWhiteSpace(toCurrency))
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(toCurrency))));

            var (_, isFailure, value, error) = await _rateService.Get(fromCurrency, toCurrency);
            if (isFailure)
                return BadRequest(error);

            return Ok(value);
        }
    
        
        private readonly IRateService _rateService;
    }
}
