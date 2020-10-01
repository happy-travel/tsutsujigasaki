using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.CurrencyConverterApi.Services;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HappyTravel.CurrencyConverterApiTests
{
    public class ConversionServiceTests
    {
        [Fact]
        public void ConversionService_ShouldThrowExceptionWhenLoggerFactoryIsnull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConversionService(null, null));
        }


        [Fact]
        public async Task Convert_ShouldReturnErrorWhenSourceCurrencyIsNotSpecified()
        {
            var service = new ConversionService(new NullLoggerFactory(), null);
            var (_, isFailure, _, error) = await service.Convert(Currencies.NotSpecified, Currencies.AED, 100m);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Convert_ShouldReturnErrorWhenTargetCurrencyIsNotSpecified()
        {
            var service = new ConversionService(new NullLoggerFactory(), null);
            var (_, isFailure, _, error) = await service.Convert(Currencies.USD, Currencies.NotSpecified, 100m);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Convert_ShouldReturnErrorWhenValuesAreNull()
        {
            var service = new ConversionService(new NullLoggerFactory(), null);
            var (_, isFailure, _, error) = await service.Convert(Currencies.USD, Currencies.AED, null);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Convert_ShouldReturnErrorWhenValuesAreEmpty()
        {
            var service = new ConversionService(new NullLoggerFactory(), null);
            var (_, isFailure, _, error) = await service.Convert(Currencies.USD, Currencies.AED, new List<decimal>(0));

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Convert_ShouldReturnInitialValuesWhenSoursAndTargetCurrenciesAreTheSame()
        {
            var service = new ConversionService(new NullLoggerFactory(), null);
            var (isSuccess, _, values, _) = await service.Convert(Currencies.USD, Currencies.USD, _values);

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
            rateServiceMock.Setup(m => m.Get(It.IsAny<Currencies>(), It.IsAny<Currencies>()))
                .ReturnsAsync(Result.Failure<decimal, ProblemDetails>(new ProblemDetails {Detail = details, Status = code}));

            var service = new ConversionService(new NullLoggerFactory(), rateServiceMock.Object);
            var (_, isFailure, _, error) = await service.Convert(Currencies.USD, Currencies.AED, _values);

            Assert.True(isFailure);
            Assert.Equal(code, error.Status);
            Assert.Equal(details, error.Detail);
        }


        [Fact]
        public async Task Convert_ShouldReturnValuesWhenSoursAndRateServiceReturnsRates()
        {
            const decimal rate = 100m;
            var rateServiceMock = new Mock<IRateService>();
            rateServiceMock.Setup(m => m.Get(It.IsAny<Currencies>(), It.IsAny<Currencies>()))
                .ReturnsAsync(Result.Ok<decimal, ProblemDetails>(rate));
            
            var service = new ConversionService(new NullLoggerFactory(), rateServiceMock.Object);
            var (isSuccess, _, values, _) = await service.Convert(Currencies.USD, Currencies.AED, _values);

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
            rateServiceMock.Setup(m => m.Get(It.IsAny<Currencies>(), It.IsAny<Currencies>()))
                .ReturnsAsync(Result.Ok<decimal, ProblemDetails>(rate));
            
            var service = new ConversionService(new NullLoggerFactory(), rateServiceMock.Object);
            var (isSuccess, _, values, _) = await service.Convert(Currencies.USD, Currencies.AED, _insaneValues);

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
