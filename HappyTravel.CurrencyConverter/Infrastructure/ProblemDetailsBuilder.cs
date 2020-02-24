using System.Collections.Generic;
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

            return Fail<T>("Argument Null Or Empty", message, HttpStatusCode.BadRequest, new Dictionary<string, object>{{nameof(variableName), variableName}});
        }


        internal static Result<T, ProblemDetails> FailAndLogNetworkException<T>(ILogger logger, HttpStatusCode statusCode, string reasonPhrase)
        {
            var message = string.Format(ErrorMessages.NetworkError, (int) LoggerEvents.NetworkException);
            logger.LogNetworkException(reasonPhrase);

            return Fail<T>("Network Exception", message, statusCode, new Dictionary<string, object>{{nameof(reasonPhrase), reasonPhrase}});
        }


        internal static Result<T, ProblemDetails> FailAndLogNoQuoteFound<T>(ILogger logger, string pair)
        {
            var message = string.Format(ErrorMessages.NoQuoteFound, pair, (int) LoggerEvents.NoQuoteFoundError);
            logger.LogNoQuoteFoundError(message);

            return Fail<T>("No Quote Found", message, HttpStatusCode.BadRequest, new Dictionary<string, object>{{nameof(pair), pair}});
        }


        internal static ProblemDetails Build(string title, string details, HttpStatusCode statusCode = HttpStatusCode.BadRequest, Dictionary<string, object>? extensions = null)
        {
            var result = new ProblemDetails
            {
                Title = title,
                Detail = details,
                Status = (int) statusCode
            };

            if (extensions == null)
                return result;

            foreach (var extension in extensions)
                result.Extensions.Add(extension);

            return result;
        }


        internal static Result<T, ProblemDetails> Fail<T>(string title, string details, HttpStatusCode statusCode = HttpStatusCode.BadRequest, Dictionary<string, object>? extensions = null)
            => Result.Failure<T, ProblemDetails>(Build(title, details, statusCode, extensions));
    }
}
