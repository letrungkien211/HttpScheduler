using KL.HttpScheduler.Api.Common;
using KL.HttpScheduler.Api.Health;
using KL.HttpScheduler.Api.Logging;
using KL.HttpScheduler.Api.Swagger;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Net.Http;
using System.Threading;

namespace KL.HttpScheduler.Api
{
    internal static class MyExtensions
    {
        public static string AppInsightsClientName = "AppInsights";
        public static HttpClient CreateAppInsightsClient(this IHttpClientFactory httpClientFactory)
        {
            return httpClientFactory.CreateClient(AppInsightsClientName);
        }
    }

    /// <summary>
    /// Start up
    /// </summary>
    public class Startup
    {
        //private Config Config { get; set; }

        /// <summary>
        /// Instructor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="services"></param>
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var actionBlockOptions = Configuration.GetSection("ActionBlock").Get<MyActionBlockOptions>();
            // https://stackoverflow.com/questions/46834697/threadpool-setminthreads-the-impact-of-setting-it
            ThreadPool.GetMinThreads(out var minw, out var minp);
            ThreadPool.SetMinThreads(minw, actionBlockOptions.MaxConcurrentTasksPerProcessor * Environment.ProcessorCount);

            // Set up configuration
            var config = Configuration.GetSection("Config").Get<Config>() ?? new Config();
            var appInsightsConfig = Configuration.GetSection("ApplicationInsights").Get<ApplicationInsightsConfig>() ?? new ApplicationInsightsConfig();
            services.Configure<MyActionBlockOptions>(Configuration.GetSection("ActionBlock"));
            services.AddSingleton(config);

            // Configure servies
            services.AddRedis(config)
                    .AddApplicationInsights(appInsightsConfig)
                    .AddHttpJobProcessor(config)
                    .AddMyMvc()
                    .AddMySwagger()
                    .AddMyHealthChecks(config)
                    .AddForUnitTests(config.UnitTest);
        }

        /// <summary>
        /// Configure 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="applicationLifetime"></param>
        /// <param name="logger"></param>
        public void Configure(
            IApplicationBuilder app,
            IHostEnvironment env,
            IHostApplicationLifetime applicationLifetime,
            ILogger<Startup> logger)
        {
            if (!app.ApplicationServices.GetService<IDatabase>().IsConnected(""))
            {
                throw new TypeLoadException($"Redis server is not ready.");
            }
            logger.LogInformation("Configure starts");

            app.ApplicationServices.GetService<SortedSetScheduleClient>();
            app.ApplicationServices.GetService<MyActionBlock>();
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                app.ApplicationServices.GetRequiredService<TelemetryConfiguration>().TelemetryChannel.Flush();
                Thread.Sleep(2000);
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseLogsApiAvailabilityMiddleware();
            app.UseMySwagger();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapCustomHealthChecks();
            });

            logger.LogInformation("Configure ends");
        }
    }
}
