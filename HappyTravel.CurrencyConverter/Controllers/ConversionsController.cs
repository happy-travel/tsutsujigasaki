using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.CurrencyConverter.Services;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.CurrencyConverter.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class ConversionsController : ControllerBase
    {
        public ConversionsController(IConversionService service)
        {
            _service = service;
        }
    

        /// <summary>
        /// Converts values from one currency to another.
        /// </summary>
        /// <param name="sourceCurrency">The source currency code</param>
        /// <param name="targetCurrency">The target currency code</param>
        /// <param name="values"></param>
        /// <returns>Pairs of original and converted values in a list</returns>
        [ProducesResponseType(typeof(Dictionary<decimal, decimal>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [HttpGet("{sourceCurrency}/{targetCurrency}")]
        public async Task<IActionResult> Convert([FromRoute] Currencies sourceCurrency, [FromRoute] Currencies targetCurrency, [FromQuery] IEnumerable<decimal> values)
        {
            var (_, isFailure, result, error) = await _service.Convert(sourceCurrency, targetCurrency, values.ToList());
            if (isFailure)
                return BadRequest(error);

            return Ok(result);
        }
    

        /// <summary>
        /// Converts the value from one currency to another.
        /// </summary>
        /// <param name="sourceCurrency">The source currency code</param>
        /// <param name="targetCurrency">The target currency code</param>
        /// <param name="value"></param>
        /// <returns>The converted value</returns>
        [ProducesResponseType(typeof(decimal), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [HttpGet("{sourceCurrency}/{targetCurrency}/{value}")]
        public async Task<IActionResult> Convert([FromRoute] Currencies sourceCurrency, [FromRoute] Currencies targetCurrency, [FromRoute] decimal value)
        {
            var (_, isFailure, result, error) = await _service.Convert(sourceCurrency, targetCurrency, value);
            if (isFailure)
                return BadRequest(error);

            return Ok(result);
        }
        
        
        private readonly IConversionService _service;
    }
}
