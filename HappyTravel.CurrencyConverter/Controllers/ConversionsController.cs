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
        [HttpGet("{sourceCurrency}/{targetCurrency}")]
        public async Task<IActionResult> Convert([FromRoute] string sourceCurrency, [FromRoute] string targetCurrency, [FromQuery] IEnumerable<decimal> values)
        {
            if (string.IsNullOrWhiteSpace(sourceCurrency))
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(sourceCurrency))));

            if (string.IsNullOrWhiteSpace(targetCurrency))
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(targetCurrency))));

            if (values is null || !values.Any())
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(values))));

            var (_, isFailure, result, error) = await _service.Convert(sourceCurrency, targetCurrency, values.ToList());
            if (isFailure)
                return BadRequest(error);

            return Ok(result);
        }
    

        [ProducesResponseType(typeof(decimal), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [HttpGet("{sourceCurrency}/{targetCurrency}/{value}")]
        public async Task<IActionResult> Convert([FromRoute] string sourceCurrency, [FromRoute] string targetCurrency, [FromRoute] decimal value)
        {
            if (string.IsNullOrWhiteSpace(sourceCurrency))
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(sourceCurrency))));

            if (string.IsNullOrWhiteSpace(targetCurrency))
                return BadRequest(ProblemDetailsBuilder.Fail<decimal>(string.Format(ErrorMessages.ArgumentNullOrEmptyError, nameof(targetCurrency))));

            var (_, isFailure, result, error) = await _service.Convert(sourceCurrency, targetCurrency, value);
            if (isFailure)
                return BadRequest(error);

            return Ok(result);
        }
        
        
        private readonly IConversionService _service;
    }
}
