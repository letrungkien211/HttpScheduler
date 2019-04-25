using KL.HttpScheduler.Api.Common;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace KL.HttpScheduler.Api
{
    public class Startup
    {
        private Config Config { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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
            services.AddSingleton<ForwardJob>();
            services.AddSingleton<MyActionBlock>();

            services.AddSingleton<SchedulerRunner>();

            if (Config.UnitTest)
            {
                services.AddSingleton<IJobProcessor, MockJobProcessor>();
            }

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            if (!app.ApplicationServices.GetService<IDatabase>().IsConnected(""))
            {
                throw new TypeLoadException($"Redis server is not ready. Host={Config.RedisConnectionString}");
            }

            app.ApplicationServices.GetService<SortedSetScheduleClient>();
            var schedulerRunner = app.ApplicationServices.GetService<SchedulerRunner>();
            var manualEvent = new ManualResetEventSlim();

            var actionBlock = app.ApplicationServices.GetService<MyActionBlock>();
            actionBlock.EnableForward = Config.EnableForward;

            applicationLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    await schedulerRunner.RunAsync(applicationLifetime.ApplicationStopped).ConfigureAwait(false);
                    await actionBlock.CompleteAsync().ConfigureAwait(false);
                    manualEvent.Set();
                    Console.WriteLine("Background stopped");
                });
            });

            applicationLifetime.ApplicationStopped.Register(() =>
            {
                if (manualEvent.Wait(TimeSpan.FromSeconds(2)))
                {
                    Console.WriteLine("Gracefully shutdown");
                }
                else
                {
                    Console.Error.WriteLine("Shutdown incorrectly");
                }
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
