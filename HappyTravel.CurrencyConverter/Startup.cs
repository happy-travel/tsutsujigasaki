using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using CacheFlow.MessagePack.Extensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.CurrencyConverter.Conventions.Serialization;
using HappyTravel.CurrencyConverter.Data;
using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.CurrencyConverter.Infrastructure.Constants;
using HappyTravel.CurrencyConverter.Infrastructure.Environments;
using HappyTravel.CurrencyConverter.Services;
using HappyTravel.ErrorHandling.Extensions;
using HappyTravel.VaultClient;
using MessagePack;
using MessagePack.CSharpFunctionalExtensions;
using MessagePack.ProblemDetails;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Samplers;
using Polly;
using Polly.Extensions.Http;

namespace HappyTravel.CurrencyConverter
{
    public class Startup
    {
        private readonly IWebHostEnvironment _environment;


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

            var messagePackOptions = MessagePackSerializerOptions.Standard
                .WithCompression(MessagePackCompression.Lz4BlockArray);

            services.AddMemoryCache()
                .AddStackExchangeRedisCache(options => { options.Configuration = EnvironmentVariableHelper.Get("Redis:Endpoint", Configuration); })
                .AddDoubleFlow()
                .AddCacheFlowMessagePackSerialization(messagePackOptions, StandardResolver.Instance, NativeDecimalResolver.Instance, 
                    CSharpFunctionalExtensionsFormatResolver.Instance, ProblemDetailsFormatResolver.Instance)
                .AddControllers()
                .AddControllersAsServices();

            services.AddTransient<IRateService, RateService>();
            services.AddTransient<IConversionService, ConversionService>();
            services.AddProblemDetailsFactory();

            services = AddTracing(services, _environment, Configuration);
            
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


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<Startup>();
            app.UseProblemDetailsExceptionHandler(env, logger);

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
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
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


        private IServiceCollection AddTracing(IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
        {
            string agentHost;
            int agentPort;
            if (environment.IsLocal())
            {
                agentHost = configuration["Jaeger:AgentHost"];
                agentPort = int.Parse(configuration["Jaeger:AgentPort"]);
            }
            else
            {
                agentHost = EnvironmentVariableHelper.Get("Jaeger:AgentHost", configuration);
                agentPort = int.Parse(EnvironmentVariableHelper.Get("Jaeger:AgentPort", configuration));
            }
            
            var serviceName = $"{environment.ApplicationName}-{environment.EnvironmentName}";
            services.AddOpenTelemetry(builder =>
            {
                builder.UseJaeger(options =>
                    {
                        options.ServiceName = serviceName;
                        options.AgentHost = agentHost;
                        options.AgentPort = agentPort;
                    })
                    .AddRequestInstrumentation()
                    .AddDependencyInstrumentation()
                    .AddCacheFlowInstrumentation()
                    .SetResource(Resources.CreateServiceResource(serviceName))
                    .SetSampler(new AlwaysOnSampler());
            });

            return services;
        }


        private static IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy()
        {
            var jitter = new Random();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt
                    => TimeSpan.FromMilliseconds(Math.Pow(500, attempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 100)));
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
    }
}
