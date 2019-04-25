using KL.HttpScheduler.Api.Common;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace KL.HttpScheduler.Api
{
    public class Startup
    {
        public static bool UnitTest = false;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = Configuration.GetSection("Config").Get<Config>() ?? new Config();

            services.AddHttpClient();

            services.AddSingleton<IDatabase>(_ =>
            {
                return (ConnectionMultiplexer.Connect(config.RedisConnectionString)).GetDatabase();
            });

            services.AddSingleton<SortedSetScheduleClient>(provider =>
            {
                return new SortedSetScheduleClient(provider.GetService<IDatabase>(), config.SortedSetKey, config.HashKey);
            });

            services.AddSingleton<IJobProcessor, JobProcessor>();
            services.AddSingleton<JobProcessorWrapper>();
            services.AddSingleton<TelemetryClient>();

            services.AddSingleton<ActionBlock<HttpJob>>(provider =>
            {
                var jobProcessor = provider.GetService<JobProcessorWrapper>();

                return new ActionBlock<HttpJob>((httpJob) =>
                {
                    return jobProcessor.ProcessAsync(httpJob);
                });
            });

            if (UnitTest)
            {
                services.AddSingleton<SchedulerRunner>();
                services.AddSingleton<IJobProcessor, MockJobProcessor>();
            }

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            app.ApplicationServices.GetService<SortedSetScheduleClient>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
