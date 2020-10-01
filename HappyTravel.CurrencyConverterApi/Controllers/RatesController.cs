using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.CurrencyConverterApi.Services;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.CurrencyConverterApi.Controllers
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


        /// <summary>
        /// Returns conversion rate for the provided currency pair.
        /// </summary>
        /// <param name="sourceCurrency">The source currency code</param>
        /// <param name="targetCurrency">The target currency code</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(decimal), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [HttpGet("{sourceCurrency}/{targetCurrency}")]
        public async Task<IActionResult> Convert([FromRoute] Currencies sourceCurrency, [FromRoute] Currencies targetCurrency)
        {
            var (_, isFailure, value, error) = await _rateService.Get(sourceCurrency, targetCurrency);
            if (isFailure)
                return BadRequest(error);

            return Ok(value);
        }
    
        
        private readonly IRateService _rateService;
    }
}
