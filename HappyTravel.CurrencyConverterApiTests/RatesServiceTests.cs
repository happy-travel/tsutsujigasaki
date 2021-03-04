using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using HappyTravel.CurrencyConverterApi.Data;
using HappyTravel.CurrencyConverterApi.Infrastructure;
using HappyTravel.CurrencyConverterApi.Services;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace HappyTravel.CurrencyConverterApiTests
{
    public class RatesServiceTests
    {
        [Fact]
        public void RatesService_should_throw_exception_when_logger_factory_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new RateService(null!, null!, null!, null!, null!));
        }


        [Fact]
        public void RatesService_should_throw_exception_when_options_are_Null()
        {
            Assert.Throws<NullReferenceException>(
                () => new RateService(new NullLoggerFactory(), null!, null!, null!, null!));
        }


        [Fact]
        public async Task Get_should_return_error_when_source_currency_is_not_specified()
        {
            var service = new RateService(new NullLoggerFactory(), null!, null!, _options, null!);
            var (_, isFailure, _, error) = await service.Get(Currencies.NotSpecified, Currencies.AED);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Get_should_return_error_when_target_currency_is_not_specified()
        {
            var service = new RateService(new NullLoggerFactory(), null!, null!, _options, null!);
            var (_, isFailure, _, error) = await service.Get(Currencies.USD, Currencies.NotSpecified);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Get_should_return_one_when_source_and_target_currencies_are_the_same()
        {
            var service = new RateService(new NullLoggerFactory(), null!, null!, _options, null!);
            var (isSuccess, _, value, _) = await service.Get(Currencies.USD, Currencies.USD);

            Assert.True(isSuccess);
            Assert.Equal(1, value);
        }


        [Fact]
        public async Task Get_should_throw_exception_when_cache_is_null()
        {
            var service = new RateService(new NullLoggerFactory(), null!, null!, _options, null!);

            await Assert.ThrowsAsync<NullReferenceException>(async ()
                => await service.Get(Currencies.USD, Currencies.AED));
        }


        [Fact]
        public async Task Get_should_throw_exception_when_client_factory_is_null()
        {
            var service = new RateService(new NullLoggerFactory(), GetCache(), null!, _options, null!);

            await Assert.ThrowsAsync<NullReferenceException>(async ()
                => await service.Get(Currencies.USD, Currencies.AED));
        }


        [Fact]
        public async Task Get_should_throw_exception_when_http_client_throws_exception()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new NetworkInformationException(-1))
                .Verifiable();
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://example.com/")
            };
            var clientFactoryMock = new Mock<IHttpClientFactory>();
            clientFactoryMock.Setup(m => m.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var service = new RateService(new NullLoggerFactory(), GetCache(), clientFactoryMock.Object, _options,
                null!);

            await Assert.ThrowsAsync<NetworkInformationException>(async ()
                => await service.Get(Currencies.USD, Currencies.AED));
        }


        [Fact]
        public async Task Get_should_return_problem_details_when_response_is_not_successful()
        {
            const HttpStatusCode status = HttpStatusCode.BadRequest;

            var service = new RateService(new NullLoggerFactory(), GetCache(),
                GetHttpClientFactory(new HttpResponseMessage(status)), _options, null!);
            var (_, isFailure, _, error) = await service.Get(Currencies.USD, Currencies.AED);

            Assert.True(isFailure);
            Assert.Equal((int) status, error.Status);
        }


        [Fact]
        public async Task Get_should_return_problem_details_when_content_is_not_currency_layer_response()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("some string")
            };

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(response), _options,
                null!);

            await Assert.ThrowsAsync<JsonReaderException>(async ()
                => await service.Get(Currencies.USD, Currencies.AED));
        }


        [Fact]
        public async Task Get_should_return_problem_details_when_rates_are_null()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    @"{
                        ""success"": true,
                        ""terms"": ""https://currencylayer.com/terms"",
                        ""privacy"": ""https://currencylayer.com/privacy"",
                        ""timestamp"": 1430401802,
                        ""source"": ""USD"",
                        ""quotes"": null
                    }")
            };

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(response), _options,
                null!);
            var (_, isFailure, _, error) = await service.Get(Currencies.USD, Currencies.AED);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Get_should_return_problem_details_when_rates_are_empty()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    @"{
                        ""success"": true,
                        ""terms"": ""https://currencylayer.com/terms"",
                        ""privacy"": ""https://currencylayer.com/privacy"",
                        ""timestamp"": 1430401802,
                        ""source"": ""USD"",
                        ""quotes"": {}
                    }")
            };

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(response), _options,
                null!);
            var (_, isFailure, _, error) = await service.Get(Currencies.USD, Currencies.AED);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Get_should_throw_exception_when_context_is_null()
        {
            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options, null!);

            await Assert.ThrowsAsync<NullReferenceException>(async ()
                => await service.Get(Currencies.USD, Currencies.AED));
        }


        [Fact]
        public async Task Get_should_return_value()
        {
            var currencyRatesMock = new List<CurrencyRate>().AsQueryable().BuildMockDbSet();
            var defaultCurrencyRatesMock = new List<DefaultCurrencyRate>().AsQueryable().BuildMockDbSet();

            var contextMock = new Mock<CurrencyConverterContext>();
            contextMock.Setup(m => m.CurrencyRates)
                .Returns(currencyRatesMock.Object);
            contextMock.Setup(m => m.DefaultCurrencyRates)
                .Returns(defaultCurrencyRatesMock.Object);
            contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options,
                contextMock.Object);
            var (isSuccess, _, _) = await service.Get(Currencies.USD, Currencies.AED);

            Assert.True(isSuccess);
        }


        [Fact]
        public async Task Get_should_return_problem_details_when_pair_not_in_the_returned_list_and_not_in_the_database()
        {
            var currencyRatesMock = new List<CurrencyRate>().AsQueryable().BuildMockDbSet();
            var defaultCurrencyRatesMock = new List<DefaultCurrencyRate>().AsQueryable().BuildMockDbSet();

            var contextMock = new Mock<CurrencyConverterContext>();
            contextMock.Setup(m => m.CurrencyRates)
                .Returns(currencyRatesMock.Object);
            contextMock.Setup(m => m.DefaultCurrencyRates)
                .Returns(defaultCurrencyRatesMock.Object);
            contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options,
                contextMock.Object);
            var (_, isFailure, _, error) = await service.Get(Currencies.USD, Currencies.NotSpecified);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Get_should_return_value_when_pair_not_in_the_returned_list()
        {
            const decimal value = 12.3456m;
            var currencyRatesMock = new List<CurrencyRate>
            {
                new CurrencyRate {Rate = value, Source = "USD", Target = "SAR", ValidFrom = DateTime.Now}
            }.AsQueryable().BuildMockDbSet();
            var defaultCurrencyRatesMock = new List<DefaultCurrencyRate>().AsQueryable().BuildMockDbSet();

            var contextMock = new Mock<CurrencyConverterContext>();
            contextMock.Setup(m => m.CurrencyRates)
                .Returns(currencyRatesMock.Object);
            contextMock.Setup(m => m.DefaultCurrencyRates)
                .Returns(defaultCurrencyRatesMock.Object);
            contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options,
                contextMock.Object);
            var (isSuccess, _, returnedValue) = await service.Get(Currencies.USD, Currencies.SAR);

            Assert.True(isSuccess);
            Assert.Equal(value, returnedValue);
        }


        [Fact]
        public async Task Get_should_return_the_last_value_when_pair_not_in_the_returned_list()
        {
            const decimal value = 12.3456m;
            var time = DateTime.UtcNow;
            var currencyRatesMock = new List<CurrencyRate>
            {
                new CurrencyRate {Rate = value, Source = "USD", Target = "SAR", ValidFrom = time},
                new CurrencyRate {Rate = value - 10m, Source = "USD", Target = "SAR", ValidFrom = time.AddMinutes(-10)}
            }.AsQueryable().BuildMockDbSet();
            var defaultCurrencyRatesMock = new List<DefaultCurrencyRate>().AsQueryable().BuildMockDbSet();

            var contextMock = new Mock<CurrencyConverterContext>();
            contextMock.Setup(m => m.CurrencyRates)
                .Returns(currencyRatesMock.Object);
            contextMock.Setup(m => m.DefaultCurrencyRates)
                .Returns(defaultCurrencyRatesMock.Object);
            contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options,
                contextMock.Object);
            var (isSuccess, _, returnedValue) = await service.Get(Currencies.USD, Currencies.SAR);

            Assert.True(isSuccess);
            Assert.Equal(value, returnedValue);
        }


        [Fact]
        public async Task Get_should_return_the_last_value_within_a_limited_time_when_pair_not_in_the_returned_list()
        {
            const decimal value = 12.3456m;
            var time = DateTime.UtcNow;
            var currencyRatesMock = new List<CurrencyRate>
            {
                new CurrencyRate {Rate = value, Source = "USD", Target = "SAR", ValidFrom = time.AddDays(-2)},
                new CurrencyRate {Rate = value, Source = "USD", Target = "SAR", ValidFrom = time},
                new CurrencyRate
                    {Rate = value - 10m, Source = "USD", Target = "SAR", ValidFrom = time.AddDays(-2).AddMinutes(-10)}
            }.AsQueryable().BuildMockDbSet();
            var defaultCurrencyRatesMock = new List<DefaultCurrencyRate>().AsQueryable().BuildMockDbSet();

            var contextMock = new Mock<CurrencyConverterContext>();
            contextMock.Setup(m => m.CurrencyRates)
                .Returns(currencyRatesMock.Object);
            contextMock.Setup(m => m.DefaultCurrencyRates)
                .Returns(defaultCurrencyRatesMock.Object);
            contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options,
                contextMock.Object);
            var (isSuccess, _, returnedValue) = await service.Get(Currencies.USD, Currencies.SAR);

            Assert.True(isSuccess);
            Assert.Equal(value, returnedValue);
        }


        [Fact]
        public async Task Get_should_return_default_rate_and_log_correction()
        {
            var defaultRate = 3.668m;
            var correction = 3.672982m - defaultRate;

            var currenciesList = new List<CurrencyRate>
            {
                new CurrencyRate {Rate = 4, Source = "USD", Target = "AED", ValidFrom = DateTime.UtcNow}
            };
            var currencyRatesMock = currenciesList.AsQueryable().BuildMockDbSet();

            var defaultCurrencyRatesMock = new List<DefaultCurrencyRate>
            {
                new DefaultCurrencyRate {Rate = defaultRate, Source = "USD", Target = "AED", ValidFrom = DateTime.UtcNow}
            }.AsQueryable().BuildMockDbSet();

            var contextMock = new Mock<CurrencyConverterContext>();
            contextMock.Setup(m => m.CurrencyRates)
                .Returns(currencyRatesMock.Object);
            contextMock.Setup(m => m.DefaultCurrencyRates)
                .Returns(defaultCurrencyRatesMock.Object);
            currencyRatesMock.Setup(d => d.AddRange(It.IsAny<IEnumerable<CurrencyRate>>()))
                .Callback<IEnumerable<CurrencyRate>>(currenciesList.AddRange);
            contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options,
                contextMock.Object);
            var (isSuccess, _, returnedValue) = await service.Get(Currencies.USD, Currencies.AED);
            var lastLoggedRate = contextMock.Object.CurrencyRates.ToList()
                .Where(r => r.Source == "USD" && r.Target == "AED")
                .OrderByDescending(r => r.ValidFrom).First();

            Assert.True(isSuccess);
            Assert.Equal(defaultRate, returnedValue);
            Assert.Equal(lastLoggedRate.RateCorrection, correction);
        }


        [Fact]
        public async Task Get_should_return_latest_default_rate()
        {
            var defaultRate = 3.668m;

            var currenciesList = new List<CurrencyRate>();
            var currencyRatesMock = currenciesList.AsQueryable().BuildMockDbSet();

            var defaultCurrencyRatesMock = new List<DefaultCurrencyRate>
            {
                new DefaultCurrencyRate {Rate = defaultRate + 0.1m, Source = "USD", Target = "AED", ValidFrom = DateTime.UtcNow.AddMinutes(-10)},
                new DefaultCurrencyRate {Rate = defaultRate, Source = "USD", Target = "AED", ValidFrom = DateTime.UtcNow}
            }.AsQueryable().BuildMockDbSet();

            var contextMock = new Mock<CurrencyConverterContext>();
            contextMock.Setup(m => m.CurrencyRates)
                .Returns(currencyRatesMock.Object);
            contextMock.Setup(m => m.DefaultCurrencyRates)
                .Returns(defaultCurrencyRatesMock.Object);
            currencyRatesMock.Setup(d => d.AddRange(It.IsAny<IEnumerable<CurrencyRate>>()))
                .Callback<IEnumerable<CurrencyRate>>(currenciesList.AddRange);
            contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options,
                contextMock.Object);

            var (isSuccess, _, returnedValue) = await service.Get(Currencies.USD, Currencies.AED);

            Assert.Equal(defaultRate, returnedValue);
        }


        private static IDoubleFlow GetCache()
        {
            var cacheMock = new Mock<IDoubleFlow>();
            cacheMock.Setup(m => m.Options)
                .Returns(new FlowOptions {CacheKeyDelimiter = "::"});
            cacheMock.Setup(m => m.GetOrSetAsync(It.IsAny<string>(),
                    It.IsAny<Func<Task<Result<decimal, ProblemDetails>>>>(), It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .Returns<string, Func<Task<Result<decimal, ProblemDetails>>>, TimeSpan, CancellationToken>((_, func, _1,
                    _2) => func());

            return cacheMock.Object;
        }


        private static IHttpClientFactory GetHttpClientFactory()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    @"{
                        ""success"": true,
                        ""terms"": ""https://currencylayer.com/terms"",
                        ""privacy"": ""https://currencylayer.com/privacy"",
                        ""timestamp"": 1430401802,
                        ""source"": ""USD"",
                        ""quotes"": {
                            ""USDAED"": 3.672982,
                            ""USDAFN"": 57.8936,
                            ""USDALL"": 126.1652,
                            ""USDAMD"": 475.306,
                            ""USDANG"": 1.78952,
                            ""USDAOA"": 109.216875,
                            ""USDARS"": 8.901966,
                            ""USDAUD"": 1.269072,
                            ""USDAWG"": 1.792375,
                            ""USDAZN"": 1.04945,
                            ""USDBAM"": 1.757305,
                        }
                    }")
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response)
                .Verifiable();
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://example.com/")
            };
            var clientFactoryMock = new Mock<IHttpClientFactory>();
            clientFactoryMock.Setup(m => m.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            return clientFactoryMock.Object;
        }


        private static IHttpClientFactory GetHttpClientFactory(HttpResponseMessage response)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response)
                .Verifiable();
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://example.com/")
            };
            var clientFactoryMock = new Mock<IHttpClientFactory>();
            clientFactoryMock.Setup(m => m.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            return clientFactoryMock.Object;
        }


        private readonly IOptions<CurrencyLayerOptions> _options = Options.Create(new CurrencyLayerOptions
        {
            ApiKey = "acab"
        });
    }
}