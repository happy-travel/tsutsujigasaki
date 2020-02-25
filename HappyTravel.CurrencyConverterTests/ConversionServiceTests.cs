﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.CurrencyConverter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HappyTravel.CurrencyConverterTests
{
    public class ConversionServiceTests
    {
        [Fact]
        public void ConversionService_ShouldThrowExceptionWhenLoggerFactoryIsnull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConversionService(null, null));
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\r")]
        public async Task Convert_ShouldReturnErrorWhenSourceCurrencyNullOrEmpty(string sourceCurrency)
        {
            var service = new ConversionService(new NullLoggerFactory(), null);
            var (_, isFailure, _, error) = await service.Convert(sourceCurrency, "AED", 100m);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\r")]
        public async Task Convert_ShouldReturnErrorWhenTargetCurrencyNullOrEmpty(string targetCurrency)
        {
            var service = new ConversionService(new NullLoggerFactory(), null);
            var (_, isFailure, _, error) = await service.Convert("USD", targetCurrency, 100m);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Convert_ShouldReturnErrorWhenValuesAreNull()
        {
            var service = new ConversionService(new NullLoggerFactory(), null);
            var (_, isFailure, _, error) = await service.Convert("USD", "AED", null);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Convert_ShouldReturnErrorWhenValuesAreEmpty()
        {
            var service = new ConversionService(new NullLoggerFactory(), null);
            var (_, isFailure, _, error) = await service.Convert("USD", "AED", new List<decimal>(0));

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Convert_ShouldReturnInitialValuesWhenSoursAndTargetCurrenciesAreTheSame()
        {
            var service = new ConversionService(new NullLoggerFactory(), null);
            var (isSuccess, _, values, _) = await service.Convert("USD", "USD", _values);

            Assert.True(isSuccess);
            Assert.Equal(_values.Count, values.Count);
            Assert.All(values, pair =>
            {
                var (k, v) = pair;
                Assert.Equal(k, v);
                Assert.Contains(k, _values);
            });
        }


        [Fact]
        public async Task Convert_ShouldReturnProblemDetailsWhenRateServiceReturnsProblemDetails()
        {
            const int code = 499;
            const string details = "Error message";
            var rateServiceMock = new Mock<IRateService>();
            rateServiceMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Failure<decimal, ProblemDetails>(new ProblemDetails {Detail = details, Status = code}));

            var service = new ConversionService(new NullLoggerFactory(), rateServiceMock.Object);
            var (_, isFailure, _, error) = await service.Convert("USD", "AED", _values);

            Assert.True(isFailure);
            Assert.Equal(code, error.Status);
            Assert.Equal(details, error.Detail);
        }


        [Fact]
        public async Task Convert_ShouldReturnValuesWhenSoursAndRateServiceReturnsRates()
        {
            const decimal rate = 100m;
            var rateServiceMock = new Mock<IRateService>();
            rateServiceMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Ok<decimal, ProblemDetails>(rate));
            
            var service = new ConversionService(new NullLoggerFactory(), rateServiceMock.Object);
            var (isSuccess, _, values, _) = await service.Convert("USD", "AED", _values);

            Assert.True(isSuccess);
            Assert.Equal(_values.Count, values.Count);
            Assert.All(values, pair =>
            {
                var (k, v) = pair;
                Assert.Equal(k * rate, v);
                Assert.Contains(k, _values);
            });
        }


        [Fact]
        public async Task Convert_ShouldReturnSaneValuesWhenSoursAndRateServiceReturnsRates()
        {
            const decimal rate = 100m;
            var rateServiceMock = new Mock<IRateService>();
            rateServiceMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Ok<decimal, ProblemDetails>(rate));
            
            var service = new ConversionService(new NullLoggerFactory(), rateServiceMock.Object);
            var (isSuccess, _, values, _) = await service.Convert("USD", "AED", _insaneValues);

            Assert.True(isSuccess);
            Assert.Equal(_insaneValues.Count(v => 0 < v), values.Count);
            Assert.All(values, pair =>
            {
                var (k, v) = pair;
                Assert.Equal(k * rate, v);
                Assert.Contains(k, _insaneValues);
            });
        }


        private readonly List<decimal> _values = new List<decimal> {100m, 200m, 300m};
        private readonly List<decimal> _insaneValues = new List<decimal> {100m, -200m, 300m};
    }
}
