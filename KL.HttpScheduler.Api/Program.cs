using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("KL.HttpScheduler.Tests")]

namespace KL.HttpScheduler.Api
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.GetMaxThreads(out var maxw, out var maxp);
            ThreadPool.GetMinThreads(out var minw, out var minp);

            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                    builder.AddFilter<ApplicationInsightsLoggerProvider>((name, level) =>
                    {
                        return level >= LogLevel.Information && name.Contains(typeof(HttpJob).Namespace);
                    });
                })
                .UseStartup<Startup>();
    }
}
