using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.CurrencyConverter
{
    public class Converter
    {
        internal Converter(in decimal rate, Currencies sourceCurrency, Currencies targetCurrency)
        {
            _rate = rate;
            _sourceCurrency = sourceCurrency;
            _targetCurrency = targetCurrency;
        }


        public MoneyAmount Convert(in MoneyAmount sourceValue)
        {
            if (sourceValue.Currency != _sourceCurrency)
                throw new ArgumentException("The source amount currency mismatches with a predefined one.");

            return Convert(in _rate, _targetCurrency, in sourceValue);
        }


        public MoneyAmount Convert(in decimal sourceValue) 
            => Convert(in _rate, _targetCurrency, new MoneyAmount(sourceValue, _sourceCurrency));


        public static MoneyAmount Convert(in decimal rate, Currencies targetCurrency, in MoneyAmount sourceValue)
        {
            var results = Convert(in rate, targetCurrency, new[] {sourceValue});
            results.TryGetValue(sourceValue, out var result);

            return result;
        }
        

        public static MoneyAmount Convert(in decimal rate, Currencies sourceCurrency, Currencies targetCurrency, in decimal value) 
            => Convert(in rate, targetCurrency, new MoneyAmount(value, sourceCurrency));


        public Dictionary<MoneyAmount, MoneyAmount> Convert(IEnumerable<decimal> sourceValues)
        {
            if (sourceValues is null)
                throw new ArgumentNullException(nameof(sourceValues));

            var amounts = sourceValues.Select(v => new MoneyAmount(v, _sourceCurrency));
            return Convert(amounts);
        }


        public Dictionary<MoneyAmount, MoneyAmount> Convert(IEnumerable<MoneyAmount> sourceValues)
        {
            if (sourceValues is null)
                throw new ArgumentNullException(nameof(sourceValues));

            var list = sourceValues.ToList();
            if (list.FirstOrDefault().Currency != _sourceCurrency)
                throw new ArgumentException("The source amount currency mismatches with a predefined one.");

            return ConvertInternal(_rate, _targetCurrency, list);
        }
        
        
        public static Dictionary<MoneyAmount, MoneyAmount> Convert(in decimal rate, Currencies targetCurrency, IEnumerable<MoneyAmount> sourceValues)
        {
            if (sourceValues is null)
                throw new ArgumentNullException(nameof(sourceValues));
            
            var list = sourceValues.ToList();
            var sourceCurrency = list.FirstOrDefault().Currency;
            CheckPreconditions(in rate, sourceCurrency, targetCurrency);

            return ConvertInternal(in rate, targetCurrency, list);
        }


        internal static void CheckPreconditions(in decimal rate, Currencies sourceCurrency, Currencies targetCurrency)
        {
            if (targetCurrency == Currencies.NotSpecified)
                throw new ArgumentException("The target currency must be specified.");

            if (sourceCurrency == Currencies.NotSpecified)
                throw new ArgumentException("The source currency must be specified.");

            if (sourceCurrency == targetCurrency && rate != 1m)
                throw new ArgumentException("You use the same currency as a source and a target.");
        }


        private static Dictionary<MoneyAmount, MoneyAmount> ConvertInternal(in decimal rate, Currencies targetCurrency, List<MoneyAmount> sourceValues)
        {
            var results = new Dictionary<MoneyAmount, MoneyAmount>(sourceValues.Count);
            foreach (var sourceValue in sourceValues)
            {
                if (results.ContainsKey(sourceValue))
                    continue;

                var targetAmount = new MoneyAmount(sourceValue.Amount * rate, targetCurrency);
                results.Add(sourceValue, targetAmount);
            }

            return results;
        }


        private readonly decimal _rate;
        private readonly Currencies _sourceCurrency;
        private readonly Currencies _targetCurrency;
    }
}
