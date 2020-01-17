using System;

namespace HappyTravel.CurrencyConverter.Data
{
    #nullable enable
    public class CurrencyRate
    {
        public decimal Rate { get; set; }
        public string? Source { get; set; }
        public string? Target { get; set; }
        public DateTime ValidFrom { get; set; }
    }
    #nullable restore
}