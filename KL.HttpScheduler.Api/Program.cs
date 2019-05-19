using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("KL.HttpScheduler.Tests")]

namespace KL.HttpScheduler.Api
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddConsole();
                    builder.AddApplicationInsights(hostingContext.Configuration["ApplicationInsights:InstrumentationKey"] ?? "");
                    builder.AddFilter<ApplicationInsightsLoggerProvider>(typeof(JobProcessorWrapper).FullName, LogLevel.Information);
                    builder.AddFilter<ApplicationInsightsLoggerProvider>(typeof(SchedulerRunner).FullName, LogLevel.Information);
                    builder.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Startup).FullName, LogLevel.Information);
                })
                .UseStartup<Startup>();
    }
}
