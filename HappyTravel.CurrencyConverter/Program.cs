using HappyTravel.CurrencyConverter.Infrastructure.Environments;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HappyTravel.CurrencyConverter
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

                            builder.AddJsonFile("appsettings.json", false, true)
                                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true);
                            builder.AddEnvironmentVariables();
                        })
                        .ConfigureLogging((context, builder) =>
                        {
                            builder.ClearProviders()
                                .AddConfiguration(context.Configuration.GetSection("Logging"));

                            var env = context.HostingEnvironment;
                            if (env.IsLocal())
                                builder.AddConsole();
                        });
                });
    }
}