using System;
using System.Collections.Generic;
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
    public class ConversionServiceTests : IClassFixture<ConversionServiceTestsFixture>
    {
        public ConversionServiceTests(ConversionServiceTestsFixture fixture)
        {
            _fixture = fixture;
         }


        [Fact]
        public void ConversionService_should_return_error_when_logger_factory_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ConversionService(null!, null!, _fixture.Factory));
        }


        [Fact]
        public async Task Convert_should_return_error_when_source_currency_is_not_specified()
        {
            var service = new ConversionService(new NullLoggerFactory(), null!, _fixture.Factory);
            var (_, isFailure, _, error) = await service.Convert(Currencies.NotSpecified, Currencies.AED, 100m);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Convert_should_return_error_when_target_currency_is_not_specified()
        {
            var service = new ConversionService(new NullLoggerFactory(), null!, _fixture.Factory);
            var (_, isFailure, _, error) = await service.Convert(Currencies.USD, Currencies.NotSpecified, 100m);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Convert_should_return_error_when_values_are_null()
        {
            var service = new ConversionService(new NullLoggerFactory(), null!, _fixture.Factory);
            var (_, isFailure, _, error) = await service.Convert(Currencies.USD, Currencies.AED, null!);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Convert_should_return_error_when_values_are_empty()
        {
            var service = new ConversionService(new NullLoggerFactory(), null!, _fixture.Factory);
            var (_, isFailure, _, error) = await service.Convert(Currencies.USD, Currencies.AED, new List<decimal>(0));

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Convert_should_return_initial_values_when_source_and_target_currencies_are_the_same()
        {
            var rateServiceMock = new Mock<IRateService>();
            rateServiceMock.Setup(s => s.Get(It.IsAny<Currencies>(), It.IsAny<Currencies>()))
                .ReturnsAsync(Result.Success<decimal, ProblemDetails>(1));

            var service = new ConversionService(new NullLoggerFactory(), rateServiceMock.Object, _fixture.Factory);
            var (isSuccess, _, values, _) = await service.Convert(Currencies.USD, Currencies.USD, _values);

            Assert.True(isSuccess);
            Assert.Equal(_values.Count, values.Count);
            Assert.All(values, pair =>
            {
                var (k, v) = pair;
                Assert.Equal(k, v);
                Assert.Contains(k.Amount, _values);
            });
        }


        [Fact]
        public async Task Convert_should_return_problem_details_when_rate_service_returns_problem_details()
        {
            const int code = 499;
            const string details = "Error message";
            var rateServiceMock = new Mock<IRateService>();
            rateServiceMock.Setup(m => m.Get(It.IsAny<Currencies>(), It.IsAny<Currencies>()))
                .ReturnsAsync(Result.Failure<decimal, ProblemDetails>(new ProblemDetails {Detail = details, Status = code}));

            var service = new ConversionService(new NullLoggerFactory(), rateServiceMock.Object, _fixture.Factory);
            var (_, isFailure, _, error) = await service.Convert(Currencies.USD, Currencies.AED, _values);

            Assert.True(isFailure);
            Assert.Equal(code, error.Status);
            Assert.Equal(details, error.Detail);
        }


        [Fact]
        public async Task Convert_should_return_values_when_source_and_rate_service_returns_rates()
        {
            const decimal rate = 100m;
            var rateServiceMock = new Mock<IRateService>();
            rateServiceMock.Setup(m => m.Get(It.IsAny<Currencies>(), It.IsAny<Currencies>()))
                .ReturnsAsync(Result.Success<decimal, ProblemDetails>(rate));
            
            var service = new ConversionService(new NullLoggerFactory(), rateServiceMock.Object, _fixture.Factory);
            var (isSuccess, _, values, _) = await service.Convert(Currencies.USD, Currencies.AED, _values);

            Assert.True(isSuccess);
            Assert.Equal(_values.Count, values.Count);
            Assert.All(values, pair =>
            {
                var (k, v) = pair;
                Assert.Equal(k.Amount * rate, v.Amount);
                Assert.Contains(k.Amount, _values);
            });
        }


        private readonly ConversionServiceTestsFixture _fixture;
        private readonly List<decimal> _values = new List<decimal> {100m, 200m, 300m};
    }
}
