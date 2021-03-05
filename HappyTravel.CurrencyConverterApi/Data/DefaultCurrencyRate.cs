using System;

namespace HappyTravel.CurrencyConverterApi.Data
{
    public class DefaultCurrencyRate
    {
        public decimal Rate { get; set; }
        public string Source { get; set; } = null!;
        public string Target { get; set; } = null!;
        public DateTime ValidFrom { get; set; }
    }
}