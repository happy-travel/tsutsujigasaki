using HappyTravel.Money.Enums;

namespace HappyTravel.CurrencyConverter.Infrastructure
{
    public class BufferPair
    {
        public Currencies SourceCurrency { get; set; }
        public Currencies TargetCurrency { get; set; }
        public decimal BufferValue { get; set; }
    }
}
