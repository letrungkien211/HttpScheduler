using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
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
                    builder.AddDebug();
                    builder.AddApplicationInsights(hostingContext.Configuration["ApplicationInsights:InstrumentationKey"] ?? "");
                })
                .UseStartup<Startup>();
    }
}
