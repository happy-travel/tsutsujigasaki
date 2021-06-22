using System;
using HappyTravel.ConsulKeyValueClient.ConfigurationProvider.Extensions;
using HappyTravel.Diplomat.ConfigurationProvider.Extensions;
using HappyTravel.Tsutsujigasaki.Api.Infrastructure.Constants;
using HappyTravel.Tsutsujigasaki.Api.Infrastructure.Environments;
using HappyTravel.StdOutLogger.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Tsutsujigasaki.Api
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
                            builder.AddEnvironmentVariables();
                            builder.AddConsulKeyValueClient(Environment.GetEnvironmentVariable("CONSUL_HTTP_ADDR") ?? throw new InvalidOperationException("Consul endpoint is not set"),
                                "tsutsujigasaki",
                                Environment.GetEnvironmentVariable("CONSUL_HTTP_TOKEN") ?? throw new InvalidOperationException("Consul http token is not set"));
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
                                    setup.IncludeScopes = true;
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