using System;
using HappyTravel.CurrencyConverterApi.Infrastructure.Constants;
using HappyTravel.CurrencyConverterApi.Infrastructure.Environments;
using HappyTravel.Diplomat.ConfigurationProvider.Extensions;
using HappyTravel.StdOutLogger.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HappyTravel.CurrencyConverterApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }


        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel()
                        .UseStartup<Startup>()
                        .ConfigureAppConfiguration((context, builder) =>
                        {
                            var environment = context.HostingEnvironment;
                            var environmentName = environment.EnvironmentName;

                            builder.AddJsonFile("appsettings.json", false, true)
                                .AddJsonFile($"appsettings.{environmentName}.json", true, true);
                            builder.AddDiplomat(
                                Environment.GetEnvironmentVariable("CONSUL_HTTP_ADDR"), 
                                Environment.GetEnvironmentVariable("CONSUL_PATH") + "/" + environmentName,
                               Environment.GetEnvironmentVariable("CONSUL_HTTP_TOKEN")
                                );
                            builder.AddEnvironmentVariables();
                        })
                        .ConfigureLogging((context, builder) =>
                        {
                            builder.ClearProviders()
                                .AddConfiguration(context.Configuration.GetSection("Logging"));

                            var env = context.HostingEnvironment;
                            if (env.IsLocal())
                            {
                                builder.AddConsole();
                            }
                            else
                            {
                                builder.AddStdOutLogger(setup =>
                                {
                                    setup.IncludeScopes = false;
                                    setup.RequestIdHeader = Common.RequestIdHeader;
                                    setup.UseUtcTimestamp = true;
                                });
                                builder.AddSentry(c =>
                                {
                                    c.Dsn = EnvironmentVariableHelper.Get("Logging:Sentry:Endpoint", context.Configuration);
                                    c.Environment = env.EnvironmentName;
                                });
                            }
                        });
                });
    }
}