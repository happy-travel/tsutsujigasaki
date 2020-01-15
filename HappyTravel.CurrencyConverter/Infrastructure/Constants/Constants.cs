using System.Collections.Generic;

namespace HappyTravel.CurrencyConverter.Infrastructure.Constants
{
    public static class Constants
    {
        public static readonly Dictionary<string, int> SupportedCurrencies = new Dictionary<string, int>
        {
            {"AED", 2},
            {"EUR", 2},
            {"SAR", 2},
            {"USD", 2}
        };
    }
}
