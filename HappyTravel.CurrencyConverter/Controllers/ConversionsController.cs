using System.Collections.Generic;
using System.Linq;
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
    public class ConversionsController : ControllerBase
    {
        public ConversionsController(IConversionService service)
        {
            _service = service;
        }
    

        [ProducesResponseType(typeof(Dictionary<decimal, decimal>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [HttpGet("{fromCurrency}/{toCurrency}")]
        public async Task<IActionResult> Convert([FromRoute] string fromCurrency, [FromRoute] string toCurrency, [FromQuery] IEnumerable<decimal> values)
        {
            if (string.IsNullOrWhiteSpace(fromCurrency))
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(fromCurrency))));

            if (string.IsNullOrWhiteSpace(toCurrency))
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(toCurrency))));

            if (values is null || !values.Any())
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(values))));

            var (_, isFailure, result, error) = await _service.Convert(fromCurrency, toCurrency, values.ToList());
            if (isFailure)
                return BadRequest(error);

            return Ok(result);
        }
    

        [ProducesResponseType(typeof(decimal), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [HttpGet("{fromCurrency}/{toCurrency}/{value}")]
        public async Task<IActionResult> Convert([FromRoute] string fromCurrency, [FromRoute] string toCurrency, [FromRoute] decimal value)
        {
            if (string.IsNullOrWhiteSpace(fromCurrency))
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(fromCurrency))));

            if (string.IsNullOrWhiteSpace(toCurrency))
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(toCurrency))));

            var (_, isFailure, result, error) = await _service.Convert(fromCurrency, toCurrency, value);
            if (isFailure)
                return BadRequest(error);

            return Ok(result);
        }
        
        
        private readonly IConversionService _service;
    }
}
