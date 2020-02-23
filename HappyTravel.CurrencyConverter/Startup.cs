using System;
using System.Net.Http;
using CacheFlow.Json.Extensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.CurrencyConverter.Conventions.Serialization;
using HappyTravel.CurrencyConverter.Data;
using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.CurrencyConverter.Infrastructure.Constants;
using HappyTravel.CurrencyConverter.Infrastructure.Environments;
using HappyTravel.CurrencyConverter.Services;
using HappyTravel.VaultClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;

namespace HappyTravel.CurrencyConverter
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _environment = environment;
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            var serializationSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCaseExceptDictionaryKeysResolver(),
                Formatting = Formatting.None
            };
            JsonConvert.DefaultSettings = () => serializationSettings;

            using var vaultClient = GetVaultClient();
            vaultClient.Login(EnvironmentVariableHelper.Get("Vault:Token", Configuration)).Wait();

            AddDatabase(services, vaultClient);

            var currencyLayerOptions = vaultClient.Get(Configuration["CurrencyLayer:Options"]).Result;
            services.Configure<CurrencyLayerOptions>(options => { options.ApiKey = currencyLayerOptions["apiKey"]; });
            services.Configure<FlowOptions>(options =>
            {
                options.DistributedToMemoryExpirationRatio = 1.0;
                options.SkipRetryInterval = TimeSpan.FromMilliseconds(200);
                options.SuppressCacheExceptions = false;
            });

            services.AddHttpClient(HttpClientNames.CurrencyLayer, client => { client.BaseAddress = new Uri("http://apilayer.net/api/"); })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetDefaultRetryPolicy());

            services.AddTransient<IRateService, RateService>();
            services.AddTransient<IConversionService, ConversionService>();

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddHealthChecks();
            services.AddMemoryCache()
                .AddStackExchangeRedisCache(options => { options.Configuration = GetRedisConfiguration(); })
                .AddDoubleFlow()
                .AddCashFlowJsonSerialization()
                .AddControllers();
        }


        private void AddDatabase(IServiceCollection services, IVaultClient vaultClient)
        {
            var databaseOptions = vaultClient.Get(Configuration["Database:Options"]).Result;
            services.AddDbContextPool<CurrencyConverterContext>(options =>
            {
                var host = databaseOptions["host"];
                var port = databaseOptions["port"];
                var password = databaseOptions["password"];
                var userId = databaseOptions["userId"];

                var connectionString = Configuration.GetConnectionString("Tsutsujigasaki");
                options.UseNpgsql(string.Format(connectionString, host, port, userId, password), builder => { builder.EnableRetryOnFailure(); });
                options.EnableSensitiveDataLogging(false);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }, 16);
        }


        private VaultClient.VaultClient GetVaultClient()
        {
            var vaultOptions = new VaultOptions
            {
                BaseUrl = new Uri(EnvironmentVariableHelper.Get("Vault:Endpoint", Configuration)),
                Engine = Configuration["Vault:Engine"],
                Role = Configuration["Vault:Role"]
            };

            return new VaultClient.VaultClient(vaultOptions, new NullLoggerFactory());
        }


        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseHealthChecks("/health");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


        public IConfiguration Configuration { get; }


        private static IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy()
        {
            var jitter = new Random();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt
                    => TimeSpan.FromMilliseconds(Math.Pow(500, attempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 100)));
        }


        private string GetRedisConfiguration()
        {
            if (_environment.IsLocal())
                return "localhost";

            return "redis-master";
        }


        private readonly IWebHostEnvironment _environment;
    }
}
