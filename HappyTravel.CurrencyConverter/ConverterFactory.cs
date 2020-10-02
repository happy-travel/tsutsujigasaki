using HappyTravel.Money.Enums;

namespace HappyTravel.CurrencyConverter
{
    public static class ConverterFactory
    {
        public static Converter Create(in decimal rate, Currencies sourceCurrency, Currencies targetCurrency)
        {
            Converter.CheckPreconditions(in rate, sourceCurrency, targetCurrency);
            return new Converter(in rate, sourceCurrency, targetCurrency);
        }
    }
}
