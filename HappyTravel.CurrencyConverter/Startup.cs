using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
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
using Microsoft.OpenApi.Models;
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

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddHealthChecks()
                .AddDbContextCheck<CurrencyConverterContext>()
                .AddCheck<ControllerResolveHealthCheck>(nameof(ControllerResolveHealthCheck));

            services.AddMemoryCache()
                .AddStackExchangeRedisCache(options => { options.Configuration = GetRedisConfiguration(); })
                .AddDoubleFlow()
                .AddCashFlowJsonSerialization()
                .AddControllers();

            services.AddTransient<IRateService, RateService>();
            services.AddTransient<IConversionService, ConversionService>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1.0", new OpenApiInfo {Title = "HappyTravel.com Currency Converter API", Version = "v1.0" });

                var apiXmlCommentsFilePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                options.IncludeXmlComments(apiXmlCommentsFilePath);

                foreach (var assembly in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                {
                    var path = Path.Combine(AppContext.BaseDirectory, $"{assembly.Name}.xml");
                    if (File.Exists(path))
                        options.IncludeXmlComments(path);
                }
            });
        }


        public void Configure(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "HappyTravel.com Currency Converter API");
                options.RoutePrefix = string.Empty;
            });


            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
        }


        public IConfiguration Configuration { get; }


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


        private static IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy()
        {
            var jitter = new Random();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt
                    => TimeSpan.FromMilliseconds(Math.Pow(500, attempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 100)));
        }


        private string GetRedisConfiguration() 
            => _environment.IsLocal() ? "localhost" : "redis-master";


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


        private readonly IWebHostEnvironment _environment;
    }
}
