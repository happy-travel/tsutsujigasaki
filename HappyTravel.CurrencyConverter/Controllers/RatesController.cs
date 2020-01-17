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
        [HttpGet("{sourceCurrency}/{targetCurrency}")]
        public async Task<IActionResult> Convert([FromRoute] string sourceCurrency, [FromRoute] string targetCurrency)
        {
            if (string.IsNullOrWhiteSpace(sourceCurrency))
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(sourceCurrency))));

            if (string.IsNullOrWhiteSpace(targetCurrency))
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(targetCurrency))));

            var (_, isFailure, value, error) = await _rateService.Get(sourceCurrency, targetCurrency);
            if (isFailure)
                return BadRequest(error);

            return Ok(value);
        }
    
        
        private readonly IRateService _rateService;
    }
}
