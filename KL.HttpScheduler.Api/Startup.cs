using KL.HttpScheduler.Api.Common;
using KL.HttpScheduler.Api.Controllers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Examples;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
        private Config Config { get; set; }

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
            Config = Configuration.GetSection("Config").Get<Config>() ?? new Config();
            var appInsightsConfig = Configuration.GetSection("ApplicationInsights").Get<ApplicationInsightsConfig>() ?? new ApplicationInsightsConfig();

            services.AddHttpClient();
            services.AddHttpClient(MyExtensions.AppInsightsClientName, client =>
            {
                if (appInsightsConfig.IsValid())
                {
                    client.BaseAddress = appInsightsConfig.ApiUrl();
                    client.DefaultRequestHeaders.Add("x-api-key", appInsightsConfig.ApiKey);
                }
            });

            services.AddSingleton<ApplicationInsightsConfig>(appInsightsConfig);

            services.AddSingleton<IDatabase>(_ =>
            {
                return (ConnectionMultiplexer.Connect(Config.RedisConnectionString)).GetDatabase();
            });

            services.AddSingleton<SortedSetScheduleClient>(provider =>
            {
                return new SortedSetScheduleClient(
                        provider.GetService<MyActionBlock>(),
                        provider.GetService<IDatabase>(),
                        Config.SortedSetKey, Config.HashKey,
                        provider.GetService<TelemetryClient>()
                        );
            });

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddSingleton<IJobProcessor, HttpJobProcessor>();
            services.AddSingleton<JobProcessorWrapper>();
            services.AddSingleton<TelemetryClient>();
            services.AddSingleton<MyActionBlock>();

            services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, o) =>
            {
                if (!string.IsNullOrEmpty(appInsightsConfig.ApiKey))
                    module.AuthenticationApiKey = appInsightsConfig.ApiKey;
            });
            services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions() { EnableAdaptiveSampling = false });


            if (!Config.IsNotRunner)
            {
                services.AddSingleton<SchedulerRunner>();
            }

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Http Jobs Scheduler", Version = "v1" });
                c.DocumentFilter<BasePathDocumentFilter>(Config.SwaggerBasePath);
                c.OperationFilter<ExamplesOperationFilter>();

                var xmlFiles = new[] {
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml",
                    $"{typeof(HttpJob).Assembly.GetName().Name}.xml"
                };
                foreach (var xmlFile in xmlFiles)
                {
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                        c.IncludeXmlComments(xmlPath);
                }
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                    });

            if (Config.UnitTest)
            {
                services.AddSingleton<IJobProcessor, MockJobProcessor>();
            }
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
            IHostingEnvironment env,
            IApplicationLifetime applicationLifetime,
            ILogger<Startup> logger
            )
        {
            var telemetryConfig = app.ApplicationServices.GetService<TelemetryConfiguration>();
            telemetryConfig.TelemetryProcessorChainBuilder.Use(next => new ApplicationInsightsFilter(next));
            telemetryConfig.TelemetryProcessorChainBuilder.Build();

            if (!app.ApplicationServices.GetService<IDatabase>().IsConnected(""))
            {
                throw new TypeLoadException($"Redis server is not ready. Host={Config.RedisConnectionString}");
            }
            logger.LogInformation("Configure starts");

            app.ApplicationServices.GetService<SortedSetScheduleClient>();

            var actionBlock = app.ApplicationServices.GetService<MyActionBlock>();
            var manualEvent = new ManualResetEventSlim();

            if (!Config.IsNotRunner)
            {
                Task.Run(async () =>
                {
                    await app.ApplicationServices.GetService<SchedulerRunner>().RunAsync(applicationLifetime.ApplicationStopped).ConfigureAwait(false);
                    await actionBlock.CompleteAsync().ConfigureAwait(false);
                    manualEvent.Set();
                    logger.LogInformation("Background stopped");
                });

                applicationLifetime.ApplicationStopped.Register(() =>
                {
                    if (manualEvent.Wait(TimeSpan.FromSeconds(2)))
                    {
                        logger.LogInformation("Gracefully shutdown");
                    }
                    else
                    {
                        logger.LogError("Shutdown incorrectly");
                    }
                    telemetryConfig.TelemetryChannel.Flush();
                    Thread.Sleep(2000);
                });
            }
            else
            {
                applicationLifetime.ApplicationStopped.Register(() =>
                {
                    Task.Run(async () =>
                    {
                        await actionBlock.CompleteAsync().ConfigureAwait(false);
                        manualEvent.Set();
                        logger.LogInformation("Background stopped");
                    });

                    if (manualEvent.Wait(TimeSpan.FromSeconds(2)))
                    {
                        logger.LogInformation("Gracefully shutdown");
                    }
                    else
                    {
                        logger.LogError("Shutdown incorrectly");
                    }
                    telemetryConfig.TelemetryChannel.Flush();
                    Thread.Sleep(2000);
                });
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Http Jobs Scheduler");
            });

            app.UseLogsApiAvailabilityMiddleware();

            app.UseMvc();

            logger.LogInformation("Configure ends");
            telemetryConfig.TelemetryChannel.Flush();
        }
    }
}
