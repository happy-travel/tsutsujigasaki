using System;

namespace HappyTravel.Tsutsujigasaki.Api.Data
{
    public class CurrencyRate
    {
        public decimal Rate { get; set; }
        public string Source { get; set; } = null!;
        public string Target { get; set; } = null!;
        public DateTime ValidFrom { get; set; }
    }
}