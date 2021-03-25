using HappyTravel.Money.Enums;

namespace HappyTravel.Tsutsujigasaki.Api.Data
{
    public class DefaultCurrencyRate
    {
        public decimal Rate { get; set; }
        public Currencies Source { get; set; }
        public Currencies Target { get; set; }
    }
}