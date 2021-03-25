namespace HappyTravel.Tsutsujigasaki.Api.Infrastructure.Constants
{
    internal static class ErrorMessages
    {
        internal static string NetworkError = "HTCC{0} A network error occured. Please retry your request after several seconds.";
        internal const string ArgumentNullOrEmpty = "HTCC{1} Provided {0} is null or empty.";
        internal const string NoQuoteFound = "HTCC{1} No quote was found for '{0}' pair.";
    }
}
