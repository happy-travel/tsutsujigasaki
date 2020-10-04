using HappyTravel.Money.Enums;

namespace HappyTravel.CurrencyConverter
{
    public interface ICurrencyConverterFactory
    {
        CurrencyConverter Create(in decimal rate, Currencies sourceCurrency, Currencies targetCurrency);
    }
}