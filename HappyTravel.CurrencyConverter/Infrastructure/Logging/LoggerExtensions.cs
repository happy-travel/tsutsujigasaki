using System;
using HappyTravel.CurrencyConverter.Services;
using Microsoft.Extensions.Logging;

namespace HappyTravel.CurrencyConverter.Infrastructure.Logging
{
    internal static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            ArgumentNullOrEmptyError = LoggerMessage.Define<string>(LogLevel.Information, GetEventId(LoggerEvents.ArgumentNullOrEmptyError), $"INF | {{message}}");
            NetworkException = LoggerMessage.Define<string>(LogLevel.Error, GetEventId(LoggerEvents.NetworkException), $"ERROR | {nameof(RateService)}: {{message}}");
            NoQuoteFoundError = LoggerMessage.Define<string>(LogLevel.Warning, GetEventId(LoggerEvents.NetworkException), $"WARN | {nameof(RateService)}: {{message}}");
            RateServiceException = LoggerMessage.Define(LogLevel.Critical, GetEventId(LoggerEvents.RateServiceException), $"CRITICAL | {nameof(RateService)}: ");
        }


        internal static void LogArgumentNullOrEmptyError(this ILogger logger, string message) => ArgumentNullOrEmptyError(logger, message, null);
        internal static void LogNetworkException(this ILogger logger, string message) => NetworkException(logger, message, null);
        internal static void LogNoQuoteFoundError(this ILogger logger, string message) => NoQuoteFoundError(logger, message, null);
        internal static void LogRateServiceException(this ILogger logger, Exception exception) => RateServiceException(logger, exception);


        private static EventId GetEventId(LoggerEvents @event) => new EventId((int) @event, @event.ToString());


        private static readonly Action<ILogger, string, Exception?> ArgumentNullOrEmptyError;
        private static readonly Action<ILogger, string, Exception?> NetworkException;
        private static readonly Action<ILogger, string, Exception?> NoQuoteFoundError;
        private static readonly Action<ILogger, Exception> RateServiceException;
    }
}
