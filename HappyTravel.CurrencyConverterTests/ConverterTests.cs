using System;
using System.Collections.Generic;
using HappyTravel.CurrencyConverter;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Xunit;

namespace HappyTravel.CurrencyConverterTests
{
    public class ConverterTests : IClassFixture<ConverterTestsFixture>
    {
        public ConverterTests(ConverterTestsFixture fixture)
        {
            _fixture = fixture;
        }


        [Theory]
        [InlineData(Currencies.AED, Currencies.AED)]
        [InlineData(Currencies.USD, Currencies.USD)]
        public void Convert_should_throw_exception_when_source_and_target_currencies_are_same(Currencies sourceCurrency, Currencies targetCurrency)
        {
            var factory = new CurrencyConverterFactory(_fixture.ExceptionalPairs);

            Assert.Throws<ArgumentException>(() => factory.Create(1.5m, sourceCurrency, targetCurrency));
        }


        [Theory]
        [InlineData(Currencies.AED, Currencies.EUR)]
        [InlineData(Currencies.AED, Currencies.USD)]
        [InlineData(Currencies.AED, Currencies.AED)]
        [InlineData(Currencies.USD, Currencies.AED)]
        public void Convert_should_convert_from_source_currency_to_target_currency(Currencies sourceCurrency, Currencies targetCurrency)
        {
            var factory = new CurrencyConverterFactory(_fixture.ExceptionalPairs);
            var converter = factory.Create(1m, sourceCurrency, targetCurrency);

            var result = converter.Convert(0m);

            Assert.Equal(targetCurrency, result.Currency);
        }


        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(10, 10)]
        [InlineData(-10, -10)]
        public void Convert_Should_convert_values(decimal rate, decimal expected)
        {
            var factory = new CurrencyConverterFactory(_fixture.ExceptionalPairs);
            var converter = factory.Create(rate, Currencies.USD, Currencies.EUR);

            var result = converter.Convert(new MoneyAmount(1m, Currencies.USD));

            Assert.Equal(expected, result.Amount);
        }


        private readonly ConverterTestsFixture _fixture;


        [Fact]
        public void Convert_should_skip_same_values()
        {
            var factory = new CurrencyConverterFactory(_fixture.ExceptionalPairs);
            var converter = factory.Create(1m, Currencies.EUR, Currencies.EUR);

            var sources = new List<MoneyAmount>
            {
                new MoneyAmount(4m, Currencies.EUR),
                new MoneyAmount(4m, Currencies.EUR),
                new MoneyAmount(2m, Currencies.EUR)
            };

            var results = converter.Convert(sources);

            Assert.Equal(2, results.Count);
        }


        [Fact]
        public void Convert_should_throw_exception_when_different_source_currency_had_passed_into_instance()
        {
            var factory = new CurrencyConverterFactory(_fixture.ExceptionalPairs);
            var converter = factory.Create(1m, Currencies.EUR, Currencies.USD);

            var amount = new MoneyAmount(1m, Currencies.AED);

            Assert.Throws<ArgumentException>(() => converter.Convert(amount));
        }


        [Fact]
        public void Convert_should_throw_exception_when_null_value_provided()
        {
            var factory = new CurrencyConverterFactory(_fixture.ExceptionalPairs);
            var converter = factory.Create(1m, Currencies.USD, Currencies.EUR);

            Assert.Throws<ArgumentNullException>(() => converter.Convert((IEnumerable<decimal>) null!));
        }


        [Fact]
        public void Convert_should_throw_exception_when_source_currency_unspecified()
        {
            var factory = new CurrencyConverterFactory(_fixture.ExceptionalPairs);

            Assert.Throws<ArgumentException>(() => factory.Create(1m, Currencies.NotSpecified, Currencies.EUR));
        }


        [Fact]
        public void Convert_should_throw_exception_when_target_currency_unspecified()
        {
            var factory = new CurrencyConverterFactory(_fixture.ExceptionalPairs);

            Assert.Throws<ArgumentException>(() => factory.Create(1m, Currencies.EUR, Currencies.NotSpecified));
        }
    }
}