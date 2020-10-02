using System;
using System.Collections.Generic;
using HappyTravel.CurrencyConverter;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Xunit;

namespace HappyTravel.CurrencyConverterTests
{
    public class ConverterTests
    {
        [Theory]
        [InlineData(Currencies.AED, Currencies.AED)]
        [InlineData(Currencies.USD, Currencies.USD)]
        public void Convert_should_throws_exception_when_source_and_target_currencies_are_same(Currencies sourceCurrency, Currencies targetCurrency)
        {
            Assert.Throws<ArgumentException>(() => Converter.Convert(1.5m, targetCurrency, new MoneyAmount(1m, sourceCurrency)));
        }


        [Theory]
        [InlineData(Currencies.AED, Currencies.EUR)]
        [InlineData(Currencies.AED, Currencies.USD)]
        [InlineData(Currencies.AED, Currencies.AED)]
        [InlineData(Currencies.USD, Currencies.AED)]
        public void Convert_should_convert_from_source_currency_to_target_currency(Currencies sourceCurrency, Currencies targetCurrency)
        {
            var result = Converter.Convert(1m, sourceCurrency, targetCurrency, 0m);

            Assert.Equal(targetCurrency, result.Currency);
        }


        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(10, 10)]
        [InlineData(-10, -10)]
        public void Convert_Should_convert_values(decimal rate, decimal expected)
        {
            var result = Converter.Convert(rate, Currencies.EUR, new MoneyAmount(1m, Currencies.USD));

            Assert.Equal(expected, result.Amount);
        }


        [Fact]
        public void Convert_should_skip_same_values()
        {
            var sources = new List<MoneyAmount>
            {
                new MoneyAmount(4m, Currencies.EUR),
                new MoneyAmount(4m, Currencies.EUR),
                new MoneyAmount(2m, Currencies.EUR)
            };

            var results = Converter.Convert(1m, Currencies.EUR, sources);

            Assert.Equal(2, results.Count);
        }


        [Fact]
        public void Convert_should_throw_exception_when_different_source_currency_had_passed_into_instance()
        {
            var amount = new MoneyAmount(1m, Currencies.AED);
            var converter = ConverterFactory.Create(1m, Currencies.EUR, Currencies.USD);

            Assert.Throws<ArgumentException>(() => converter.Convert(amount));
        }


        [Fact]
        public void Convert_should_throw_exception_when_null_value_provided()
        {
            Assert.Throws<ArgumentNullException>(() => Converter.Convert(1m, Currencies.NotSpecified, null!));
        }


        [Fact]
        public void Convert_should_throw_exception_when_source_currency_unspecified()
        {
            Assert.Throws<ArgumentException>(() => Converter.Convert(1m, Currencies.EUR, new MoneyAmount(1m, Currencies.NotSpecified)));
        }


        [Fact]
        public void Convert_should_throw_exception_when_target_currency_unspecified()
        {
            Assert.Throws<ArgumentException>(() => Converter.Convert(1m, Currencies.NotSpecified, new MoneyAmount(1m, Currencies.EUR)));
        }
    }
}