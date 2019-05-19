using KL.HttpScheduler.Api.Common;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler.Api
{
    /// <summary>
    /// Start up
    /// </summary>
    public class Startup
    {
        private Config Config { get; set; }
        private ILogger<Startup> Logger { get; }

        /// <summary>
        /// Instructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        private IConfiguration Configuration { get; }

        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="services"></param>
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Logger.LogInformation("ConfigureServices starts");
            Config = Configuration.GetSection("Config").Get<Config>() ?? new Config();

            services.AddHttpClient();

            services.AddSingleton<IDatabase>(_ =>
            {
                return (ConnectionMultiplexer.Connect(Config.RedisConnectionString)).GetDatabase();
            });

            services.AddSingleton<SortedSetScheduleClient>(provider =>
            {
                return new SortedSetScheduleClient(provider.GetService<IDatabase>(), Config.SortedSetKey, Config.HashKey);
            });

            services.AddSingleton<IJobProcessor, JobProcessor>();
            services.AddSingleton<JobProcessorWrapper>();
            services.AddSingleton<TelemetryClient>();
            services.AddSingleton<MyActionBlock>();

            services.AddSingleton<SchedulerRunner>();

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
                foreach (var xmlFile in xmlFiles) {
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
            Logger.LogInformation("ConfigureServices ends");
        }

        /// <summary>
        /// Configure 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="applicationLifetime"></param>
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            IApplicationLifetime applicationLifetime
            )
        {
            if (!app.ApplicationServices.GetService<IDatabase>().IsConnected(""))
            {
                throw new TypeLoadException($"Redis server is not ready. Host={Config.RedisConnectionString}");
            }
            Logger.LogInformation("Configure starts");

            app.ApplicationServices.GetService<SortedSetScheduleClient>();
            var schedulerRunner = app.ApplicationServices.GetService<SchedulerRunner>();
            var manualEvent = new ManualResetEventSlim();

            var actionBlock = app.ApplicationServices.GetService<MyActionBlock>();

            Task.Run(async () =>
            {
                await schedulerRunner.RunAsync(applicationLifetime.ApplicationStopped).ConfigureAwait(false);
                await actionBlock.CompleteAsync().ConfigureAwait(false);
                manualEvent.Set();
                Logger.LogInformation("Background stopped");
            });

            applicationLifetime.ApplicationStopped.Register(() =>
            {
                if (manualEvent.Wait(TimeSpan.FromSeconds(2)))
                {
                    Logger.LogInformation("Gracefully shutdown");
                }
                else
                {
                    Logger.LogError("Shutdown incorrectly");
                }
                TelemetryConfiguration.Active.TelemetryChannel.Flush();
                Thread.Sleep(2000);
            });

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

            app.UseMvc();

            Logger.LogInformation("Configure ends");
            TelemetryConfiguration.Active.TelemetryChannel.Flush();
        }
    }
}
