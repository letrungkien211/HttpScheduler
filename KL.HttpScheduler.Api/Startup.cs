using KL.HttpScheduler.Api.Common;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler.Api
{
    public class Startup
    {
        private Config Config { get; set; }
        private ILogger<Startup> Logger { get; }
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public IConfiguration Configuration { get; }

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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

            applicationLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    await schedulerRunner.RunAsync(applicationLifetime.ApplicationStopped).ConfigureAwait(false);
                    await actionBlock.CompleteAsync().ConfigureAwait(false);
                    manualEvent.Set();
                    Logger.LogInformation("Background stopped");
                });
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
        }
    }
}
