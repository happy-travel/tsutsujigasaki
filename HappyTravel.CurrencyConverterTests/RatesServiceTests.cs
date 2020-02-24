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
using HappyTravel.CurrencyConverter.Data;
using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.CurrencyConverter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace HappyTravel.CurrencyConverterTests
{
    public class RatesServiceTests
    {
        [Fact]
        public void RatesService_ShouldThrowExceptionWhenLoggerFactoryIsnull()
        {
            Assert.Throws<ArgumentNullException>(() => new RateService(null, null, null, null, null));
        }


        [Fact]
        public void RatesService_ShouldThrowExceptionWhenOptionsAreNull()
        {
            Assert.Throws<NullReferenceException>(() => new RateService(new NullLoggerFactory(), null, null, null, null));
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\r")]
        public async Task Get_ShouldReturnErrorWhenSourceCurrencyNullOrEmpty(string sourceCurrency)
        {
            var service = new RateService(new NullLoggerFactory(), null, null, _options, null);
            var (_, isFailure, _, error) = await service.Get(sourceCurrency, "AED");

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\r")]
        public async Task Get_ShouldReturnErrorWhenTargetCurrencyNullOrEmpty(string targetCurrency)
        {
            var service = new RateService(new NullLoggerFactory(), null, null, _options, null);
            var (_, isFailure, _, error) = await service.Get("USD", targetCurrency);

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Get_ShouldThrowExceptionWhenCacheIsNull()
        {
            var service = new RateService(new NullLoggerFactory(), null, null, _options, null);

            await Assert.ThrowsAsync<NullReferenceException>(async () => await service.Get("USD", "AED"));
        }


        [Fact]
        public async Task Get_ShouldThrowExceptionWhenClientFactoryIsNull()
        {
            var service = new RateService(new NullLoggerFactory(), GetCache(), null, _options, null);

            await Assert.ThrowsAsync<NullReferenceException>(async () => await service.Get("USD", "AED"));
        }


        [Fact]
        public async Task Get_ShouldThrowExceptionWhenHttpClientThrowsException()
        {
           var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new NetworkInformationException(-1))
                .Verifiable();
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://example.com/")
            };
            var clientFactoryMock = new Mock<IHttpClientFactory>();
            clientFactoryMock.Setup(m => m.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var service = new RateService(new NullLoggerFactory(), GetCache(), clientFactoryMock.Object, _options, null);

            await Assert.ThrowsAsync<NetworkInformationException>(async () => await service.Get("USD", "AED"));
        }


        [Fact]
        public async Task Get_ShouldReturnProblemDetailsWhenResponseIsNotSuccessful()
        {
            const HttpStatusCode status = HttpStatusCode.BadRequest;

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(new HttpResponseMessage(status)), _options, null);
            var (_, isFailure, _, error) = await service.Get("USD", "AED");

            Assert.True(isFailure);
            Assert.Equal((int) status, error.Status);
        }


        [Fact]
        public async Task Get_ShouldReturnProblemDetailsWhenContentIsNotCurrencyLayerResponse()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("some string")
            };
            
            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(response), _options, null);

            await Assert.ThrowsAsync<JsonReaderException>(async () => await service.Get("USD", "AED"));
        }


        [Fact]
        public async Task Get_ShouldReturnProblemDetailsWhenRatesAreNull()
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
            
            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(response), _options, null);
            var (_, isFailure, _, error) = await service.Get("USD", "AED");

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Get_ShouldReturnProblemDetailsWhenRatesAreEmpty()
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
            
            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(response), _options, null);
            var (_, isFailure, _, error) = await service.Get("USD", "AED");

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Get_ShouldThrowExceptionWhenContextIsNull()
        {
            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options, null);

            await Assert.ThrowsAsync<NullReferenceException>(async () => await service.Get("USD", "AED"));
        }


        [Fact]
        public async Task Get_ShouldReturnValue()
        {
            var currencyRatesMock = new List<CurrencyRate>().AsQueryable().BuildMockDbSet();
            
            var contextMock = new Mock<CurrencyConverterContext>();
            contextMock.Setup(m => m.CurrencyRates)
                .Returns(currencyRatesMock.Object);
            contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options, contextMock.Object);
            var (isSuccess, _, returnedValue) = await service.Get("USD", "AED");

            Assert.True(isSuccess);
            //Because of serialization reasons
            Assert.Equal(3.672982m, returnedValue);
        }


        [Fact]
        public async Task Get_ShouldReturnProblemDetailsWhenPairNotInTheReturnedListAndNotInTheDatabase()
        {
            var currencyRatesMock = new List<CurrencyRate>().AsQueryable().BuildMockDbSet();
            
            var contextMock = new Mock<CurrencyConverterContext>();
            contextMock.Setup(m => m.CurrencyRates)
                .Returns(currencyRatesMock.Object);
            contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options, contextMock.Object);
            var (_, isFailure, _, error) = await service.Get("USD", "RUR");

            Assert.True(isFailure);
            Assert.Equal(400, error.Status);
        }


        [Fact]
        public async Task Get_ShouldReturnValueWhenPairNotInTheReturnedList()
        {
            const decimal value = 12.3456m;
            var currencyRatesMock = new List<CurrencyRate>
            {
                new CurrencyRate{Rate = value, Source = "USD", Target = "RUR", ValidFrom = DateTime.Now}
            }.AsQueryable().BuildMockDbSet();
            
            var contextMock = new Mock<CurrencyConverterContext>();
            contextMock.Setup(m => m.CurrencyRates)
                .Returns(currencyRatesMock.Object);
            contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options, contextMock.Object);
            var (isSuccess, _, returnedValue) = await service.Get("USD", "RUR");

            Assert.True(isSuccess);
            Assert.Equal(value, returnedValue);
        }


        [Fact]
        public async Task Get_ShouldReturnTheLastValueWhenPairNotInTheReturnedList()
        {
            const decimal value = 12.3456m;
            var time = DateTime.UtcNow;
            var currencyRatesMock = new List<CurrencyRate>
            {
                new CurrencyRate{Rate = value, Source = "USD", Target = "RUR", ValidFrom = time},
                new CurrencyRate{Rate = value - 10m, Source = "USD", Target = "RUR", ValidFrom = time.AddMinutes(-10)}
            }.AsQueryable().BuildMockDbSet();
            
            var contextMock = new Mock<CurrencyConverterContext>();
            contextMock.Setup(m => m.CurrencyRates)
                .Returns(currencyRatesMock.Object);
            contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options, contextMock.Object);
            var (isSuccess, _, returnedValue) = await service.Get("USD", "RUR");

            Assert.True(isSuccess);
            Assert.Equal(value, returnedValue);
        }


        [Fact]
        public async Task Get_ShouldReturnTheLastValueWithinALimitedTimeWhenPairNotInTheReturnedList()
        {
            const decimal value = 12.3456m;
            var time = DateTime.UtcNow;
            var currencyRatesMock = new List<CurrencyRate>
            {
                new CurrencyRate{Rate = value, Source = "USD", Target = "RUR", ValidFrom = time.AddDays(-2)},
                new CurrencyRate{Rate = value, Source = "USD", Target = "RUR", ValidFrom = time},
                new CurrencyRate{Rate = value - 10m, Source = "USD", Target = "RUR", ValidFrom = time.AddDays(-2).AddMinutes(-10)}
            }.AsQueryable().BuildMockDbSet();
            
            var contextMock = new Mock<CurrencyConverterContext>();
            contextMock.Setup(m => m.CurrencyRates)
                .Returns(currencyRatesMock.Object);
            contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new RateService(new NullLoggerFactory(), GetCache(), GetHttpClientFactory(), _options, contextMock.Object);
            var (isSuccess, _, returnedValue) = await service.Get("USD", "RUR");

            Assert.True(isSuccess);
            Assert.Equal(value, returnedValue);
        }


        private static IDoubleFlow GetCache()
        {
            var cacheMock = new Mock<IDoubleFlow>();
            cacheMock.Setup(m => m.Options)
                .Returns(new FlowOptions{CacheKeyDelimiter = "::"});
            cacheMock.Setup(m => m.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<Result<decimal, ProblemDetails>>>>(), It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
                .Returns<string, Func<Task<Result<decimal, ProblemDetails>>>, TimeSpan, CancellationToken>((_, func, _1, _2) => func());

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
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
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
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
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
