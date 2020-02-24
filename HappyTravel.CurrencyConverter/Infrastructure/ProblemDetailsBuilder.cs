using System.Net;
using CSharpFunctionalExtensions;
using HappyTravel.CurrencyConverter.Infrastructure.Constants;
using HappyTravel.CurrencyConverter.Infrastructure.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyTravel.CurrencyConverter.Infrastructure
{
    internal static class ProblemDetailsBuilder
    {
        internal static Result<T, ProblemDetails> FailAndLogArgumentNullOrEmpty<T>(ILogger logger, string variableName)
        {
            var message = string.Format(ErrorMessages.ArgumentNullOrEmpty, variableName, (int) LoggerEvents.ArgumentNullOrEmptyError);
            logger.LogArgumentNullOrEmptyError(message);

            return Fail<T>(message);
        }


        internal static Result<T, ProblemDetails> FailAndLogNetworkException<T>(ILogger logger, HttpStatusCode statusCode, string reasonPhrase)
        {
            var message = string.Format(ErrorMessages.NetworkError, (int) LoggerEvents.NetworkException);
            logger.LogNetworkException(reasonPhrase);

            return Fail<T>(message, statusCode);
        }


        internal static Result<T, ProblemDetails> FailAndLogNoQuoteFound<T>(ILogger logger, string pair)
        {
            var message = string.Format(ErrorMessages.NoQuoteFound, pair, (int) LoggerEvents.NoQuoteFoundError);
            logger.LogNoQuoteFoundError(message);

            return Fail<T>(message);
        }


        internal static ProblemDetails Build(string details, HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
            => new ProblemDetails
            {
                Detail = details,
                Status = (int) statusCode
            };


        internal static Result<T, ProblemDetails> Fail<T>(string details, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            => Result.Failure<T, ProblemDetails>(Build(details, statusCode));
    }
}
