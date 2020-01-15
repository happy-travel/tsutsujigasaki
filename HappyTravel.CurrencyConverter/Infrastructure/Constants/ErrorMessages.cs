namespace HappyTravel.CurrencyConverter.Infrastructure.Constants
{
    internal static class ErrorMessages
    {
        internal static string NetworkError = "HTCC78001 A network error occured. Please retry your request after several seconds.";
        internal static string ArgumentNullOrEmptyError = "HTCC78011 Provided {0} is null or empty.";
        internal static string NoQuoteFound = "HTCC78021 No quote was found for '{0}' pair.";
    }
}
