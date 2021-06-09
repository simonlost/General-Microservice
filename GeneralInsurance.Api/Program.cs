using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GeneralInsurance.Api.Middleware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace GeneralInsurance.Api
{
    public class Program
    {
        public static string AppVersion => Environment.GetEnvironmentVariable("APP_VERISON") ?? "undefined";
        public static int Main(string[] args)
        {
            Console.WriteLine("Starting.....");
            Serilog.Debugging.Selflog.Enable(Console.Error);
            try
            {
                var host = BuildWebHost(args);
                Log.Information("Starting Web Host");
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Host terminated unexpectedly {ex}");
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
        
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UserHealthEndpoints()
                .UseSerilog((WebHostBuilderContext, loggerConfiguration) => 
                    loggerConfiguration
                        .ReadFrom.Configuration(WebHostBuilderContext.Configuration)
                        .Enrich.WithProperty("app-microservice-general-insurance")
                        .MinimumLevel.Information()
                        .MinimumLevelOverride("Microsoft", LogEventLevel.Warning)
                        .MinimumLevelOverride("System", LogEventLevel.Warning)
                        .MinimumLevelOverride("System.Net.Http", LogEventLevel.Information)
                        .Enrich.WithProperty("AppVersion", AppVersion)
                        .Enrich.WithMachineName()
                        .Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails())
                .Build();
        

    }
}
