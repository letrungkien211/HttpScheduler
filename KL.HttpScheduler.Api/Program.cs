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
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
